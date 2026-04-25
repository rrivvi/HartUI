using CuoreUI.Components;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;
using Color = System.Drawing.Color;

namespace CuoreUI.Controls.Forms.Internal
{
    public partial class ComboBoxDropDownForm : Form
    {
        private List<string> Items = new List<string> { };
        private Control TargetControl = null;

        int scrollOffset = 0;
        bool dragging = false;
        int dragStartY = 0;
        int scrollStartOffset = 0;

        // todo: expose these as properties
        internal int buttonHeight = 36;
        internal int buttonPadding = 6;
        internal int formPadding = 2;
        const int scrollbarWidth = 8;

        protected override void OnMouseDown(MouseEventArgs e)
        {
            var bar = GetScrollbarRect();
            if (bar.Contains(e.Location))
            {
                dragging = true;
                dragStartY = e.Y;
                scrollStartOffset = scrollOffset;
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            dragging = false;
            base.OnMouseUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (dragging)
            {
                int maxScroll = GetMaxScroll();
                int trackHeight = Height - BarHeight();
                if (trackHeight <= 0) return;

                int dy = e.Y - dragStartY;
                float pct = (float)dy / trackHeight;
                scrollOffset = (int)(scrollStartOffset + pct * maxScroll);
                scrollOffset = Math.Max(0, Math.Min(maxScroll, scrollOffset));
            }

            Invalidate();
            base.OnMouseMove(e);
        }

        int GetContentHeight()
        {
            int buttonOffsetSize = buttonHeight + buttonPadding - 2;
            return buttonPadding + formPadding + formPadding + Items.Count * buttonOffsetSize;
        }

        int GetMaxScroll()
        {
            return Math.Max(0, GetContentHeight() - Height);
        }

        int BarHeight()
        {
            int content = GetContentHeight();
            if (content <= Height)
            {
                return Height;
            }

            float pct = (float)Height / content;
            return Math.Max(20, (int)(Height * pct));
        }

        Rectangle GetScrollbarRect()
        {
            int h = BarHeight();
            int maxScroll = GetMaxScroll();
            int y = 0;
            if (maxScroll > 0)
            {
                float pct = (float)scrollOffset / maxScroll;
                y = (int)((Height - h) * pct);
            }
            return new Rectangle(Width - scrollbarWidth - 4, y + 4, scrollbarWidth, h - (formPadding + buttonPadding + 1));
        }

        public ComboBoxDropDownForm()
        {
            InitializeComponent();
            SetStyle(ControlStyles.SupportsTransparentBackColor | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }

        internal EventHandler SelectedItemChanged;
        cuiFormRounder formRounder;

        protected override void OnHandleCreated(EventArgs e)
        {
            formRounder = new cuiFormRounder();
            base.OnHandleCreated(e);
            formRounder.OutlineColor = Color.FromArgb(48, 128, 128, 128);
            formRounder.TargetForm = this;
        }

        internal bool canShow = true;

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            int maxScroll = GetMaxScroll();
            if (maxScroll <= 0)
            {
                return;
            }

            scrollOffset -= Math.Sign(e.Delta) * (buttonHeight + buttonPadding - formPadding);
            scrollOffset = Math.Max(0, Math.Min(maxScroll, scrollOffset));

            Invalidate();

            base.OnMouseWheel(e);
        }

        public bool Show(Control attachToControl, System.Collections.Generic.List<string> comboBoxItems)
        {
            if (canShow == false)
            {
                return false;
            }

            canShow = false;
            Owner = attachToControl.FindForm();
            TargetControl = attachToControl;
            Items = comboBoxItems;

            if (attachToControl is cuiComboBox ccb)
            {
                MaxDropDownHeight = ccb.MaxDropDownHeight;
            }

            CalculateNewLocation(attachToControl);
            CalculateNewSize();
            Show();
            if (formRounder != null)
            {
                formRounder.roundedFormObj?.DrawForm(null, null);
                formRounder.roundedFormObj?.BringToFront();
            }
            Focus();
            LostFocus += ComboBoxDropDownForm_LostFocus;
            VerifyScrollbarVisibility();

            return true;
        }

        private void VerifyScrollbarVisibility()
        {
            if (!(GetContentHeight() > Height))
            {
                scrollOffset = 0;
                dragging = false;
            }
        }

        private void CalculateNewLocation(Control attachToControl)
        {
            Location = attachToControl.PointToScreen(Point.Empty) + new Size(-formPadding, attachToControl.Height + 2 - formPadding);
        }

        private async void ComboBoxDropDownForm_LostFocus(object sender, System.EventArgs e)
        {
            if (formRounder != null)
            {
                formRounder.roundedFormObj?.Hide();
            }
            Hide();
            LostFocus -= ComboBoxDropDownForm_LostFocus;

            // prevents accidents
            await Task.Delay(250);
            canShow = true;
        }

        internal int MaxDropDownHeight = 240;

        void CalculateNewSize()
        {
            int buttonOffsetSize = buttonHeight + buttonPadding - 2;
            int doubleFormPadding = formPadding * 2;

            int newHeight = buttonPadding - 1;

            int count = Items.Count;
            for (int i = 0; i < count; i++)
            {
                newHeight += buttonOffsetSize;
                if (newHeight > MaxDropDownHeight)
                    break;
            }

            Size = new Size(TargetControl.Width + 1 + doubleFormPadding, newHeight + 1 + doubleFormPadding);
            formRounder.roundedFormObj.Region = null;
        }

        internal int _selectedIndex = -1;
        public int SelectedIndex
        {
            get
            {
                return _selectedIndex;
            }
            set
            {
                _selectedIndex = value;
                SelectedItemChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        protected override void OnClick(EventArgs e)
        {
            Point clickPoint = PointToClient(Cursor.Position);

            if (dragging || GetScrollbarRect().Contains(clickPoint))
                return;

            int buttonOffsetSize = buttonHeight + buttonPadding - 2;
            int currentItemY = buttonPadding - scrollOffset;
            int doublePadding = buttonPadding * 2;
            bool isOverflowing = GetContentHeight() > Height;

            int width = Width - doublePadding - 1 - (isOverflowing ? scrollbarWidth : 0);

            for (int i = 0; i < Items.Count; i++)
            {
                Rectangle itemRect = new Rectangle(buttonPadding, currentItemY, width, buttonHeight);

                if (itemRect.Contains(clickPoint))
                {
                    SelectedIndex = i;
                    break;
                }

                currentItemY += buttonOffsetSize;
            }

            base.OnClick(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (TargetControl == null)
                return;

            var g = e.Graphics;

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            int currentItemY = buttonPadding - scrollOffset;
            int buttonOffsetSize = buttonHeight + buttonPadding - 2;
            int doublePadding = buttonPadding * 2;
            bool isOverflowing = GetContentHeight() > Height;

            int width = Width - doublePadding - 1 - (isOverflowing ? scrollbarWidth : 0);

            g.SetClip(ClientRectangle);
            Point cursorPosition = PointToClient(Cursor.Position);

            for (int i = 0; i < Items.Count; i++)
            {
                Rectangle itemRect = new Rectangle(buttonPadding, currentItemY, width, buttonHeight);
                bool isSelected = SelectedIndex == i;
                bool isHover = itemRect.Contains(cursorPosition);

                if (isHover || isSelected)
                {
                    using (SolidBrush hoverBrush = new SolidBrush(Color.FromArgb(32, 128, 128, 128)))
                    using (GraphicsPath itemPath = Helpers.GeneralHelper.RoundRect(itemRect, new Padding(8)))
                    {
                        g.FillPath(hoverBrush, itemPath);
                    }

                    if (isSelected)
                    {
                        Rectangle selectedIndicator = itemRect;
                        selectedIndicator.Inflate(0, -9);
                        selectedIndicator.Width = 3;

                        using (GraphicsPath selectedIndicatorPath = Helpers.GeneralHelper.RoundRect(selectedIndicator, new Padding(2)))
                        {
                            g.FillPath(Brushes.Gray, selectedIndicatorPath);
                        }
                    }
                }

                Point textPosition = new Point(itemRect.X + 8, currentItemY + (itemRect.Height - Font.Height) / 2);
                TextRenderer.DrawText(g, Items[i], Font, textPosition, ForeColor);

                currentItemY += buttonOffsetSize;
            }

            if (isOverflowing)
            {
                using (SolidBrush scrollbarBrush = new SolidBrush(Color.FromArgb(96, 128, 128, 128)))
                using (GraphicsPath scrollbarRoundedPath = Helpers.GeneralHelper.RoundRect(GetScrollbarRect(), 4))
                {
                    g.FillPath(scrollbarBrush, scrollbarRoundedPath);
                }
            }
            //base.OnPaint(e);
        }
    }
}
