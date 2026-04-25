using CuoreUI.Helpers;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using static CuoreUI.Helpers.DrawingHelper;

namespace CuoreUI.Controls
{
    [Description("Modern take on the checkbox")]
    [ToolboxBitmap(typeof(ProgressBar))]
    [DefaultEvent("CheckedChanged")]
    public partial class cuiSwitch : UserControl
    {
        private const int Duration = 350;

        private readonly Timer animationTimer;
        private readonly Stopwatch stopwatch = new Stopwatch();

        private float startX;
        private float targetX;
        private float thumbX;

        private bool animating;

        public cuiSwitch()
        {
            InitializeComponent();

            DoubleBuffered = true;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);

            Size = new Size(48, 26);
            MinimumSize = new Size(12, 8);
            Cursor = Cursors.Hand;

            animationTimer = new Timer
            {
                Interval = LazyTimeDelta // uses your helper
            };
            animationTimer.Tick += AnimationTick;
        }

        private void AnimationTick(object sender, EventArgs e)
        {
            double t = stopwatch.Elapsed.TotalMilliseconds / Duration;

            if (t >= 1.0)
            {
                thumbX = targetX;
                animating = false;
                animationTimer.Stop();
                Invalidate();
                return;
            }

            double eased = DrawingHelper.EasingFunctions.FromEasingType(EasingTypes.SextOut, t, 1);

            thumbX = (float)(startX + (targetX - startX) * eased);

            Invalidate();
        }

        private void StartAnimation(bool instant = false)
        {
            UpdateTargetX();

            if (instant)
            {
                thumbX = targetX;
                animating = false;
                Invalidate();
                return;
            }

            startX = thumbX;

            stopwatch.Restart();
            animating = true;
            animationTimer.Start();
        }

        private void UpdateTargetX()
        {
            int thumbSize = Height - 7;

            if (Checked)
            {
                targetX = Width - 3.5f - thumbSize - (OutlineThickness / 2) + 0.5f;
            }
            else
            {
                targetX = (Height / 2f) - (thumbSize / 2f) + (OutlineThickness / 2) - 1.5f;
            }
        }

        private void UpdateThumbRect()
        {
            int thumbDim = Height - 7;

            thumbRect = new RectangleF(thumbX, 3, thumbDim, thumbDim);
            thumbRect.Offset(0.5f, 0.5f);
            thumbRect.Inflate(-(int)(OutlineThickness), -(int)(OutlineThickness));
            thumbRect.Inflate(ThumbSizeModifier);
        }

        private bool privateChecked;

        [Category("CuoreUI")]
        public bool Checked
        {
            get => privateChecked;
            set
            {
                if (privateChecked == value)
                    return;

                privateChecked = value;
                CheckedChanged?.Invoke(this, EventArgs.Empty);

                StartAnimation();
            }
        }

        public event EventHandler CheckedChanged;

        private Color privateBackground = DrawingHelper.PrimaryColor;

        [Category("CuoreUI")]
        public Color CheckedBackground
        {
            get => privateBackground;
            set { privateBackground = value; Invalidate(); }
        }

        private Color privateUncheckedBackground = Color.FromArgb(64, 128, 128, 128);

        [Category("CuoreUI")]
        public Color UncheckedBackground
        {
            get => privateUncheckedBackground;
            set { privateUncheckedBackground = value; Invalidate(); }
        }

        private Color privateCheckedForeground = Color.White;

        [Category("CuoreUI")]
        public Color CheckedForeground
        {
            get => privateCheckedForeground;
            set { privateCheckedForeground = value; Invalidate(); }
        }

        private Color privateUncheckedForeground = Color.White;

        [Category("CuoreUI")]
        public Color UncheckedForeground
        {
            get => privateUncheckedForeground;
            set { privateUncheckedForeground = value; Invalidate(); }
        }

        private Color privateOutlineColor = Color.Empty;

        [Category("CuoreUI")]
        public Color UncheckedOutlineColor
        {
            get => privateOutlineColor;
            set { privateOutlineColor = value; Invalidate(); }
        }

        private Color privateCheckedOutlineColor = Color.Empty;

        [Category("CuoreUI")]
        public Color CheckedOutlineColor
        {
            get => privateCheckedOutlineColor;
            set { privateCheckedOutlineColor = value; Invalidate(); }
        }

        private float privateOutlineThickness = 1f;

        [Category("CuoreUI")]
        public float OutlineThickness
        {
            get => privateOutlineThickness;
            set { privateOutlineThickness = value; Invalidate(); }
        }

        private bool privateShowSymbols;

        [Category("CuoreUI")]
        public bool ShowSymbols
        {
            get => privateShowSymbols;
            set { privateShowSymbols = value; Invalidate(); }
        }

        private Size privateThumbShrinkSize;

        [Category("CuoreUI")]
        public Size ThumbSizeModifier
        {
            get => privateThumbShrinkSize;
            set { privateThumbShrinkSize = value; Invalidate(); }
        }

        private RectangleF thumbRect;

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            int rounding = (Height / 2) - 1;

            Rectangle cr = ClientRectangle;
            cr.Inflate(-1, -1);
            cr.Inflate(-(int)OutlineThickness, -(int)OutlineThickness);

            using (GraphicsPath bg = GeneralHelper.RoundRect(cr, rounding - (int)OutlineThickness))
            using (SolidBrush bgBrush = new SolidBrush(Checked ? CheckedBackground : UncheckedBackground))
            {
                e.Graphics.FillPath(bgBrush, bg);

                using (Pen pen = new Pen(Checked ? CheckedOutlineColor : UncheckedOutlineColor, OutlineThickness))
                {
                    e.Graphics.PixelOffsetMode = PixelOffsetMode.Default;
                    e.Graphics.DrawPath(pen, bg);
                }
            }

            UpdateThumbRect();

            using (SolidBrush thumbBrush = new SolidBrush(Checked ? CheckedForeground : UncheckedForeground))
            {
                e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                e.Graphics.FillEllipse(thumbBrush, thumbRect);
            }

            base.OnPaint(e);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            UpdateTargetX();
            thumbX = targetX;
            Invalidate();
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            if (!animating)
                Checked = !Checked;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            StartAnimation(true);
        }
    }
}