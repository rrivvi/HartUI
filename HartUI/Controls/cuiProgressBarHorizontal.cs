using HartUI.Helpers;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace HartUI.Controls
{
    [ToolboxBitmap(typeof(ProgressBar))]
    public partial class cuiProgressBarHorizontal : UserControl
    {
        public cuiProgressBarHorizontal()
        {
            InitializeComponent();
            DoubleBuffered = true;
            AutoScaleMode = AutoScaleMode.None;
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }

        private int privateValue = 50;

        [Category("HartUI")]
        public int Value
        {
            get
            {
                return privateValue;
            }
            set
            {
                privateValue = value;
                Invalidate();
            }
        }

        private int privateMaxValue = 100;

        [Category("HartUI")]
        public int MaxValue
        {
            get
            {
                return privateMaxValue;
            }
            set
            {
                privateMaxValue = value;
                Invalidate();
            }
        }

        private bool privateFlipped = false;

        [Category("HartUI")]
        public bool Flipped
        {
            get
            {
                return privateFlipped;
            }
            set
            {
                privateFlipped = value;
                Invalidate();
            }
        }

        private Color privateBackground = Color.FromArgb(64, 128, 128, 128);

        [Category("HartUI")]
        public Color Background
        {
            get
            {
                return privateBackground;
            }
            set
            {
                privateBackground = value;
                Invalidate();
            }
        }

        private Color privateForeground = Helpers.DrawingHelper.PrimaryColor;

        [Category("HartUI")]
        public Color Foreground
        {
            get
            {
                return privateForeground;
            }
            set
            {
                privateForeground = value;
                Invalidate();
            }
        }

        private int privateRounding = 8;

        [Category("HartUI")]
        public int Rounding
        {
            get
            {
                return privateRounding;
            }
            set
            {
                if (value > (ClientRectangle.Height / 2))
                {
                    privateRounding = ClientRectangle.Height / 2;
                    Rounding = privateRounding;
                }
                else
                {
                    privateRounding = value;
                }
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBilinear;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            e.Graphics.TextRenderingHint = TextRenderingHint.AntiAlias;

            using (GraphicsPath roundBackground = GeneralHelper.RoundRect(ClientRectangle, Rounding))
            {
                float filledPercent = (float)Value / MaxValue;

                if (Flipped)
                {
                    filledPercent = 1f - filledPercent;
                }

                float foreWidth = Flipped ? ClientRectangle.Width - (ClientRectangle.Width * filledPercent) : ClientRectangle.Width * filledPercent;
                RectangleF foreHalf = new RectangleF(
                    Flipped ? ClientRectangle.Width - foreWidth : 0,
                    0,
                    foreWidth,
                    ClientRectangle.Height
                );

                using (SolidBrush brush = new SolidBrush(Background))
                {
                    e.Graphics.FillPath(brush, roundBackground);
                }

                if (foreWidth > 0)
                {
                    using (GraphicsPath graphicsPath = GeneralHelper.RoundRect(
                        new Rectangle((int)foreHalf.X, (int)foreHalf.Y, (int)Math.Ceiling(foreHalf.Width), (int)Math.Ceiling(foreHalf.Height)),
                        Rounding))
                    using (SolidBrush brush = new SolidBrush(Foreground))
                    {
                        e.Graphics.FillPath(brush, graphicsPath);
                    }
                }
            }

            base.OnPaint(e);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }
    }
}
