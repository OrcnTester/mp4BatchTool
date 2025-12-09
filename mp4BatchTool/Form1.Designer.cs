using System;
using System.Diagnostics;

namespace mp4BatchTool
{
    partial class Form1
    {

		/// <summary>
		///Gerekli tasarımcı değişkeni.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		///Kullanılan tüm kaynakları temizleyin.
		/// </summary>
		///<param name="disposing">yönetilen kaynaklar dispose edilmeliyse doğru; aksi halde yanlış.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer üretilen kod

		/// <summary>
		/// Tasarımcı desteği için gerekli metot - bu metodun 
		///içeriğini kod düzenleyici ile değiştirmeyin.
		/// </summary>
		private void InitializeComponent()
		{
			System.Windows.Forms.Button btnHelp;
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
			this.btnAddFiles = new System.Windows.Forms.Button();
			this.btnRemoveAudio = new System.Windows.Forms.Button();
			this.btnMerge = new System.Windows.Forms.Button();
			this.btnClear = new System.Windows.Forms.Button();
			this.lblStatus = new System.Windows.Forms.Label();
			this.lblOverlayCaption = new System.Windows.Forms.Label();
			this.txtOverlayText = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.txtStartTime = new System.Windows.Forms.TextBox();
			this.progressBar1 = new System.Windows.Forms.ProgressBar();
			this.chkTimestamp = new System.Windows.Forms.CheckBox();
			this.chkUseGpu = new System.Windows.Forms.CheckBox();
			this.btnCancel = new System.Windows.Forms.Button();
			this.txtConsole = new System.Windows.Forms.RichTextBox();
			this.linkLabel1 = new System.Windows.Forms.LinkLabel();
			this.linkLabel2 = new System.Windows.Forms.LinkLabel();
			this.lstFiles = new mp4BatchTool.LogoListBox();
			this.chkScale720 = new System.Windows.Forms.CheckBox();
			btnHelp = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// btnHelp
			// 
			btnHelp.Location = new System.Drawing.Point(233, 136);
			btnHelp.Name = "btnHelp";
			btnHelp.Size = new System.Drawing.Size(85, 33);
			btnHelp.TabIndex = 15;
			btnHelp.Text = "YARDIM";
			btnHelp.UseVisualStyleBackColor = true;
			btnHelp.Click += new System.EventHandler(this.btnHelp_Click);
			// 
			// btnAddFiles
			// 
			this.btnAddFiles.Location = new System.Drawing.Point(12, 136);
			this.btnAddFiles.Name = "btnAddFiles";
			this.btnAddFiles.Size = new System.Drawing.Size(203, 33);
			this.btnAddFiles.TabIndex = 1;
			this.btnAddFiles.Text = "Dosya Ekle";
			this.btnAddFiles.UseVisualStyleBackColor = true;
			this.btnAddFiles.Click += new System.EventHandler(this.btnAddFiles_Click);
			// 
			// btnRemoveAudio
			// 
			this.btnRemoveAudio.Location = new System.Drawing.Point(17, 185);
			this.btnRemoveAudio.Name = "btnRemoveAudio";
			this.btnRemoveAudio.Size = new System.Drawing.Size(117, 52);
			this.btnRemoveAudio.TabIndex = 2;
			this.btnRemoveAudio.Text = "Sesleri Sil";
			this.btnRemoveAudio.UseVisualStyleBackColor = true;
			this.btnRemoveAudio.Click += new System.EventHandler(this.btnRemoveAudio_Click);
			// 
			// btnMerge
			// 
			this.btnMerge.Location = new System.Drawing.Point(1180, 185);
			this.btnMerge.Name = "btnMerge";
			this.btnMerge.Size = new System.Drawing.Size(203, 68);
			this.btnMerge.TabIndex = 3;
			this.btnMerge.Text = "Tek MP4 Yap";
			this.btnMerge.UseVisualStyleBackColor = true;
			this.btnMerge.Click += new System.EventHandler(this.btnMerge_Click);
			// 
			// btnClear
			// 
			this.btnClear.Location = new System.Drawing.Point(1180, 136);
			this.btnClear.Name = "btnClear";
			this.btnClear.Size = new System.Drawing.Size(203, 33);
			this.btnClear.TabIndex = 4;
			this.btnClear.Text = "Listeyi Temizle";
			this.btnClear.UseVisualStyleBackColor = true;
			this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
			// 
			// lblStatus
			// 
			this.lblStatus.AutoSize = true;
			this.lblStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.lblStatus.Font = new System.Drawing.Font("Consolas", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
			this.lblStatus.Location = new System.Drawing.Point(0, 692);
			this.lblStatus.Name = "lblStatus";
			this.lblStatus.Size = new System.Drawing.Size(60, 22);
			this.lblStatus.TabIndex = 5;
			this.lblStatus.Text = "Hazır";
			// 
			// lblOverlayCaption
			// 
			this.lblOverlayCaption.AutoSize = true;
			this.lblOverlayCaption.Location = new System.Drawing.Point(1196, 274);
			this.lblOverlayCaption.Name = "lblOverlayCaption";
			this.lblOverlayCaption.Size = new System.Drawing.Size(97, 13);
			this.lblOverlayCaption.TabIndex = 6;
			this.lblOverlayCaption.Text = "Video üzerine Yazı:";
			this.lblOverlayCaption.Click += new System.EventHandler(this.lblOverlayCaption_Click);
			// 
			// txtOverlayText
			// 
			this.txtOverlayText.Location = new System.Drawing.Point(1299, 271);
			this.txtOverlayText.Name = "txtOverlayText";
			this.txtOverlayText.Size = new System.Drawing.Size(82, 20);
			this.txtOverlayText.TabIndex = 7;
			this.txtOverlayText.Text = "OnTeknikZemin";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.ImageAlign = System.Drawing.ContentAlignment.BottomRight;
			this.label1.Location = new System.Drawing.Point(1148, 310);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(149, 13);
			this.label1.TabIndex = 8;
			this.label1.Text = "Başlangıç Saati (HH:MM:SS) :";
			// 
			// txtStartTime
			// 
			this.txtStartTime.Location = new System.Drawing.Point(1303, 307);
			this.txtStartTime.Name = "txtStartTime";
			this.txtStartTime.Size = new System.Drawing.Size(78, 20);
			this.txtStartTime.TabIndex = 9;
			this.txtStartTime.Text = "00:00:00";
			// 
			// progressBar1
			// 
			this.progressBar1.Location = new System.Drawing.Point(12, 271);
			this.progressBar1.Name = "progressBar1";
			this.progressBar1.Size = new System.Drawing.Size(306, 33);
			this.progressBar1.TabIndex = 10;
			// 
			// chkTimestamp
			// 
			this.chkTimestamp.AutoSize = true;
			this.chkTimestamp.Checked = true;
			this.chkTimestamp.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkTimestamp.Location = new System.Drawing.Point(176, 189);
			this.chkTimestamp.Name = "chkTimestamp";
			this.chkTimestamp.Size = new System.Drawing.Size(104, 17);
			this.chkTimestamp.TabIndex = 11;
			this.chkTimestamp.Text = "Zaman Pulu Bas";
			this.chkTimestamp.UseVisualStyleBackColor = true;
			this.chkTimestamp.CheckedChanged += new System.EventHandler(this.chkTimestamp_CheckedChanged);
			// 
			// chkUseGpu
			// 
			this.chkUseGpu.AutoSize = true;
			this.chkUseGpu.Location = new System.Drawing.Point(17, 243);
			this.chkUseGpu.Name = "chkUseGpu";
			this.chkUseGpu.Size = new System.Drawing.Size(147, 17);
			this.chkUseGpu.TabIndex = 12;
			this.chkUseGpu.Text = "GPU ile encode (NVENC)";
			this.chkUseGpu.UseVisualStyleBackColor = true;
			// 
			// btnCancel
			// 
			this.btnCancel.Enabled = false;
			this.btnCancel.Location = new System.Drawing.Point(1128, 349);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(255, 46);
			this.btnCancel.TabIndex = 13;
			this.btnCancel.Text = "Görevi Sonlandır";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// txtConsole
			// 
			this.txtConsole.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
			this.txtConsole.Location = new System.Drawing.Point(12, 321);
			this.txtConsole.Name = "txtConsole";
			this.txtConsole.ReadOnly = true;
			this.txtConsole.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
			this.txtConsole.Size = new System.Drawing.Size(306, 255);
			this.txtConsole.TabIndex = 14;
			this.txtConsole.Text = "";
			// 
			// linkLabel1
			// 
			this.linkLabel1.AutoSize = true;
			this.linkLabel1.Location = new System.Drawing.Point(1268, 692);
			this.linkLabel1.Name = "linkLabel1";
			this.linkLabel1.Size = new System.Drawing.Size(122, 13);
			this.linkLabel1.TabIndex = 18;
			this.linkLabel1.TabStop = true;
			this.linkLabel1.Text = "github.com/OrcnTester/";
			this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
			// 
			// linkLabel2
			// 
			this.linkLabel2.AutoSize = true;
			this.linkLabel2.Location = new System.Drawing.Point(1234, 667);
			this.linkLabel2.Name = "linkLabel2";
			this.linkLabel2.Size = new System.Drawing.Size(156, 13);
			this.linkLabel2.TabIndex = 19;
			this.linkLabel2.TabStop = true;
			this.linkLabel2.Text = "instagram.com/OnTeknikZemin";
			this.linkLabel2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel2_LinkClicked);
			// 
			// lstFiles
			// 
			this.lstFiles.BackgroundLogo = null;
			this.lstFiles.Dock = System.Windows.Forms.DockStyle.Top;
			this.lstFiles.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
			this.lstFiles.FormattingEnabled = true;
			this.lstFiles.Location = new System.Drawing.Point(0, 0);
			this.lstFiles.Name = "lstFiles";
			this.lstFiles.Size = new System.Drawing.Size(1402, 121);
			this.lstFiles.TabIndex = 0;
			// 
			// chkScale720
			// 
			this.chkScale720.AutoSize = true;
			this.chkScale720.Checked = true;
			this.chkScale720.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkScale720.Location = new System.Drawing.Point(176, 220);
			this.chkScale720.Name = "chkScale720";
			this.chkScale720.Size = new System.Drawing.Size(105, 17);
			this.chkScale720.TabIndex = 20;
			this.chkScale720.Text = "720p\'ye Scale et";
			this.chkScale720.UseVisualStyleBackColor = true;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackgroundImage = global::mp4BatchTool.Properties.Resources._10zmnlogo;
			this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
			this.ClientSize = new System.Drawing.Size(1402, 714);
			this.Controls.Add(this.chkScale720);
			this.Controls.Add(this.linkLabel2);
			this.Controls.Add(this.linkLabel1);
			this.Controls.Add(btnHelp);
			this.Controls.Add(this.txtConsole);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.chkUseGpu);
			this.Controls.Add(this.chkTimestamp);
			this.Controls.Add(this.progressBar1);
			this.Controls.Add(this.txtStartTime);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.txtOverlayText);
			this.Controls.Add(this.lblOverlayCaption);
			this.Controls.Add(this.lblStatus);
			this.Controls.Add(this.btnClear);
			this.Controls.Add(this.btnMerge);
			this.Controls.Add(this.btnRemoveAudio);
			this.Controls.Add(this.btnAddFiles);
			this.Controls.Add(this.lstFiles);
			this.HelpButton = true;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "Form1";
			this.Text = "OnTeknikZemin";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		//private System.Windows.Forms.ListBox lstFiles;
		private mp4BatchTool.LogoListBox lstFiles;   // namespace'ini kendi projenle eşle

		private System.Windows.Forms.Button btnAddFiles;
        private System.Windows.Forms.Button btnRemoveAudio;
        private System.Windows.Forms.Button btnMerge;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblOverlayCaption;
        private System.Windows.Forms.TextBox txtOverlayText;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtStartTime;
        private System.Windows.Forms.ProgressBar progressBar1;
        protected System.Windows.Forms.CheckBox chkTimestamp;
        private System.Windows.Forms.CheckBox chkUseGpu;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.RichTextBox txtConsole;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.LinkLabel linkLabel2;
        private System.Windows.Forms.CheckBox chkScale720;
    }
}

