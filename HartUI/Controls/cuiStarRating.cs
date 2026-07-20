using HartUI.Helpers;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HartUI.Controls
{
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
                    privateRounding = value;
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

            using (SolidBrush starBrush = new SolidBrush(StarColor))
            using (SolidBrush previewBrush = new SolidBrush(Color.FromArgb(StarColor.A / 2, StarColor)))
            using (SolidBrush backgroundBrush = new SolidBrush(BackColor))
            using (Pen starBorderPen = new Pen(StarColor, StarBorderSize))
            {
                for (int i = 0; i < StarCount; i++)
                {
                    int starLeft = i * (starWidth + spacing);
                    Rectangle starRect = new Rectangle(starLeft, 0, starWidth, this.Height);
                    starRect.Offset(starWidth / 2, 0);
                    starRect.Inflate(-StarBorderSize, -StarBorderSize);
                    starRect.Offset(StarBorderSize / 2, StarBorderSize / 2);

                    using (GraphicsPath starPath = GeneralHelper.Star(
                               starLeft + starWidth / 2, Height / 2, starWidth / 2, starWidth / 3.8f, Rounding))
                    {
                        int currentStarState = GetStarState(Rating, i);
                        int currentStarPreviewState = effectiveHoverRating.HasValue
                            ? GetStarState(effectiveHoverRating.Value, i)
                            : 0;

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
    }
}
