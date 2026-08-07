using HartUI.Helpers;
using HartUI.Misc.Internal;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace HartUI.Controls
{
    [Designer(typeof(DesignerIntegration.HartControlDesigner))]
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

                focusedIndex = Math.Min(focusedIndex, BoxAmount);

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

        private int focusedIndex = 0;

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
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            int spacingBetweenBoxes = (Width - (Height * BoxAmount)) / (BoxAmount - 1);
            int boxSizeWithSpacingOffset = spacingBetweenBoxes + Height;

            using (StringFormat sf = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            using (SolidBrush unfocusedBrush = new SolidBrush(UnfocusedColor))
            using (Pen unfocusedPen = new Pen(UnfocusedBorderColor))
            using (SolidBrush unfocusedText = new SolidBrush(UnfocusedTextColor))
            {
                int currentPosition = 0;
                int highlightIndex = Math.Min(focusedIndex, BoxAmount - 1);

                for (int i = 0; i < BoxAmount; i++)
                {
                    Rectangle boxRectangle = new Rectangle(currentPosition, 0, Height - 1, Height - 1);

                    using (GraphicsPath gp = GeneralHelper.RoundRect(boxRectangle, Rounding))
                    {
                        if (i == highlightIndex && Focused)
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

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            focusedIndex = Math.Min(focusedIndex, Content.Length);
            Invalidate();
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
            Focus();

            int spacingBetweenBoxes = (Width - (Height * BoxAmount)) / (BoxAmount - 1);
            int boxSizeWithSpacingOffset = spacingBetweenBoxes + Height;
            int currentPosition = 0;

            bool clickedInBox = false;

            for (int i = 0; i < BoxAmount; i++)
            {
                var currentBoxRect = new Rectangle(currentPosition, 0, Height - 1, Height - 1);

                if (currentBoxRect.Contains(e.Location))
                {
                    clickedInBox = true;
                    focusedIndex = i < Content.Length ? i : Content.Length;
                    Invalidate();
                    break;
                }

                currentPosition += boxSizeWithSpacingOffset;
            }

            if (!clickedInBox)
            {
                focusedIndex = Content.Length;
                Invalidate();
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (!Focused)
            {
                return;
            }

            if (e.KeyCode == Keys.Left)
            {
                focusedIndex = Math.Max(0, focusedIndex - 1);
                Invalidate();
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }

            if (e.KeyCode == Keys.Right)
            {
                focusedIndex = Math.Min(Content.Length, focusedIndex + 1);
                Invalidate();
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }

            if (e.KeyCode == Keys.Home)
            {
                focusedIndex = 0;
                Invalidate();
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }

            if (e.KeyCode == Keys.End)
            {
                focusedIndex = Content.Length;
                Invalidate();
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }

            if (e.Control && e.KeyCode == Keys.V)
            {
                string clipboardText = Clipboard.GetText();
                if (!string.IsNullOrEmpty(clipboardText))
                {
                    char[] validChars = clipboardText
                        .Where(c => char.IsLetterOrDigit(c) && (!OnlyDigit || char.IsDigit(c)))
                        .Take(BoxAmount - focusedIndex)
                        .ToArray();

                    foreach (char c in validChars)
                    {
                        OnKeyPress(new KeyPressEventArgs(c));
                    }
                }

                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }

            if (e.KeyCode == Keys.Back)
            {
                if (focusedIndex > 0)
                {
                    int removeIndex = focusedIndex - 1;
                    if (removeIndex < Content.Length)
                    {
                        Content = Content.Remove(removeIndex, 1);
                    }
                    focusedIndex--;
                }

                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }

            if (e.Modifiers != Keys.None && e.Modifiers != Keys.Shift)
            {
                e.Handled = true;
            }
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (!char.IsLetterOrDigit(e.KeyChar) || (OnlyDigit && !char.IsDigit(e.KeyChar)))
            {
                e.Handled = true;
                return;
            }

            if (focusedIndex >= BoxAmount)
            {
                e.Handled = true;
                return;
            }

            char upper = char.ToUpper(e.KeyChar);

            if (focusedIndex < Content.Length)
            {
                char[] chars = Content.ToCharArray();
                chars[focusedIndex] = upper;
                Content = new string(chars);
            }
            else
            {
                Content += upper;
            }

            focusedIndex = Math.Min(focusedIndex + 1, BoxAmount);

            e.Handled = true;
            base.OnKeyPress(e);
        }

        protected override bool IsInputKey(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Left:
                case Keys.Right:
                case Keys.Home:
                case Keys.End:
                    return true;
            }

            return base.IsInputKey(keyData);
        }
    }
}