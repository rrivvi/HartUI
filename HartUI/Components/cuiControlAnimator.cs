using HartUI.Helpers;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static HartUI.Helpers.DrawingHelper;

namespace HartUI.Components
{
    [Description("Animate your control's location and/or opacity with easing functions")]
    [ToolboxBitmap(typeof(TrackBar))]
    public partial class cuiControlAnimator : Component
    {
        private readonly PaintEventHandler paintHandler;

        public cuiControlAnimator()
        {
            InitializeComponent();
            paintHandler = (sender, e) =>
            {
                if (animationFinished || !AnimateOpacity || currentControlOpacity > 254)
                    return;

                Control paintedControl = (Control)sender;

                Rectangle expandedRect = paintedControl.ClientRectangle;
                expandedRect.Inflate(2, 2);

                using (SolidBrush br = new SolidBrush(Color.FromArgb(currentControlOpacity, paintedControl.BackColor)))
                {
                    e.Graphics.FillRectangle(br, expandedRect);
                }
            };
        }

        private Control privateTargetControl;

        [Category("HartUI")]
        [Description("The control to animate.")]
        public Control TargetControl
        {
            get => privateTargetControl;
            set
            {
                if (ReferenceEquals(privateTargetControl, value))
                {
                    return;
                }

                CancelAnimation();

                if (privateTargetControl != null)
                {
                    privateTargetControl.HandleCreated -= PrivateTargetControl_HandleCreated;
                    privateTargetControl.Paint -= paintHandler;
                    privateTargetControl.LocationChanged -= TargetControl_LocationChanged;
                }

                privateTargetControl = null;

                if (value == null)
                {
                    return;
                }

                if (value is Form)
                {
                    if (DesignMode)
                    {
                        MessageBox.Show("Please use 'cuiFormAnimator' to animate Forms!", "HartUI");
                    }

                    return;
                }

                privateTargetControl = value;
                privateTargetControl.HandleCreated += PrivateTargetControl_HandleCreated;
            }
        }

        private void PrivateTargetControl_HandleCreated(object sender, EventArgs e)
        {
            if (AnimateOnStart)
            {
                _ = PlayAnimation();
            }
        }

        private int privateDuration = 1000;

        [Category("HartUI")]
        [Description("How long the animation should last in milliseconds. (ms)")]
        public int Duration
        {
            get => privateDuration;
            set => privateDuration = Math.Max(1, value);
        }

        private bool privateAnimateOpacity = false;

        [Category("HartUI")]
        [Description("Animates 'opacity' of the control from 0 -> 1.")]
        public bool AnimateOpacity
        {
            get => privateAnimateOpacity;
            set => privateAnimateOpacity = value;
        }

        [Category("HartUI")]
        [Description("Choose the easing type that suits the best.")]
        public EasingTypes EasingType { get; set; } = EasingTypes.QuadInOut;

        [Category("HartUI")]
        [Description("Where the TargetControl should be moved to.")]
        public Point TargetLocation { get; set; } = Point.Empty;

        [Category("HartUI")]
        [Description("Animate control when first shown on screen.")]
        public bool AnimateOnStart { get; set; } = true;

        [Category("HartUI")]
        [Description("Either move to TargetLocation or ignore animating location.")]
        public bool AnimateLocation { get; set; } = true;

        private OpacityEnum privateTargetOpacity = OpacityEnum.Visible;

        [Category("HartUI")]
        [Description("Target opacity (0 - 255) for the control when animation completes.")]
        public OpacityEnum TargetOpacity
        {
            get => privateTargetOpacity;
            set => privateTargetOpacity = value;
        }

        public enum OpacityEnum
        {
            Visible = 255,
            Transparent = 0
        }

        Point expectedLocation;
        Point externalOffset;

        bool writingLocation;

        Point animationOrigin;
        Point animationDestination;

        bool animating = false;
        bool animationFinished = true;

        byte currentControlOpacity = 255;

        CancellationTokenSource animationCts;

        [Browsable(false)]
        public bool IsAnimating => animating;

        private void WriteAnimatedLocation(Control target, Point location)
        {
            writingLocation = true;

            try
            {
                expectedLocation = location;
                target.Location = location;
            }
            finally
            {
                writingLocation = false;
            }
        }

        public async Task PlayAnimation()
        {
            if (animating || TargetControl == null || DesignMode)
                return;

            animating = true;
            animationFinished = false;

            CancellationTokenSource cts = new CancellationTokenSource();
            animationCts = cts;
            CancellationToken token = cts.Token;

            Control target = TargetControl;

            target.Paint += paintHandler;
            target.LocationChanged += TargetControl_LocationChanged;

            try
            {
                animationOrigin = target.Location;
                animationDestination = TargetLocation;

                expectedLocation = animationOrigin;
                externalOffset = Point.Empty;

                DateTime lastFrameTime = DateTime.Now;

                bool shouldAnimateLocationNow = AnimateLocation;
                bool animateTowardsVisible = TargetOpacity == OpacityEnum.Visible;

                AnimationStarted?.Invoke(this, EventArgs.Empty);

                double progress = 0;

                while (!animationFinished)
                {
                    token.ThrowIfCancellationRequested();

                    DateTime rightnow = DateTime.Now;
                    double elapsedMilliseconds = (rightnow - lastFrameTime).TotalMilliseconds;

                    progress += elapsedMilliseconds / Duration;
                    progress = Math.Min(progress, 1.0);

                    double eased = EasingFunctions.FromEasingType(EasingType, progress);

                    // Location
                    if (shouldAnimateLocationNow)
                    {
                        Point animationLocation = new Point(
                            animationOrigin.X + (int)((animationDestination.X - animationOrigin.X) * eased),
                            animationOrigin.Y + (int)((animationDestination.Y - animationOrigin.Y) * eased));

                        Point nextLocation = new Point(
                            animationLocation.X + externalOffset.X,
                            animationLocation.Y + externalOffset.Y);

                        WriteAnimatedLocation(target, nextLocation);
                    }

                    // "Opacity"
                    if (AnimateOpacity)
                    {
                        if (animateTowardsVisible)
                        {
                            currentControlOpacity = (byte)((1 - (eased * 100)) * 2.5d);
                        }
                        else
                        {
                            currentControlOpacity = (byte)((eased * 100) * 2.5d);
                        }
                        target.Invalidate();
                    }

                    // Check if can exit
                    if (progress >= 1.0)
                    {
                        progress = 1.0;

                        if (shouldAnimateLocationNow)
                        {
                            WriteAnimatedLocation(target, new Point(
                                animationDestination.X + externalOffset.X,
                                animationDestination.Y + externalOffset.Y));
                        }

                        animationFinished = true;
                        break;
                    }

                    lastFrameTime = rightnow;
                    await Task.Delay(1000 / DrawingHelper.GetHighestRefreshRate(), token);
                }

                AnimationEnded?.Invoke(this, EventArgs.Empty);
            }
            catch (OperationCanceledException)
            {
                // Cancelled or TargetControl was changed/disposed mid animation
            }
            finally
            {
                target.Paint -= paintHandler;
                target.LocationChanged -= TargetControl_LocationChanged;

                animating = false;
                animationFinished = true;

                if (ReferenceEquals(animationCts, cts))
                {
                    animationCts = null;
                }

                cts.Dispose();
            }
        }

        private void TargetControl_LocationChanged(object sender, EventArgs e)
        {
            if (writingLocation)
                return;

            Control changedControl = (Control)sender;

            externalOffset = new Point(
                externalOffset.X + (changedControl.Location.X - expectedLocation.X),
                externalOffset.Y + (changedControl.Location.Y - expectedLocation.Y));

            expectedLocation = changedControl.Location;
        }

        public void CancelAnimation()
        {
            animationCts?.Cancel();
        }

        [Category("HartUI")]
        public event EventHandler AnimationEnded;

        [Category("HartUI")]
        public event EventHandler AnimationStarted;
    }
}