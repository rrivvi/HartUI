using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace HartUI.Controls
{
    [Description("Unchecks other group buttons in the same Parent when pressed")]
    [ToolboxBitmap(typeof(Button))]
    [DefaultEvent("Click")]
    public partial class cuiButtonGroup : cuiButton
    {
        public cuiButtonGroup()
        {
            InitializeComponent();
        }

        [Category("HartUI")]
        public override bool Checked
        {
            get
            {
                return privateChecked;
            }
            set
            {
                if (privateChecked != value)
                {
                    if (value)
                    {
                        UpdateGroup();
                    }

                    privateChecked = value;
                    RaiseCheckedChanged();
                }

                Invalidate();
            }
        }

        protected override bool IsRenderedChecked => Checked;

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool CheckButton
        {
            get { return true; }
            set { }
        }

        protected override void AdjustBackgroundRectangle(ref Rectangle rect)
        {
            if (Rounding.Left == 0 & Rounding.Bottom == 0)
            {
                rect.Inflate(-1, 0);
            }
        }

        private int privateGroup = 0;

        [Category("HartUI")]
        [Description("The group for this and other cuiButtonGroup controls to uncheck when clicked.")]
        public int Group
        {
            get
            {
                return privateGroup;
            }
            set
            {
                privateGroup = value;
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            UpdateState(ClientRectangle.Contains(e.Location), true);
            base.OnMouseUp(e);
        }

        private void UpdateState(bool isInside, bool updateGroup)
        {
            if (updateGroup && isInside)
            {
                Checked = true;
            }

            state = isInside ? ButtonStates.Hovered : ButtonStates.Normal;
            Invalidate();
        }

        private void UpdateGroup()
        {
            var parentControls = Parent?.Controls;
            if (parentControls != null)
            {
                foreach (Control ctrl in parentControls)
                {
                    if (ctrl is cuiButtonGroup cbg && cbg != this && cbg.Group == Group)
                    {
                        cbg.Checked = ReferenceEquals(cbg, this);
                    }
                }
            }
        }

        public override void PerformClick()
        {
            UpdateState(true, true);
            OnClick(EventArgs.Empty);
        }
    }
}