using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HartUI.Controls
{
    [Description("cuiPanel with a gradient instead")]
    public partial class cuiPanelGradient : Panel
    {
        public cuiPanelGradient()
        {
            InitializeComponent();
            DoubleBuffered = true;
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            Invalidate();
        }

        Color privatePanelColor1 = Helpers.DrawingHelper.PrimaryColor;

        [Category("HartUI")]
        public Color PanelColor1
        {
            get
            {
                return privatePanelColor1;
            }
            set
            {
                privatePanelColor1 = value;
                Invalidate();
            }
        }

        Color privatePanelColor2 = Color.Transparent;

        [Category("HartUI")]
        public Color PanelColor2
        {
            get
            {
                return privatePanelColor2;
            }
            set
            {
                privatePanelColor2 = value;
                Invalidate();
            }
        }

        Color privatePanelOutlineColor1 = Helpers.DrawingHelper.PrimaryColor;

        [Category("HartUI")]
        public Color PanelOutlineColor1
        {
            get
            {
                return privatePanelOutlineColor1;
            }
            set
            {
                privatePanelOutlineColor1 = value;
                Invalidate();
            }
        }

        Color privatePanelOutlineColor2 = Helpers.DrawingHelper.PrimaryColor;

        [Category("HartUI")]
        public Color PanelOutlineColor2
        {
            get
            {
                return privatePanelOutlineColor2;
            }
            set
            {
                privatePanelOutlineColor2 = value;
                Invalidate();
            }
        }

        float privateOutlineThickness = 1;

        [Category("HartUI")]
        public float OutlineThickness
        {
            get
            {
                return privateOutlineThickness;
            }
            set
            {
                privateOutlineThickness = value;
                Invalidate();
            }
        }

        Padding privateRounding = new Padding(8, 8, 8, 8);

        [Category("HartUI")]
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

        float privateGradientAngle = 0;

        [Category("HartUI")]
        public float GradientAngle
        {
            get
            {
                return privateGradientAngle;
            }
            set
            {
                privateGradientAngle = value % 360;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            Rectangle modifiedCR = ClientRectangle;
            modifiedCR.Inflate(-1, -1);

            using (GraphicsPath roundBackground = Helpers.GeneralHelper.RoundRect(modifiedCR, Rounding))
            {
                using (Pen br = new Pen(BackColor))
                using (LinearGradientBrush brush = new LinearGradientBrush(
                    modifiedCR, privatePanelColor1, privatePanelColor2, privateGradientAngle, true))
                {
                    e.Graphics.FillPath(brush, roundBackground);
                    e.Graphics.PixelOffsetMode = PixelOffsetMode.Default;
                    //e.Graphics.DrawPath(br, roundBackground); // offset fix
                }

                using (LinearGradientBrush borderBrush = new LinearGradientBrush(
                    modifiedCR, privatePanelOutlineColor1, privatePanelOutlineColor2, privateGradientAngle, true))
                {
                    using (Pen pen = new Pen(borderBrush, privateOutlineThickness))
                    {
                        e.Graphics.DrawPath(pen, roundBackground);
                    }
                }
            }

            base.OnPaint(e);
        }
    }
}