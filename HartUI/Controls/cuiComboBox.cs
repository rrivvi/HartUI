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
    [DefaultEvent("SelectedIndexChanged")]
    [ToolboxBitmap(typeof(ComboBox))]
    public partial class cuiComboBox : UserControl
    {
        private int privateSelectedIndex = -1;

        [Category("HartUI")]
        public int SelectedIndex
        {
            get
            {
                return privateSelectedIndex;
            }
            set
            {
                privateSelectedIndex = value;
                if (Items != null)
                {
                    privateSelectedItem = SelectedIndex >= 0 ? Items[privateSelectedIndex] : "";
                }
                SelectedIndexChanged?.Invoke(this, EventArgs.Empty);

                Invalidate();
            }
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

        private string privateSelectedItem = string.Empty;

        [Category("HartUI")]
        public string SelectedItem
        {
            get
            {
                return privateSelectedItem;
            }
            set
            {
                if (value != null)
                {
                    if (value == string.Empty)
                    {
                        SelectedIndex = -1;
                    }
                    else if (Items != null && Items.Contains(value))
                    {
                        privateSelectedItem = value;
                        SelectedIndex = privateItems.IndexOf(privateSelectedItem);
                    }
                }
            }
        }

        private List<string> privateItems = new List<string> { "Item 1", "Item 2", "Item 3" };

        [Category("HartUI")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public string[] Items
        {
            get => privateItems.ToArray();
            set
            {
                privateItems = value != null ? new List<string>(value) : new List<string>();

                if (privateItems.Count > 0 && privateSelectedItem != null)
                {
                    if (!privateItems.Contains(privateSelectedItem))
                    {
                        SelectedIndex = 0;
                    }
                }
            }
        }

        [Category("HartUI")]
        public event EventHandler SelectedIndexChanged;

        public cuiComboBox()
        {
            InitializeComponent();
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            ForeColor = Color.Gray;
        }

        private Color privateBackgroundColor = Color.FromArgb(255, 255, 255);

        [Category("HartUI")]
        public Color BackgroundColor
        {
            get
            {
                return privateBackgroundColor;
            }
            set
            {
                privateBackgroundColor = value;
                Invalidate();
            }
        }

        private Color privateOutlineColor = Color.FromArgb(64, 128, 128, 128);

        [Category("HartUI")]
        public Color OutlineColor
        {
            get
            {
                return privateOutlineColor;
            }
            set
            {
                privateOutlineColor = value;
                Invalidate();
            }
        }

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

        private float privateOutlineThickness = 1;

        [Category("HartUI")]
        public float OutlineThickness
        {
            get
            {
                return privateOutlineThickness;
            }
            set
            {
                privateOutlineThickness = value;
                Invalidate();
            }
        }

        public bool isBrowsingOptions = false;

        private string privateNoSelectionText = "None";

        [Category("HartUI")]
        public string NoSelectionText
        {
            get
            {
                return privateNoSelectionText;
            }
            set
            {
                privateNoSelectionText = value;
                if (SelectedIndex == -1)
                {
                    Invalidate();
                }
            }
        }

        [Category("HartUI")]
        public int Rounding
        {
            get; set;
        } = 8;

        [Category("HartUI")]
        public bool SortAlphabetically { get; set; } = true;

        protected override void OnClick(EventArgs e)
        {
            //MessageBox.Show($"{_items.Contains(_selectedItem)}, {_selectedItem}");
            if (privateItems == null || privateItems.Count == 0)
            {
                return;
            }

            if (SortAlphabetically)
            {
                Array.Sort(Items, StringComparer.OrdinalIgnoreCase);
            }

            PreloadedForms.ComboBoxDropDownForm.BackColor = privateDropDownBackgroundColor;
            PreloadedForms.ComboBoxDropDownForm.ForeColor = DropDownForeColor;
            PreloadedForms.ComboBoxDropDownForm._selectedIndex = privateItems.IndexOf(privateSelectedItem);

            // The ComboBoxDropDownForm.Show method returns a bool:
            // true means the drop down appeared successfully
            // false means the user is clicking rapidly on this combo box and doesn't mean anything bad
            if (PreloadedForms.ComboBoxDropDownForm.Show(this, privateItems))
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
            //MessageBox.Show("changed");
            DetachEventListeners();
            SelectedIndex = PreloadedForms.ComboBoxDropDownForm.SelectedIndex;
        }

        private void ComboBoxDropDownForm_LostFocus(object sender, EventArgs e)
        {
            DetachEventListeners();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (SelectedIndex == -1 && privateSelectedItem != string.Empty)
            {
                privateSelectedItem = string.Empty;
                Invalidate();
            }

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            Rectangle cr = ClientRectangle;
            cr.Inflate(-1, -1);

            using (GraphicsPath roundBackground = GeneralHelper.RoundRect(cr, Rounding))
            using (SolidBrush backgroundBrush = new SolidBrush(BackgroundColor))
            using (Pen pen = new Pen(OutlineColor, OutlineThickness))
            using (StringFormat centerText = new StringFormat { Alignment = StringAlignment.Center })
            using (SolidBrush foreBrush = new SolidBrush(ForeColor))
            using (SolidBrush arrowBrush = new SolidBrush(ExpandArrowColor))
            {
                g.FillPath(backgroundBrush, roundBackground);
                g.PixelOffsetMode = PixelOffsetMode.Default;
                g.DrawPath(pen, roundBackground);

                string tempItemString = SelectedItem == string.Empty ? NoSelectionText : privateSelectedItem;
                g.DrawString(
                    tempItemString,
                    Font,
                    foreBrush,
                    new Point(Width / 2, (Height / 2) - (Font.Height / 2)),
                    centerText);

                Rectangle expandRect = ClientRectangle;
                expandRect.Width = Height / 2;
                expandRect.X = ClientRectangle.Right - Height / 2;
                expandRect.Height = expandRect.Width;
                expandRect.Offset(-expandRect.Width / 2, expandRect.Height / 2);

                expandRect.Width /= 2;
                expandRect.X = ClientRectangle.Right - Height / 2;
                expandRect.Height = expandRect.Width;
                expandRect.Offset(-expandRect.Width / 2, expandRect.Height / 2);

                using (GraphicsPath expandAvailable = isBrowsingOptions
                           ? GeneralHelper.RoundTriangle(expandRect, 2, true)
                           : GeneralHelper.RoundTriangle(expandRect, 2))
                {
                    g.FillPath(arrowBrush, expandAvailable);
                }
            }

            base.OnPaint(e);
        }

        private Color privateExpandColor = Color.Gray;

        [Category("HartUI")]
        public Color ExpandArrowColor
        {
            get
            {
                return privateExpandColor;
            }
            set
            {
                privateExpandColor = value;
                Invalidate();
            }
        }

        public void AddItem(string itemToAdd)
        {
            privateItems.Add(itemToAdd);
            Invalidate();
        }

        public void RemoveItem(string itemToRemove)
        {
            if (privateItems.Remove(itemToRemove))
            {
                Invalidate();
            }
        }
    }
}
