using HartUI.Helpers;
using HartUI.Misc.Internal;
using HartUI.Properties;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace HartUI.Controls
{
    [Description("Lets the user select a file or drop it onto the control")]
    [DefaultEvent("FileDropped")]
    public partial class cuiFileDropper : Control
    {
        private bool hover = false;
        private readonly StringFormat sf = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

        bool showKeyboardFocus = InputManager.LastInputWasKeyboard;

        [Category("HartUI")]
        public bool Multiselect { get; set; } = false;

        private Color privatePanelColor = Color.FromArgb(16, 255, 255, 255);

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

        private Color privatePanelOutlineColor = Color.FromArgb(128, 128, 128, 128);

        [Category("HartUI")]
        public Color DashedOutlineColor
        {
            get
            {
                return privatePanelOutlineColor;
            }
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

        private bool privateDashedOutline = true;

        [Category("HartUI")]
        public bool DashedOutline
        {
            get
            {
                return privateDashedOutline;
            }
            set
            {
                privateDashedOutline = value;
                Invalidate();
            }
        }

        private int privateDashLength = 8;

        [Category("HartUI")]
        public int DashLength
        {
            get
            {
                return privateDashLength;
            }
            set
            {
                privateDashLength = value;
                Invalidate();
            }
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

        public cuiFileDropper()
        {
            InitializeComponent();
            AllowDrop = true;
            ForeColor = Color.Gray;
            Cursor = Cursors.Hand;
            Size = new Size(270, 135);
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }

        [Category("HartUI")]
        public string NormalContent { get; set; } = "Drop file here";

        [Category("HartUI")]
        public string HoverContent { get; set; } = "Release to drop";

        [Category("HartUI")]
        [Description("Example: txt files (*.txt)|*.txt|All files (*.*)|*.*\nLeave empty for all file extensions.")]
        public string Filter { get; set; } = "";

        private Color privateHoverForeColor = Color.FromArgb(128, 128, 128, 128);

        [Category("HartUI")]
        public Color HoverForeColor
        {
            get => privateHoverForeColor;
            set { privateHoverForeColor = value; Invalidate(); }
        }

        [Category("HartUI")]
        public Color NormalForeColor
        {
            get => ForeColor;
            set { ForeColor = value; Invalidate(); }
        }

        private Color privateHoverUploadForeColor = Helpers.DrawingHelper.PrimaryColor;

        [Category("HartUI")]
        public Color HoverUploadForeColor
        {
            get => privateHoverUploadForeColor;
            set { privateHoverUploadForeColor = value; Invalidate(); }
        }

        private Color privateForeUploadColor = Helpers.DrawingHelper.PrimaryColor;

        [Category("HartUI")]
        public Color NormalUploadForeColor
        {
            get => privateForeUploadColor;
            set { privateForeUploadColor = value; Invalidate(); }
        }

        private bool privateClickToUpload = true;

        [Category("HartUI")]
        public bool UploadWithClick
        {
            get
            {
                return privateClickToUpload;
            }
            set
            {
                privateClickToUpload = value;
                Invalidate();
            }
        }

        [Category("HartUI")]
        public string UploadContent { get; set; } = "Click to upload";

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            Rectangle modifiedCR = ClientRectangle;
            modifiedCR.Width -= 1;
            modifiedCR.Height -= 1;

            modifiedCR.Inflate(-(int)(OutlineThickness), -(int)(OutlineThickness));

            using (GraphicsPath roundBackground = GeneralHelper.RoundRect(modifiedCR, Rounding))
            using (SolidBrush brush = new SolidBrush(PanelColor))
            using (Pen pen = new Pen(DashedOutlineColor, OutlineThickness) { DashStyle = DashedOutline ? DashStyle.Dash : DashStyle.Solid })
            using (SolidBrush textBrush = new SolidBrush(hover ? HoverForeColor : NormalForeColor))
            {
                if (DashedOutline)
                {
                    pen.DashStyle = DashStyle.Custom;
                    pen.DashPattern = new float[] { DashLength, DashLength };
                }

                e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                e.Graphics.FillPath(brush, roundBackground);
                e.Graphics.PixelOffsetMode = PixelOffsetMode.Default;
                e.Graphics.DrawPath(pen, roundBackground);

                string line1 = hover ? HoverContent : NormalContent;
                string line2 = UploadWithClick ? (hover ? UploadContent : UploadContent) : null;

                SizeF size1 = e.Graphics.MeasureString(line1, Font);
                SizeF size2 = line2 != null ? e.Graphics.MeasureString(line2, Font) : SizeF.Empty;

                float totalHeight = size1.Height + (line2 != null ? size2.Height : 0f);
                float startY = modifiedCR.Top + (modifiedCR.Height - totalHeight) / 2;

                RectangleF focusContentRect;

                if (privateImage != null)
                {
                    int imageHalfHeight = privateImageSize.Height / 2;
                    int halfPadding = ImagePadding / 2;

                    Rectangle imageRectangle = new Rectangle(
                        Width / 2 - privateImageSize.Width / 2,
                        (int)(startY - imageHalfHeight - halfPadding),
                        privateImageSize.Width,
                        privateImageSize.Height
                    );
                    e.Graphics.DrawImage(privateImage, imageRectangle);

                    using (ImageAttributes imageAttributes = DrawingHelper.Imaging.CreateTintImageAttributes(ImageTint))
                    {
                        e.Graphics.DrawImage(
                            privateImage,
                            imageRectangle,
                            0, 0, privateImage.Width, privateImage.Height,
                            GraphicsUnit.Pixel,
                            imageAttributes
                        );
                    }

                    int imageRectHalfHeight = imageRectangle.Height / 2;

                    RectangleF textRect1 = new RectangleF(
                        modifiedCR.Left,
                        startY + imageRectHalfHeight + halfPadding,
                        modifiedCR.Width,
                        size1.Height
                    );
                    e.Graphics.DrawString(line1, Font, textBrush, textRect1, sf);

                    if (line2 != null)
                    {
                        using (SolidBrush uploadTextBrush = new SolidBrush(hover ? HoverUploadForeColor : NormalUploadForeColor))
                        {
                            RectangleF textRect2 = new RectangleF(
                                modifiedCR.Left,
                                startY + size1.Height + imageRectHalfHeight + halfPadding,
                                modifiedCR.Width,
                                size2.Height
                            );
                            e.Graphics.DrawString(line2, Font, uploadTextBrush, textRect2, sf);

                            focusContentRect = GetFocusContentRect(imageRectangle, size1, size2, imageRectangle.Top, textRect2.Bottom);
                        }
                    }
                    else
                    {
                        focusContentRect = GetFocusContentRect(imageRectangle, size1, SizeF.Empty, imageRectangle.Top, textRect1.Bottom);
                    }
                }
                else
                {
                    RectangleF textRect1 = new RectangleF(modifiedCR.Left, startY, modifiedCR.Width, size1.Height);
                    e.Graphics.DrawString(line1, Font, textBrush, textRect1, sf);

                    if (line2 != null)
                    {
                        using (SolidBrush uploadTextBrush = new SolidBrush(hover ? HoverUploadForeColor : NormalUploadForeColor))
                        {
                            RectangleF textRect2 = new RectangleF(modifiedCR.Left, startY + size1.Height, modifiedCR.Width, size2.Height);
                            e.Graphics.DrawString(line2, Font, uploadTextBrush, textRect2, sf);

                            focusContentRect = GetFocusContentRect(null, size1, size2, textRect1.Top, textRect2.Bottom);
                        }
                    }
                    else
                    {
                        focusContentRect = GetFocusContentRect(null, size1, SizeF.Empty, textRect1.Top, textRect1.Bottom);
                    }
                }

                if (Focused && showKeyboardFocus)
                {
                    RectangleF focusRect = focusContentRect;
                    focusRect.Inflate(4, 4);

                    using (GraphicsPath focusPath = GeneralHelper.RoundRect(focusRect, new Padding(6)))
                    using (Pen focusPen = new Pen(HoverUploadForeColor, 1))
                    {
                        e.Graphics.DrawPath(focusPen, focusPath);
                    }
                }
            }

            base.OnPaint(e);
        }

        private RectangleF GetFocusContentRect(Rectangle? imageRectangle, SizeF size1, SizeF size2, float contentTop, float contentBottom)
        {
            float maxWidth = Math.Max(imageRectangle?.Width ?? 0, Math.Max(size1.Width, size2.Width));
            float centerX = Width / 2f;

            return new RectangleF(centerX - maxWidth / 2f, contentTop, maxWidth, contentBottom - contentTop);
        }

        protected override void OnDragEnter(DragEventArgs drgevent)
        {
            base.OnDragEnter(drgevent);
            if (drgevent.Data.GetDataPresent(DataFormats.FileDrop))
                drgevent.Effect = DragDropEffects.Copy;
        }
        protected override void OnDragOver(DragEventArgs drgevent)
        {
            base.OnDragOver(drgevent);
            if (drgevent.Data.GetDataPresent(DataFormats.FileDrop))
            {
                drgevent.Effect = DragDropEffects.Copy;
                bool alreadyHovering = hover;
                hover = true;

                if (alreadyHovering != hover)
                {
                    Invalidate();
                }
            }
        }
        protected override void OnDragLeave(EventArgs e)
        {
            base.OnDragLeave(e);
            hover = false;
            Invalidate();
        }

        private Image privateImage = Resources.ic_fluent_folder_add_24_regular;

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

        private Size privateImageSize = new Size(24, 24);

        [Category("HartUI")]
        public Size ImageSize
        {
            get
            {
                return privateImageSize;
            }
            set
            {
                privateImageSize = value;
                Invalidate();
            }
        }

        private Color privateImageColor = Color.Gray;

        [Category("HartUI")]
        public Color ImageTint
        {
            get
            {
                return privateImageColor;
            }
            set
            {
                privateImageColor = value;
                Invalidate();
            }
        }

        private int privateImagePadding = 2;

        [Category("HartUI")]
        public int ImagePadding
        {
            get
            {
                return privateImagePadding;
            }
            set
            {
                privateImagePadding = value;
                Invalidate();
            }
        }

        protected override void OnDragDrop(DragEventArgs drgevent)
        {
            base.OnDragDrop(drgevent);
            hover = false;
            Invalidate();

            object Data = drgevent.Data.GetData(DataFormats.FileDrop);
            if (Data is string[] fileList)
            {
                if (fileList != null && fileList.Length > 0)
                {
                    HandleDroppedPaths(fileList);
                }
            }
        }

        protected virtual void HandleDroppedPaths(string[] paths)
        {
            string[] AllowedFileExtensions = GetExtensionsFromFilter();


            var validFiles = (AllowedFileExtensions.Length > 0
                ? paths.Where(f => AllowedFileExtensions.Any(ext => f.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                : paths);

            // cuiFileDropper specific
            validFiles = validFiles.Where(f => !f.EndsWith("\\") && File.Exists(f));

            if (validFiles.Count() == 0)
            {
                return;
            }

            FileNames = validFiles.ToArray();
            FileName = FileNames[0];

            if (FileNames.Length > 1)
            {
                FileDropped?.Invoke(this, new FileDroppedEventArgs(FileNames));
            }
            else
            {
                FileDropped?.Invoke(this, new FileDroppedEventArgs(FileName));
            }
        }

        public string[] GetExtensionsFromFilter()
        {
            if (string.IsNullOrWhiteSpace(Filter))
                return Array.Empty<string>();

            var extensions = Filter
                .Split('|')
                .Where((_, index) => index % 2 == 1)
                .SelectMany(part => part.Split(';'))
                .Select(ext => ext.TrimStart('*').ToLowerInvariant())
                .Where(ext => !string.IsNullOrWhiteSpace(ext))
                .Distinct()
                .ToArray();

            return extensions.Contains(".*") ? Array.Empty<string>() : extensions;
        }

        [Category("HartUI")]
        public string FileName { get; private set; }

        [Category("HartUI")]
        public string[] FileNames { get; private set; }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            showKeyboardFocus = false;
            Focus();
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            PerformUpload();
        }

        protected virtual void PerformUpload()
        {
            if (!UploadWithClick)
            {
                return;
            }

            bool MultiselectNow = Multiselect;
            using (OpenFileDialog ofd = new OpenFileDialog() { Filter = Filter, Multiselect = MultiselectNow })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    if (MultiselectNow)
                    {
                        FileNames = ofd.FileNames;
                        FileDropped?.Invoke(this, new FileDroppedEventArgs(FileNames));
                    }
                    else
                    {
                        FileName = ofd.FileName;
                        FileDropped?.Invoke(this, new FileDroppedEventArgs(FileName));
                    }
                }
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyCode == Keys.Escape)
            {
                showKeyboardFocus = false;
                InvokeLostFocus(this, e);
                return;
            }

            showKeyboardFocus = true;

            if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;

                PerformUpload();
            }
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            showKeyboardFocus = InputManager.LastInputWasKeyboard;
            Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            Invalidate();
        }

        protected override bool IsInputKey(Keys keyData)
        {
            if (keyData == Keys.Space || keyData == Keys.Enter)
            {
                return true;
            }

            return base.IsInputKey(keyData);
        }

        [Category("HartUI")]
        public event EventHandler<FileDroppedEventArgs> FileDropped;
    }

    public class FileDroppedEventArgs : EventArgs
    {
        public FileDroppedEventArgs(string fileName)
        {
            FileName = fileName;
            FileNames = new string[] { fileName };
            OneFileDropped = true;
        }

        public FileDroppedEventArgs(string[] fileNames)
        {
            FileNames = fileNames;
            FileName = fileNames.FirstOrDefault();

            if (fileNames.Length == 1)
            {
                OneFileDropped = true;
            }
        }

        public bool OneFileDropped { get; private set; } = false;
        public string FileName;
        public string[] FileNames { get; }
    }
}
