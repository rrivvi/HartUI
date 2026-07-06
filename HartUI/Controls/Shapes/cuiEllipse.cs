using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HartUI.Controls.Shapes
{
    public partial class cuiEllipse : UserControl
    {
        private Color privateOutlineColor = Color.Empty;

        [Category("HartUI")]
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

        [Category("HartUI")]
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

        [Category("HartUI")]
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

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }

        public cuiEllipse()
        {
            InitializeComponent();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            Rectangle modifiedCR = ClientRectangle;
            modifiedCR.Width -= 1;
            modifiedCR.Height -= 1;

            modifiedCR.Inflate(-OutlineThickness, -OutlineThickness);

            using (SolidBrush panelBrush = new SolidBrush(PanelColor))
            using (Pen outlinePen = new Pen(OutlineColor, OutlineThickness))
            {
                e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                e.Graphics.FillEllipse(panelBrush, modifiedCR);
                e.Graphics.PixelOffsetMode = PixelOffsetMode.Default;
                e.Graphics.DrawEllipse(outlinePen, modifiedCR);
            }
        }
    }
}
