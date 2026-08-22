using HartUI.Helpers;
using HartUI.Misc.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HartUI.Controls
{
    [Designer(typeof(DesignerIntegration.HartControlDesigner))]
    [ToolboxBitmap(typeof(ListBox))]
    public partial class cuiListbox : Control
    {
        public cuiListbox()
        {
            SetStyle(ControlStyles.UserPaint
                | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.ResizeRedraw
                | ControlStyles.Selectable
                | ControlStyles.SupportsTransparentBackColor, true);

            UpdateStyles();
            DoubleBuffered = true;
            TabStop = true;

            Items = new ObjectCollection(this);

            ForeColor = Color.FromArgb(84, 84, 84);
            SelectionMode = SelectionMode.One;

            InitializeComponent();

            autoScrollTimer = new Timer { Interval = autoScrollInterval };
            autoScrollTimer.Tick += AutoScrollTimer_Tick;
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

        private Timer autoScrollTimer;
        private int autoScrollDelta = 0;
        private const int autoScrollInterval = 30;
        private const int autoScrollMaxStep = 40;

        [Category("HartUI")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor("System.Windows.Forms.Design.ListControlStringCollectionEditor", typeof(UITypeEditor))]
        [MergableProperty(false)]
        public ObjectCollection Items { get; }

        private int privateItemHeight = 38;

        [Category("HartUI")]
        public int ItemHeight
        {
            get
            {
                return privateItemHeight;
            }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "ItemHeight must be greater than zero.");
                }

                if (privateItemHeight != value)
                {
                    privateItemHeight = value;
                    ClampScrollOffset();
                    Invalidate();
                }
            }
        }

        private SelectionMode privateSelectionMode = SelectionMode.One;

        [Category("HartUI")]
        public SelectionMode SelectionMode
        {
            get
            {
                return privateSelectionMode;
            }
            set
            {
                if (privateSelectionMode != value)
                {
                    privateSelectionMode = value;
                    ClearSelected();
                }
            }
        }

        private readonly HashSet<int> privateSelectedIndices = new HashSet<int>();

        public event EventHandler SelectedIndexChanged;

        protected virtual void OnSelectedIndexChanged(EventArgs e)
        {
            SelectedIndexChanged?.Invoke(this, e);
        }

        public bool GetSelected(int index)
        {
            return privateSelectedIndices.Contains(index);
        }

        private void SetSelectedInternal(int index, bool value)
        {
            if (index < 0 || index >= Items.Count)
            {
                return;
            }

            if (value)
            {
                privateSelectedIndices.Add(index);
            }
            else
            {
                privateSelectedIndices.Remove(index);
            }
        }

        public void SetSelected(int index, bool value)
        {
            if (index < 0 || index >= Items.Count)
            {
                return;
            }

            if (SelectionMode == SelectionMode.One)
            {
                privateSelectedIndices.Clear();
                if (value)
                {
                    privateSelectedIndices.Add(index);
                }
            }
            else
            {
                SetSelectedInternal(index, value);
            }

            OnSelectedIndexChanged(EventArgs.Empty);
            Invalidate();
        }

        public void ClearSelected()
        {
            if (privateSelectedIndices.Count == 0)
            {
                return;
            }

            privateSelectedIndices.Clear();
            OnSelectedIndexChanged(EventArgs.Empty);
            Invalidate();
        }

        public int[] SelectedIndices
        {
            get
            {
                List<int> indices = new List<int>(privateSelectedIndices);
                indices.Sort();
                return indices.ToArray();
            }
        }

        public int SelectedIndex
        {
            get
            {
                int min = -1;

                foreach (int i in privateSelectedIndices)
                {
                    if (min == -1 || i < min)
                    {
                        min = i;
                    }
                }

                return min;
            }
            set
            {
                if (value < -1 || value >= Items.Count)
                {
                    return;
                }

                privateSelectedIndices.Clear();

                if (value >= 0)
                {
                    privateSelectedIndices.Add(value);
                }

                privateFocusedIndex = value;

                OnSelectedIndexChanged(EventArgs.Empty);
                Invalidate();
            }
        }

        public object SelectedItem
        {
            get
            {
                int index = SelectedIndex;
                return index >= 0 && index < Items.Count ? Items[index] : null;
            }
        }

        private void SelectRange(int start, int end)
        {
            privateSelectedIndices.Clear();

            int lo = Math.Max(0, Math.Min(start, end));
            int hi = Math.Min(Items.Count - 1, Math.Max(start, end));

            for (int i = lo; i <= hi; i++)
            {
                SetSelectedInternal(i, true);
            }

            OnSelectedIndexChanged(EventArgs.Empty);
            Invalidate();
        }

        private void ExtendRange(int start, int end)
        {
            int lo = Math.Max(0, Math.Min(start, end));
            int hi = Math.Min(Items.Count - 1, Math.Max(start, end));

            for (int i = lo; i <= hi; i++)
            {
                SetSelectedInternal(i, true);
            }

            OnSelectedIndexChanged(EventArgs.Empty);
            Invalidate();
        }

        // called when items are added / removed / replaced
        internal void OnItemsChanged()
        {
            privateSelectedIndices.RemoveWhere(i => i < 0 || i >= Items.Count);

            if (privateFocusedIndex >= Items.Count)
            {
                privateFocusedIndex = Items.Count - 1;
            }

            if (privateAnchorIndex >= Items.Count)
            {
                privateAnchorIndex = Items.Count - 1;
            }

            ClampScrollOffset();
            Invalidate();
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
        private int privateFocusedIndex = -1;
        private bool showKeyboardFocus = false;

        private bool privateIsPreviewDragging = false;
        private int privatePreviewStart = -1;
        private int privatePreviewEnd = -1;

        private const int scrollbarWidth = 8;

        private int scrollOffset = 0;
        private bool dragging = false;
        private int dragStartY = 0;
        private int scrollStartOffset = 0;

        private int GetContentHeight()
        {
            return Items.Count * ItemHeight;
        }

        private bool IsOverflowing
        {
            get
            {
                return GetContentHeight() > ClientSize.Height;
            }
        }

        private int GetMaxScroll()
        {
            return Math.Max(0, GetContentHeight() - ClientSize.Height + 4);
        }

        private void ClampScrollOffset()
        {
            int maxScroll = GetMaxScroll();
            scrollOffset = Math.Max(0, Math.Min(maxScroll, scrollOffset));
        }

        private int BarHeight()
        {
            int content = GetContentHeight();
            int height = ClientSize.Height;

            if (content <= height || content <= 0)
            {
                return height;
            }

            float pct = (float)height / content;
            return Math.Max(20, (int)(height * pct));
        }

        private int GetScrollGutterStart()
        {
            return ClientSize.Width - scrollbarWidth - 4;
        }

        private Rectangle GetScrollbarRect()
        {
            int h = BarHeight();
            int maxScroll = GetMaxScroll();
            int y = 0;

            if (maxScroll > 0)
            {
                float pct = (float)scrollOffset / maxScroll;
                y = (int)((ClientSize.Height - h) * pct);
            }

            return new Rectangle(GetScrollGutterStart(), y + 4, scrollbarWidth, Math.Max(12, h - 8));
        }

        private void EnsureItemVisible(int index)
        {
            if (index < 0 || index >= Items.Count)
            {
                return;
            }

            int itemTop = index * ItemHeight;
            int itemBottom = itemTop + ItemHeight + 4;

            if (itemTop < scrollOffset)
            {
                scrollOffset = itemTop;
            }
            else if (itemBottom > scrollOffset + ClientSize.Height)
            {
                scrollOffset = itemBottom - ClientSize.Height;
            }

            ClampScrollOffset();
        }

        private int GetIndexFromPoint(Point pt)
        {
            if (Items.Count == 0 || !ClientRectangle.Contains(pt))
            {
                return -1;
            }

            if (IsOverflowing && pt.X >= GetScrollGutterStart())
            {
                return -1;
            }

            int index = (pt.Y + scrollOffset) / ItemHeight;
            return index >= 0 && index < Items.Count ? index : -1;
        }

        private int GetClampedIndexFromPoint(Point pt)
        {
            if (Items.Count == 0)
            {
                return -1;
            }

            int y = Math.Max(0, Math.Min(ClientSize.Height - 1, pt.Y));
            int index = (y + scrollOffset) / ItemHeight;
            return Math.Max(0, Math.Min(Items.Count - 1, index));
        }

        private void UpdateDragSelectionFromPoint(Point pt)
        {
            int index = GetClampedIndexFromPoint(pt);

            if (index < 0)
            {
                return;
            }

            if (SelectionMode == SelectionMode.One)
            {
                SelectedIndex = index;
                privateAnchorIndex = index;
            }
            else if (SelectionMode == SelectionMode.MultiExtended && privateIsPreviewDragging)
            {
                privatePreviewEnd = index;
            }

            privateFocusedIndex = index;
            EnsureItemVisible(index);
        }

        private void UpdateAutoScroll(int mouseY)
        {
            int overflowTop = -mouseY;
            int overflowBottom = mouseY - ClientSize.Height;

            if (overflowTop > 0)
            {
                StartAutoScroll(-GetAutoScrollStep(overflowTop));
            }
            else if (overflowBottom > 0)
            {
                StartAutoScroll(GetAutoScrollStep(overflowBottom));
            }
            else
            {
                StopAutoScroll();
            }
        }

        private static int GetAutoScrollStep(int overflow)
        {
            // scales with overflow
            int step = 6 + (overflow / 2);
            return Math.Min(autoScrollMaxStep, step);
        }

        private void StartAutoScroll(int delta)
        {
            autoScrollDelta = delta;

            if (!autoScrollTimer.Enabled)
            {
                autoScrollTimer.Start();
            }
        }

        private void StopAutoScroll()
        {
            if (autoScrollTimer.Enabled)
            {
                autoScrollTimer.Stop();
            }
            autoScrollDelta = 0;
        }

        private void AutoScrollTimer_Tick(object sender, EventArgs e)
        {
            if (autoScrollDelta == 0)
            {
                StopAutoScroll();
                return;
            }

            int previousOffset = scrollOffset;
            scrollOffset += autoScrollDelta;
            ClampScrollOffset();

            if (scrollOffset == previousOffset)
            {
                return;
            }

            UpdateDragSelectionFromPoint(PointToClient(Cursor.Position));
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

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

                g.FillPath(itemBrush, path2);

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

                    bool isOverflowing = IsOverflowing;
                    int itemAreaWidth = isOverflowing ? ClientSize.Width - scrollbarWidth - 3 : ClientSize.Width;

                    int first = scrollOffset / ItemHeight;
                    int firstItemPixelOffset = scrollOffset % ItemHeight;
                    int visibleCount = (ClientSize.Height / ItemHeight) + 2;
                    int last = Math.Min(Items.Count, first + visibleCount);

                    for (int i = first; i < last; i++)
                    {
                        int y = (i - first) * ItemHeight - firstItemPixelOffset;

                        Rectangle itemRect = new Rectangle(0, y, itemAreaWidth, ItemHeight);
                        itemRect.Inflate(-4, -2);
                        itemRect.Offset(0, 2);

                        int yCenterString = -2 + itemRect.Y + (ItemHeight - Font.Height) / 2;
                        string itemText = Items[i]?.ToString() ?? string.Empty;

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

                            if (Focused && showKeyboardFocus && privateFocusedIndex == i)
                            {
                                Rectangle focusRect = itemRect;
                                focusRect.Inflate(-1, -1);

                                using (GraphicsPath focusPath = GeneralHelper.RoundRect(focusRect, ItemRounding > 0 ? ItemRounding : 4))
                                using (Pen focusPen = new Pen(ItemHoverForegroundColor, 1))
                                {
                                    g.DrawPath(focusPen, focusPath);
                                }
                            }
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

                    if (isOverflowing)
                    {
                        using (SolidBrush scrollbarBrush = new SolidBrush(Color.FromArgb(96, 128, 128, 128)))
                        using (GraphicsPath scrollbarPath = GeneralHelper.RoundRect(GetScrollbarRect(), 4))
                        {
                            g.FillPath(scrollbarBrush, scrollbarPath);
                        }
                    }
                }

                // Reset the clip after drawing
                g.ResetClip();

                // draw the outline after everything, so that
                // the items dont render above the outline
                g.PixelOffsetMode = PixelOffsetMode.Default;
                g.DrawPath(bgPen, path2);
            }

            base.OnPaint(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (!Focused)
            {
                Focus();
            }

            showKeyboardFocus = false;

            if (e.Button != MouseButtons.Left)
            {
                base.OnMouseDown(e);
                return;
            }

            if (IsOverflowing && GetScrollbarRect().Contains(e.Location))
            {
                dragging = true;
                dragStartY = e.Y;
                scrollStartOffset = scrollOffset;
                Capture = true;
                Invalidate();
                base.OnMouseDown(e);
                return;
            }

            int index = GetIndexFromPoint(e.Location);

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
                        Capture = true;
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

                privateFocusedIndex = index;
                EnsureItemVisible(index);
                Invalidate();
                ResumeLayout(true);
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            StopAutoScroll();

            if (dragging)
            {
                dragging = false;
                Capture = false;
                Invalidate();
                base.OnMouseUp(e);
                return;
            }

            if (privateIsPreviewDragging)
            {
                SuspendLayout();

                SelectRange(privatePreviewStart, privatePreviewEnd);
                privateAnchorIndex = privatePreviewStart;
                privateFocusedIndex = privatePreviewEnd;

                privateIsPreviewDragging = false;
                privatePreviewStart = -1;
                privatePreviewEnd = -1;
                Capture = false;

                Invalidate();
                ResumeLayout(true);
            }
            else if (Capture)
            {
                Capture = false;
            }

            base.OnMouseUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            showKeyboardFocus = false;

            if (dragging)
            {
                int maxScroll = GetMaxScroll();
                int trackHeight = ClientSize.Height - BarHeight();

                if (trackHeight > 0 && maxScroll > 0)
                {
                    int dy = e.Y - dragStartY;
                    float pct = (float)dy / trackHeight;
                    scrollOffset = (int)(scrollStartOffset + pct * maxScroll);
                    ClampScrollOffset();
                }

                Invalidate();
                base.OnMouseMove(e);
                return;
            }

            bool isDragSelecting = e.Button == MouseButtons.Left
                && (SelectionMode == SelectionMode.One
                    || (SelectionMode == SelectionMode.MultiExtended && privateIsPreviewDragging));

            if (isDragSelecting && IsOverflowing)
            {
                UpdateAutoScroll(e.Y);
            }
            else
            {
                StopAutoScroll();
            }

            privateHoveredIndex = GetIndexFromPoint(e.Location);

            if (isDragSelecting)
            {
                UpdateDragSelectionFromPoint(e.Location);
            }

            Invalidate();
            base.OnMouseMove(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            Invalidate();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            int maxScroll = GetMaxScroll();

            if (maxScroll > 0)
            {
                int linesPerNotch = SystemInformation.MouseWheelScrollLines;
                if (linesPerNotch <= 0)
                {
                    linesPerNotch = 3;
                }

                int notches = e.Delta / 120;
                scrollOffset -= notches * linesPerNotch * ItemHeight;
                ClampScrollOffset();

                privateHoveredIndex = GetIndexFromPoint(PointToClient(Cursor.Position));

                Invalidate();

                if (e is HandledMouseEventArgs hme)
                {
                    hme.Handled = true;
                }
            }

            base.OnMouseWheel(e);
        }

        private void cuiListbox_MouseLeave(object sender, EventArgs e)
        {
            if (ClientRectangle.Contains(Cursor.Position) == false)
            {
                privateHoveredIndex = -1;
            }
        }

        private int PageSize()
        {
            return Math.Max(1, ClientSize.Height / ItemHeight);
        }

        private void ApplyKeyboardSelection(bool extendSelection)
        {
            switch (SelectionMode)
            {
                case SelectionMode.None:
                    break;

                case SelectionMode.One:
                    SelectedIndex = privateFocusedIndex;
                    privateAnchorIndex = privateFocusedIndex;
                    break;

                case SelectionMode.MultiSimple:
                    privateAnchorIndex = privateFocusedIndex;
                    break;

                case SelectionMode.MultiExtended:
                    if (extendSelection && privateAnchorIndex >= 0)
                    {
                        ExtendRange(privateAnchorIndex, privateFocusedIndex);
                    }
                    else
                    {
                        privateAnchorIndex = privateFocusedIndex;

                        bool ctrl = (ModifierKeys & Keys.Control) == Keys.Control;
                        if (!ctrl)
                        {
                            SelectRange(privateFocusedIndex, privateFocusedIndex);
                        }
                    }
                    break;
            }
        }

        private void SetFocusedIndex(int index, bool extendSelection)
        {
            if (Items.Count == 0)
            {
                return;
            }

            index = Math.Max(0, Math.Min(Items.Count - 1, index));

            showKeyboardFocus = true;
            privateFocusedIndex = index;

            ApplyKeyboardSelection(extendSelection);

            EnsureItemVisible(privateFocusedIndex);
            Invalidate();
        }

        private void MoveFocusedIndex(int delta, bool extendSelection)
        {
            if (Items.Count == 0)
            {
                return;
            }

            int start = privateFocusedIndex >= 0 ? privateFocusedIndex : Math.Max(0, SelectedIndex);
            SetFocusedIndex(start + delta, extendSelection);
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Up:
                    MoveFocusedIndex(-1, false);
                    return true;

                case Keys.Down:
                    MoveFocusedIndex(1, false);
                    return true;

                case Keys.Up | Keys.Shift:
                    MoveFocusedIndex(-1, true);
                    return true;

                case Keys.Down | Keys.Shift:
                    MoveFocusedIndex(1, true);
                    return true;

                case Keys.Home:
                    SetFocusedIndex(0, false);
                    return true;

                case Keys.End:
                    SetFocusedIndex(Items.Count - 1, false);
                    return true;

                case Keys.PageUp:
                    MoveFocusedIndex(-PageSize(), false);
                    return true;

                case Keys.PageDown:
                    MoveFocusedIndex(PageSize(), false);
                    return true;
            }

            return base.ProcessDialogKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space
                && (SelectionMode == SelectionMode.MultiSimple || SelectionMode == SelectionMode.MultiExtended)
                && privateFocusedIndex >= 0)
            {
                SetSelected(privateFocusedIndex, !GetSelected(privateFocusedIndex));
                e.Handled = true;
                return;
            }

            base.OnKeyDown(e);
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);

            showKeyboardFocus = InputManager.LastInputWasKeyboard;

            if (showKeyboardFocus && privateFocusedIndex < 0)
            {
                privateFocusedIndex = SelectedIndex >= 0 ? SelectedIndex : (Items.Count > 0 ? 0 : -1);
            }

            Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            Invalidate();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            ClampScrollOffset();
            Invalidate();
        }

        public class ObjectCollection : IList
        {
            private readonly cuiListbox owner;
            private readonly List<object> innerList = new List<object>();

            internal ObjectCollection(cuiListbox owner)
            {
                this.owner = owner;
            }

            public int Count => innerList.Count;
            public bool IsFixedSize => false;
            public bool IsReadOnly => false;
            public bool IsSynchronized => false;
            public object SyncRoot => this;

            public object this[int index]
            {
                get => innerList[index];
                set
                {
                    innerList[index] = value;
                    owner.OnItemsChanged();
                }
            }

            public int Add(object item)
            {
                innerList.Add(item);
                int index = innerList.Count - 1;
                owner.OnItemsChanged();
                return index;
            }

            public void AddRange(IEnumerable items)
            {
                foreach (object item in items)
                {
                    innerList.Add(item);
                }

                owner.OnItemsChanged();
            }

            public void Clear()
            {
                innerList.Clear();
                owner.OnItemsChanged();
            }

            public bool Contains(object item)
            {
                return innerList.Contains(item);
            }

            public void CopyTo(Array array, int index)
            {
                ((IList)innerList).CopyTo(array, index);
            }

            public IEnumerator GetEnumerator()
            {
                return innerList.GetEnumerator();
            }

            public int IndexOf(object item)
            {
                return innerList.IndexOf(item);
            }

            public void Insert(int index, object item)
            {
                innerList.Insert(index, item);
                owner.OnItemsChanged();
            }

            public void Remove(object item)
            {
                innerList.Remove(item);
                owner.OnItemsChanged();
            }

            public void RemoveAt(int index)
            {
                innerList.RemoveAt(index);
                owner.OnItemsChanged();
            }
        }
    }
}