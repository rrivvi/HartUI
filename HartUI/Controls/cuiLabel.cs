using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HartUI.Controls
{
    [Description("Label with more alignment settings and RegEx parsing")]
    [ToolboxBitmap(typeof(Label))]
    public partial class cuiLabel : UserControl
    {
        public cuiLabel()
        {
            InitializeComponent();
            DoubleBuffered = true;
            AutoScaleMode = AutoScaleMode.None;
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
        }

        private string privateContent = "Your text here!";

        [Category("HartUI")]
        public string Content
        {
            get => privateContent;
            set
            {
                // allow newlines in text content (HartUI originally used regex unescape for this)
                privateContent = (value ?? string.Empty)
                    .Replace("\\r\\n", "\r\n")
                    .Replace("\\n", "\n")
                    .Replace("\\r", "\r")
                    .Replace("\\t", "\t");

                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            Rectangle cr = ClientRectangle;

            using (StringFormat stringFormat = new StringFormat() { Alignment = HorizontalAlignment, LineAlignment = VerticalAlignment })
            using (SolidBrush brush = new SolidBrush(ForeColor))
            {
                if (Height % 2 == 1)
                {
                    cr.Height -= 1;
                }

                e.Graphics.DrawString(privateContent, Font, brush, cr, stringFormat);
            }

            base.OnPaint(e);
        }

        private StringAlignment privateHorizontalAlignment = StringAlignment.Center;
        private StringAlignment privateVerticalAlignment = StringAlignment.Near;

        [Category("HartUI")]
        public StringAlignment HorizontalAlignment
        {
            get
            {
                return privateHorizontalAlignment;
            }
            set
            {
                privateHorizontalAlignment = value;
                Invalidate();
            }
        }

        [Category("HartUI")]
        public StringAlignment VerticalAlignment
        {
            get
            {
                return privateVerticalAlignment;
            }
            set
            {
                privateVerticalAlignment = value;
                Invalidate();
            }
        }
    }
}
