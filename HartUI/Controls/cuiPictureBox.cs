using HartUI.Helpers;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace HartUI.Controls
{
    [ToolboxBitmap(typeof(PictureBox))]
    public partial class cuiPictureBox : UserControl
    {
        public cuiPictureBox()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        private Image privateContent = null;

        private Bitmap cachedImage = null;
        private TextureBrush cachedImageBrush = null;

        private Matrix transformMatrix = new Matrix();

        [Category("HartUI")]
        public Image Content
        {
            get => privateContent;
            set
            {
                if (privateContent == value)
                    return;

                privateContent = value;
                RebuildCache();
            }
        }

        private Padding privateCornerRadius = new Padding(8);

        [Category("HartUI")]
        public Padding Rounding
        {
            get => privateCornerRadius;
            set
            {
                privateCornerRadius = value;
                Invalidate();
            }
        }

        private Color privateImageTint = Color.White;

        [Category("HartUI")]
        public Color ImageTint
        {
            get => privateImageTint;
            set
            {
                if (privateImageTint == value)
                    return;

                privateImageTint = value;
                RebuildCache();
            }
        }

        private int privateRotation = 0;

        [Category("HartUI")]
        public int Rotation
        {
            get => privateRotation;
            set
            {
                value %= 360;
                if (privateRotation == value)
                    return;

                privateRotation = value;
                UpdateTransform();
                Invalidate();
            }
        }

        private Color privatePanelOutlineColor = Color.Empty;

        [Category("HartUI")]
        public Color PanelOutlineColor
        {
            get => privatePanelOutlineColor;
            set
            {
                privatePanelOutlineColor = value;
                Invalidate();
            }
        }

        private float privateOutlineThickness = 1;

        [Category("HartUI")]
        public float OutlineThickness
        {
            get => privateOutlineThickness;
            set
            {
                privateOutlineThickness = value;
                Invalidate();
            }
        }

        private void RebuildCache()
        {
            DisposeCache();

            if (privateContent == null)
            {
                Invalidate();
                return;
            }

            // Create tinted bitmap
            cachedImage = new Bitmap(privateContent.Width, privateContent.Height);

            using (Graphics g = Graphics.FromImage(cachedImage))
            using (ImageAttributes attr = new ImageAttributes())
            {
                float r = ImageTint.R / 255f;
                float gC = ImageTint.G / 255f;
                float b = ImageTint.B / 255f;
                float a = ImageTint.A / 255f;

                var matrix = new ColorMatrix(new float[][]
                {
                    new float[] {r, 0, 0, 0, 0},
                    new float[] {0, gC, 0, 0, 0},
                    new float[] {0, 0, b, 0, 0},
                    new float[] {0, 0, 0, a, 0},
                    new float[] {0, 0, 0, 0, 1}
                });

                attr.SetColorMatrix(matrix);

                g.DrawImage(privateContent,
                    new Rectangle(0, 0, cachedImage.Width, cachedImage.Height),
                    0, 0, privateContent.Width, privateContent.Height,
                    GraphicsUnit.Pixel, attr);
            }

            cachedImageBrush = new TextureBrush(cachedImage, WrapMode.Clamp);

            UpdateTransform();
            Invalidate();
        }

        private void UpdateTransform()
        {
            if (cachedImageBrush == null || cachedImage == null)
                return;

            transformMatrix.Reset();

            float scaleX = (float)Width / cachedImage.Width;
            float scaleY = (float)Height / cachedImage.Height;

            transformMatrix.Scale(scaleX, scaleY);
            transformMatrix.RotateAt(privateRotation, new PointF(
                cachedImage.Width / 2f,
                cachedImage.Height / 2f));

            cachedImageBrush.Transform = transformMatrix;
        }

        private void DisposeCache()
        {
            cachedImageBrush?.Dispose();
            cachedImageBrush = null;

            cachedImage?.Dispose();
            cachedImage = null;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateTransform();
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (cachedImageBrush == null)
                return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            Rectangle rect = ClientRectangle;
            rect.Inflate(-1, -1);

            using (GraphicsPath path = GeneralHelper.RoundRect(rect, Rounding))
            {
                e.Graphics.FillPath(cachedImageBrush, path);

                if (OutlineThickness > 0 && PanelOutlineColor != Color.Empty)
                {
                    using (Pen pen = new Pen(PanelOutlineColor, OutlineThickness))
                    {
                        e.Graphics.PixelOffsetMode = PixelOffsetMode.Default;
                        e.Graphics.DrawPath(pen, path);
                    }
                }
            }

            base.OnPaint(e);
        }
    }
}