using HartUI.Helpers;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using static HartUI.Controls.cuiButton;

namespace HartUI.Controls
{
    [Description("Unchecks other group buttons in the same Parent when pressed")]
    [ToolboxBitmap(typeof(Button))]
    [DefaultEvent("Click")]
    public partial class cuiButtonGroup : UserControl
    {
        public cuiButtonGroup()
        {
            InitializeComponent();
            DoubleBuffered = true;
            ForeColor = Color.Black;
            Font = new Font("Microsoft Sans Serif", 9.75f);
            SetStyle(ControlStyles.ResizeRedraw, true);
            Padding = new Padding(12);
        }

        public event EventHandler CheckedChanged;

        private string privateContent = "Your text here!";

        [Category("HartUI")]
        public string Content
        {
            get
            {
                return privateContent;
            }
            set
            {
                privateContent = value ?? string.Empty;
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

        private Padding privateRounding = new Padding(8, 8, 8, 8);

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

        private Color privateNormalBackground = Color.White;

        [Category("HartUI")]
        public Color NormalBackground
        {
            get
            {
                return privateNormalBackground;
            }
            set
            {
                privateNormalBackground = value;
                Invalidate();
            }
        }

        private Color privateHoverBackground = Color.White;

        [Category("HartUI")]
        public Color HoverBackground
        {
            get
            {
                return privateHoverBackground;
            }
            set
            {
                privateHoverBackground = value;
                Invalidate();
            }
        }

        private Color privatePressedBackground = Color.WhiteSmoke;

        [Category("HartUI")]
        public Color PressedBackground
        {
            get
            {
                return privatePressedBackground;
            }
            set
            {
                privatePressedBackground = value;
                Invalidate();
            }
        }

        private Color privateNormalOutline = Color.FromArgb(64, 128, 128, 128);

        [Category("HartUI")]
        public Color NormalOutline
        {
            get
            {
                return privateNormalOutline;
            }
            set
            {
                privateNormalOutline = value;
                Invalidate();
            }
        }

        private Color privateHoverOutline = Color.FromArgb(32, 128, 128, 128);

        [Category("HartUI")]
        public Color HoverOutline
        {
            get
            {
                return privateHoverOutline;
            }
            set
            {
                privateHoverOutline = value;
                Invalidate();
            }
        }

        private Color privatePressedOutline = Color.FromArgb(64, 128, 128, 128);

        [Category("HartUI")]
        public Color PressedOutline
        {
            get
            {
                return privatePressedOutline;
            }
            set
            {
                privatePressedOutline = value;
                Invalidate();
            }
        }

        private bool privateChecked = false;

        [Category("HartUI")]
        public bool Checked
        {
            get
            {
                return privateChecked;
            }
            set
            {
                if (privateChecked != value)
                {
                    if (value)
                    {
                        UpdateGroup();
                    }

                    privateChecked = value;
                    CheckedChanged?.Invoke(this, EventArgs.Empty);
                }

                Invalidate();
            }
        }

        private int state = ButtonStates.Normal;
        private SolidBrush privateBrush = new SolidBrush(Color.Black);
        private Pen privatePen = new Pen(Color.Black);
        StringFormat stringFormat = new StringFormat()
        {
            Alignment = StringAlignment.Near,
            Trimming = StringTrimming.None,
            FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.FitBlackBox
        };

        private Image privateImage = null;

        [Category("HartUI")]
        public Image Image
        {
            get
            {
                return privateImage;
            }
            set
            {
                privateImage = value;
                Invalidate();
            }
        }

        private Color privateCheckedBackground = Helpers.DrawingHelper.PrimaryColor;

        [Category("HartUI")]
        public Color CheckedBackground
        {
            get
            {
                return privateCheckedBackground;
            }
            set
            {
                privateCheckedBackground = value;
                Invalidate();
            }
        }

        private Color privateCheckedOutline = Helpers.DrawingHelper.PrimaryColor;

        [Category("HartUI")]
        public Color CheckedOutline
        {
            get
            {
                return privateCheckedOutline;
            }
            set
            {
                privateCheckedOutline = value;
                Invalidate();
            }
        }

        private StringAlignment privateTextAlignment = StringAlignment.Center;

        [Category("HartUI")]
        public StringAlignment TextAlignment
        {
            get
            {
                return privateTextAlignment;
            }
            set
            {
                privateTextAlignment = value;
                Invalidate();
            }
        }

        private float privateOutlineThickness = 1f;

        [Category("HartUI")]
        public float OutlineThickness
        {
            get
            {
                return privateOutlineThickness;
            }
            set
            {
                privateOutlineThickness = Math.Max(value, 0);
                privatePen.Width = privateOutlineThickness;
                Invalidate();
            }
        }

        private Point privateImageExpand = Point.Empty;

        [Category("HartUI")]
        [Description("The default size for Image is Font.Height. Adjust the size manually here if you need to.")]
        public Point ImageExpand
        {
            get
            {
                return privateImageExpand;
            }
            set
            {
                privateImageExpand = value;
                Invalidate();
            }
        }

        Color privateCheckedForeColor = Color.White;

        [Category("HartUI")]
        public Color CheckedForeColor
        {
            get
            {
                return privateCheckedForeColor;
            }
            set
            {
                privateCheckedForeColor = value;
                Invalidate();
            }
        }

        Color privatePressedForeColor = Color.FromArgb(32, 32, 32);

        [Category("HartUI")]
        public Color PressedForeColor
        {
            get
            {
                return privatePressedForeColor;
            }
            set
            {
                privatePressedForeColor = value;
                Invalidate();
            }
        }

        [Category("HartUI")]
        public Color NormalForeColor
        {
            get
            {
                return ForeColor;
            }
            set
            {
                ForeColor = value;
            }
        }

        Color privateHoverForeColor = Color.DimGray;

        [Category("HartUI")]
        public Color HoverForeColor
        {
            get
            {
                return privateHoverForeColor;
            }
            set
            {
                privateHoverForeColor = value;
                Invalidate();
            }
        }

        private int textSpacing = 2;
        [Category("HartUI")]
        [Description("Space between the image and the text (if Image isn't null)")]
        public int TextSpacing
        {
            get
            {
                return textSpacing;
            }
            set
            {
                textSpacing = value;
                Invalidate();
            }
        }

        [Category("HartUI")]
        [Description("Text padding for when TextAlignment is set to \"Near\" or \"Far\". For finer control, edit the `Padding` property instead.")]
        public int TextPadding
        {
            get
            {
                return Padding.All;
            }
            set
            {
                Padding = new Padding(value);
                Invalidate();
            }
        }

        private SizeF cachedTextSize = SizeF.Empty;
        private string lastMeasuredText = null;
        private Font lastMeasuredFont = null;

        private void UpdateTextCache(Graphics g)
        {
            if (privateContent != lastMeasuredText || Font != lastMeasuredFont)
            {
                cachedTextSize = g.MeasureString(privateContent, Font);
                lastMeasuredText = privateContent;
                lastMeasuredFont = Font;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            Rectangle modifiedCR = ClientRectangle;
            modifiedCR.Width -= 1;
            modifiedCR.Height -= 1;

            if (Rounding.Left == 0 & Rounding.Bottom == 0)
            {
                modifiedCR.Inflate(-1, 0);
            }

            Color renderedBackgroundColor = Color.Empty;
            Color renderedOutlineColor = Color.Empty;
            Color renderedTint = Color.Empty;
            Color renderedForeColor = Color.Empty;

            if (Checked)
            {
                renderedBackgroundColor = CheckedBackground;
                renderedOutlineColor = CheckedOutline;
                renderedTint = CheckedImageTint;
                renderedForeColor = CheckedForeColor;
            }
            else
            {
                switch (state)
                {
                    case ButtonStates.Normal:
                        renderedBackgroundColor = NormalBackground;
                        renderedOutlineColor = NormalOutline;
                        renderedForeColor = NormalForeColor;
                        renderedTint = NormalImageTint;
                        break;

                    case ButtonStates.Hovered:
                        renderedBackgroundColor = HoverBackground;
                        renderedOutlineColor = HoverOutline;
                        renderedTint = HoverImageTint;
                        renderedForeColor = HoverForeColor;
                        break;

                    case ButtonStates.Pressed:
                        renderedBackgroundColor = PressedBackground;
                        renderedOutlineColor = PressedOutline;
                        renderedTint = PressedImageTint;
                        renderedForeColor = PressedForeColor;
                        break;
                }
            }

            privateBrush.Color = renderedBackgroundColor;
            privatePen.Color = renderedOutlineColor;
            privatePen.Width = privateOutlineThickness;

            GraphicsPath roundBackground;
            if (OutlineThickness > 0)
            {
                if (renderedOutlineColor == Color.Empty || renderedOutlineColor == Color.Transparent)
                {
                    // draw with PixelOffsetMode.HighQuality.
                    // the HighQuality actually draws accurately,
                    // so we need to resize the background rectangle, 
                    // so that it takes up the correct space, minus the outline
                    modifiedCR.Width -= 1;
                    modifiedCR.Height -= 1;
                    modifiedCR.X += 1;
                    modifiedCR.Y += 1;

                    roundBackground = GeneralHelper.RoundRect(modifiedCR, Rounding);
                    e.Graphics.FillPath(privateBrush, roundBackground);
                    e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Default;
                }
                else
                {
                    roundBackground = GeneralHelper.RoundRect(modifiedCR, Rounding);
                    e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Default;
                    e.Graphics.FillPath(privateBrush, roundBackground);
                    e.Graphics.DrawPath(privatePen, roundBackground);
                }
            }
            else
            {
                roundBackground = GeneralHelper.RoundRect(modifiedCR, Rounding);
                e.Graphics.FillPath(privateBrush, roundBackground);
            }

            // because roundBackground is not inside an using statement, it needs to be diposed here
            roundBackground.Dispose();

            Rectangle textRectangle = ClientRectangle;
            int textY = (Height / 2) - (Font.Height / 2);
            textRectangle.Y = textY;

            Rectangle imageRectangle = textRectangle;
            imageRectangle.Height = Font.Height;
            imageRectangle.Width = imageRectangle.Height;
            imageRectangle.Inflate(ImageExpand.X, ImageExpand.Y);

            UpdateTextCache(e.Graphics);
            int textWidth = (int)Math.Ceiling(cachedTextSize.Width);

            int imageWidth = privateImage != null ? imageRectangle.Width : 0;
            int combinedWidth = imageWidth + (privateImage != null ? textSpacing : 0) + textWidth;

            int startX;

            switch (TextAlignment)
            {
                case StringAlignment.Near:
                    startX = Padding.Left;
                    break;

                case StringAlignment.Far:
                    startX = ClientRectangle.Width - Padding.Right - combinedWidth;
                    break;

                default: // StringAlignment.Center
                    startX = (ClientRectangle.Width - combinedWidth) / 2;
                    break;
            }

            if (privateImage != null)
            {
                imageRectangle.X = startX;
                textRectangle.X = startX + imageWidth + textSpacing;
            }
            else
            {
                textRectangle.X = startX;
            }

            using (SolidBrush brush = new SolidBrush(renderedForeColor))
            {
                e.Graphics.DrawString(privateContent, Font, brush, textRectangle, stringFormat);
            }

            if (privateImage != null)
            {
                if (renderedTint == Color.White)
                {
                    e.Graphics.DrawImage(
                    privateImage,
                    imageRectangle,
                    0, 0, privateImage.Width, privateImage.Height,
                    GraphicsUnit.Pixel);
                }
                else
                {
                    if (colorMatrix == null || renderedTint != lastImageTint)
                    {
                        float tintR = renderedTint.R / 255f;
                        float tintG = renderedTint.G / 255f;
                        float tintB = renderedTint.B / 255f;
                        float tintA = renderedTint.A / 255f;

                        // Create a color matrix that will apply the tint color
                        colorMatrix = new ColorMatrix(new float[][]
                        {
            new float[] {tintR, 0, 0, 0, 0},
            new float[] {0, tintG, 0, 0, 0},
            new float[] {0, 0, tintB, 0, 0},
            new float[] {0, 0, 0, tintA, 0},
            new float[] {0, 0, 0, 0, 1}
                        });
                    }

                    if (renderedTint != lastImageTint)
                    {
                        if (imageAttributes == null)
                        {
                            imageAttributes = new ImageAttributes();
                        }

                        imageAttributes.SetColorMatrix(colorMatrix);
                    }

                    // Draw the image with the tint
                    e.Graphics.DrawImage(
                        privateImage,
                        imageRectangle,
                        0, 0, privateImage.Width, privateImage.Height,
                        GraphicsUnit.Pixel,
                        imageAttributes);
                }

                lastImageTint = renderedTint;
            }
            base.OnPaint(e);
        }

        private Color lastImageTint = Color.Empty;
        private ColorMatrix colorMatrix = null;
        private ImageAttributes imageAttributes = null;

        private Color privateImageTint = Color.Black;

        [Category("HartUI")]
        public Color NormalImageTint
        {
            get
            {
                return privateImageTint;
            }
            set
            {
                privateImageTint = value;
                Invalidate();
            }
        }

        private Color privateHoverImageTint = Color.DimGray;

        [Category("HartUI")]
        public Color HoverImageTint
        {
            get
            {
                return privateHoverImageTint;
            }
            set
            {
                privateHoverImageTint = value;
                Invalidate();
            }
        }

        private Color privateCheckedImageTint = Color.White;

        [Category("HartUI")]
        public Color CheckedImageTint
        {
            get
            {
                return privateCheckedImageTint;
            }
            set
            {
                privateCheckedImageTint = value;
                Invalidate();
            }
        }

        private Color privatePressedImageTint = Color.FromArgb(32, 32, 32);

        [Category("HartUI")]
        public Color PressedImageTint
        {
            get
            {
                return privatePressedImageTint;
            }
            set
            {
                privatePressedImageTint = value;
                Invalidate();
            }
        }

        private int privateGroup = 0;

        [Category("HartUI")]
        [Description("The group for this and other cuiButtonGroup controls to uncheck when clicked.")]
        public int Group
        {
            get
            {
                return privateGroup;
            }
            set
            {
                privateGroup = value;
                Invalidate();
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            state = ButtonStates.Pressed;
            Focus();
            Invalidate();
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            UpdateState(ClientRectangle.Contains(e.Location), true);
            base.OnMouseUp(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            state = ButtonStates.Normal;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            state = ButtonStates.Hovered;
            Invalidate();
            base.OnMouseEnter(e);
        }

        private void UpdateState(bool isInside, bool updateGroup)
        {
            if (updateGroup && isInside)
            {
                UpdateGroup();
            }

            state = isInside ? ButtonStates.Hovered : ButtonStates.Normal;
            Invalidate();
        }

        private void UpdateGroup()
        {
            var parentControls = Parent?.Controls;
            if (parentControls != null)
            {
                foreach (Control ctrl in parentControls)
                {
                    if (ctrl is cuiButtonGroup cbg && cbg != this && cbg.Group == Group)
                    {
                        cbg.Checked = ReferenceEquals(cbg, this);
                    }
                }
            }
        }

        public void PerformClick()
        {
            UpdateState(true, true);
            OnClick(EventArgs.Empty);
        }
    }
}
