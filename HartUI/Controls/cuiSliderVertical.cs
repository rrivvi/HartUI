using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace HartUI.Controls
{
    [ToolboxBitmap(typeof(TrackBar))]
    [DefaultEvent("ValueChanged")]
    public partial class cuiSliderVertical : cuiSlider
    {
        public cuiSliderVertical()
        {
            InitializeComponent();
        }

        private bool privateUpsideDown = false;

        [Category("HartUI")]
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

        protected override void UpdateThumbRectangle(out float halfThumb)
        {
            float thumbWidth = (Width / 8f) * 5;
            float halfThumbWidth = thumbWidth / 2;

            double progInverted = GetProgressHalfNormalized();
            ThumbRectangle = new RectangleF(
                (Width / 2) - halfThumbWidth - 1,
                (float)((Height * GetProgressPercentage()) - ((ThumbRectangle.Height / 2) * progInverted) - (1 * progInverted)),
                thumbWidth,
                thumbWidth);

            if (UpsideDown)
            {
                ThumbRectangle.Y = Height - ThumbRectangle.Y - ThumbRectangle.Height - 2;
            }

            halfThumb = halfThumbWidth;
        }

        protected override RectangleF GetTrackRectangle(float halfThumbSize)
        {
            RectangleF trackRectangle = new RectangleF(0, 0, (Width / 8) + 0.5f, Height - 1);
            trackRectangle.X = (Width / 2) - (trackRectangle.Width / 2) - 0.5f;
            trackRectangle.Inflate(0, -halfThumbSize);
            return trackRectangle;
        }

        protected override int GetTrackCornerRadius(RectangleF trackRectangle)
        {
            return (int)((trackRectangle.Width + 0.5f) / 2);
        }

        protected override void UpdateValueFromMousePosition(MouseEventArgs e)
        {
            float thumbHeight = ThumbRectangle.Height;
            float progress = Clamp((float)(e.Y - (thumbHeight / 2)) / (Height - thumbHeight), 0f, 1f);

            if (UpsideDown)
            {
                progress = 1 - progress;
            }

            Value = MinValue + progress * (MaxValue - MinValue);
        }

        protected override int GetStepDirection(Keys keyCode)
        {
            int direction = base.GetStepDirection(keyCode);

            if (UpsideDown && (keyCode == Keys.Up || keyCode == Keys.Down))
            {
                return direction;
            }

            return -direction;
        }
    }
}
