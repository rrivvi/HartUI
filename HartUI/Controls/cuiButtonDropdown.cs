using HartUI.Helpers;
using HartUI.Misc.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace HartUI.Controls
{
    [Description("Unchecks other group buttons in the same Parent when pressed")]
    [ToolboxBitmap(typeof(Button))]
    [DefaultEvent("ItemClicked")]
    public partial class cuiButtonDropdown : cuiButton
    {
        public cuiButtonDropdown()
        {
            InitializeComponent();
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool CheckButton
        {
            get { return false; }
            set { }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool Checked
        {
            get { return false; }
            set { }
        }

        private int privateMaxDropDownHeight = 240;

        [Category("HartUI")]
        [Description("How big the drop down popup can be at maximum.")]
        public int MaxDropDownHeight
        {
            get
            {
                return privateMaxDropDownHeight;
            }
            set
            {
                privateMaxDropDownHeight = value;
                Invalidate();
            }
        }

        private int privateItemHeight = 26;
        [Category("HartUI")]
        [Description("How big the drop down popup can be at maximum.")]
        public int ItemHeight
        {
            get
            {
                return privateItemHeight;
            }
            set
            {
                privateItemHeight = value;
                Invalidate();
            }
        }

        [Serializable]
        public class DropdownItem
        {
            public Image Image { get; set; }
            public string Content { get; set; }

            public override string ToString()
            {
                return Content ?? string.Empty;
            }
        }

        [Category("HartUI")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<DropdownItem> Items { get; } = new List<DropdownItem>();

        public class DropdownItemClickedEventArgs : EventArgs
        {
            public int Index { get; }
            public DropdownItem Item { get; }

            public DropdownItemClickedEventArgs(int index, DropdownItem item)
            {
                Index = index;
                Item = item;
            }
        }

        [Category("HartUI")]
        [Description("Raised when the user presses an item inside the drop down.")]
        public event EventHandler<DropdownItemClickedEventArgs> ItemClicked;

        private Color privateDropDownBackgroundColor = Color.White;

        [Category("HartUI")]
        public Color DropDownBackgroundColor
        {
            get
            {
                return privateDropDownBackgroundColor;
            }
            set
            {
                privateDropDownBackgroundColor = value;
                Invalidate();
            }
        }

        private Color privateDropDownForeColor = Color.FromArgb(27, 27, 27);

        [Category("HartUI")]
        public Color DropDownForeColor
        {
            get
            {
                return privateDropDownForeColor;
            }
            set
            {
                privateDropDownForeColor = value;
                Invalidate();
            }
        }

        public bool isBrowsingOptions = false;

        protected override void OnClick(EventArgs e)
        {
            if (Items == null || Items.Count == 0)
            {
                base.OnClick(e);
                return;
            }

            List<string> itemContents = Items.Select(item => item.Content ?? string.Empty).ToList();
            List<Image> itemImages = Items.Select(item => item.Image).ToList();

            PreloadedForms.ComboBoxDropDownForm.BackColor = DropDownBackgroundColor;
            PreloadedForms.ComboBoxDropDownForm.ForeColor = DropDownForeColor;

            // cuiButtonDropdown has no concept of current selection
            PreloadedForms.ComboBoxDropDownForm._selectedIndex = -1;
            PreloadedForms.ComboBoxDropDownForm.buttonHeight = ItemHeight;

            // false means the user is clicking rapidly on this button and doesn't mean anything bad
            if (PreloadedForms.ComboBoxDropDownForm.Show(this, itemContents, itemImages))
            {
                PreloadedForms.ComboBoxDropDownForm.SelectedItemChanged += DropDown_SelectedItemChanged;
                PreloadedForms.ComboBoxDropDownForm.LostFocus += ComboBoxDropDownForm_LostFocus;

                isBrowsingOptions = true;
                Invalidate();
            }

            base.OnClick(e);
        }

        void DetachEventListeners()
        {
            isBrowsingOptions = false;
            Invalidate();
            PreloadedForms.ComboBoxDropDownForm.Owner.Focus();
            PreloadedForms.ComboBoxDropDownForm.LostFocus -= ComboBoxDropDownForm_LostFocus;
            PreloadedForms.ComboBoxDropDownForm.SelectedItemChanged -= DropDown_SelectedItemChanged;
        }

        private void DropDown_SelectedItemChanged(object sender, EventArgs e)
        {
            int clickedIndex = PreloadedForms.ComboBoxDropDownForm.SelectedIndex;
            DetachEventListeners();

            // -1 means the dropdown was dismissed
            if (clickedIndex >= 0 && clickedIndex < Items.Count)
            {
                ItemClicked?.Invoke(this, new DropdownItemClickedEventArgs(clickedIndex, Items[clickedIndex]));
            }
        }

        private void ComboBoxDropDownForm_LostFocus(object sender, EventArgs e)
        {
            DetachEventListeners();
        }

        private int arrowSize => Math.Max(0, Font.Height - 6);
        protected override int RightAccessoryWidth => arrowSize;

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Rectangle expandRect = new Rectangle(
                ContentRightEdge - 2,
                ((Height - arrowSize) / 2) - 2,
                arrowSize,
                arrowSize);

            using (SolidBrush arrowBrush = new SolidBrush(ForeColor))
            using (GraphicsPath expandPath = isBrowsingOptions
                       ? GeneralHelper.RoundTriangle(expandRect, 2, true)
                       : GeneralHelper.RoundTriangle(expandRect, 2))
            {
                e.Graphics.FillPath(arrowBrush, expandPath);
            }
        }
    }
}
