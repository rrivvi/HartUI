using HartUI.Helpers;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HartUI.Controls
{
    [ToolboxBitmap(typeof(ListBox))]
    public partial class cuiListbox : ListBox
    {
        public cuiListbox()
        {
            InitializeComponent();
            DoubleBuffered = true;
            //SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.UserPaint, true);

            UpdateStyles();
            DrawMode = DrawMode.OwnerDrawFixed;
            BorderStyle = BorderStyle.None;
            ItemHeight = 34;
            ForeColor = Color.FromArgb(84, 84, 84);
            SelectionMode = SelectionMode.One;
            Font = new Font("Microsoft YaHei UI", 9, FontStyle.Regular);
        }

        public override Color BackColor
        {
            get
            {
                if (privateExplicitBackColor.HasValue)
                {
                    return privateExplicitBackColor.Value;
                }

                Control target = Parent ?? FindForm();
                return target != null ? target.BackColor : SystemColors.Window;
            }
            set
            {
                if (value.IsEmpty || value == Color.Transparent)
                {
                    if (privateExplicitBackColor.HasValue)
                    {
                        privateExplicitBackColor = null;
                        Invalidate();
                        OnBackColorChanged(EventArgs.Empty);
                    }
                }
                else if (privateExplicitBackColor != value)
                {
                    privateExplicitBackColor = value;
                    Invalidate();
                    OnBackColorChanged(EventArgs.Empty);
                }
            }
        }

        private Color? privateExplicitBackColor = null;
        private Control hookedParent = null;

        public bool ShouldSerializeBackColor()
        {
            return privateExplicitBackColor.HasValue;
        }

        public new void ResetBackColor()
        {
            BackColor = Color.Empty;
        }

        private void HandleParentBackColorChanged(object sender, EventArgs e)
        {
            if (!privateExplicitBackColor.HasValue)
            {
                Invalidate();
                OnBackColorChanged(EventArgs.Empty);
            }
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);

            if (hookedParent != null)
            {
                hookedParent.BackColorChanged -= HandleParentBackColorChanged;
            }

            hookedParent = Parent;

            if (hookedParent != null)
            {
                hookedParent.BackColorChanged += HandleParentBackColorChanged;
            }

            if (!privateExplicitBackColor.HasValue)
            {
                Invalidate();
                OnBackColorChanged(EventArgs.Empty);
            }
        }

        private Padding privateRounding = new Padding(8);

        [Category("HartUI")]
        public Padding Rounding
        {
            get
            {
                return privateRounding;
            }
            set
            {
                privateRounding = value;
                Invalidate();
            }
        }

        private Color privateOutlineColor = Color.FromArgb(128, 128, 128, 128);

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

        private int privateOutlineThickness = 1;

        [Category("HartUI")]
        public int OutlineThickness
        {
            get
            {
                return privateOutlineThickness;
            }
            set
            {
                if (value >= 1)
                {
                    privateOutlineThickness = value;
                    Invalidate();
                }
            }
        }

        private int privateItemRounding = 8;

        [Category("HartUI")]
        public int ItemRounding
        {
            get
            {
                return privateItemRounding;
            }
            set
            {
                if (value > 0)
                {
                    if (value > (ItemHeight / 2))
                    {
                        privateItemRounding = (ItemHeight / 2) + 1;
                    }
                    else
                    {
                        privateItemRounding = value;
                    }
                }
                else
                {
                    throw new Exception("ItemRounding cannot be greater than half of Item Height");
                }
                Invalidate();
            }
        }

        private Color privateBackgroundColor = Color.White;

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

        private Color privateItemHoveredBackgroundColor = Color.FromArgb(32, 128, 128, 128);

        [Category("HartUI")]
        public Color ItemHoverBackgroundColor
        {
            get
            {
                return privateItemHoveredBackgroundColor;
            }
            set
            {
                privateItemHoveredBackgroundColor = value;
                Invalidate();
            }
        }

        private Color privateItemHoveredForegroundColor = Color.DimGray;

        [Category("HartUI")]
        public Color ItemHoverForegroundColor
        {
            get
            {
                return privateItemHoveredForegroundColor;
            }
            set
            {
                privateItemHoveredForegroundColor = value;
                Invalidate();
            }
        }

        private Color privateForegroundColor = Color.DimGray;

        [Category("HartUI")]
        public Color ForegroundColor
        {
            get
            {
                return privateForegroundColor;
            }
            set
            {
                privateForegroundColor = value;
                Invalidate();
            }
        }

        private Color privateItemBackgroundColor = Color.Empty;

        [Category("HartUI")]
        public Color ItemBackgroundColor
        {
            get
            {
                return privateItemBackgroundColor;
            }
            set
            {
                privateItemBackgroundColor = value;
                Invalidate();
            }
        }

        private Color privateItemSelectedBackgroundColor = Helpers.DrawingHelper.PrimaryColor;

        [Category("HartUI")]
        public Color ItemSelectedBackgroundColor
        {
            get
            {
                return privateItemSelectedBackgroundColor;
            }
            set
            {
                privateItemSelectedBackgroundColor = value;
                Invalidate();
            }
        }

        private Color privateSelectedForegroundColor = Color.White;

        [Category("HartUI")]
        public Color SelectedForegroundColor
        {
            get
            {
                return privateSelectedForegroundColor;
            }
            set
            {
                privateSelectedForegroundColor = value;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            Rectangle cr = ClientRectangle;
            Rectangle backgroundRect = cr;
            backgroundRect.Inflate(5, 5);
            backgroundRect.Offset(-1, -1);
            cr.Width -= 1;
            cr.Height -= 1;

            using (Brush bgBrush = new SolidBrush(BackColor))
            using (GraphicsPath path2 = GeneralHelper.RoundRect(cr, Rounding))
            using (Brush itemBrush = new SolidBrush(BackgroundColor))
            using (Pen bgPen = new Pen(OutlineColor, OutlineThickness))
            {
                g.FillRectangle(bgBrush, backgroundRect);

                e.Graphics.FillPath(itemBrush, path2);
                e.Graphics.PixelOffsetMode = PixelOffsetMode.Default;
                e.Graphics.DrawPath(bgPen, path2);
                e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                // Clip to path2 to prevent overflow
                g.SetClip(path2, CombineMode.Intersect);

                // Now draw items inside clipped area
                using (Brush selectedBrush = new SolidBrush(ItemSelectedBackgroundColor))
                using (Brush selectedTextBrush = new SolidBrush(SelectedForegroundColor))
                using (Brush hoverBrush = new SolidBrush(ItemHoverBackgroundColor))
                using (Brush hoverTextBrush = new SolidBrush(ItemHoverForegroundColor))
                using (Brush normalBrush = new SolidBrush(ItemBackgroundColor))
                using (Brush normalTextBrush = new SolidBrush(ForegroundColor))
                using (Brush previewBrush = new SolidBrush(Color.FromArgb(
                    ItemSelectedBackgroundColor.A / 2,
                    ItemSelectedBackgroundColor)))
                {
                    // These are references and should not be disposed
                    Brush itemBackgroundBrush, itemForegroundBrush;

                    int first = TopIndex;
                    int visibleCount = (ClientSize.Height / ItemHeight) + 2;
                    int last = Math.Min(Items.Count, first + visibleCount);

                    for (int i = first; i < last; i++)
                    {
                        int y = (i - first) * ItemHeight;

                        Rectangle itemRect = new Rectangle(0, y, ClientSize.Width, ItemHeight);
                        itemRect.Inflate(-4, -2);
                        itemRect.Offset(0, 2);

                        int yCenterString = itemRect.Y + (ItemHeight - Font.Height) / 2;
                        string itemText = Items[i].ToString();

                        bool renderItemAsPreview = privateIsPreviewDragging
                            && i >= Math.Min(privatePreviewStart, privatePreviewEnd)
                            && i <= Math.Max(privatePreviewStart, privatePreviewEnd);

                        if (renderItemAsPreview)
                        {
                            itemBackgroundBrush = previewBrush;
                            itemForegroundBrush = normalTextBrush;
                        }
                        else if (GetSelected(i))
                        {
                            itemBackgroundBrush = selectedBrush;
                            itemForegroundBrush = selectedTextBrush;
                        }
                        else if (privateHoveredIndex == i)
                        {
                            itemBackgroundBrush = hoverBrush;
                            itemForegroundBrush = hoverTextBrush;
                        }
                        else
                        {
                            itemBackgroundBrush = normalBrush;
                            itemForegroundBrush = normalTextBrush;
                        }

                        if (ItemRounding > 0)
                        {
                            using (GraphicsPath itemPath = GeneralHelper.RoundRect(itemRect, ItemRounding))
                            {
                                g.FillPath(itemBackgroundBrush, itemPath);
                            }
                        }
                        else
                        {
                            g.FillRectangle(itemBackgroundBrush, itemRect);
                        }

                        g.DrawString(itemText, Font, itemForegroundBrush, itemRect.X + 6, yCenterString);
                    }
                }

                // Reset the clip after drawing
                g.ResetClip();
            }

            base.OnPaint(e);
        }

        private int privateHoveredIndex = -1;
        public int HoveredIndex
        {
            get
            {
                return privateHoveredIndex;
            }
        }

        private int privateAnchorIndex = -1;

        private bool privateIsPreviewDragging = false;
        private int privatePreviewStart = -1;
        private int privatePreviewEnd = -1;

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                base.OnMouseDown(e);
                return;
            }

            int index = IndexFromPoint(e.Location);

            if (index >= 0 && index < Items.Count)
            {
                SuspendLayout();

                switch (SelectionMode)
                {
                    case SelectionMode.None:
                        break;

                    case SelectionMode.One:
                        SelectedIndex = index;
                        privateAnchorIndex = index;
                        break;

                    case SelectionMode.MultiSimple:
                        SetSelected(index, !GetSelected(index));
                        privateAnchorIndex = index;
                        break;

                    case SelectionMode.MultiExtended:
                        bool ctrl = (ModifierKeys & Keys.Control) == Keys.Control;
                        bool shift = (ModifierKeys & Keys.Shift) == Keys.Shift;

                        if (shift && ctrl && privateAnchorIndex >= 0)
                        {
                            ExtendRange(privateAnchorIndex, index);
                        }
                        else if (shift && privateAnchorIndex >= 0)
                        {
                            SelectRange(privateAnchorIndex, index);
                        }
                        else if (ctrl)
                        {
                            SetSelected(index, !GetSelected(index));
                            privateAnchorIndex = index;
                        }
                        else
                        {
                            privateIsPreviewDragging = true;
                            privatePreviewStart = index;
                            privatePreviewEnd = index;
                            privateAnchorIndex = index;
                            Capture = true;
                        }
                        break;
                }

                Invalidate();
                ResumeLayout(true);
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (privateIsPreviewDragging)
            {
                SuspendLayout();

                SelectRange(privatePreviewStart, privatePreviewEnd);
                privateAnchorIndex = privatePreviewStart;

                privateIsPreviewDragging = false;
                privatePreviewStart = -1;
                privatePreviewEnd = -1;
                Capture = false;

                Invalidate();
                ResumeLayout(true);
            }

            base.OnMouseUp(e);
        }

        private void SelectRange(int start, int end)
        {
            ClearSelected();

            int lo = Math.Max(0, Math.Min(start, end));
            int hi = Math.Min(Items.Count - 1, Math.Max(start, end));

            for (int i = lo; i <= hi; i++)
            {
                SetSelected(i, true);
            }
        }

        private void ExtendRange(int start, int end)
        {
            int lo = Math.Max(0, Math.Min(start, end));
            int hi = Math.Min(Items.Count - 1, Math.Max(start, end));

            for (int i = lo; i <= hi; i++)
            {
                SetSelected(i, true);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            privateHoveredIndex = IndexFromPoint(e.Location);

            if (e.Button == MouseButtons.Left)
            {
                if (SelectionMode == SelectionMode.One)
                {
                    OnMouseDown(e);
                }
                else if (SelectionMode == SelectionMode.MultiExtended && privateIsPreviewDragging)
                {
                    if (privateHoveredIndex >= 0 && privateHoveredIndex < Items.Count)
                    {
                        privatePreviewEnd = privateHoveredIndex;
                    }
                }
            }

            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            Invalidate();
        }

        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            SuspendLayout();
            Invalidate();
            ResumeLayout(true);
            base.OnSelectedIndexChanged(e);
        }

        //wndproc to refresh listbox on scroll
        private const int WM_VSCROLL = 0x115;
        private const int WM_MSCROLL = 0x20A;

        // Mouse button (for multiselect support)
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_LBUTTONDBLCLK = 0x0203;

        protected override void WndProc(ref Message m)
        {
            if ((m.Msg == WM_LBUTTONDOWN || m.Msg == WM_LBUTTONDBLCLK)
                && (SelectionMode == SelectionMode.MultiSimple || SelectionMode == SelectionMode.MultiExtended))
            {
                if (!Focused)
                {
                    Focus();
                }

                long lParam = m.LParam.ToInt64();
                int x = (short)(lParam & 0xFFFF);
                int y = (short)((lParam >> 16) & 0xFFFF);

                OnMouseDown(new MouseEventArgs(MouseButtons.Left, 1, x, y, 0));
                return;
            }

            base.WndProc(ref m);

            if (m.Msg == WM_VSCROLL || m.Msg == WM_MSCROLL)
            {
                SuspendLayout();
                Invalidate();
                ResumeLayout(true);
            }
        }

        private void cuiListbox_MouseLeave(object sender, EventArgs e)
        {
            if (ClientRectangle.Contains(Cursor.Position) == false)
            {
                privateHoveredIndex = -1;
            }
        }
    }
}