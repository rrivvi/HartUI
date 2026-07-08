using HartUI.Helpers;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HartUI.Controls
{
    [Description("Loading spinner animated control")]
    [ToolboxBitmap(typeof(BackgroundWorker))]
    public partial class cuiSpinner : Control
    {
        private readonly Timer animationTimer = new Timer();
        private readonly Stopwatch stopwatch = new Stopwatch();

        private long lastElapsedTicks;

        private float privateRotateSpeed = 2f;
        private float privateRotation = 0f;
        private float privateArcSize = 5f;

        private const float ArcDegrees = 90f;

        private Color privateArcColor = DrawingHelper.PrimaryColor;
        private Color privateRingColor = Color.FromArgb(64, 128, 128, 128);

        private bool privateRotateEnabled = true;

        public cuiSpinner()
        {
            InitializeComponent();

            DoubleBuffered = true;

            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            animationTimer.Interval = Math.Max(1, DrawingHelper.LazyTimeDelta);
            animationTimer.Tick += AnimationTimer_Tick;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            if (RotateEnabled)
            {
                StartAnimation();
            }
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            StopAnimation();

            base.OnHandleDestroyed(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                animationTimer.Tick -= AnimationTimer_Tick;
                animationTimer.Stop();
                animationTimer.Dispose();

                stopwatch.Stop();

                components?.Dispose();
            }

            base.Dispose(disposing);
        }

        private void StartAnimation()
        {
            if (!IsHandleCreated || animationTimer.Enabled)
            {
                return;
            }

            stopwatch.Start();

            lastElapsedTicks = stopwatch.ElapsedTicks;

            animationTimer.Start();
        }

        private void StopAnimation()
        {
            animationTimer.Stop();
            stopwatch.Stop();
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            if (!RotateEnabled || !IsHandleCreated)
            {
                return;
            }

            long currentElapsedTicks = stopwatch.ElapsedTicks;

            long deltaTicks = currentElapsedTicks - lastElapsedTicks;

            lastElapsedTicks = currentElapsedTicks;

            float deltaSeconds = (float)deltaTicks / Stopwatch.Frequency;

            Rotation = privateRotation + (privateRotateSpeed * 50f * deltaSeconds);
        }

        [Category("HartUI")]
        [DefaultValue(true)]
        public bool RotateEnabled
        {
            get => privateRotateEnabled;
            set
            {
                if (privateRotateEnabled == value)
                {
                    return;
                }

                privateRotateEnabled = value;

                if (value)
                {
                    StartAnimation();
                }
                else
                {
                    StopAnimation();
                }

                Invalidate();
            }
        }

        [Category("HartUI")]
        [DefaultValue(2f)]
        public float RotateSpeed
        {
            get => privateRotateSpeed;
            set
            {
                privateRotateSpeed = value;
                Invalidate();
            }
        }

        [Category("HartUI")]
        public Color ArcColor
        {
            get => privateArcColor;
            set
            {
                privateArcColor = value;
                Invalidate();
            }
        }

        [Category("HartUI")]
        public Color RingColor
        {
            get => privateRingColor;
            set
            {
                privateRingColor = value;
                Invalidate();
            }
        }

        [Category("HartUI")]
        public float Rotation
        {
            get => privateRotation;
            set
            {
                float wrapped = value % 360f;

                if (wrapped < 0)
                {
                    wrapped += 360f;
                }

                if (privateRotation == wrapped)
                {
                    return;
                }

                privateRotation = wrapped;
                Invalidate();
            }
        }

        [Category("HartUI")]
        [DefaultValue(5f)]
        public float Thickness
        {
            get => privateArcSize;
            set
            {
                privateArcSize = Math.Max(0.5f, Math.Min(value, 100f));
                Invalidate();
            }
        }

        private RectangleF GetSpinnerBounds()
        {
            float spinnerThickness = Math.Max(1f, Thickness * 2f);

            float side = Math.Min(ClientSize.Width, ClientSize.Height);

            if (side <= 0f)
            {
                return RectangleF.Empty;
            }

            side = Math.Max(side, spinnerThickness * 2f + spinnerThickness);

            float x = (ClientSize.Width - side) / 2f;
            float y = (ClientSize.Height - side) / 2f;

            float inset = spinnerThickness / 2f;

            return new RectangleF(
                x + inset,
                y + inset,
                side - spinnerThickness,
                side - spinnerThickness);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            RectangleF bounds = GetSpinnerBounds();

            if (bounds.Width <= 0f || bounds.Height <= 0f)
            {
                return;
            }

            float spinnerThickness = Math.Max(1f, Thickness * 2f);

            using (Pen ringPen = new Pen(RingColor, spinnerThickness))
            using (Pen arcPen = new Pen(ArcColor, spinnerThickness))
            {
                arcPen.StartCap = LineCap.Round;
                arcPen.EndCap = LineCap.Round;

                e.Graphics.DrawArc(ringPen, bounds, 0f, 360f);
                e.Graphics.DrawArc(arcPen, bounds, privateRotation, ArcDegrees);
            }
        }

        private void cuiSpinner_Load(object sender, EventArgs e)
        {
            Rotation = 0;
        }
    }
}