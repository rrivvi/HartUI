using System.ComponentModel;
using System.Drawing;

namespace HartUI.Controls
{
    [Description("Show progress step-by-step to the user")]
    public partial class cuiProgressTrackerVertical : cuiProgressTrackerHorizontal
    {
        public cuiProgressTrackerVertical() : base()
        {
        }

        protected override Size DefaultControlSize => new Size(58, 480);

        protected override int GetCrossExtent() => Width;
        protected override int GetPrimaryExtent() => Height;
        protected override int TextGap => 2;
        protected override StringFormat CreateStringFormat() => new StringFormat { LineAlignment = StringAlignment.Center };
        protected override Point MakePoint(int primary, int secondary) => new Point(secondary, primary);
        protected override Rectangle MakeRect(int primary, int secondary, int size) => new Rectangle(secondary, primary, size, size);
    }
}
