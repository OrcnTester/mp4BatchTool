# MP4 Batch Tool 🎬

Small WinForms utility I built to batch-process long MP4 drone videos (DJI & others).

It automates a workflow that was very painful to do by hand:

- Remove all audio tracks
- Downscale to **720p**
- Optionally overlay a **dynamic timestamp** on the video  
  - For DJI files, timestamp is parsed from the **filename**  
  - For normal MP4s, timestamp comes from the **file creation time**
- Optionally use **GPU encoding (NVENC)** instead of CPU
- Show **global progress bar** and **ETA**
- Show **live ffmpeg logs** in a console-like panel
- Allow **cancellation** of the current batch safely

---

## ✨ Features

- 🖥 **WinForms UI**
  - Drag & drop MP4 files into a list
  - Buttons:
    - `Ses Sil` → per-file processing (720p, mute, optional timestamp)
    - `Tek MP4 Yap` → merge multiple videos into one
    - `İptal Et` → cancel ongoing ffmpeg process
    - `Listeyi Temizle`
  - Checkboxes:
    - `Zaman pulu bas` → toggle timestamp overlay
    - `GPU ile encode (NVENC)` → toggle between `libx264` and `h264_nvenc`

- 🎞 **Timestamp overlay**
  - DJI pattern: `DJI_YYYYMMDDHHMMSS_0001_D.mp4`
    - Extracts `YYYY-MM-DD` + time as start
    - Uses ffmpeg `drawtext` + `eif` to render a **dynamic clock** (`t + baseSeconds`)
  - Non-DJI:
    - Uses `File.GetCreationTime(...)` as base DateTime
  - Example overlay: `2025-12-03 00:34:46` and the time **keeps running** with the video

- 📉 **Resolution & bitrate**
  - All processed files are scaled to **720p**:
    - `scale=-2:720` (keeps aspect ratio, width even)
  - Target bitrate: ~ **2000 kbps**
  - NVENC: `-c:v h264_nvenc -preset fast`
  - CPU: `-c:v libx264 -preset veryfast`

- 🧮 **Progress + ETA**
  - Uses `ffprobe` to get duration of each input file
  - Uses ffmpeg `-progress pipe:1` and parses `out_time=...`
  - Shows:
    - Global percent (%)
    - Estimated time remaining (ETA)
  - Works for both per-file processing and merge

- ❌ **Cancel support**
  - `İptal Et` sets `_cancelRequested = true`
  - Kills current ffmpeg process
  - Cleans up state without crashing
  - UI returns to an idle/ready state

- 📜 **Built-in “console”**
  - A `RichTextBox` shows ffmpeg logs live:
    - `[O] ...` for stdout
    - `[E] ...` for stderr
  - Gives the “CLI-level transparency” inside the GUI

---

## 🧩 How it works (high-level)

- **ffprobe**  
  Used to query duration (seconds) for each MP4:

  ```bash
  ffprobe -v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 "video.mp4"
  ```

- **ffmpeg drawtext**  
  Timestamp filter is built from either DJI filename or file creation time:

  ```text
  drawtext=fontfile='C\Windows\Fonts\arial.ttf':
           text='2025-12-03 %{eif:(t+baseSeconds)/3600:d:2}:%{eif:mod((t+baseSeconds)/60,60):d:2}:%{eif:mod(t+baseSeconds,60):d:2}':
           x=10: y=h-th-20:
           fontsize=24: fontcolor=white:
           box=1: boxcolor=black@0.5: boxborderw=5
  ```

- **GPU vs CPU**
  - **GPU (NVENC)**:

    ```bash
    -c:v h264_nvenc -preset fast -b:v 2000k -maxrate 2000k -bufsize 4000k
    ```

  - **CPU (libx264)**:

    ```bash
    -c:v libx264 -preset veryfast -b:v 2000k -maxrate 2000k -bufsize 4000k
    ```

- **Merge mode**
  - Builds a concat list file:

    ```text
    file 'file1.mp4'
    file 'file2.mp4'
    ...
    ```

  - If no overlay text / time is provided:
    - Fast path: `-f concat -safe 0 -i list.txt -c copy`
  - If overlay is used:
    - Re-encodes with filter + mute + progress.

---

## 🛠 Requirements

- Windows 10+ (tested on desktop)
- .NET Framework (WinForms)  
  > You may need to retarget the project depending on your environment.
- `ffmpeg` and `ffprobe` installed  
  - Project expects them at:

    ```text
    C:\ffmpeg\bin\ffmpeg.exe
    C:\ffmpeg\bin\ffprobe.exe
    ```

  - Or update the paths in `Form1.cs`.

- Optional: NVIDIA GPU with **NVENC** support  
  - And an ffmpeg build compiled with NVENC enabled.

---

## 🚀 Usage

1. Clone the repository:

   ```bash
   git clone https://github.com/OrcnTester/mp4-batch-tool.git
   ```

2. Open the solution in Visual Studio (or your IDE of choice).

3. Make sure `ffmpeg.exe` and `ffprobe.exe` are available at:
   - `C:\ffmpeg\bin\ffmpeg.exe`
   - `C:\ffmpeg\bin\ffprobe.exe`  
   or update the paths in the code.

4. Build and run the WinForms app.

5. In the UI:
   - Drag & drop MP4 files into the list **or** use `Dosya Ekle`.
   - İsteğe göre:
     - `Zaman pulu bas` → open/close timestamp overlay
     - `GPU ile encode (NVENC)` → toggle GPU encoding
   - `Ses Sil`:
     - Generates per-file `*_noaudio.mp4` (720p, muted, optional timestamp)
   - `Tek MP4 Yap`:
     - Merges selected files into a single MP4
   - `İptal Et`:
     - Cancels current processing
   - Watch progress, ETA and ffmpeg logs at the bottom.

---

## 🇹🇷 Kısaca Türkçe

Bu küçük araç, özellikle **DJI drone videolarını** toplu işlemek için yazdığım bir WinForms uygulaması:

- Sesi tamamen siliyor  
- Videoyu 720p’ye çekiyor  
- İster DJI dosya adından, ister dosyanın oluşturulma tarihinden **zaman damgası** üretiyor  
- GPU (NVENC) / CPU arasında seçim yapabiliyorsun  
- Toplam % ilerleme ve tahmini kalan süre gösteriyor  
- ffmpeg log’larını arayüzde canlı olarak görebiliyorsun  
- İşleri yarıda bırakmak için **iptal butonu** var

---

## 🧭 Roadmap / Ideas

- Configurable output resolution & bitrate
- CLI mode (headless) using the same core logic
- Presets for different workflows (YouTube, archive, review copies)
- Packaging ffmpeg with the app (configurable path instead of hard-coded)

---

## 📜 License

MIT – feel free to fork, tweak and use in your own workflows.
