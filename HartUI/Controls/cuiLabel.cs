using System;
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
                privateContent = value ?? string.Empty;
                AdjustSize();
                Invalidate();
            }
        }

        private StringAlignment privateHorizontalAlignment = StringAlignment.Center;
        private StringAlignment privateVerticalAlignment = StringAlignment.Near;

        [Category("HartUI")]
        [DefaultValue(StringAlignment.Center)]
        public StringAlignment HorizontalAlignment
        {
            get => privateHorizontalAlignment;
            set
            {
                privateHorizontalAlignment = value;
                Invalidate();
            }
        }

        [Category("HartUI")]
        [DefaultValue(StringAlignment.Near)]
        public StringAlignment VerticalAlignment
        {
            get => privateVerticalAlignment;
            set
            {
                privateVerticalAlignment = value;
                Invalidate();
            }
        }

        private bool wordWrap;

        [Category("HartUI")]
        [DefaultValue(false)]
        public bool WordWrap
        {
            get => wordWrap;
            set
            {
                if (wordWrap == value) return;
                wordWrap = value;
                AdjustSize();
                Invalidate();
            }
        }

        private bool autoEllipsis;

        [Category("HartUI")]
        [DefaultValue(false)]
        public bool AutoEllipsis
        {
            get => autoEllipsis;
            set
            {
                if (autoEllipsis == value) return;
                autoEllipsis = value;
                Invalidate();
            }
        }

        private StringTrimming trimming = StringTrimming.None;

        [Category("HartUI")]
        [DefaultValue(StringTrimming.None)]
        public StringTrimming Trimming
        {
            get => trimming;
            set
            {
                if (trimming == value) return;
                trimming = value;
                Invalidate();
            }
        }

        [Category("HartUI")]
        [DefaultValue(false)]
        public override bool AutoSize
        {
            get => base.AutoSize;
            set
            {
                if (base.AutoSize == value) return;
                base.AutoSize = value;
                AdjustSize();
            }
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            if (string.IsNullOrEmpty(privateContent))
            {
                return new Size(Padding.Horizontal, Padding.Vertical);
            }

            using (StringFormat stringFormat = CreateStringFormat())
            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
            {
                SizeF layoutArea = (wordWrap && proposedSize.Width > 0 && proposedSize.Width < int.MaxValue)
                    ? new SizeF(Math.Max(1, proposedSize.Width - Padding.Horizontal), int.MaxValue)
                    : new SizeF(int.MaxValue, int.MaxValue);

                SizeF measured = g.MeasureString(privateContent, Font, layoutArea, stringFormat);

                return new Size(
                    (int)Math.Ceiling(measured.Width) + Padding.Horizontal,
                    (int)Math.Ceiling(measured.Height) + Padding.Vertical);
            }
        }

        private void AdjustSize()
        {
            if (AutoSize)
            {
                Size = GetPreferredSize(Size);
            }
        }

        private StringFormat CreateStringFormat()
        {
            StringFormat format = new StringFormat
            {
                Alignment = privateHorizontalAlignment,
                LineAlignment = privateVerticalAlignment,
                // AutoSize means no clipping so use EllipsisCharacter
                Trimming = (autoEllipsis && !AutoSize) ? StringTrimming.EllipsisCharacter : trimming
            };

            if (!wordWrap)
            {
                format.FormatFlags |= StringFormatFlags.NoWrap;
            }

            return format;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            Rectangle cr = ClientRectangle;
            cr.X += Padding.Left;
            cr.Y += Padding.Top;
            cr.Width -= Padding.Horizontal;
            cr.Height -= Padding.Vertical;

            if (cr.Height % 2 == 1)
            {
                cr.Height -= 1;
            }

            using (StringFormat stringFormat = CreateStringFormat())
            using (SolidBrush brush = new SolidBrush(ForeColor))
            {
                e.Graphics.DrawString(privateContent, Font, brush, cr, stringFormat);
            }

            base.OnPaint(e);
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            AdjustSize();
            Invalidate();
        }

        protected override void OnForeColorChanged(EventArgs e)
        {
            base.OnForeColorChanged(e);
            Invalidate();
        }

        protected override void OnPaddingChanged(EventArgs e)
        {
            base.OnPaddingChanged(e);
            AdjustSize();
            Invalidate();
        }
    }
}
