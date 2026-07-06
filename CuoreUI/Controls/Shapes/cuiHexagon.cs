using CuoreUI.Helpers;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CuoreUI.Controls.Shapes
{
    public partial class cuiHexagon : UserControl
    {
        public cuiHexagon()
        {
            InitializeComponent();
        }

        private Color privateOutlineColor = Color.Empty;

        [Category("CuoreUI")]
        public Color OutlineColor
        {
            get
            {
                return privateOutlineColor;
            }
            set
            {
                privateOutlineColor = value;
                Invalidate();
            }
        }

        private Color privatePanelColor = Helpers.DrawingHelper.PrimaryColor;

        [Category("CuoreUI")]
        public Color PanelColor
        {
            get
            {
                return privatePanelColor;
            }
            set
            {
                privatePanelColor = value;
                Invalidate();
            }
        }

        private int privateOutlineThickness = 1;

        [Category("CuoreUI")]
        public int OutlineThickness
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

        private int privateRounding = 5;

        [Category("CuoreUI")]
        public int Rounding
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

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            Rectangle modifiedCR = ClientRectangle;
            modifiedCR.Width -= 1;
            modifiedCR.Height -= 1;

            modifiedCR.Inflate(-OutlineThickness, -OutlineThickness);

            using (GraphicsPath hexagonPath = GeneralHelper.RoundHexagon(modifiedCR, Rounding))
            using (SolidBrush panelBrush = new SolidBrush(PanelColor))
            using (Pen outlinePen = new Pen(OutlineColor, OutlineThickness))
            {
                e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                e.Graphics.FillPath(panelBrush, hexagonPath);
                e.Graphics.PixelOffsetMode = PixelOffsetMode.Default;
                e.Graphics.DrawPath(outlinePen, hexagonPath);
            }

            base.OnPaint(e);
        }
    }
}
