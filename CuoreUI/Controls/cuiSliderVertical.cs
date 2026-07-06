using CuoreUI.Helpers;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CuoreUI.Controls
{
    [ToolboxBitmap(typeof(TrackBar))]
    [DefaultEvent("ValueChanged")]
    public partial class cuiSliderVertical : UserControl
    {
        public cuiSliderVertical()
        {
            InitializeComponent();
            DoubleBuffered = true;
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }

        private bool privateUpsideDown = false;

        [Category("CuoreUI")]
        public bool UpsideDown
        {
            get
            {
                return privateUpsideDown;
            }
            set
            {
                privateUpsideDown = value;
                Invalidate();
            }
        }

        private float privateValue = 100;
        private float privateMinValue = 0;
        private float privateMaxValue = 100;

        // double ranging from [0 - 1]
        public double GetProgressPercentage()
        {
            // if this is true what are you even doing
            if (MaxValue == MinValue)
                return 0;

            return (double)(Value - MinValue) / (MaxValue - MinValue);
        }

        // double randing from [-1 - 1]
        private double GetProgressHalfNormalized()
        {
            double progress = GetProgressPercentage();
            progress = (-progress);

            if (progress < 0)
            {
                progress = -progress;
            }

            return progress * 2;
        }

        [Category("CuoreUI")]
        public float Value
        {
            get
            {
                return privateValue;
            }
            set
            {
                if (value >= privateMinValue && value <= privateMaxValue)
                {
                    bool isNewValue = value != privateValue;

                    privateValue = (int)value;

                    UpdateThumbRectangle();
                    Invalidate();

                    if (isNewValue)
                    {
                        ValueChanged?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
        }

        private void UpdateThumbRectangle()
        {
            float thumbWidth = (Width / 8f) * 5;
            float halfThumbWidth = thumbWidth / 2;

            double progInverted = GetProgressHalfNormalized();
            ThumbRectangle = new RectangleF((Width / 2) - halfThumbWidth - 1, (float)((Height * GetProgressPercentage()) - ((ThumbRectangle.Height / 2) * progInverted) - (1 * progInverted)), thumbWidth, thumbWidth);

            if (UpsideDown)
            {
                ThumbRectangle.Y = Height - ThumbRectangle.Y - ThumbRectangle.Height - 2;
            }
        }

        private void UpdateThumbRectangle(out float halfThumb)
        {
            float thumbWidth = (Width / 8f) * 5;
            float halfThumbWidth = thumbWidth / 2;

            double progInverted = GetProgressHalfNormalized();

            //  (float)((Height * GetProgress()) - ((ThumbRectangle.Height / 2) * progInverted) - (1 * progInverted))
            //  (Width / 2) - halfThumbHeight - 1
            ThumbRectangle = new RectangleF((Width / 2) - halfThumbWidth - 1, (float)((Height * GetProgressPercentage()) - ((ThumbRectangle.Height / 2) * progInverted) - (1 * progInverted)), thumbWidth, thumbWidth);

            if (UpsideDown)
            {
                ThumbRectangle.Y = Height - ThumbRectangle.Y - ThumbRectangle.Height - 2;
            }

            halfThumb = halfThumbWidth;
        }

        [Category("CuoreUI")]
        public event EventHandler ValueChanged;

        [Category("CuoreUI")]
        public float MinValue
        {
            get
            {
                return privateMinValue;
            }
            set
            {
                if (value < privateMaxValue)
                {
                    privateMinValue = value;
                    if (privateMinValue > privateValue)
                    {
                        privateValue = privateMinValue;
                    }
                    Invalidate();
                }
            }
        }

        [Category("CuoreUI")]
        public float MaxValue
        {
            get
            {
                return privateMaxValue;
            }
            set
            {
                if (value > privateMinValue)
                {
                    privateMaxValue = value;
                    if (privateMaxValue < privateValue)
                    {
                        privateValue = privateMaxValue;
                    }
                    Invalidate();
                }
            }
        }

        private Color privateTrackColor = Color.FromArgb(64, 128, 128, 128);

        [Category("CuoreUI")]
        public Color TrackColor
        {
            get
            {
                return privateTrackColor;
            }
            set
            {
                privateTrackColor = value;
                Invalidate();
            }
        }

        private Color privateThumbColor = Helpers.DrawingHelper.PrimaryColor;

        [Category("CuoreUI")]
        public Color ThumbColor
        {
            get
            {
                return privateThumbColor;
            }
            set
            {
                privateThumbColor = value;
                Invalidate();
            }
        }

        RectangleF ThumbRectangle = Rectangle.Empty;

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            RectangleF trackRectangle = new RectangleF(0, 0, (Width / 8) + 0.5f, Height - 1);
            trackRectangle.X = (Width / 2) - (trackRectangle.Width / 2) - 0.5f;

            float halfThumbHeight;
            UpdateThumbRectangle(out halfThumbHeight);

            trackRectangle.Inflate(0, -halfThumbHeight);
            using (GraphicsPath trackPath = GeneralHelper.RoundRect(trackRectangle, (int)((trackRectangle.Width + 0.5f) / 2)))
            using (SolidBrush trackBrush = new SolidBrush(TrackColor))
            {
                e.Graphics.FillPath(trackBrush, trackPath);
            }

            using (Pen thumbOutlinePen = new Pen(BackColor, ThumbOutlineThickness))
            using (SolidBrush thumbBrush = new SolidBrush(ThumbColor))
            {
                e.Graphics.DrawRectangles(thumbOutlinePen, new RectangleF[] { ThumbRectangle });
                e.Graphics.FillEllipse(thumbBrush, ThumbRectangle);
            }

            base.OnPaint(e);
        }

        private int privateThumbOutlineThickness = 5;

        [Category("CuoreUI")]
        public int ThumbOutlineThickness
        {
            get
            {
                return privateThumbOutlineThickness;
            }
            set
            {
                privateThumbOutlineThickness = value;
                Invalidate();
            }
        }

        protected override void OnResize(EventArgs e)
        {
            UpdateThumbRectangle();
            base.OnResize(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
            OnMouseMove(new MouseEventArgs(MouseButtons.Left, 1, PointToClient(Cursor.Position).X, PointToClient(Cursor.Position).Y, 0));
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (e.Button == MouseButtons.Left)
            {
                float thumbHeight = ThumbRectangle.Height;
                float progress = Clamp((float)(e.Y - (thumbHeight / 2)) / (Height - thumbHeight), 0f, 1f);

                if (UpsideDown)
                {
                    progress = 1 - progress;
                }

                Value = MinValue + progress * (MaxValue - MinValue);
            }
        }

        public static float Clamp(float value, float min, float max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }
    }
}