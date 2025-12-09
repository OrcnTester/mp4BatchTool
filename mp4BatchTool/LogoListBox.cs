using System;
using System.Drawing;
using System.Windows.Forms;

namespace mp4BatchTool   // 👈 senin namespace'in bu, doğru
{
	public class LogoListBox : ListBox
	{
		public Image BackgroundLogo { get; set; }

		public LogoListBox()
		{
			// Kendimiz çizeceğiz
			this.DrawMode = DrawMode.OwnerDrawFixed;
			this.DoubleBuffered = true;
			this.ResizeRedraw = true;
		}

		protected override void OnPaintBackground(PaintEventArgs pevent)
		{
			// Liste arka planını biz boyayalım
			pevent.Graphics.Clear(this.BackColor);

			// Logo yoksa çık
			if (BackgroundLogo == null)
				return;

			DrawLogo(pevent.Graphics);
		}

		protected override void OnDrawItem(DrawItemEventArgs e)
		{
			// Item yoksa çık
			if (e.Index < 0 || e.Index >= Items.Count)
				return;

			bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

			// Arka plan (seçili / seçili değil)
			Color backColor = selected ? SystemColors.Highlight : this.BackColor;
			Color foreColor = selected ? SystemColors.HighlightText : this.ForeColor;

			using (var backBrush = new SolidBrush(backColor))
			{
				e.Graphics.FillRectangle(backBrush, e.Bounds);
			}

			// Yazıyı çiz
			string text = Items[e.Index]?.ToString() ?? string.Empty;

			TextRenderer.DrawText(
				e.Graphics,
				text,
				this.Font,
				e.Bounds,
				foreColor,
				TextFormatFlags.Left | TextFormatFlags.VerticalCenter
			);

			// Focus rectangle
			e.DrawFocusRectangle();
		}

		private void DrawLogo(Graphics g)
		{
			var logo = BackgroundLogo;
			if (logo == null) return;

			int logoW = logo.Width;
			int logoH = logo.Height;

			// Logo büyükse biraz küçült
			float scale = 1f;
			if (logoW > Width || logoH > Height)
			{
				float sx = (float)Width / logoW;
				float sy = (float)Height / logoH;
				scale = Math.Min(sx, sy) * 0.5f; // %50 civarı, göz yormasın
				logoW = (int)(logoW * scale);
				logoH = (int)(logoH * scale);
			}

			int x = (Width - logoW) / 2;
			int y = (Height - logoH) / 2;

			g.DrawImage(logo, x, y, logoW, logoH);
		}
	}
}
