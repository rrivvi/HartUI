using CuoreUI.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CuoreUI.Controls
{
    [ToolboxBitmap(typeof(TabControl))]
    public partial class cuiTabControl : UserControl
    {
        private bool hoveringInteractive = false;
        public List<TabPage> Pages = new List<TabPage>();
        private int _selectedIndex = -1;

        private bool privateShowPlus = true;
        public bool ShowPlus
        {
            get { return privateShowPlus; }
            set
            {
                privateShowPlus = value;
                Invalidate();
            }
        }

        private bool privateShowDelete = true;
        public bool ShowDelete
        {
            get { return privateShowDelete; }
            set
            {
                privateShowDelete = value;
                Invalidate();
            }
        }

        private int privateRounding = 8;
        public int Rounding
        {
            get { return privateRounding; }
            set
            {
                privateRounding = value;
                Invalidate();
            }
        }

        public cuiTabControl()
        {
            InitializeComponent();
            DoubleBuffered = true;
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);

            MouseWheel += CustomTabControl_MouseWheel;
            MouseDown += tb_MouseDown;
            MouseUp += tb_MouseUp;
            MouseMove += tb_MouseMove;
        }

        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set { SelectTab(value); }
        }

        public TabPage SelectedTab
        {
            get
            {
                if (_selectedIndex < 0 || _selectedIndex >= Pages.Count)
                {
                    return null;
                }

                return Pages[_selectedIndex];
            }
            set
            {
                SelectTab(value);
            }
        }

        private int scrollOffset = 0;
        private int tabHeight = 42;

        [Browsable(true)]
        [Category("CuoreUI")]
        [Description("Height of the tab headers.")]
        public int TabHeight
        {
            get { return tabHeight; }
            set
            {
                tabHeight = Math.Max(16, value);
                Invalidate();
            }
        }

        private int tabWidth = 104;

        [Browsable(true)]
        [Category("CuoreUI")]
        [Description("Minimum width of the tab headers.")]
        public int TabWidth
        {
            get { return tabWidth; }
            set
            {
                tabWidth = Math.Max(16, value);
                Invalidate();
            }
        }

        private const int ScrollSpeed = 80;
        private const int CloseBoxSize = 16;
        private const int TabPadding = 6;

        public int ScrollbarHeight { get; set; } = 8;
        public Rectangle scrollbarThumbRect;
        private bool draggingThumb = false;
        private int dragOffsetX;

        private string namingConvention = "TabPage";

        [Category("CuoreUI")]
        public int ScrollOffset
        {
            get { return scrollOffset; }
            set
            {
                scrollOffset = Math.Max(0, value);
                Invalidate();
            }
        }

        [Category("CuoreUI")]
        public string NamingConvention
        {
            get { return namingConvention; }
            set { namingConvention = value ?? "TabPage"; }
        }

        public string GetUniqueTabName()
        {
            int i = 1;
            while (true)
            {
                string name = $"{namingConvention}{i}";
                if (!Pages.Exists(p => p.Title == name))
                {
                    return name;
                }
                i++;
            }
        }

        public TabPage AddTab(Bitmap image, bool DisposeImageOnDisposal = false) => AddTab(GetUniqueTabName(), image, DisposeImageOnDisposal);
        public TabPage AddTab(bool DisposeImageOnDisposal) => AddTab(GetUniqueTabName(), null, DisposeImageOnDisposal);
        public TabPage AddTab() => AddTab(GetUniqueTabName());

        public TabPage AddTab(string name, Bitmap image = null, bool DisposeImageOnDisposal = false)
        {
            TabPage page = new TabPage();
            page.DisposeImageOnDisposal = DisposeImageOnDisposal;
            page.Image = image;
            page.Title = name;
            page.Dock = DockStyle.Bottom;
            page.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            page.Height = Height - TabHeight - ScrollbarHeight;
            page.Width = Width;
            page.Top = TabHeight + ScrollbarHeight;
            page.BackColor = BackColor;
            Pages.Add(page);
            TabAdded?.Invoke(this, new TabAddedEventArgs(page));

            if (_selectedIndex == -1)
            {
                SelectTab(0);
            }

            scrollAlpha = 255;

            Invalidate();
            return page;
        }

        public event EventHandler<TabAddedEventArgs> TabAdded;
        public event EventHandler SelectedTabChanged;
        public event EventHandler<int> TabRemoved;

        public void RemoveTab(int index)
        {
            if (index < 0 || index >= Pages.Count)
            {
                return;
            }

            bool removedSelected = index == _selectedIndex;

            Pages.RemoveAt(index);
            TabRemoved?.Invoke(this, index);

            if (Pages.Count == 0)
            {
                Controls.Clear();
                _selectedIndex = -1;
            }
            else if (removedSelected)
            {
                SelectTab(Math.Min(index, Pages.Count - 1));
            }
            else if (index < _selectedIndex)
            {
                _selectedIndex--;
            }

            Invalidate();
        }

        public void SelectTab(int index)
        {
            if (index < 0 || index >= Pages.Count || index == _selectedIndex)
            {
                return;
            }

            Controls.Clear();
            var page = Pages[index];
            Controls.Add(page);
            _selectedIndex = index;
            SelectedTabChanged?.Invoke(this, EventArgs.Empty);
            Invalidate();
        }

        public void SelectTab(TabPage page)
        {
            if (!Pages.Contains(page))
            {
                return;
            }

            SelectTab(Pages.IndexOf(page));
        }

        public Size ImageExpand { get; set; } = new Size(-4, -4);
        public Size TextOffset { get; set; } = new Size(0, 0);

        public Color SelectedImageTint { get; set; } = Color.Black;
        public Color UnselectedImageTint { get; set; } = Color.FromArgb(64, 64, 64);
        public Color HoverImageTint { get; set; } = Color.FromArgb(32, 32, 32);

        public Color UnselectedBackgroundColor { get; set; } = Color.FromArgb(244, 244, 244);
        public Color SelectedBackgroundColor { get; set; } = Color.White;
        public Color HoverBackgroundColor { get; set; } = Color.FromArgb(252, 252, 252);

        public Color SelectedTextColor { get; set; } = Color.Black;
        public Color UnselectedTextColor { get; set; } = Color.FromArgb(64, 64, 64);
        public Color HoverTextColor { get; set; } = Color.FromArgb(32, 32, 32);

        public Color PlusColor { get; set; } = Color.Gray;

        public Color UnselectedDeleteColor { get; set; } = Color.Gray;
        public Color SelectedDeleteColor { get; set; } = Color.Crimson;
        public Color HoverDeleteColor { get; set; } = Color.FromArgb(32, 32, 32);

        public Color ScrollbarColor { get; set; } = Color.FromArgb(128, 128, 128);

        private SizeF cachedTextSize = SizeF.Empty;

        // measure only once for font height
        private void MeasureTextSize(Graphics g)
        {
            if (cachedTextSize == SizeF.Empty)
                cachedTextSize = g.MeasureString("A", Font);
        }

        private int GetImageDrawSize()
        {
            int size = Math.Max(16, Math.Min(24, TabHeight - 10));
            Rectangle r = new Rectangle(0, 0, size, size);
            r.Inflate(ImageExpand.Width, ImageExpand.Height);

            if (r.Width < 1) r.Width = 1;
            if (r.Height < 1) r.Height = 1;

            return r.Width;
        }

        private int GetTabWidth(TabPage page)
        {
            int width = TabPadding * 2;

            if (page != null && page.Image != null)
            {
                width += GetImageDrawSize() + 4;
            }

            string text = page == null ? string.Empty : (page.Title ?? string.Empty);
            width += TextRenderer.MeasureText(
                text,
                Font,
                Size.Empty,
                TextFormatFlags.SingleLine | TextFormatFlags.NoPadding
            ).Width;

            if (ShowDelete)
            {
                width += CloseBoxSize + 14;
            }

            return Math.Max(TabWidth, width);
        }

        private int GetTabsTotalWidth()
        {
            int width = 0;
            for (int i = 0; i < Pages.Count; i++)
            {
                width += GetTabWidth(Pages[i]);
                width += TabPadding;
            }
            return width;
        }

        private Rectangle GetCloseRect(Rectangle tabRect)
        {
            return new Rectangle(
                tabRect.Right - CloseBoxSize - 10,
                tabRect.Top + (TabHeight - CloseBoxSize) / 2 + 1,
                CloseBoxSize - 4,
                CloseBoxSize - 4
            );
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(BackColor);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            int tabsTotalWidth = GetTabsTotalWidth();
            int totalWidth = tabsTotalWidth + (ShowPlus ? TabHeight : 0);

            if (totalWidth < Width)
            {
                scrollOffset = 0;
            }
            else
            {
                scrollOffset = Math.Max(0, Math.Min(scrollOffset, totalWidth - Width));
            }

            MeasureTextSize(g);

            using (var unselectedBackgroundBrush = new SolidBrush(UnselectedBackgroundColor))
            using (var selectedBackgroundBrush = new SolidBrush(SelectedBackgroundColor))
            using (var hoverBackgroundBrush = new SolidBrush(HoverBackgroundColor))
            using (var selectedTextBrush = new SolidBrush(SelectedTextColor))
            using (var unselectedTextBrush = new SolidBrush(UnselectedTextColor))
            using (var hoverTextBrush = new SolidBrush(HoverTextColor))
            {
                int x = 0;

                for (int i = 0; i < Pages.Count; i++)
                {
                    var currentPage = Pages[i];
                    int currentTabWidth = GetTabWidth(currentPage);
                    int drawX = x - scrollOffset;

                    if (drawX + currentTabWidth < 0 || drawX > Width)
                    {
                        x += currentTabWidth + TabPadding;
                        continue;
                    }

                    var tabRect = new Rectangle(drawX, 0, currentTabWidth, TabHeight);
                    bool isSelected = i == _selectedIndex;
                    bool isHover = i == _hoverIndex && !isSelected;

                    using (var path = GeneralHelper.RoundRect(tabRect, Rounding))
                    {
                        g.FillPath(isSelected ? selectedBackgroundBrush :
                                      isHover ? hoverBackgroundBrush :
                                      unselectedBackgroundBrush, path);
                    }

                    int contentLeft = tabRect.Left + TabPadding;
                    int contentRight = tabRect.Right - TabPadding;

                    if (ShowDelete)
                    {
                        contentRight -= CloseBoxSize + 10;
                    }

                    if (currentPage.Image != null)
                    {
                        using (Bitmap currentImage = DrawingHelper.Imaging.TintBitmap(
                            currentPage.Image,
                            isSelected ? SelectedImageTint :
                            isHover ? HoverImageTint :
                            UnselectedImageTint))
                        {
                            int imageSize = GetImageDrawSize();
                            Rectangle imageRect = new Rectangle(
                                contentLeft,
                                tabRect.Top + (TabHeight - imageSize) / 2,
                                imageSize,
                                imageSize
                            );

                            g.DrawImage(currentImage, imageRect);

                            contentLeft += imageSize + 4;
                        }
                    }

                    string title = currentPage.Title ?? string.Empty;
                    Rectangle textRect = new Rectangle(
                        contentLeft + TextOffset.Width,
                        tabRect.Top + TextOffset.Height,
                        Math.Max(0, contentRight - contentLeft - TextOffset.Width),
                        TabHeight
                    );

                    TextRenderer.DrawText(
                        g,
                        title,
                        Font,
                        textRect,
                        isSelected ? SelectedTextColor : isHover ? HoverTextColor : UnselectedTextColor,
                        TextFormatFlags.SingleLine |
                        TextFormatFlags.VerticalCenter |
                        TextFormatFlags.Left |
                        TextFormatFlags.EndEllipsis |
                        TextFormatFlags.NoPadding
                    );

                    if (ShowDelete)
                    {
                        var closeRect = GetCloseRect(tabRect);

                        using (Pen ClosePen = new Pen(isSelected ? SelectedDeleteColor : isHover ? HoverDeleteColor : UnselectedDeleteColor) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                        using (var closePath = GeneralHelper.Crossmark(closeRect))
                        {
                            g.DrawPath(ClosePen, closePath);
                        }
                    }

                    x += currentTabWidth + TabPadding;
                }
            }

            void DrawGradient(Rectangle rect, bool reverse = false)
            {
                using (var brush = new LinearGradientBrush(
                    rect,
                    reverse ? BackColor : Color.Transparent,
                    reverse ? Color.Transparent : BackColor,
                    LinearGradientMode.Horizontal))
                {
                    g.FillRectangle(brush, rect);
                }
            }

            bool isOverfilled = totalWidth > Width;
            if (isOverfilled)
            {
                float visibleRatio = Width / (float)totalWidth;
                int thumbWidth = Math.Max(30, (int)(Width * visibleRatio));
                int maxThumbPos = Math.Max(0, Width - thumbWidth);
                int thumbX = 0;

                if (totalWidth > Width && maxThumbPos > 0)
                {
                    thumbX = (int)(scrollOffset / (float)(totalWidth - Width) * maxThumbPos);
                }

                scrollbarThumbRect = new Rectangle(thumbX, TabHeight + ScrollbarHeight / 2 - 3, thumbWidth - 1, ScrollbarHeight);

                using (var scrollBrush = new SolidBrush(Color.FromArgb(scrollAlpha, ScrollbarColor)))
                using (var scrollPath = GeneralHelper.RoundRect(scrollbarThumbRect, ScrollbarHeight / 2 - 1))
                {
                    g.FillPath(scrollBrush, scrollPath);

                    using (SolidBrush UnselectedTextBrush = new SolidBrush(UnselectedTextColor))
                        if (scrollOffset < totalWidth - Width)
                        {
                            DrawGradient(new Rectangle(Width - 30, 0, 32, TabHeight + ScrollbarHeight));
                            using (GraphicsPath rightScrollArrow = GeneralHelper.LeftArrow(new Rectangle(Width - 6, TabHeight + ScrollbarHeight / 2 - 3, 6, 7)))
                            using (Matrix matrix = new Matrix())
                            {
                                RectangleF bounds = rightScrollArrow.GetBounds();
                                matrix.Translate(-bounds.X - bounds.Width / 2, 0, MatrixOrder.Append);
                                matrix.Scale(-1, 1, MatrixOrder.Append);
                                matrix.Translate(bounds.X + bounds.Width / 2, 0, MatrixOrder.Append);
                                rightScrollArrow.Transform(matrix);

                                g.FillPath(UnselectedTextBrush, rightScrollArrow);
                            }
                        }

                    if (scrollOffset > 0)
                    {
                        DrawGradient(new Rectangle(-2, 0, 33, TabHeight + ScrollbarHeight), true);

                        using (SolidBrush UnselectedTextBrush = new SolidBrush(UnselectedTextColor))
                        using (GraphicsPath leftScrollArrow = GeneralHelper.LeftArrow(new Rectangle(0, TabHeight + ScrollbarHeight / 2 - 3, 6, 7)))
                        {
                            g.FillPath(UnselectedTextBrush, leftScrollArrow);
                        }
                    }
                }
            }
            else
            {
                scrollbarThumbRect = Rectangle.Empty;
            }

            if (ShowPlus)
            {
                e.Graphics.PixelOffsetMode = PixelOffsetMode.Default;
                int addX = tabsTotalWidth - scrollOffset;
                var addRect = new Rectangle(addX, 0, TabHeight, TabHeight);

                using (Pen plusPen = new Pen(PlusColor))
                using (GraphicsPath plus = GeneralHelper.Plus(new Rectangle(addRect.Left + addRect.Width / 2 - 6, (int)(addRect.Top + (TabHeight - cachedTextSize.Height) / 2), 12, 12)))
                {
                    g.DrawPath(plusPen, plus);
                }
            }
        }

        private int _hoverIndex = -1;

        private void CustomTabControl_MouseWheel(object sender, MouseEventArgs e)
        {
            int totalWidth = GetTabsTotalWidth() + (ShowPlus ? TabHeight : 0);
            if (totalWidth <= Width)
            {
                return;
            }

            scrollOffset -= e.Delta > 0 ? ScrollSpeed : -ScrollSpeed;
            Invalidate();
        }

        protected override bool IsInputKey(Keys keyData)
        {
            return keyData is Keys.Left || keyData is Keys.Right || base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left)
            {
                scrollOffset -= ScrollSpeed;
                Invalidate();
            }
            else if (e.KeyCode == Keys.Right)
            {
                scrollOffset += ScrollSpeed;
                Invalidate();
            }

            base.OnKeyDown(e);
        }

        private void tb_MouseDown(object sender, MouseEventArgs e)
        {
            int x = 0;

            for (int i = 0; i < Pages.Count; i++)
            {
                int currentTabWidth = GetTabWidth(Pages[i]);
                var tabRect = new Rectangle(x - scrollOffset, 0, currentTabWidth, TabHeight);

                if (!tabRect.Contains(e.Location))
                {
                    x += currentTabWidth + TabPadding;
                    continue;
                }

                if (ShowDelete)
                {
                    var closeRect = GetCloseRect(tabRect);

                    if (closeRect.Contains(e.Location))
                    {
                        RemoveTab(i);
                        if (_selectedIndex == i)
                        {
                            SelectTab(Math.Min(i, Pages.Count - 1));
                        }
                        return;
                    }
                }

                SelectTab(i);
                return;
            }

            if (ShowPlus)
            {
                int tabsTotalWidth = GetTabsTotalWidth();
                var addRect = new Rectangle(tabsTotalWidth - scrollOffset, 0, TabHeight, TabHeight);
                if (addRect.Contains(e.Location))
                {
                    SelectTab(AddTab());
                    return;
                }
            }

            Rectangle expandedThumbRect = scrollbarThumbRect;
            expandedThumbRect.Inflate(2, 2);
            if (expandedThumbRect.Contains(e.Location))
            {
                draggingThumb = true;
                dragOffsetX = e.X - scrollbarThumbRect.X;
            }
        }

        private void tb_MouseUp(object sender, MouseEventArgs e)
        {
            draggingThumb = false;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (_hoverIndex != -1)
            {
                _hoverIndex = -1;
                Invalidate();
            }
        }

        private void tb_MouseMove(object sender, MouseEventArgs e)
        {
            int newHover = -1;
            bool newHoveringInteractive = false;

            int x = 0;
            for (int i = 0; i < Pages.Count; i++)
            {
                int currentTabWidth = GetTabWidth(Pages[i]);
                var tabRect = new Rectangle(x - scrollOffset, 0, currentTabWidth, TabHeight);

                if (tabRect.Contains(e.Location))
                {
                    newHover = i;
                    if (ShowDelete)
                    {
                        var closeRect = GetCloseRect(tabRect);

                        if (closeRect.Contains(e.Location))
                            newHoveringInteractive = true;
                    }
                    break;
                }

                x += currentTabWidth + TabPadding;
            }

            if (ShowPlus)
            {
                int tabsTotalWidth = GetTabsTotalWidth();
                var addRect = new Rectangle(tabsTotalWidth - scrollOffset, 0, TabHeight, TabHeight);
                if (addRect.Contains(e.Location))
                {
                    newHover = Pages.Count;
                    newHoveringInteractive = true;
                }
            }

            if (newHover != _hoverIndex)
            {
                _hoverIndex = newHover;
                Invalidate();
            }

            if (newHoveringInteractive != hoveringInteractive)
            {
                hoveringInteractive = newHoveringInteractive;
                Cursor = hoveringInteractive ? Cursors.Hand : Cursors.Default;
            }

            if (draggingThumb)
            {
                int totalWidth = GetTabsTotalWidth() + (ShowPlus ? TabHeight : 0);
                int thumbWidth = scrollbarThumbRect.Width;
                int maxThumbX = Math.Max(0, Width - thumbWidth);

                if (maxThumbX > 0 && totalWidth > Width)
                {
                    int newThumbX = Math.Max(0, Math.Min(e.X - dragOffsetX, maxThumbX));
                    float ratio = newThumbX / (float)maxThumbX;
                    scrollOffset = (int)((totalWidth - Width) * ratio);
                }

                Invalidate();
            }
            else
            {
                Rectangle expandedScrollRect = scrollbarThumbRect;
                expandedScrollRect.Inflate(2, 2);
                if (expandedScrollRect.Contains(e.Location))
                    scrollAlpha = 255;
                Invalidate(scrollbarThumbRect);
            }
        }

        bool scrollbarUntouchable = false;
        byte scrollAlpha = 0;

        private void scrollbarTimer_Tick(object sender, EventArgs e)
        {
            if (scrollAlpha == 0)
            {
                return;
            }

            if (draggingThumb || scrollbarUntouchable || scrollbarThumbRect.Contains(PointToClient(Cursor.Position)))
            {
                scrollAlpha = 255;
            }
            else
            {
                scrollAlpha = (byte)(((scrollAlpha * 80) + 128) / 85);
            }

            Rectangle expandedThumbRect = scrollbarThumbRect;
            expandedThumbRect.Inflate(2, 2);
            Invalidate(expandedThumbRect);
        }
    }

    [ToolboxItem(false)]
    public class TabPage : Panel
    {
        public bool DisposeImageOnDisposal = false;
        public string Title { get; set; }
        public Bitmap Image { get; set; }

        public TabPage()
        {
            BackColor = Color.White;
        }

        protected override void Dispose(bool disposing)
        {
            if (DisposeImageOnDisposal)
            {
                Image?.Dispose();
                Image = null;
            }

            base.Dispose(disposing);
        }
    }

    public class TabAddedEventArgs : EventArgs
    {
        public TabAddedEventArgs(TabPage addedPage)
        {
            Tab = addedPage;
        }

        public TabPage Tab { get; set; }
    }
}