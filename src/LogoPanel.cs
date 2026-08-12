using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ConfigCrate.EmptyFolderRadar
{
    internal sealed class LogoPanel : Control
    {
        public LogoPanel() { DoubleBuffered = true; Size = new Size(54, 54); }
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
            Color mint = Color.FromArgb(92, 225, 181); Color ink = Color.FromArgb(10, 43, 35);
            using (SolidBrush b = new SolidBrush(mint)) g.FillRectangle(b, ClientRectangle);
            using (Pen p = new Pen(ink, 3.2F) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round })
            {
                g.DrawLine(p, 9, 18, 24, 18); g.DrawLine(p, 24, 18, 28, 23); g.DrawLine(p, 28, 23, 44, 23);
                g.DrawLine(p, 44, 23, 40, 41); g.DrawLine(p, 40, 41, 10, 41); g.DrawLine(p, 10, 41, 9, 18);
                g.DrawArc(p, 19, 8, 24, 24, 205, 255); g.DrawLine(p, 37, 27, 45, 35);
            }
        }
    }
}
