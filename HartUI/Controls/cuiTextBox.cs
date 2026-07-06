using HartUI.Helpers;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

// THIS FILE CONTAINS A MODIFIED VERSION OF A TEXTBOX TAKEN FROM:
// https://github.com/RJCodeAdvance/Custom-TextBox-2--Rounded-Placeholder

// ORIGINAL AUTHOR: RJCodeAdvance
// LICENSE: Unlicense (https://unlicense.org/)

// MODIFICATIONS HAVE BEEN MADE TO THE ORIGINAL CODE TO SUIT HartUI
// LIKE ADDING CERTAIN PROPERTIES, OR SLIGHTLY MODIFYING HOW THE CONTROL IS DRAWN

namespace HartUI.Controls
{
    [ToolboxBitmap(typeof(TextBox))]
    [DefaultEvent("ContentChanged")]
    public partial class cuiTextBox : UserControl
    {
        private Color privateBackgroundColor = Color.White;
        private Color privateFocusBackgroundColor = Color.White;

        private Color privateBorderColor = Color.FromArgb(128, 128, 128, 128);

        private Color privateFocusBorderColor = Helpers.DrawingHelper.PrimaryColor;
        private int privateBorderSize = 1;
        private bool privateUnderlinedStyle = true;

        internal bool internalIsFocused = false;
        private bool privateIsFocused
        {
            get
            {
                return internalIsFocused;
            }
            set
            {
                internalIsFocused = value;
                contentTextField.BackColor = value ? FocusBackgroundColor : BackgroundColor;
                placeholderTextField.BackColor = contentTextField.BackColor;
                Invalidate();
            }
        }

        private Padding privateBorderRadius = new System.Windows.Forms.Padding(8, 8, 8, 8);
        private string privatePlaceholderText = "";
        private bool privateIsPlaceholder = false;

        public event EventHandler ContentChanged;

        public cuiTextBox()
        {
            InitializeComponent();
            base.BackColor = Color.Empty;
            ForeColor = Color.Gray;
            Multiline = false;
            Load += OnLoad;
            GotFocus += OnLoad;
            PlaceholderText = "Placeholder text..";
        }

        private void OnLoad(object sender, EventArgs e)
        {
            this.privateIsFocused = false;
        }

        [Category("HartUI")]
        public Color BackgroundColor
        {
            get
            {
                return privateBackgroundColor;
            }
            set
            {
                privateBackgroundColor = value;
                if (DesignMode)
                {
                    contentTextField.BackColor = value;
                    placeholderTextField.BackColor = value;
                }
                else
                {
                    contentTextField.BackColor = privateIsFocused ? FocusBackgroundColor : value;
                    placeholderTextField.BackColor = contentTextField.BackColor;
                }
                Invalidate();
            }
        }

        [Category("HartUI")]
        public Color FocusBackgroundColor
        {
            get
            {
                return privateFocusBackgroundColor;
            }
            set
            {
                privateFocusBackgroundColor = value;
                if (DesignMode)
                {
                    contentTextField.BackColor = value;
                    placeholderTextField.BackColor = value;
                }
                else
                {
                    contentTextField.BackColor = privateIsFocused ? FocusBackgroundColor : value;
                    placeholderTextField.BackColor = contentTextField.BackColor;
                }
                Invalidate();
            }
        }

        [Category("HartUI")]
        public Color OutlineColor
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

        [Category("HartUI")]
        public Color FocusOutlineColor
        {
            get
            {
                return privateFocusBorderColor;
            }
            set
            {
                privateFocusBorderColor = value;
            }
        }

        [Category("HartUI")]
        private int OutlineThickness
        {
            get
            {
                return privateBorderSize;
            }
            set
            {
                if (value >= 1)
                {
                    privateBorderSize = value;
                    Invalidate();
                }
            }
        }

        [Category("HartUI")]
        public bool UnderlinedStyle
        {
            get
            {
                return privateUnderlinedStyle;
            }
            set
            {
                privateUnderlinedStyle = value;
                Invalidate();
            }
        }

        [Category("HartUI")]
        public bool PasswordChar
        {
            get
            {
                return contentTextField.UseSystemPasswordChar;
            }
            set
            {
                contentTextField.UseSystemPasswordChar = value;
            }
        }

        [Category("HartUI")]
        public bool Multiline
        {
            get
            {
                return contentTextField.Multiline;
            }
            set
            {
                contentTextField.Multiline = value;
                placeholderTextField.Multiline = value;
            }
        }

        [Category("HartUI")]
        private new Color BackColor
        {
            get
            {
                return base.BackColor;
            }
            set
            {
                value = Color.FromArgb(255, value); // prevent transparency crashes
                base.BackColor = value;
            }
        }

        [Category("HartUI")]
        public override Color ForeColor
        {
            get
            {
                return contentTextField.ForeColor;
            }
            set
            {
                base.ForeColor = value;
                contentTextField.ForeColor = value;
                contentTextField.Invalidate();
            }
        }

        [Category("HartUI")]
        public override Font Font
        {
            get
            {
                return base.Font;
            }
            set
            {
                base.Font = value;
                contentTextField.Font = value;
                placeholderTextField.Font = value;
            }
        }

        protected string actualText = "";

        [Category("HartUI")]
        public string Content
        {
            get
            {
                return actualText;
            }
            set
            {
                actualText = value;
                contentTextField.Text = value;

                UpdatePlaceholder();
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

        [Category("HartUI")]
        public Padding Rounding
        {
            get
            {
                return privateBorderRadius;
            }
            set
            {
                if (value == new Padding(0, 0, 0, 0))
                {
                    value = new Padding(2, 2, 2, 2);
                }
                if (value.All >= 0 || value.All == -1)
                {
                    privateBorderRadius = value;
                    Invalidate();
                }
            }
        }

        [Category("HartUI")]
        public Color PlaceholderColor
        {
            get
            {
                return placeholderTextField.ForeColor;
            }
            set
            {
                placeholderTextField.ForeColor = value;
            }
        }

        [Category("HartUI")]
        public string PlaceholderText
        {
            get
            {
                return privatePlaceholderText;
            }
            set
            {
                privatePlaceholderText = value;
                UpdatePlaceholder();
            }
        }

        private Size privateTextOffset = new Size(0, 0);
        [Category("HartUI")]
        public Size TextOffset
        {
            get
            {
                return privateTextOffset;
            }
            set
            {
                privateTextOffset = value;
                Invalidate();
            }
        }

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

        private Point privateImageExpand = Point.Empty;

        [Category("HartUI")]
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

        private Color privateImageTint = Color.White;

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

        private Color privateFocusImageTint = Color.White;

        [Category("HartUI")]
        public Color FocusImageTint
        {
            get
            {
                return privateFocusImageTint;
            }
            set
            {
                privateFocusImageTint = value;
                Invalidate();
            }
        }

        private Point privateImageOffset = new Point(0, 0);

        [Category("HartUI")]
        public Point ImageOffset
        {
            get
            {
                return privateImageOffset;
            }
            set
            {
                privateImageOffset = value;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            placeholderTextField.Visible = privateIsPlaceholder;
            Graphics g = e.Graphics;

            using (SolidBrush bgBrush = new SolidBrush(BackColor))
            {
                e.Graphics.FillRectangle(bgBrush, ClientRectangle);
            }

            Padding newPadding;

            if (Multiline)
            {
                int b = (Rounding.All / 2) + (Font.Height / 8);
                newPadding = new Padding(Font.Height, b, Font.Height, b);
            }
            else
            {
                int newTextboxY = (Height / 2) - (Font.Height / 2);
                if (newTextboxY < 0)
                {
                    newTextboxY = -newTextboxY;
                }
                newPadding = new Padding(Font.Height, newTextboxY, Font.Height, 0);
            }

            newPadding.Left += TextOffset.Width;
            newPadding.Right += TextOffset.Width;
            newPadding.Top += TextOffset.Height;
            newPadding.Bottom += TextOffset.Height;

            Padding = newPadding;

            if (privateBorderRadius.All > 1 || privateBorderRadius.All == -1) // Rounded
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                var rectBorderSmooth = ClientRectangle;
                var rectBorder = Rectangle.Inflate(rectBorderSmooth, -OutlineThickness, -OutlineThickness);
                //rectBorder.Offset(-BorderSize, -BorderSize);

                int smoothSize = privateBorderSize > 0 ? privateBorderSize : 1;

                using (SolidBrush bgBrush = new SolidBrush(privateIsFocused ? FocusBackgroundColor : BackgroundColor))
                using (GraphicsPath pathBorderSmooth = GeneralHelper.RoundRect(rectBorderSmooth, Rounding))
                using (GraphicsPath pathBorder = GeneralHelper.RoundRect(rectBorder, Rounding - new Padding(OutlineThickness, OutlineThickness, OutlineThickness, OutlineThickness) - new Padding(1, 1, 1, 1)))
                using (Pen penBorderSmooth = new Pen(BackColor, smoothSize))
                using (Pen penBorder = new Pen(privateIsFocused ? FocusOutlineColor : OutlineColor, OutlineThickness) { Alignment = PenAlignment.Center })
                {
                    e.Graphics.FillPath(bgBrush, pathBorder);

                    if (UnderlinedStyle)
                    {
                        // Draw border smoothing
                        g.DrawPath(penBorderSmooth, pathBorderSmooth);

                        RectangleF bounds = pathBorder.GetBounds();
                        RectangleF bottomHalfBounds = new RectangleF(bounds.X + 1, bounds.Y + bounds.Height / 2, bounds.Width - 1, bounds.Height / 2 + 1);

                        using (Region bottomHalfRegion = new Region(bottomHalfBounds))
                        {
                            // Set the clipping region for the path
                            g.SetClip(bottomHalfRegion, CombineMode.Intersect);

                            // Draw the bottom half
                            e.Graphics.PixelOffsetMode = PixelOffsetMode.Default;
                            g.DrawPath(penBorder, pathBorder);

                            // Reset the clipping region
                            g.ResetClip();
                        }
                    }
                    else // Normal
                    {
                        // Draw border smoothing
                        g.DrawPath(penBorderSmooth, pathBorderSmooth);

                        g.DrawPath(penBorder, pathBorder);
                    }
                }
            }
            else // Square/Normal TextBox
            {
                // Draw border
                using (Pen penBorder = new Pen(OutlineColor, OutlineThickness))
                {
                    Region?.Dispose();
                    Region = new Region(ClientRectangle);

                    penBorder.Alignment = System.Drawing.Drawing2D.PenAlignment.Inset;
                    if (privateIsFocused)
                    {
                        penBorder.Color = FocusOutlineColor;
                    }

                    if (UnderlinedStyle)
                    {
                        g.DrawLine(penBorder, 0, Height - 1, Width, Height - 1);
                    }
                    else // Normal
                    {
                        g.DrawRectangle(penBorder, 0, 0, Width - 0.5F, Height - 0.5F);
                    }
                }
            }

            if (privateImage != null)
            {
                Color renderedTint = internalIsFocused ? FocusImageTint : NormalImageTint;
                Rectangle imageRectangle = new Rectangle(contentTextField.Height, contentTextField.Location.Y, contentTextField.Height, contentTextField.Height);
                imageRectangle.Inflate(ImageExpand.X, ImageExpand.Y);
                imageRectangle.Offset(privateImageOffset);

                float tintR = renderedTint.R / 255f;
                float tintG = renderedTint.G / 255f;
                float tintB = renderedTint.B / 255f;
                float tintA = renderedTint.A / 255f;

                // Create a color matrix that will apply the tint color
                ColorMatrix colorMatrix = new ColorMatrix(new float[][]
                {
            new float[] {tintR, 0, 0, 0, 0},
            new float[] {0, tintG, 0, 0, 0},
            new float[] {0, 0, tintB, 0, 0},
            new float[] {0, 0, 0, tintA, 0},
            new float[] {0, 0, 0, 0, 1}
                });

                // Create image attributes and set the color matrix
                using (ImageAttributes imageAttributes = new ImageAttributes())
                {
                    imageAttributes.SetColorMatrix(colorMatrix);

                    // Draw the image with the tint
                    e.Graphics.DrawImage(
                        privateImage,
                        imageRectangle,
                        0, 0, privateImage.Width, privateImage.Height,
                        GraphicsUnit.Pixel,
                        imageAttributes);
                }
            }

            base.OnPaint(e);
        }

        protected void UpdatePlaceholder()
        {
            placeholderTextField.Text = PlaceholderText;

            if (actualText == "" && !internalIsFocused)
            {
                placeholderTextField.Visible = true;
                privateIsPlaceholder = true;
            }
            else
            {
                placeholderTextField.Visible = false;
                privateIsPlaceholder = false;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            actualText = contentTextField.Text;
            UpdatePlaceholder();
            ContentChanged?.Invoke(this, e);
        }
        private void textBox1_Click(object sender, EventArgs e)
        {
            OnClick(e);
        }

        private void textBox1_MouseEnter(object sender, EventArgs e)
        {
            OnMouseEnter(e);
        }
        private void textBox1_MouseLeave(object sender, EventArgs e)
        {
            OnMouseLeave(e);
        }
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            OnKeyPress(e);
        }

        private void textBox1_Enter(object sender, EventArgs e)
        {
            privateIsFocused = true;
            UpdatePlaceholder();
        }
        private void textBox1_Leave(object sender, EventArgs e)
        {
            privateIsFocused = false;
            Invalidate();
            UpdatePlaceholder();
        }

        private void cuiTextBox2_Click(object sender, EventArgs e)
        {
            contentTextField.Focus();
            Invalidate();
        }

        private void textBox2_MouseDown(object sender, MouseEventArgs e)
        {
            cuiTextBox2_Click(sender, e);
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (placeholderTextField.Text != PlaceholderText)
            {
                placeholderTextField.Text = PlaceholderText;
            }
        }
    }
}
