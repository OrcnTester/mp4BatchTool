using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions; // <--- DJI tarih parse için
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.WebRequestMethods;


namespace mp4BatchTool
{
	public partial class Form1 : Form
	{
		// ffmpeg iptal + process takip
		private volatile bool _cancelRequested = false;
		private readonly object _procLock = new object();
		private Process _currentFfmpegProcess = null;

		private const string HelpText =
@"MP4 Batch Tool – Kullanım Kılavuzu

Bu araç, birden fazla MP4 videoyu toplu olarak işlemen için tasarlandı.
Güncel sürümde video işleme 3 farklı mod üzerinden yapılır:
- Sadece sesi kaldır (video kopyalanır, en hızlı yöntem)
- 720p’ye düşürme seçeneği
- İsteğe bağlı zaman pulu (timestamp) basma
- NVENC GPU hızlandırma desteği
- Tek çıktıda video birleştirme (concat + overlay)
- İlerleme çubuğu ve kalan süre (ETA)

------------------------------------------------------------
1) Ön Koşullar
------------------------------------------------------------
- Bilgisayarda ffmpeg ve ffprobe kurulu olmalıdır.
- Program şu yolları kullanır:
  C:\ffmpeg\bin\ffmpeg.exe
  C:\ffmpeg\bin\ffprobe.exe

------------------------------------------------------------
2) Dosya Listesine Video Ekleme
------------------------------------------------------------
- MP4 dosyalarını sürükleyip bırakabilir veya 'Dosya Ekle' butonuyla seçebilirsin.
- Sadece .mp4 uzantılı ve listede tekrar olmayan dosyalar eklenir.

------------------------------------------------------------
3) Zaman Pulu (Timestamp) – isteğe bağlı
------------------------------------------------------------
- 'Zaman pulu bas' işaretli ise:
  - Videonun üzerine akan tarih-saat overlay'i eklenir.
  - DJI dosya adlarında (DJI_YYYYMMDDhhmmss_...) tarih ve saat otomatik çözümlenir.
  - DJI değilse, dosyanın oluşturulma tarihi (creation time) kullanılır.
  - 720p seçeneği açıksa scale + timestamp birlikte uygulanır.
  - 720p kapalıysa timestamp orijinal çözünürlükte basılır.
- İşaretli değilse:
  - Videoya timestamp eklenmez.

------------------------------------------------------------
4) 720p’ye Scale Et – isteğe bağlı
------------------------------------------------------------
- '720p’ye scale et' işaretli ise:
  - Video yeniden encode edilerek 720p’ye düşürülür.
- Timestamp kapalı + 720p kapalı → video kopyalanır (encode yok, en hızlı mod).

------------------------------------------------------------
5) GPU ile Encode (NVENC)
------------------------------------------------------------
- 'GPU ile encode (NVENC)' işaretli ise:
  - Yeniden encode gerektiğinde h264_nvenc kullanılır.
  - Uygun NVIDIA GPU ve NVENC destekli ffmpeg build’i gereklidir.
- İşaretli değilse CPU (libx264) ile encode yapılır.

------------------------------------------------------------
6) 'Ses Sil' Butonu – 3 çalışma modu
------------------------------------------------------------
Her dosya için seçili seçeneklere göre davranış:

A) Timestamp KAPALI + 720p KAPALI
   - Video yeniden encode edilmez.
   - Ses kaldırılır, video kopyalanır.
   - Çıktı: orijinal_ad_noaudio.mp4 (EN HIZLI YÖNTEM)

B) Timestamp KAPALI + 720p AÇIK
   - Video 720p’ye düşürülür ve ses kaldırılır.
   - Encode GPU/CPU seçimine göre yapılır.

C) Timestamp AÇIK (720p açık veya kapalı)
   - Timestamp overlay eklenir.
   - Gerekirse 720p’ye scale edilir.
   - Videonun yeniden encode edilmesi zorunludur.

İlerleme çubuğu ve tahmini kalan süre (ETA) tüm modlarda çalışır.

------------------------------------------------------------
7) 'Tek MP4 Yap' Butonu
------------------------------------------------------------
- Listedeki en az 2 videoyu birleştirir.

A) Overlay alanları boş ise:
   - Videolar yeniden encode edilmeden concat yöntemiyle birleştirilir.
   - Hızlıdır ve kalite kaybı olmaz (–c copy).

B) Sabit metin ve/veya başlangıç saati girilmişse:
   - Üstüne yazı basılır (drawtext).
   - Bu durumda yeniden encode zorunludur ve ses kaldırılır.
   - GPU/CPU tercihine göre işlem yapılır.

------------------------------------------------------------
8) İptal Et
------------------------------------------------------------
- Devam eden ffmpeg işlemini durdurur.
- Durum mesajı güncellenir ve butonlar normal hale döner.

------------------------------------------------------------
9) Listeyi Temizle
------------------------------------------------------------
- Dosya listesini temizler (gerçek dosyalar silinmez).

------------------------------------------------------------
10) Performans
------------------------------------------------------------
- Süre; video uzunluğu, çözünürlük, yeniden encode gerekliliği,
  preset ve donanım gücüne göre değişir.
- NVENC destekli GPU’lar büyük setlerde ciddi hız kazandırır.
- En hızlı mod: Timestamp KAPALI + 720p KAPALI (video kopyalama).

------------------------------------------------------------
11) Hata Durumları
------------------------------------------------------------
- 'ffmpeg hata kodu: ...' görürsen:
  - ffmpeg/ffprobe yollarını,
  - giriş dosyalarının sağlamlığını,
  - NVENC kullanıyorsan donanım ve ffmpeg desteğini kontrol et.";


		// ffmpeg console log alanı (txtConsole RichTextBox'ına yazar)
		private void AppendLog(string line)
		{
			if (string.IsNullOrWhiteSpace(line))
				return;

			if (InvokeRequired)
			{
				BeginInvoke(new Action<string>(AppendLog), line);
				return;
			}

			const int maxLength = 50000;
			if (txtConsole.TextLength > maxLength)
			{
				txtConsole.Clear();
				txtConsole.AppendText("[log trimmed]" + Environment.NewLine);
			}

			txtConsole.AppendText(line + Environment.NewLine);
			txtConsole.SelectionStart = txtConsole.TextLength;
			txtConsole.ScrollToCaret();
		}

		public Form1()
		{
			InitializeComponent();
			// Resource'taki logo'yu arkaya veriyoruz
			lstFiles.BackgroundLogo = Properties.Resources._10zmnlogo;  // logo ismine göre düzelt

			// Drag & drop desteği
			lstFiles.AllowDrop = true;
			lstFiles.DragEnter += LstFiles_DragEnter;
			lstFiles.DragDrop += LstFiles_DragDrop;
		}

		// ---- Drag & Drop ----
		private void LstFiles_DragEnter(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
				e.Effect = DragDropEffects.Copy;
		}

		private void LstFiles_DragDrop(object sender, DragEventArgs e)
		{
			var files = (string[])e.Data.GetData(DataFormats.FileDrop);
			AddFiles(files);
		}

		// ---- Dosya ekleme butonu ----
		private void btnAddFiles_Click(object sender, EventArgs e)
		{
			using (var ofd = new OpenFileDialog())
			{
				ofd.Filter = "MP4 Files|*.mp4";
				ofd.Multiselect = true;

				if (ofd.ShowDialog() == DialogResult.OK)
				{
					AddFiles(ofd.FileNames);
				}
			}
		}

		private void AddFiles(IEnumerable<string> files)
		{
			foreach (var f in files)
			{
				if (Path.GetExtension(f).Equals(".mp4", StringComparison.OrdinalIgnoreCase)
					&& !lstFiles.Items.Contains(f))
				{
					lstFiles.Items.Add(f);
				}
			}

			UpdateStatus($"{lstFiles.Items.Count} dosya listede.");
		}

		// ---- DJI dosya adından tarih-saat çekme ----
		// Örnek isim: DJI_20251203003446_0001_D.mp4
		// Çıkış: 2025-12-03 00:34:46
		private bool TryGetDjiStartTime(string filePath, out DateTime startTime)
		{
			startTime = default;

			string fileName = Path.GetFileNameWithoutExtension(filePath);
			// Örn: "DJI_20251203003446_0001_D"

			var match = Regex.Match(fileName, @"^DJI_(\d{14})_");
			if (!match.Success)
				return false;

			string ts = match.Groups[1].Value; // "20251203003446"

			int year = int.Parse(ts.Substring(0, 4));
			int month = int.Parse(ts.Substring(4, 2));
			int day = int.Parse(ts.Substring(6, 2));
			int hour = int.Parse(ts.Substring(8, 2));
			int minute = int.Parse(ts.Substring(10, 2));
			int second = int.Parse(ts.Substring(12, 2));

			startTime = new DateTime(year, month, day, hour, minute, second);
			return true;
		}

		// ---- DJI timestamp'i drawtext filtresine çevirme ----
		private string BuildTimestampDrawTextFilter(DateTime dt)
		{
			// Tarih sabit, saat akacak.
			// Örn: DJI_20251203003446_0001_D  ->  Tarih: 2025-12-03, Saat başlangıcı: 00:34:46
			string dateStr = dt.ToString("yyyy-MM-dd");

			// Güvenli hale getir (tek tırnak, backslash vs.)
			string escapedDate = dateStr
				.Replace("\\", "\\\\")
				.Replace("'", "\\'");

			// Gün içi saniye (sadece saat kısmı): 00:34:46 -> (0*3600 + 34*60 + 46)
			int baseSeconds = dt.Hour * 3600 + dt.Minute * 60 + dt.Second;

			// ffmpeg drawtext içinde dinamik HH:MM:SS üretimi
			// t = videonun saniye cinsinden geçen süresi
			string timeExpr =
				$"%{{eif\\:(t+{baseSeconds})/3600\\:d\\:2}}\\:" +
				$"%{{eif\\:mod((t+{baseSeconds})/60\\,60)\\:d\\:2}}\\:" +
				$"%{{eif\\:mod(t+{baseSeconds}\\,60)\\:d\\:2}}";

			// Ekranda gözükecek final text:
			// "2025-12-03 00:34:46" gibi ama saat kısmı t ile akıyor
			string finalText = $"{escapedDate} {timeExpr}";

			string filter =
				"drawtext=fontfile='C\\\\Windows\\\\Fonts\\\\arial.ttf'" +
				$": text='{finalText}'" +
				": x=10: y=h-th-20" +
				": fontsize=24: fontcolor=white" +
				": box=1: boxcolor=black@0.5: boxborderw=5";

			return filter;
		}

		// ---- ffmpeg (sadece çalıştır, progress yok; console log + iptal var) ----
		private Task RunFfmpegAsync(string arguments)
		{
			return Task.Run(() =>
			{
				var psi = new ProcessStartInfo
				{
					FileName = @"C:\ffmpeg\bin\ffmpeg.exe",
					Arguments = arguments,
					UseShellExecute = false,
					CreateNoWindow = true,
					RedirectStandardOutput = true,
					RedirectStandardError = true
				};

				Process proc = null;

				try
				{
					proc = new Process { StartInfo = psi };

					proc.OutputDataReceived += (s, e) =>
					{
						if (e.Data == null) return;
						if (_cancelRequested) return;

						AppendLog("[O] " + e.Data);
					};

					proc.ErrorDataReceived += (s, e) =>
					{
						if (e.Data == null) return;
						if (_cancelRequested) return;

						AppendLog("[E] " + e.Data);
					};

					lock (_procLock)
					{
						_currentFfmpegProcess = proc;
					}

					proc.Start();
					proc.BeginOutputReadLine();
					proc.BeginErrorReadLine();
					proc.WaitForExit();

					int exitCode = proc.ExitCode;

					if (_cancelRequested)
						return;

					if (exitCode != 0)
					{
						throw new Exception("ffmpeg hata kodu: " + exitCode);
					}
				}
				finally
				{
					lock (_procLock)
					{
						_currentFfmpegProcess = null;
					}

					proc?.Dispose();
				}
			});
		}


		// ---- SES SİL butonu ----
		// Artık TÜM dosyalarda:
		//  - Video 720p'ye scale edilir (1080p -> 720p vb.)
		//  - Checkbox işaretliyse: timestamp basılır (DJI: dosya adından, değilse: file creation time)
		//  - Checkbox işaretli değilse: timestamp YOK, sadece 720p + ses sil
		//  - Progress bar + ETA güncel kalır
		private async void btnRemoveAudio_Click(object sender, EventArgs e)
		{
			if (lstFiles.Items.Count == 0)
			{
				MessageBox.Show("Önce listeye MP4 dosyaları ekle.");
				return;
			}

			btnRemoveAudio.Enabled = false;
			btnMerge.Enabled = false;
			_cancelRequested = false;
			btnCancel.Enabled = true;


			try
			{
				var files = lstFiles.Items.Cast<string>().ToList();
				int fileCount = files.Count;

				// Tüm dosyaların süresini oku (saniye cinsinden)
				var durations = new List<double>();
				double totalDuration = 0;

				foreach (var f in files)
				{
					double d = GetVideoDurationSeconds(f);
					durations.Add(d);
					totalDuration += d;
				}

				var sw = Stopwatch.StartNew();
				UpdateProgress(0.0, null);

				for (int idx = 0; idx < fileCount; idx++)
				{
					if (_cancelRequested)
					{
						UpdateStatus("İşlem iptal edildi.");
						break;
					}
					string file = files[idx];
					string dir = Path.GetDirectoryName(file);
					string name = Path.GetFileNameWithoutExtension(file);
					string output = Path.Combine(dir, name + "_noaudio.mp4");

					// Bu dosyadan önceki toplam süre
					double processedBefore = 0;
					if (totalDuration > 0)
					{
						for (int j = 0; j < idx; j++)
							processedBefore += durations[j];
					}

					// Checkbox durumları
					bool addTimestamp = chkTimestamp.Checked;
					bool scale720 = chkScale720.Checked;

					string fullFilter = null;
					string statusPrefix;

					// Timestamp varsa: DateTime çıkar (DJI adı veya creation time)
					DateTime tsDateTime = DateTime.MinValue;
					bool isDji = false;

					if (addTimestamp)
					{
						isDji = TryGetDjiStartTime(file, out DateTime djiTime);
						if (isDji)
							tsDateTime = djiTime;
						else
							tsDateTime = System.IO.File.GetCreationTime(file);

						string drawTextFilter = BuildTimestampDrawTextFilter(tsDateTime);

						if (scale720)
						{
							// 720p + timestamp
							fullFilter = $"scale=-2:720,{drawTextFilter}";
							statusPrefix = isDji
								? "Ses siliyor + 720p + DJI timestamp basıyor"
								: "Ses siliyor + 720p + creation time timestamp basıyor";
						}
						else
						{
							// Orijinal çözünürlük + timestamp
							fullFilter = drawTextFilter;
							statusPrefix = isDji
								? "Ses siliyor + DJI timestamp basıyor"
								: "Ses siliyor + creation time timestamp basıyor";
						}
					}
					else
					{
						if (scale720)
						{
							// Timestamp yok ama 720p istiyor
							fullFilter = "scale=-2:720";
							statusPrefix = "Ses siliyor + 720p (timestamp kapalı)";
						}
						else
						{
							// FAST PATH: sadece ses sil, video kopya
							statusPrefix = "Ses siliyor (video kopya, timestamp ve scale kapalı)";
						}
					}

					double fileDur = durations[idx];
					UpdateStatus($"{statusPrefix} ({idx + 1}/{fileCount}): {name}");

					bool useGpu = chkUseGpu.Checked;

					string args;

					if (!addTimestamp && !scale720)
					{
						// *** En hızlı mod: video kopya, sadece ses sil ***
						args =
							$"-y -v error -i \"{file}\" " +
							"-an -c:v copy " +
							"-progress pipe:1 -nostats " +
							$"\"{output}\"";
					}
					else
					{
						// Buraya geldiysen mutlaka bir filtre var (scale ve/veya drawtext)
						// GPU/CPU seçimine göre encode
						if (useGpu)
						{
							// GPU encode (NVENC)
							args =
								$"-y -v error -i \"{file}\" " +
								$"-vf \"{fullFilter}\" " +
								"-an -c:v h264_nvenc -preset fast " +
								"-b:v 2000k -maxrate 2000k -bufsize 4000k " +
								"-progress pipe:1 -nostats " +
								$"\"{output}\"";
						}
						else
						{
							// CPU encode (libx264)
							args =
								$"-y -v error -i \"{file}\" " +
								$"-vf \"{fullFilter}\" " +
								"-an -c:v libx264 -preset veryfast " +
								"-b:v 2000k -maxrate 2000k -bufsize 4000k " +
								"-progress pipe:1 -nostats " +
								$"\"{output}\"";
						}
					}

					AppendLog("");
					AppendLog("=== [SES SİL] ffmpeg komutu ===");
					AppendLog("ffmpeg " + args);

					await RunFfmpegWithProgressAsync(
						args,
						fileDur,
						progressWithinFile =>
						{
							double ratio;

							if (totalDuration > 0 && fileDur > 0)
							{
								double done = processedBefore + progressWithinFile * fileDur;
								ratio = done / totalDuration;
							}
							else
							{
								ratio = (idx + progressWithinFile) / fileCount;
							}

							var elapsed = sw.Elapsed;
							TimeSpan? eta = null;

							if (ratio > 0.0001)
							{
								double totalSecEst = elapsed.TotalSeconds / ratio;
								double remainingSec = totalSecEst - elapsed.TotalSeconds;
								if (remainingSec < 0) remainingSec = 0;
								eta = TimeSpan.FromSeconds(remainingSec);
							}

							UpdateProgress(ratio, eta);
						});

				}

				// Tamamlandı
				UpdateProgress(1.0, TimeSpan.Zero);
				UpdateStatus("Tüm dosyalar işlendi.");
				MessageBox.Show("Bitti! Her dosya için 720p, sessiz ve (açıksa) timestamp'li *_noaudio.mp4 oluşturuldu.");
			}
			catch (Exception ex)
			{
				if (!_cancelRequested)
				{
					MessageBox.Show("Hata: " + ex.Message);
				}
				else
				{
					UpdateStatus("İşlem iptal edildi.");
				}
			}
			finally
			{
					btnRemoveAudio.Enabled = true;
					btnMerge.Enabled = true;
					btnCancel.Enabled = false;
					_cancelRequested = false;
			}
		}



		// ---- TEK MP4 YAP butonu ----
		private async void btnMerge_Click(object sender, EventArgs e)
		{
			if (lstFiles.Items.Count < 2)
			{
				MessageBox.Show("Birleştirmek için en az 2 MP4 ekle.");
				return;
			}

			var files = lstFiles.Items.Cast<string>().ToList();

			using (var sfd = new SaveFileDialog())
			{
				sfd.Filter = "MP4 Video|*.mp4";
				sfd.FileName = "merged.mp4";

				if (sfd.ShowDialog() != DialogResult.OK)
					return;

				string outputPath = sfd.FileName;

				btnRemoveAudio.Enabled = false;
				btnMerge.Enabled = false;
				_cancelRequested = false;
				btnCancel.Enabled = true;


				try
				{
					// 0) Toplam süreyi hesapla (saniye cinsinden)
					double totalDuration = 0;
					foreach (var f in files)
						totalDuration += GetVideoDurationSeconds(f);

					// 1) concat list dosyası
					string tempList = Path.Combine(
						Path.GetTempPath(),
						"ffmpeg_concat_" + Guid.NewGuid() + ".txt"
					);

					var lines = files.Select(f => $"file '{f.Replace("'", "\\'")}'");
					System.IO.File.WriteAllLines(tempList, lines);

					// 2) Overlay metinleri
					string overlayText = txtOverlayText.Text.Trim();   // statik kısım (tarih + koordinat)
					string startTimeText = txtStartTime.Text.Trim();   // HH:MM:SS

					bool hasOverlayText = !string.IsNullOrWhiteSpace(overlayText);
					bool hasStartTime = !string.IsNullOrWhiteSpace(startTimeText);

					// Başlangıç saatini saniyeye çevir (HH:MM:SS -> total seconds)
					int startSeconds = 0;
					if (hasStartTime)
					{
						if (TimeSpan.TryParse(startTimeText, out var ts))
						{
							startSeconds = (int)ts.TotalSeconds;
						}
						else
						{
							MessageBox.Show("Başlangıç saati formatı geçersiz. Örnek: 14:30:00");
							return;
						}
					}

					// Overlay hiç yoksa (ne yazı ne saat) -> hızlı concat
					bool addOverlay = hasOverlayText || hasStartTime;

					string ffArgs;

					if (!addOverlay)
					{
						// HIZLI MERGE: yeniden encode yok ama progress var
						ffArgs =
							$"-y -v error -f concat -safe 0 -i \"{tempList}\" " +
							"-c copy " +
							"-progress pipe:1 -nostats " +
							$"\"{outputPath}\"";
					}
					else
					{
						// 3) Statik kısmı güvenli hale getir
						string staticPart = overlayText;
						if (staticPart.Length == 0)
							staticPart = ""; // sadece saat de gösterebiliriz

						string escapedStatic = staticPart
							.Replace("\\", "\\\\")
							.Replace("'", "\\'");

						// 4) Dinamik saat ifadesi
						// Eğer başlangıç saati yoksa, 00:00:00'dan başlasın
						// t = geçen süre (saniye)
						int baseSeconds = startSeconds; // 0 veya gerçek başlangıç saniyesi

						string timeExpr =
							$"%{{eif\\:(t+{baseSeconds})/3600\\:d\\:2}}\\:" +
							$"%{{eif\\:mod((t+{baseSeconds})/60\\,60)\\:d\\:2}}\\:" +
							$"%{{eif\\:mod(t+{baseSeconds}\\,60)\\:d\\:2}}";

						// Son gösterilecek text:
						// - Sadece statik varsa: "STATIC"
						// - Sadece saat varsa: "HH:MM:SS" (dynamic)
						// - İkisi de varsa: "STATIC | HH:MM:SS"
						string finalText;
						if (hasOverlayText && hasStartTime)
							finalText = $"{escapedStatic} | {timeExpr}";
						else if (hasOverlayText && !hasStartTime)
							finalText = escapedStatic; // sadece statik
						else // !hasOverlayText && hasStartTime
							finalText = timeExpr; // sadece dinamik saat

						string filter =
							"drawtext=fontfile='C\\\\Windows\\\\Fonts\\\\arial.ttf'" +
							$": text='{finalText}'" +
							": x=w-tw-20: y=h-th-20" +
							": fontsize=24: fontcolor=white" +
							": box=1: boxcolor=black@0.5: boxborderw=5";

						bool useGpu = chkUseGpu.Checked;

						// Yazı bastığımız için yeniden encode şart
						// (ve sesleri de kaldırıyoruz -> -an)
						if (useGpu)
						{
							// GPU encode (NVENC)
							ffArgs =
								$"-y -v error -f concat -safe 0 -i \"{tempList}\" " +
								$"-vf \"{filter}\" " +
								"-c:v h264_nvenc -preset fast " +
								"-b:v 2000k -maxrate 2000k -bufsize 4000k -an " +
								"-progress pipe:1 -nostats " +
								$"\"{outputPath}\"";
						}
						else
						{
							// CPU encode (libx264)
							ffArgs =
								$"-y -v error -f concat -safe 0 -i \"{tempList}\" " +
								$"-vf \"{filter}\" " +
								"-c:v libx264 -crf 23 -preset veryfast -an " +
								"-progress pipe:1 -nostats " +
								$"\"{outputPath}\"";
						}
						AppendLog("");
						AppendLog("=== [MERGE] ffmpeg komutu ===");
						AppendLog("ffmpeg " + ffArgs);


					}

					var sw = System.Diagnostics.Stopwatch.StartNew();
					UpdateProgress(0.0, null);
					UpdateStatus("Dosyalar birleştiriliyor...");

					if (totalDuration > 0)
					{
						// ffmpeg out_time / totalDuration üzerinden global % + ETA
						await RunFfmpegWithProgressAsync(
							ffArgs,
							totalDuration,
							progress =>
							{
								double ratio = progress;
								if (ratio < 0) ratio = 0;
								if (ratio > 1) ratio = 1;

								var elapsed = sw.Elapsed;
								TimeSpan? eta = null;

								if (ratio > 0.0001)
								{
									double totalSecEst = elapsed.TotalSeconds / ratio;
									double remainingSec = totalSecEst - elapsed.TotalSeconds;
									if (remainingSec < 0) remainingSec = 0;
									eta = TimeSpan.FromSeconds(remainingSec);
								}

								UpdateProgress(ratio, eta);
							});
					}
					else
					{
						// Süre okunamadıysa: normal çalış, sonunda %100 yap
						await RunFfmpegAsync(ffArgs);
						UpdateProgress(1.0, TimeSpan.Zero);
					}

					UpdateStatus("Birleştirme tamam.");
					MessageBox.Show("Birleştirme bitti!");
				}
				catch (Exception ex)
				{
					if (!_cancelRequested)
					{
						MessageBox.Show("Hata: " + ex.Message);
					}
					else
					{
						UpdateStatus("İşlem iptal edildi.");
					}
				}
				finally
				{
					btnRemoveAudio.Enabled = true;
					btnMerge.Enabled = true;
					btnCancel.Enabled = false;
					_cancelRequested = false;
				}
			}
		}




		// ---- Listeyi Temizle ----
		private void btnClear_Click(object sender, EventArgs e)
		{
			lstFiles.Items.Clear();
			UpdateStatus("Liste temizlendi.");
		}
		private double GetVideoDurationSeconds(string filePath)
		{
			try
			{
				var psi = new ProcessStartInfo
				{
					FileName = @"C:\ffmpeg\bin\ffprobe.exe", // ffprobe yolu
					Arguments = $"-v error -show_entries format=duration " +
								"-of default=noprint_wrappers=1:nokey=1 " +
								$"\"{filePath}\"",
					UseShellExecute = false,
					CreateNoWindow = true,
					RedirectStandardOutput = true,
					RedirectStandardError = true
				};

				using (var proc = new Process { StartInfo = psi })
				{
					proc.Start();
					string output = proc.StandardOutput.ReadToEnd();
					proc.WaitForExit();

					string s = output.Trim().Replace(',', '.');

					if (double.TryParse(
							s,
							NumberStyles.Any,
							CultureInfo.InvariantCulture,
							out double seconds))
					{
						return seconds;
					}
				}
			}
			catch
			{
				// ffprobe yoksa ya da hata olursa 0 döneriz (fallback)
			}

			return 0; // bilinmiyor
		}
		private void UpdateProgress(double ratio, TimeSpan? eta = null)
		{
			if (InvokeRequired)
			{
				BeginInvoke(new Action<double, TimeSpan?>(UpdateProgress), ratio, eta);
				return;
			}

			int percent = (int)Math.Round(ratio * 100);
			if (percent < 0) percent = 0;
			if (percent > 100) percent = 100;

			if (progressBar1 != null)
			{
				progressBar1.Minimum = 0;
				progressBar1.Maximum = 100;
				progressBar1.Value = percent;
			}

			if (eta.HasValue)
			{
				lblStatus.Text = $"İşleniyor... %{percent} ~ Kalan tahmini: {eta.Value:hh\\:mm\\:ss}";
			}
			else
			{
				lblStatus.Text = $"İşleniyor... %{percent}";
			}
		}

		// ---- ffmpeg (progress + console log + iptal destekli) ----
		private Task RunFfmpegWithProgressAsync(string arguments, double fileDurationSeconds, Action<double> onProgress)
		{
			return Task.Run(() =>
			{
				var psi = new ProcessStartInfo
				{
					FileName = @"C:\ffmpeg\bin\ffmpeg.exe",
					Arguments = arguments,            // içinde -progress pipe:1 olmalı
					UseShellExecute = false,
					CreateNoWindow = true,
					RedirectStandardOutput = true,   // progress + log
					RedirectStandardError = true     // log
				};

				Process proc = null;

				try
				{
					proc = new Process { StartInfo = psi };

					// stdout: hem progress parse, hem log
					proc.OutputDataReceived += (s, e) =>
					{
						if (e.Data == null) return;
						if (_cancelRequested) return;

						string line = e.Data.Trim();
						AppendLog("[O] " + line);

						// Örn: out_time=00:12:34.123456
						if (line.StartsWith("out_time="))
						{
							string t = line.Substring("out_time=".Length);

							if (TimeSpan.TryParse(t, CultureInfo.InvariantCulture, out var ts)
								&& fileDurationSeconds > 0)
							{
								double p = ts.TotalSeconds / fileDurationSeconds;
								if (p < 0) p = 0;
								if (p > 1) p = 1;

								onProgress?.Invoke(p);
							}
						}
						else if (line.StartsWith("progress=") && line.Contains("end"))
						{
							onProgress?.Invoke(1.0);
						}
					};

					// stderr: sadece log
					proc.ErrorDataReceived += (s, e) =>
					{
						if (e.Data == null) return;
						if (_cancelRequested) return;

						string line = e.Data.Trim();
						AppendLog("[E] " + line);
					};

					lock (_procLock)
					{
						_currentFfmpegProcess = proc;
					}

					proc.Start();
					proc.BeginOutputReadLine();
					proc.BeginErrorReadLine();
					proc.WaitForExit();

					int exitCode = proc.ExitCode;

					if (_cancelRequested)
						return;

					if (exitCode != 0)
					{
						throw new Exception("ffmpeg hata kodu: " + exitCode);
					}
				}
				finally
				{
					lock (_procLock)
					{
						_currentFfmpegProcess = null;
					}

					proc?.Dispose();
				}
			});
		}


		// ---- Status Label güncelleme ----
		private void UpdateStatus(string text)
		{
			if (InvokeRequired)
			{
				BeginInvoke(new Action<string>(UpdateStatus), text);
				return;
			}

			lblStatus.Text = text;
		}

        private void chkTimestamp_CheckedChanged(object sender, EventArgs e)
        {

        }

		private void btnCancel_Click(object sender, EventArgs e)
		{
			_cancelRequested = true;
			UpdateStatus("İşlem iptal ediliyor...");

			lock (_procLock)
			{
				try
				{
					_currentFfmpegProcess?.Kill();
				}
				catch
				{
					// Süreç zaten bitmiş olabilir, boşver
				}
			}
		}

        private void lblOverlayCaption_Click(object sender, EventArgs e)
        {

        }

			private void btnHelp_Click(object sender, EventArgs e)
		{
			// Yeni bir form oluştur
			var helpForm = new Form
			{
				Text = "MP4 Batch Tool – Yardım",
				StartPosition = FormStartPosition.CenterParent,
				Size = new System.Drawing.Size(800, 600),    // İstersen değiştir
				MinimizeBox = false,
				MaximizeBox = true,                         // Büyütmek istersen
				ShowInTaskbar = false
			};

			// İçine multi-line, read-only, scroll'lu bir TextBox koyalım
			var txtHelp = new TextBox
			{
				Multiline = true,
				ReadOnly = true,
				ScrollBars = ScrollBars.Vertical,
				Dock = DockStyle.Fill,
				Text = HelpText,
				Font = new System.Drawing.Font("Consolas", 9f), // İsteğe bağlı, monospaced
				WordWrap = false                                // Satırlar kırılmasın istiyorsan false
			};

			helpForm.Controls.Add(txtHelp);

			// Modal açalım
			helpForm.ShowDialog(this);
		}

		private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			string url = "https://github.com/OrcnTester";

			try
			{
				// .NET Framework’te genelde bu tek satır bile yeter:
				// System.Diagnostics.Process.Start(url);

				// Hem .NET Framework hem .NET 5+ uyumlu olsun diye:
				var psi = new System.Diagnostics.ProcessStartInfo
				{
					FileName = url,
					UseShellExecute = true
				};
				System.Diagnostics.Process.Start(psi);

				// İstersen tıklandı olarak işaretle:
				linkLabel1.LinkVisited = true;
			}
			catch (Exception ex)
			{
				MessageBox.Show("Tarayıcı açılamadı: " + ex.Message);
			}
		}

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
			string url = "https://www.instagram.com/onteknikzemin/";

			try
			{
				// .NET Framework’te genelde bu tek satır bile yeter:
				// System.Diagnostics.Process.Start(url);

				// Hem .NET Framework hem .NET 5+ uyumlu olsun diye:
				var psi = new System.Diagnostics.ProcessStartInfo
				{
					FileName = url,
					UseShellExecute = true
				};
				System.Diagnostics.Process.Start(psi);

				// İstersen tıklandı olarak işaretle:
				linkLabel1.LinkVisited = true;
			}
			catch (Exception ex)
			{
				MessageBox.Show("Tarayıcı açılamadı: " + ex.Message);
			}
		}
    }
}
