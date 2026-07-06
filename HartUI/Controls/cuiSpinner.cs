using HartUI.Helpers;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using static HartUI.Helpers.DrawingHelper;

namespace HartUI.Controls
{
    [Description("Loading spinner animated control")]
    [ToolboxBitmap(typeof(BackgroundWorker))]
    public partial class cuiSpinner : Control
    {
        private readonly Timer animationTimer = new Timer();
        private readonly Stopwatch stopwatch = Stopwatch.StartNew();
        private long lastElapsedTicks;

        private float privateRotateSpeed = 2f;
        private float privateRotation = 0f;
        private float privateArcSize = 5f;
        private const float ArcDegrees = 90f;

        private Color privateArcColor = DrawingHelper.PrimaryColor;
        private Color privateRingColor = Color.FromArgb(64, 128, 128, 128);

        bool alreadySpinning = false;

        public cuiSpinner()
        {
            InitializeComponent();

            DoubleBuffered = true;
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            lastElapsedTicks = stopwatch.ElapsedTicks;

            // Use the refresh-rate-derived interval here, not as the delta itself.
            animationTimer.Interval = Math.Max(1, DrawingHelper.LazyTimeDelta);
            animationTimer.Tick += AnimationTimer_Tick;

            if (alreadySpinning == false)
            {
                FrameDrawn -= RotateOnFrameDrawn;
                FrameDrawn += RotateOnFrameDrawn;
                alreadySpinning = true;
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            if (!DesignMode && RotateEnabled)
            {
                if (!animationTimer.Enabled)
                {
                    lastElapsedTicks = stopwatch.ElapsedTicks;
                    animationTimer.Start();
                }
            }
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            if (animationTimer.Enabled)
            {
                animationTimer.Stop();
            }

            base.OnHandleDestroyed(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            if (disposing)
            {
                animationTimer.Stop();
                animationTimer.Dispose();
            }

            base.Dispose(disposing);
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

            // Preserves the old feel: RotateSpeed 2 was roughly 100 deg/sec.
            Rotation = privateRotation + (privateRotateSpeed * 50f * deltaSeconds);
        }

        private void RotateOnFrameDrawn(object sender, EventArgs e)
        {
            // Keep for compatibility with your existing event flow.
            // The timer is the actual animation driver now.
            if (RotateEnabled)
            {
                AnimationTimer_Tick(sender, e);
            }
        }

        [Category("HartUI")]
        public float RotateSpeed
        {
            get => privateRotateSpeed;
            set
            {
                privateRotateSpeed = value;
                Invalidate();
            }
        }

        public bool RotateEnabled = true;

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
                if (wrapped < 0f)
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
        public float Thickness
        {
            get => privateArcSize;
            set
            {
                privateArcSize = value;
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

            // Keep the control drawable even when very small.
            side = Math.Max(side, spinnerThickness * 2f + spinnerThickness);

            float x = (ClientSize.Width - side) / 2f;
            float y = (ClientSize.Height - side) / 2f;

            // Leave room so the pen does not clip at the edges.
            float inset = spinnerThickness / 2f;
            return new RectangleF(x + inset, y + inset, side - spinnerThickness, side - spinnerThickness);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (DesignMode && !animationTimer.Enabled)
            {
                animationTimer.Start();
            }
            else if (!DesignMode && animationTimer.Enabled && !RotateEnabled)
            {
                animationTimer.Stop();
            }

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            float spinnerThickness = Math.Max(1f, Thickness * 2f);
            RectangleF bounds = GetSpinnerBounds();

            if (bounds.Width <= 0f || bounds.Height <= 0f)
            {
                return;
            }

            using (Pen ringPen = new Pen(RingColor, spinnerThickness))
            using (Pen arcPen = new Pen(ArcColor, spinnerThickness)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round
            })
            {
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