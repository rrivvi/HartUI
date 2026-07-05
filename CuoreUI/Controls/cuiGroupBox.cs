using CuoreUI.Helpers;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CuoreUI.Controls
{
    [ToolboxBitmap(typeof(GroupBox))]
    public partial class cuiGroupBox : Panel
    {
        public cuiGroupBox()
        {
            InitializeComponent();
            SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint, true);
            Content = "Group Box";
        }

        private Padding privateRounding = new Padding(4);

        [Category("CuoreUI")]
        [Description("How round the inside border is.")]
        public Padding Rounding
        {
            get
            {
                return privateRounding;
            }
            set
            {
                privateRounding = value;
                Invalidate();
            }
        }

        private Color privateBorderColor = Color.FromArgb(64, 128, 128, 128);

        [Category("CuoreUI")]
        [Description("The color of the inner border.")]
        public Color BorderColor
        {
            get
            {
                return privateBorderColor;
            }
            set
            {
                privateBorderColor = value;
                Invalidate();
            }
        }

        private string privateContent = "cuiGroupBox";

        [Category("CuoreUI")]
        [Description("The text that appears on top left.")]
        public string Content
        {
            get
            {
                return privateContent;
            }
            set
            {
                privateContent = value;
                Invalidate();
            }
        }

        public new string Text
        {
            get
            {
                return Content;
            }
            set
            {
                Content = value;
            }
        }

        public override void ResetText()
        {
            Content = string.Empty;
            base.ResetText();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            //                   left,           top,                            right,          bottom
            Padding = new Padding(Rounding.Left, Rounding.Top + Font.Height - 2, Rounding.Right, Rounding.Bottom);
            Rectangle modifiedCR = new Rectangle(0, Font.Height / 2, Width - 1, Height - Font.Height / 2 - 1);

            using (Pen borderPen = new Pen(privateBorderColor))
            using (SolidBrush textBrush = new SolidBrush(ForeColor))
            using (GraphicsPath roundedPath = GeneralHelper.RoundRect(modifiedCR, Rounding))
            using (SolidBrush backgroundBrush = new SolidBrush(BackColor))
            {
                g.DrawPath(borderPen, roundedPath);

                Size textSize = TextRenderer.MeasureText(Content, Font);
                Rectangle textRect = new Rectangle(8, 0, textSize.Width + Font.Height, textSize.Height);
                g.FillRectangle(backgroundBrush, textRect);
                g.DrawString(Content, Font, textBrush, textRect, new StringFormat() { Alignment = StringAlignment.Center });
            }

            base.OnPaint(e);
        }
    }
}
