using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace HartUI.Controls
{
    [Description("Bring back resizing for FormBorderStyle.None forms")]
    [ToolboxBitmap(typeof(Form))]

    public partial class cuiResizeGrip : UserControl
    {
        Point lastMousePoint = new Point(-1, -1);
        Form privateTargetForm;
        private GraphicsPath cachedGripPath;
        private bool gripPathDirty = true;

        [Category("HartUI")]
        public Form TargetForm
        {
            get
            {
                return privateTargetForm;
            }
            set
            {
                privateTargetForm = value;
            }
        }

        private Color privateGripColor = Color.Gray;

        [Category("HartUI")]
        public Color GripColor
        {
            get
            {
                return privateGripColor;
            }
            set
            {
                privateGripColor = value;
                Invalidate();
            }
        }

        private bool privateGripTexture = true;

        [Category("HartUI")]
        public bool GripTexture
        {
            get
            {
                return privateGripTexture;
            }
            set
            {
                privateGripTexture = value;
                Invalidate();
            }
        }

        Timer dragTimer = new Timer();

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        private const int VK_LBUTTON = 0x01;

        public cuiResizeGrip()
        {
            InitializeComponent();

            Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            dragTimer.Tick += DragTimer_Tick;
            Size = new Size(24, 24);
            Cursor = Cursors.SizeNWSE;

            Timer refreshTimer = new Timer();
            refreshTimer.Interval = 10000;
            refreshTimer.Start();
            refreshTimer.Tick += (e, s) =>
            {
                dragTimer.Interval = 10000 / Helpers.DrawingHelper.GetHighestRefreshRate();
            };
        }

        private void DragTimer_Tick(object sender, EventArgs e)
        {
            if (TargetForm != null)
            {
                bool isLeftButtonDown = (GetAsyncKeyState(VK_LBUTTON) & 0x8000) != 0;
                if (isLeftButtonDown == false)
                {
                    dragTimer.Stop();
                    return;
                }

                if (lastMousePoint == new Point(-1, -1))
                {
                    lastMousePoint = Cursor.Position;
                }

                Point currentMousePoint = Cursor.Position;
                Point mouseDelta = GetDelta(currentMousePoint, lastMousePoint);
                lastMousePoint = currentMousePoint;
                TargetForm.Size = Size.Subtract(TargetForm.Size, (Size)mouseDelta);

            }
        }

        private static Point GetDelta(Point p1, Point p2)
        {
            double deltaX = p2.X - p1.X;
            double deltaY = p2.Y - p1.Y;

            return new Point((int)deltaX, (int)deltaY);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            Focus();

            lastMousePoint = new Point(-1, -1);

            dragTimer.Interval = 1000 / Helpers.DrawingHelper.GetHighestRefreshRate();
            dragTimer.Stop();
            dragTimer.Start();
        }

        private Size privateTextureOffset = new Size(-2, -2);

        [Category("HartUI")]
        public Size TextureOffset
        {
            get
            {
                return privateTextureOffset;
            }
            set
            {
                privateTextureOffset = value;
                gripPathDirty = true;
                Invalidate();
            }
        }

        GraphicsPath GetGripPath()
        {
            if (!gripPathDirty && cachedGripPath != null)
                return cachedGripPath;

            cachedGripPath?.Dispose();

            int halfSize = GripSize;
            int size = GripSize * 2;

            var gp = new GraphicsPath();

            void AddRect(int x, int y)
            {
                gp.AddRectangle(new Rectangle(
                    x + TextureOffset.Width,
                    y + TextureOffset.Height,
                    halfSize,
                    halfSize));
            }

            if (!SkipBottomRightSquare)
                AddRect(Width - size, Height - size);

            AddRect(Width - size, Height - (size * 2));
            AddRect(Width - size, Height - (size * 3));
            AddRect(Width - (size * 2), Height - size);
            AddRect(Width - (size * 3), Height - size);
            AddRect(Width - (size * 2), Height - (size * 2));

            cachedGripPath = gp;
            gripPathDirty = false;

            return cachedGripPath;
        }

        private int privateGripSize = 2;

        [Category("HartUI")]
        public int GripSize
        {
            get
            {
                return privateGripSize;
            }
            set
            {
                privateGripSize = value;
                gripPathDirty = true;
                Invalidate();
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            gripPathDirty = true;
            base.OnSizeChanged(e);
        }

        private bool privateSkipBottomRightSquare = false;

        [Category("HartUI")]
        public bool SkipBottomRightSquare
        {
            get => privateSkipBottomRightSquare;
            set
            {
                privateSkipBottomRightSquare = value;
                gripPathDirty = true;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (GripTexture)
            {
                using (SolidBrush br = new SolidBrush(GripColor))
                {
                    // GetGripPath should not be disposed as the method
                    // controls the life cycle by itself
                    e.Graphics.FillPath(br, GetGripPath());
                }
            }

            if (TargetForm != null)
            {
                Location = new Point(TargetForm.ClientSize.Width - Width, TargetForm.ClientSize.Height - Height);
            }

            base.OnPaint(e);
        }
    }
}
