using HartUI.Helpers;
using HartUI.Misc.Internal;
using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace HartUI.Controls
{
    [Designer(typeof(DesignerIntegration.HartControlDesigner))]
    [ToolboxBitmap(typeof(PictureBox))]
    public partial class cuiPictureBox : UserControl
    {
        public cuiPictureBox()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            TabStop = false;
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

        private PictureBoxSizeMode privateSizeMode = PictureBoxSizeMode.StretchImage;

        [Category("HartUI")]
        public PictureBoxSizeMode SizeMode
        {
            get => privateSizeMode;
            set
            {
                if (privateSizeMode == value)
                {
                    return;
                }

                privateSizeMode = value;

                if (privateSizeMode == PictureBoxSizeMode.AutoSize && privateContent != null)
                {
                    ApplyAutoSize();
                }

                UpdateTransform();
                Invalidate();
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
            using (ImageAttributes attr = DrawingHelper.Imaging.CreateTintImageAttributes(ImageTint))
            {
                g.DrawImage(privateContent,
                    new Rectangle(0, 0, cachedImage.Width, cachedImage.Height),
                    0, 0, privateContent.Width, privateContent.Height,
                    GraphicsUnit.Pixel, attr);
            }

            cachedImageBrush = new TextureBrush(cachedImage, WrapMode.Clamp);

            if (privateSizeMode == PictureBoxSizeMode.AutoSize)
            {
                ApplyAutoSize();
            }

            UpdateTransform();
            Invalidate();
        }

        private void ApplyAutoSize()
        {
            Size newSize = privateContent.Size;
            if (Size == newSize)
            {
                return;
            }

            IDesignerHost host = GetService(typeof(IDesignerHost)) as IDesignerHost;
            if (host == null)
            {
                Size = newSize;
                return;
            }

            IComponentChangeService changeService = GetService(typeof(IComponentChangeService)) as IComponentChangeService;
            PropertyDescriptor sizeProperty = TypeDescriptor.GetProperties(this)["Size"];

            using (DesignerTransaction transaction = host.CreateTransaction($"Set {Name}.Size"))
            {
                changeService?.OnComponentChanging(this, sizeProperty);
                Size = newSize;
                changeService?.OnComponentChanged(this, sizeProperty, null, newSize);
                transaction.Commit();
            }
        }

        private void UpdateTransform()
        {
            if (cachedImageBrush == null || cachedImage == null)
                return;

            transformMatrix.Reset();

            float scaleX, scaleY;
            float offsetX = 0f, offsetY = 0f;

            switch (privateSizeMode)
            {
                case PictureBoxSizeMode.Normal:
                case PictureBoxSizeMode.AutoSize:
                    scaleX = 1f;
                    scaleY = 1f;
                    break;

                case PictureBoxSizeMode.CenterImage:
                    scaleX = 1f;
                    scaleY = 1f;
                    offsetX = (Width - cachedImage.Width) / 2f;
                    offsetY = (Height - cachedImage.Height) / 2f;
                    break;

                case PictureBoxSizeMode.Zoom:
                    float ratio = Math.Min((float)Width / cachedImage.Width, (float)Height / cachedImage.Height);
                    scaleX = ratio;
                    scaleY = ratio;
                    offsetX = (Width - cachedImage.Width * ratio) / 2f;
                    offsetY = (Height - cachedImage.Height * ratio) / 2f;
                    break;

                default: // StretchImage
                    scaleX = (float)Width / cachedImage.Width;
                    scaleY = (float)Height / cachedImage.Height;
                    break;
            }

            transformMatrix.Scale(scaleX, scaleY);
            transformMatrix.RotateAt(privateRotation, new PointF(
                cachedImage.Width / 2f,
                cachedImage.Height / 2f));
            transformMatrix.Translate(offsetX, offsetY, MatrixOrder.Append);

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