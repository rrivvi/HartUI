using System.Drawing;
using System.Windows.Forms;

namespace HartUI.Controls
{
    [ToolboxBitmap(typeof(ProgressBar))]
    public partial class cuiProgressBarVertical : cuiProgressBarHorizontal
    {
        public cuiProgressBarVertical() : base()
        {
            Size = new Size(Height, Width);
        }

        protected override RectangleF GetForegroundRect(float filledPercent)
        {
            float foreHeight = Flipped ? ClientRectangle.Height - (ClientRectangle.Height * filledPercent) : ClientRectangle.Height * filledPercent;
            return new RectangleF(
                0,
                Flipped ? 0 : ClientRectangle.Height - foreHeight,
                ClientRectangle.Width,
                foreHeight
            );
        }
    }
}
