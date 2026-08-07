using HartUI.Helpers;
using HartUI.Misc.Internal;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HartUI.Controls
{
    [Designer(typeof(DesignerIntegration.HartControlDesigner))]
    [Description("Select a rating in stars")]
    [ToolboxBitmap(typeof(ToolTip))]
    [DefaultEvent("RatingChanged")]
    public partial class cuiStarRating : Control
    {
        public cuiStarRating()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            Size = new Size(150, 28);
        }

        [Category("HartUI")]
        public event EventHandler RatingChanged;

        private int privateStarCount = 5;
        private int privateRating = 2;

        private int? hoverRating = null;

        private Color privateStarColor = Helpers.DrawingHelper.PrimaryColor;
        private int privateStarBorderSize = 1;

        bool showKeyboardFocus = InputManager.LastInputWasKeyboard;

        [Category("HartUI")]
        public int StarCount
        {
            get
            {
                return privateStarCount;
            }
            set
            {
                privateStarCount = value;
                Invalidate();
            }
        }

        [Category("HartUI")]
        public int Rating
        {
            get
            {
                return privateRating;
            }
            set
            {
                if (privateRating != value)
                {
                    privateRating = value;
                    RatingChanged?.Invoke(this, EventArgs.Empty);
                }
                Invalidate();
            }
        }

        private float privateRounding = 1;

        [Category("HartUI")]
        public float Rounding
        {
            get
            {
                return privateRounding;
            }
            set
            {
                if (privateRounding != value)
                {
                    privateRounding = Math.Max(0, value);
                    Invalidate();
                }
            }
        }

        [Category("HartUI")]
        public Color StarColor
        {
            get
            {
                return privateStarColor;
            }
            set
            {
                privateStarColor = value;
                Invalidate();
            }
        }

        [Category("HartUI")]
        public int StarBorderSize
        {
            get
            {
                return privateStarBorderSize;
            }
            set
            {
                privateStarBorderSize = value;
                Invalidate();
            }
        }

        private bool privateAllowUserInteraction = true;

        [Category("HartUI")]
        public bool AllowUserInteraction
        {
            get
            {
                return privateAllowUserInteraction;
            }
            set
            {
                privateAllowUserInteraction = value;

                if (!privateAllowUserInteraction)
                {
                    hoverRating = null;
                }

                Invalidate();
            }
        }

        private static int GetStarState(int rating, int starIndex)
        {
            if ((starIndex + 1) * 2 <= rating)
            {
                return 2;
            }

            if (starIndex * 2 + 1 == rating)
            {
                return 1;
            }

            return 0;
        }

        bool ShouldShowFocus(int index, int lastFilledStar, int starState)
        {
            return (index == lastFilledStar && (starState == 2 || starState == 1))
                    || (index == 0 && lastFilledStar <= 0);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            int starWidth = Height - 2;
            int spacing = starWidth / 5;

            int? effectiveHoverRating = (AllowUserInteraction && hoverRating.HasValue)
                ? hoverRating
                : null;
            int lastFilledStar = Rating == 0 ? -1 : (Rating - 1) / 2;

            using (SolidBrush starBrush = new SolidBrush(StarColor))
            using (SolidBrush previewBrush = new SolidBrush(Color.FromArgb(StarColor.A / 2, StarColor)))
            using (SolidBrush backgroundBrush = new SolidBrush(BackColor))
            using (Pen starBorderPen = new Pen(StarColor, StarBorderSize))
            {
                for (int i = 0; i < StarCount; i++)
                {
                    int currentStarState = GetStarState(Rating, i);
                    int currentStarPreviewState = effectiveHoverRating.HasValue
                        ? GetStarState(effectiveHoverRating.Value, i)
                        : 0;

                    int starLeft = i * (starWidth + spacing);
                    Rectangle starRect = new Rectangle(starLeft, 0, starWidth, this.Height);

                    starRect.Offset(starWidth / 2, 0);
                    starRect.Inflate(-StarBorderSize, -StarBorderSize);
                    starRect.Offset(StarBorderSize / 2, StarBorderSize / 2);

                    bool isCurrentlyFocusable = Focused && showKeyboardFocus;
                    bool showFocus = ShouldShowFocus(i, lastFilledStar, currentStarState);

                    if (isCurrentlyFocusable && showFocus)
                    {
                        const int focusStateMargin = 2;
                        float outerStarScale = (starWidth / 2f + focusStateMargin) / (starWidth / 2f);
                        float innerStarScale = (starWidth / 2f - focusStateMargin) / (starWidth / 2f);

                        using (GraphicsPath outerStar = GeneralHelper.Star(
                            starLeft + starWidth / 2,
                            Height / 2,
                            starWidth / 2,
                            starWidth / 3.8f,
                            Rounding,
                            outerStarScale))
                        {
                            using (Pen insetBackgroundPen = new Pen(BackColor, 4))
                            {
                                e.Graphics.DrawPath(insetBackgroundPen, outerStar);
                            }
                            e.Graphics.DrawPath(starBorderPen, outerStar);
                        }

                        // Dont draw the inner star for the first star because itd look weird
                        if (lastFilledStar > 0 || (lastFilledStar <= 0 && currentStarState != 0))
                        {
                            using (GraphicsPath innerStar = GeneralHelper.Star(
                                starLeft + starWidth / 2,
                                Height / 2,
                                starWidth / 2,
                                starWidth / 3.8f,
                                Math.Max(0, Rounding - 1),
                                innerStarScale))
                            {
                                if (currentStarState == 2)
                                {
                                    e.Graphics.FillPath(starBrush, innerStar);
                                }
                                else if (currentStarState == 1)
                                {
                                    e.Graphics.FillPath(starBrush, innerStar);

                                    starRect.Inflate(StarBorderSize, StarBorderSize);
                                    starRect.Offset(-(StarBorderSize / 2), -(StarBorderSize / 2));

                                    e.Graphics.FillRectangle(backgroundBrush, starRect);
                                }

                                e.Graphics.DrawPath(starBorderPen, innerStar);
                            }
                        }
                    }
                    else
                    {
                        using (GraphicsPath starPath = GeneralHelper.Star(
                               starLeft + starWidth / 2, Height / 2, starWidth / 2, starWidth / 3.8f, Rounding))
                        {
                            if (currentStarState == 2)
                            {
                                e.Graphics.FillPath(starBrush, starPath);
                            }
                            else if (currentStarState == 1)
                            {
                                e.Graphics.FillPath(starBrush, starPath);

                                starRect.Inflate(StarBorderSize, StarBorderSize);
                                starRect.Offset(-(StarBorderSize / 2), -(StarBorderSize / 2));

                                e.Graphics.FillRectangle(backgroundBrush, starRect);

                                if (currentStarPreviewState == 2)
                                {
                                    using (Region rightHalfRegion = new Region(starPath))
                                    {
                                        rightHalfRegion.Intersect(starRect);
                                        e.Graphics.FillRegion(previewBrush, rightHalfRegion);
                                    }
                                }
                            }
                            else if (currentStarPreviewState > 0)
                            {
                                e.Graphics.FillPath(previewBrush, starPath);

                                if (currentStarPreviewState == 1)
                                {
                                    starRect.Inflate(StarBorderSize, StarBorderSize);
                                    starRect.Offset(-(StarBorderSize / 2), -(StarBorderSize / 2));

                                    e.Graphics.FillRectangle(backgroundBrush, starRect);
                                }
                            }

                            e.Graphics.DrawPath(starBorderPen, starPath);
                        }
                    }
                }
            }

            base.OnPaint(e);
        }

        private int CalculateRatingFromMouseX(int x)
        {
            int starWidth = Height - 2;
            int spacing = starWidth / 5;
            int starCount = 5;

            int mouseX = x + 5;

            if (mouseX < 0)
            {
                return 0;
            }
            else if (mouseX > starCount * (starWidth + spacing))
            {
                return 10;
            }
            else
            {
                int starClicked = (mouseX - spacing) / (starWidth + spacing);
                float remainder = (mouseX - spacing) % (starWidth + spacing);

                if (remainder > starWidth / 2)
                {
                    return (starClicked + 1) * 2;
                }
                else
                {
                    return starClicked * 2 + 1;
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (AllowUserInteraction == false)
            {
                return;
            }

            int calculatedRating = CalculateRatingFromMouseX(e.X);

            if (hoverRating != calculatedRating)
            {
                hoverRating = calculatedRating;
                Invalidate();
            }

            if (e.Button == MouseButtons.Left)
            {
                showKeyboardFocus = false;
                Rating = calculatedRating;
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            if (hoverRating.HasValue)
            {
                hoverRating = null;
                Invalidate();
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
            OnMouseMove(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (AllowUserInteraction == false)
            {
                return;
            }

            if (e.KeyCode == Keys.Escape)
            {
                showKeyboardFocus = false;
                InvokeLostFocus(this, e);
                return;
            }

            showKeyboardFocus = true;

            if (e.KeyCode == Keys.Right || e.KeyCode == Keys.Up)
            {
                Rating = Math.Min(Rating + 1, StarCount * 2);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Down)
            {
                Rating = Math.Max(Rating - 1, 0);
                e.Handled = true;
                e.SuppressKeyPress = true;
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
            if (keyData == Keys.Left || keyData == Keys.Right ||
                keyData == Keys.Up || keyData == Keys.Down)
            {
                return true;
            }

            return base.IsInputKey(keyData);
        }
    }
}
