using System.Windows.Forms;
using System.Drawing;

namespace Sonar
{
    public class VerticalProgressBar : ProgressBar
    {
        public VerticalProgressBar()
        {
                this.SetStyle(ControlStyles.UserPaint, true);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.Style |= 0x04;
                return cp;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Rectangle bar = e.ClipRectangle;
            // assues 2px of padding on each side.
            bar.Height = (int)(bar.Height * ((double)Value / Maximum)) - 4;
            if (ProgressBarRenderer.IsSupported)
                ProgressBarRenderer.DrawVerticalBar(e.Graphics, e.ClipRectangle);
            bar.Width = bar.Width - 4;

            float y = e.ClipRectangle.Height - bar.Height - 2;
            Brush b = new SolidBrush(this.ForeColor);
            
            e.Graphics.FillRectangle(b, 2, y, bar.Width, bar.Height);
            
            b.Dispose();
        }

    }
}
