using HartUI.Helpers;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace HartUI.Controls
{
    [Description("Lets the user input characters in separate boxes")]
    [DefaultEvent("FinishedTypingContent")]
    public partial class cuiOTPasswordBox : UserControl
    {
        public cuiOTPasswordBox()
        {
            InitializeComponent();
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
        }

        [Category("HartUI")]
        public bool OnlyDigit { get; set; } = false;

        [Category("HartUI")]
        public event EventHandler FinishedTypingContent;

        [Category("HartUI")]
        public event EventHandler NotFinishedTypingContent;

        private string privateContent = "";

        [Category("HartUI")]
        public string Content
        {
            get
            {
                return privateContent;
            }
            set
            {
                bool wasFullBefore = privateContent.Length >= BoxAmount;

                privateContent = value;

                if (privateContent.Length >= BoxAmount)
                {
                    FinishedTypingContent?.Invoke(this, EventArgs.Empty);
                }
                else if (wasFullBefore && privateContent.Length < BoxAmount)
                {
                    NotFinishedTypingContent?.Invoke(this, EventArgs.Empty);
                }

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

        private int privateBoxAmount = 6;

        [Category("HartUI")]
        public int BoxAmount
        {
            get
            {
                return privateBoxAmount;
            }
            set
            {
                privateBoxAmount = value;

                if (Content.Length > BoxAmount)
                {
                    Content = Content.Substring(BoxAmount - 1);
                }

                Invalidate();
            }
        }

        private Color privateUnfocusedColor = Color.White;

        [Category("HartUI")]
        public Color UnfocusedColor
        {
            get
            {
                return privateUnfocusedColor;
            }
            set
            {
                privateUnfocusedColor = value;
                Invalidate();
            }
        }

        private Color privateFocusedColor = Color.White;

        [Category("HartUI")]
        public Color FocusedColor
        {
            get
            {
                return privateFocusedColor;
            }
            set
            {
                privateFocusedColor = value;
                Invalidate();
            }
        }

        private Color privateUnfocusedBorderColor = Color.FromArgb(64, 128, 128, 128);

        [Category("HartUI")]
        public Color UnfocusedBorderColor
        {
            get
            {
                return privateUnfocusedBorderColor;
            }
            set
            {
                privateUnfocusedBorderColor = value;
                Invalidate();
            }
        }

        private Color privateFocusedBorderColor = DrawingHelper.PrimaryColor;

        [Category("HartUI")]
        public Color FocusedBorderColor
        {
            get
            {
                return privateFocusedBorderColor;
            }
            set
            {
                privateFocusedBorderColor = value;
                Invalidate();
            }
        }

        private Color privateUnfocusedTextColor = Color.Gray;

        [Category("HartUI")]
        public Color UnfocusedTextColor
        {
            get
            {
                return privateUnfocusedTextColor;
            }
            set
            {
                privateUnfocusedTextColor = value;
                Invalidate();
            }
        }

        private Color privateFocusedTextColor = Color.Black;

        [Category("HartUI")]
        public Color FocusedTextColor
        {
            get
            {
                return privateFocusedTextColor;
            }
            set
            {
                privateFocusedTextColor = value;
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
                privateRounding = value;
                Invalidate();
            }
        }

        private int focusedIndex = -1;

        private bool privateUnderlinedStyle = true;

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

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            int spacingBetweenBoxes = (Width - (Height * BoxAmount)) / (BoxAmount - 1);
            int boxSizeWithSpacingOffset = spacingBetweenBoxes + Height;

            using (StringFormat sf = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            using (SolidBrush unfocusedBrush = new SolidBrush(UnfocusedColor))
            using (Pen unfocusedPen = new Pen(UnfocusedBorderColor))
            using (SolidBrush unfocusedText = new SolidBrush(UnfocusedTextColor))
            {
                int currentPosition = 0;

                for (int i = 0; i < BoxAmount; i++)
                {
                    Rectangle boxRectangle = new Rectangle(currentPosition, 0, Height - 1, Height - 1);

                    using (GraphicsPath gp = GeneralHelper.RoundRect(boxRectangle, Rounding))
                    {
                        if (i == focusedIndex && Focused)
                        {
                            using (SolidBrush focusedBrush = new SolidBrush(FocusedColor))
                            using (Pen focusedPen = new Pen(FocusedBorderColor))
                            using (SolidBrush focusedText = new SolidBrush(FocusedTextColor))
                            {
                                e.Graphics.FillPath(focusedBrush, gp);

                                if (Content.Length > i)
                                {
                                    e.Graphics.DrawString(Content[i].ToString(), Font, focusedText, boxRectangle, sf);
                                }

                                if (UnderlinedStyle)
                                {
                                    RectangleF bounds = gp.GetBounds();
                                    RectangleF bottomHalfBounds = new RectangleF(bounds.X + 1, bounds.Y + bounds.Height / 2, bounds.Width - 1.5f, bounds.Height / 2 + 1);

                                    using (Region bottomHalfRegion = new Region(bottomHalfBounds))
                                    {
                                        // Sets the clipping region for the path
                                        e.Graphics.SetClip(bottomHalfRegion, CombineMode.Intersect);

                                        // Draws the the bottom half
                                        e.Graphics.DrawPath(focusedPen, gp);

                                        // Reset the clipping region for the next box
                                        e.Graphics.ResetClip();
                                    }
                                }
                                else
                                {
                                    e.Graphics.PixelOffsetMode = PixelOffsetMode.Default;
                                    e.Graphics.DrawPath(focusedPen, gp);
                                }
                            }
                        }
                        else
                        {
                            e.Graphics.FillPath(unfocusedBrush, gp);

                            if (Content.Length > i)
                            {
                                e.Graphics.DrawString(Content[i].ToString(), Font, unfocusedText, boxRectangle, sf);
                            }

                            if (UnderlinedStyle)
                            {
                                RectangleF bounds = gp.GetBounds();
                                RectangleF bottomHalfBounds = new RectangleF(bounds.X + 1, bounds.Y + bounds.Height / 2, bounds.Width - 1, bounds.Height / 2 + 1);

                                using (Region bottomHalfRegion = new Region(bottomHalfBounds))
                                {
                                    // Sets the clipping region for the path
                                    e.Graphics.SetClip(bottomHalfRegion, CombineMode.Intersect);

                                    e.Graphics.PixelOffsetMode = PixelOffsetMode.Default;

                                    // Draws the the bottom half
                                    e.Graphics.DrawPath(unfocusedPen, gp);

                                    // Reset the clipping region for the next box
                                    e.Graphics.ResetClip();
                                }
                            }
                            else
                            {
                                e.Graphics.DrawPath(unfocusedPen, gp);
                            }
                        }
                    }

                    currentPosition += boxSizeWithSpacingOffset;
                }
            }

            base.OnPaint(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            int spacingBetweenBoxes = (Width - (Height * BoxAmount)) / (BoxAmount - 1);
            int boxSizeWithSpacingOffset = spacingBetweenBoxes + Height;
            int currentPosition = 0;

            bool cursorInAnyBox = false;

            for (int i = 0; i < BoxAmount; i++)
            {
                var currentBoxRect = new Rectangle(currentPosition, 0, Height - 1, Height - 1);

                if (currentBoxRect.Contains(e.Location))
                {
                    cursorInAnyBox = true;
                    break;
                }

                currentPosition += boxSizeWithSpacingOffset;
            }

            Cursor = cursorInAnyBox ? Cursors.IBeam : Cursors.Arrow;

            base.OnMouseMove(e);
        }

        protected override void OnLostFocus(EventArgs e)
        {
            Invalidate();
            base.OnLostFocus(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            // this time onmousedown is at the top, because we want to potentially redraw AFTER we got focus
            base.OnMouseDown(e);

            int spacingBetweenBoxes = (Width - (Height * BoxAmount)) / (BoxAmount - 1);
            int boxSizeWithSpacingOffset = spacingBetweenBoxes + Height;
            int currentPosition = 0;

            for (int i = 0; i < BoxAmount; i++)
            {
                var currentBoxRect = new Rectangle(currentPosition, 0, Height - 1, Height - 1);

                bool cursorInCurrentBox = currentBoxRect.Contains(e.Location);
                Cursor = cursorInCurrentBox ? Cursors.Arrow : Cursors.IBeam;
                if (cursorInCurrentBox)
                {
                    if (focusedIndex != i)
                    {
                        if (i < Content.Length)
                        {
                            focusedIndex = i;
                            Invalidate();
                        }
                        else
                        {
                            focusedIndex = Content.Length;
                            Invalidate();
                        }
                    }
                }

                currentPosition += boxSizeWithSpacingOffset;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                focusedIndex = BoxAmount;
                Invalidate();
                return;
            }

            if (!Focused)
            {
                return;
            }

            if (focusedIndex > BoxAmount - 1 && Content.Length < BoxAmount)
            {
                focusedIndex = Content.Length;
            }

            if (e.Modifiers == Keys.Control && e.KeyCode == Keys.V)
            {
                string clipboardText = Clipboard.GetText();
                if (clipboardText != null)
                {
                    if (BoxAmount - Content.Length >= clipboardText.Length)
                    {
                        foreach (char c in clipboardText)
                        {
                            OnKeyPress(new KeyPressEventArgs(c));
                        }
                    }
                    else
                    {
                        char[] firstLetters = clipboardText
                            .Where(c => char.IsLetterOrDigit(c) && (!OnlyDigit || char.IsDigit(c)))
                            .Take(BoxAmount - Content.Length)
                            .ToArray();
                        foreach (char c in firstLetters)
                        {
                            OnKeyPress(new KeyPressEventArgs(c));
                        }
                    }
                }
            }

            if (e.Modifiers != Keys.None && e.Modifiers != Keys.Shift)
            {
                e.Handled = e.Modifiers != Keys.None;
            }

            if (e.KeyCode == Keys.Back && Content.Length > 0)
            {
                while (Content.Length < focusedIndex + 2)
                {
                    focusedIndex -= 1;
                }

                Content = Content.Substring(0, Content.Length - 1);
            }

            base.OnKeyDown(e);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            try
            {
                if (!char.IsLetterOrDigit(e.KeyChar) || (OnlyDigit && !char.IsDigit(e.KeyChar)))
                {
                    e.Handled = true;
                    return;
                }

                if (Content.Length > focusedIndex + 1)
                {
                    char[] chars = Content.ToCharArray();
                    chars[focusedIndex] = char.ToUpper(e.KeyChar);
                    focusedIndex++;
                    Content = new string(chars);

                    e.Handled = true;
                    return;
                }

                if (Content.Length > BoxAmount)
                {
                    e.Handled = true;
                    return;
                }

                while (Content.Length > focusedIndex - 1)
                {
                    focusedIndex++;
                }

                Content += char.ToUpper(e.KeyChar);
                e.Handled = true;

                base.OnKeyPress(e);
            }
            // chars like ;
            catch (IndexOutOfRangeException) { }
        }
    }
}
