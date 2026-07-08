using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using static HartUI.Helpers.DrawingHelper;
using static HartUI.Helpers.GeneralHelper;

namespace HartUI.Controls
{
    [Description("HSV Color picker wheel, triangle inside")]
    [DefaultEvent("SelectedColor")]
    public partial class cuiColorPickerWheel : UserControl
    {
        private Bitmap privateHueBitmap;
        private Bitmap privateTriangleBitmap;

        private const float Sin60 = 0.8660254037844386f;
        private const float Cos60 = 0.5f;
        private const double RadToDeg = 57.295779513082320876798154814105d;

        private PointF[] trianglePoints = new PointF[3];
        private PointF[] trianglePointsV2 = new PointF[3];
        private int cachedGeometrySize = -1;
        private int cachedGeometryThickness = -1;

        public cuiColorPickerWheel()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.ResizeRedraw, false);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            privateHueBitmap?.Dispose();
            privateHueBitmap = null;
            privateTriangleBitmap?.Dispose();
            privateTriangleBitmap = null;
            UpdateClickedRectangleFromColor();
            Invalidate();
        }

        private int privateWheelThickness = 16;
        [Category("HartUI")]
        [Description("The Hue ring's thickness. The bigger it is, the smaller the triangle inside.")]
        public int WheelThickness
        {
            get
            {
                return privateWheelThickness;
            }
            set
            {
                privateWheelThickness = value;
                privateHueBitmap?.Dispose();
                privateHueBitmap = null;
                privateTriangleBitmap?.Dispose();
                privateTriangleBitmap = null;
                Invalidate();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryBarycentricCoords(PointF p, PointF a, PointF b, PointF c, out float w1, out float w2, out float w3)
        {
            BarycentricCoords(p, a, b, c, out w1, out w2, out w3);
            return w1 >= 0f && w2 >= 0f && w3 >= 0f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void HsvToArgb(double hue, double saturation, double value, byte alpha, out int argb)
        {
            if (saturation <= 0d)
            {
                int gray = ClampColor((int)Math.Round(value * 255d));
                argb = unchecked((int)((uint)alpha << 24 | (uint)gray << 16 | (uint)gray << 8 | (uint)gray));
                return;
            }

            hue %= 360d;
            if (hue < 0d) hue += 360d;

            double h = hue / 60d;
            int sector = (int)h;
            double f = h - sector;

            double scaled = value * 255d;
            int v = ClampColor((int)Math.Round(scaled));
            int p = ClampColor((int)Math.Round(scaled * (1d - saturation)));
            int q = ClampColor((int)Math.Round(scaled * (1d - saturation * f)));
            int t = ClampColor((int)Math.Round(scaled * (1d - saturation * (1d - f))));

            int r, g, b;
            switch (sector)
            {
                case 0: r = v; g = t; b = p; break;
                case 1: r = q; g = v; b = p; break;
                case 2: r = p; g = v; b = t; break;
                case 3: r = p; g = q; b = v; break;
                case 4: r = t; g = p; b = v; break;
                default: r = v; g = p; b = q; break;
            }

            argb = unchecked((int)((uint)alpha << 24 | (uint)r << 16 | (uint)g << 8 | (uint)b));
        }

        #region hue ring & sat/val triangle
        private void GenerateHueBitmap()
        {
            int size = Math.Min(Width, Height);
            if (size <= 0)
            {
                return;
            }

            int outerRadius = size / 2 - 1;
            int innerRadius = outerRadius - WheelThickness;
            int outer2 = outerRadius * outerRadius;
            int inner2 = innerRadius * innerRadius;

            Bitmap bmp = new Bitmap(size, size, PixelFormat.Format32bppArgb);
            BitmapData bmpData = null;

            try
            {
                bmpData = bmp.LockBits(new Rectangle(0, 0, size, size), ImageLockMode.WriteOnly, bmp.PixelFormat);

                int strideInts = bmpData.Stride / 4;
                int[] pixels = new int[strideInts * size];
                int cx = size / 2;
                int cy = size / 2;

                Parallel.For(0, size, y =>
                {
                    int dy = y - cy;
                    int row = y * strideInts;

                    for (int x = 0; x < size; x++)
                    {
                        int dx = x - cx;
                        int dist2 = dx * dx + dy * dy;

                        if (dist2 >= inner2 && dist2 <= outer2)
                        {
                            double angle = Math.Atan2(dy, dx) * RadToDeg;
                            if (angle < 0d) angle += 360d;

                            HsvToArgb(angle, 1d, 1d, 255, out int argb);
                            pixels[row + x] = argb;
                        }
                    }
                });

                Marshal.Copy(pixels, 0, bmpData.Scan0, pixels.Length);
            }
            finally
            {
                if (bmpData != null)
                {
                    bmp.UnlockBits(bmpData);
                }
            }

            privateHueBitmap?.Dispose();
            privateHueBitmap = bmp;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ComputeTriangleVertices(PointF center, float r, out PointF pHue, out PointF pWhite, out PointF pBlack)
        {
            pHue = new PointF(center.X, center.Y - r);
            pWhite = new PointF(center.X + r * Sin60 - 1f, center.Y + r * Cos60 - 1f);
            pBlack = new PointF(center.X - r * Sin60, center.Y + r * Cos60 - 1f);
        }

        private void EnsureGeometry()
        {
            int size = Math.Min(Width, Height);
            if (size == cachedGeometrySize && WheelThickness == cachedGeometryThickness)
            {
                return;
            }

            cachedGeometrySize = size;
            cachedGeometryThickness = WheelThickness;

            float cx = Width / 2f;
            float cy = Height / 2f;
            int outerRadius = size / 2 - 1;
            int innerRadius = outerRadius - WheelThickness;

            float r = innerRadius - 1f;

            ComputeTriangleVertices(new PointF(cx, cy), r, out trianglePoints[0], out trianglePoints[1], out trianglePoints[2]);

            trianglePointsV2[0] = new PointF(cx - 1f, cy - r);
            trianglePointsV2[1] = trianglePoints[1];
            trianglePointsV2[2] = trianglePoints[2];
        }

        private void GenerateTriangleBitmap(double hue, int size, PointF pHue, PointF pWhite, PointF pBlack)
        {
            Bitmap bmp = new Bitmap(size, size, PixelFormat.Format32bppArgb);
            BitmapData bmpData = null;

            try
            {
                bmpData = bmp.LockBits(new Rectangle(0, 0, size, size), ImageLockMode.WriteOnly, bmp.PixelFormat);

                int strideInts = bmpData.Stride / 4;
                int[] pixels = new int[strideInts * size];

                for (int y = 0; y < size; y++)
                {
                    int row = y * strideInts;

                    for (int x = 0; x < size; x++)
                    {
                        PointF p = new PointF(x, y);

                        if (TryBarycentricCoords(p, pHue, pWhite, pBlack, out float w1, out float w2, out float w3))
                        {
                            double value = w1 + w2;
                            double saturation = value > 0.0 ? w1 / value : 0.0;

                            HsvToArgb(hue, saturation, value, 255, out int argb);
                            pixels[row + x] = argb;
                        }
                    }
                }

                Marshal.Copy(pixels, 0, bmpData.Scan0, pixels.Length);
            }
            finally
            {
                if (bmpData != null)
                {
                    bmp.UnlockBits(bmpData);
                }
            }

            privateTriangleBitmap?.Dispose();
            privateTriangleBitmap = bmp;
        }
        #endregion

        double lastValidHue = 0;
        double previouslyPaintedHue = 0;
        double privateHue = 0;
        double privateSaturation = 0;
        double privateValue = 0;

        protected override void OnPaint(PaintEventArgs e)
        {
            // ensure triangle geometry is cached
            EnsureGeometry();

            int size = Math.Min(Width, Height);
            int x = (Width - size) / 2;
            int y = (Height - size) / 2;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // hue ring
            if (privateHueBitmap == null)
            {
                GenerateHueBitmap();
            }

            e.Graphics.DrawImage(privateHueBitmap, x, y, size, size);

            // val/sat triangle
            if (privateTriangleBitmap == null || previouslyPaintedHue != privateHue)
            {
                previouslyPaintedHue = privateHue;

                // GenerateTriangleBitmap works in bitmap-local space (top-left origin),
                // while trianglePoints is cached in control space. Translate by the
                // ring bitmap's draw offset (x, y) to convert - no separate geometry
                // computation needed here anymore.
                PointF pHueLocal = new PointF(trianglePoints[0].X - x, trianglePoints[0].Y - y);
                PointF pWhiteLocal = new PointF(trianglePoints[1].X - x, trianglePoints[1].Y - y);
                PointF pBlackLocal = new PointF(trianglePoints[2].X - x, trianglePoints[2].Y - y);

                GenerateTriangleBitmap(privateHue, size, pHueLocal, pWhiteLocal, pBlackLocal);
            }

            using (Pen antialiasPen = new Pen(BackColor, 4))
            using (Pen whereClickPen1 = new Pen(Color.FromArgb(128, 0, 0, 0), 2f)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round
            })
            {
                Rectangle modifiedCR = ClientRectangle;
                modifiedCR.Size = new Size(size, size);
                modifiedCR.X = x;
                modifiedCR.Y = y;
                modifiedCR.Inflate(-1, -1);

                // outer + inner ring borders (fake anti aliasing)
                e.Graphics.DrawEllipse(antialiasPen, modifiedCR);
                modifiedCR.Inflate(-WheelThickness, -WheelThickness);
                e.Graphics.DrawEllipse(antialiasPen, modifiedCR);

                // inner triangle
                e.Graphics.DrawImage(privateTriangleBitmap, x, y, size, size);

                // triangle borders (fake anti aliasing)
                antialiasPen.Width = 2;
                e.Graphics.DrawPolygon(antialiasPen, trianglePoints);
                e.Graphics.DrawPolygon(antialiasPen, trianglePointsV2);

                int centerX = Width / 2;
                int centerY = Height / 2;

                double radians = privateHue * (Math.PI / 180.0);

                float cos = (float)Math.Cos(radians);
                float sin = (float)Math.Sin(radians);

                float radius = (Width > Height ? centerY : centerX) - 2;

                float startX = centerX + radius * cos;
                float startY = centerY + radius * sin;

                PointF p1hueSelectorPoint = new PointF(startX, startY);
                PointF p2hueSelectorPoint = PointTowardsCenter(
                    p1hueSelectorPoint,
                    centerX,
                    centerY,
                    privateWheelThickness);

                e.Graphics.DrawEllipse(whereClickPen1, clickRectangle);

                whereClickPen1.Width = 4f;
                e.Graphics.DrawLine(whereClickPen1,
                    p1hueSelectorPoint.X, p1hueSelectorPoint.Y,
                    p2hueSelectorPoint.X, p2hueSelectorPoint.Y);

                whereClickPen1.Width = 0.4f;
                whereClickPen1.Color = Color.White;
                e.Graphics.DrawEllipse(whereClickPen1, clickRectangle);

                whereClickPen1.Width = 3f;
                e.Graphics.DrawLine(whereClickPen1,
                    p1hueSelectorPoint.X, p1hueSelectorPoint.Y,
                    p2hueSelectorPoint.X, p2hueSelectorPoint.Y);
            }

            base.OnPaint(e);
        }

        private PointF PointTowardsCenter(PointF inputPoint, float centerX, float centerY, double distance)
        {
            double dx = centerX - inputPoint.X;
            double dy = centerY - inputPoint.Y;
            double len = Math.Sqrt(dx * dx + dy * dy);

            if (len == 0.0) // ??
            {
                return inputPoint;
            }
            if (distance >= len)
            {
                return new PointF(centerX, centerY);
            }

            double ux = dx / len;
            double uy = dy / len;

            return new PointF(
                (float)(inputPoint.X + ux * distance),
                (float)(inputPoint.Y + uy * distance)
            );
        }


        // WHERE THE USER IS CLICKING
        // 0 - normal
        // 1 - hue ring
        // 2 - sat/val triangle
        int colorPickerState = ColorPickerStates.Idle;

        public static class ColorPickerStates
        {
            public const int Idle = 0;
            public const int ChangingHue = 1;
            public const int ChangingSatVal = 2;
        }

        [Category("HartUI")]
        [Description("Any change in hue, brightness or saturation will invoke this event.")]
        public event EventHandler ContentChanged;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            ColorToHSV(privateContent, out double h, out double s, out double v);

            privateHue = h;
            privateSaturation = s;
            privateValue = v;

            if (s > 0.0001)
            {
                lastValidHue = h;
            }

            UpdateClickedRectangleFromColor();
        }

        private Color privateContent = Color.Red;
        [Category("HartUI")]
        public Color Content
        {
            get
            {
                return privateContent;
            }
            set
            {
                privateContent = value;

                if (DesignMode)
                {
                    ColorToHSV(value, out privateHue, out _, out _);
                }
                else
                {
                    ColorToHSV(value, out double h, out double s, out double v);

                    // When the value is extreme (near black or near white),
                    // the HSV conversion loses accuracy. This ensures the hue
                    // doesn't go crazy when that's the case
                    if (s > 0.0001)
                    {
                        privateHue = h;
                        lastValidHue = h;
                    }
                    else
                    {
                        privateHue = lastValidHue;
                    }

                    privateSaturation = s;
                    privateValue = v;

                    // if color changes, but mouse is not over this wheel, fire the SelectedColor event
                    // (means the color was changed programatically, and not by the user)
                    // do not use ClientRectangle.Contains(PointToClient(Cursor.Position))
                    // since if the user were to click a control on TOP OF this wheel, the event wouldn't fire
                    if (!isMouseOnControl)
                    {
                        SelectedColor?.Invoke(this, EventArgs.Empty);
                    }
                }

                if (colorPickerState == ColorPickerStates.Idle)
                {
                    UpdateClickedRectangleFromColor();
                }

                ContentChanged?.Invoke(this, EventArgs.Empty);
                Invalidate();
            }
        }

        private void UpdateClickedRectangleFromColor()
        {
            EnsureGeometry();

            PointF pHue = trianglePoints[0];
            PointF pWhite = trianglePoints[1];
            PointF pBlack = trianglePoints[2];

            ColorToHSV(privateContent, out double h, out double s, out double v);

            double w1 = s * v;              // towards pHue
            double w2 = v * (1.0 - s);      // towards pWhite
            double w3 = 1.0 - v;            // towards pBlack

            float x = (float)(w1 * pHue.X + w2 * pWhite.X + w3 * pBlack.X);
            float y = (float)(w1 * pHue.Y + w2 * pWhite.Y + w3 * pBlack.Y);

            PointF candidate = new PointF(x, y);

            // clamp candidate inside triangle
            if (!PointInTriangle(candidate, pHue, pWhite, pBlack))
            {
                candidate = ClosestPointOnTriangle(candidate, pHue, pWhite, pBlack);
            }

            clickRectangle = new RectangleF(candidate.X - 4, candidate.Y - 4, 8, 8);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            OnMouseMove(e);
        }

        RectangleF clickRectangle = new RectangleF(-8, -8, 8, 8);

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                PointF center = new PointF(Width / 2f, Height / 2f);

                byte currentAlpha = Content.A;

                // changing hue (ring)
                if (colorPickerState == ColorPickerStates.ChangingHue)
                {
                    float dx = e.X - center.X;
                    float dy = e.Y - center.Y;
                    double angle = Math.Atan2(dy, dx) * RadToDeg;
                    if (angle < 0)
                    {
                        angle += 360;
                    }

                    privateHue = angle;
                    lastValidHue = angle;
                    privateTriangleBitmap?.Dispose();
                    privateTriangleBitmap = null;

                    // Content recalculates hsv, which can destroy hue
                    // when saturation or value are extreme (near black or near white)
                    privateContent = ColorFromHSV(
                        privateHue,
                        privateSaturation,
                        privateValue,
                        currentAlpha);

                    ContentChanged?.Invoke(this, EventArgs.Empty);
                    Invalidate();
                }
                // changing saturation or value (triangle)
                else if (colorPickerState == ColorPickerStates.ChangingSatVal)
                {
                    EnsureGeometry();

                    PointF p1 = trianglePoints[0];
                    PointF p2 = trianglePoints[1];
                    PointF p3 = trianglePoints[2];

                    PointF p = e.Location;
                    if (!PointInTriangle(p, p1, p2, p3))
                    {
                        // nearest point in triangle from mouse cursor location
                        p = ClosestPointOnTriangle(p, p1, p2, p3);
                    }

                    BarycentricCoords(p, p1, p2, p3, out float w1, out float w2, out float w3);

                    privateValue = w1 + w2;
                    privateSaturation = privateValue > 0.0 ? w1 / privateValue : 0.0;

                    clickRectangle.X = (int)p.X - 4;
                    clickRectangle.Y = (int)p.Y - 4;

                    // don't replace privateContent with Content
                    // Content calculates new hue values with GetHue,
                    // but we don't want to change the hue while the user is changing the saturation and value
                    privateContent = ColorFromHSV(
                        privateHue,
                        privateSaturation,
                        privateValue,
                        currentAlpha);

                    ContentChanged?.Invoke(this, EventArgs.Empty);
                    Invalidate();
                }
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (IsInHueRing(e.Location))
            {
                colorPickerState = ColorPickerStates.ChangingHue;
            }
            else if (IsInValueTriangle(e.Location))
            {
                colorPickerState = ColorPickerStates.ChangingSatVal;
            }
            else
            {
                colorPickerState = ColorPickerStates.Idle;
            }
        }

        [Category("HartUI")]
        [Description("Gets invoked whenever the user releases their mouse, and the color has changed.")]
        public event EventHandler SelectedColor;

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (colorPickerState != ColorPickerStates.Idle)
            {
                SelectedColor?.Invoke(this, EventArgs.Empty);
            }

            colorPickerState = ColorPickerStates.Idle;
            base.OnMouseUp(e);
        }

        private bool IsInHueRing(Point point)
        {
            int size = Math.Min(Width, Height);
            Point center = new Point(Width / 2, Height / 2);

            int dx = point.X - center.X;
            int dy = point.Y - center.Y;
            double dist = Math.Sqrt(dx * dx + dy * dy);

            int outerRadius = size / 2 - 1;
            int innerRadius = outerRadius - WheelThickness;

            return dist >= innerRadius && dist <= outerRadius;
        }

        private bool IsInValueTriangle(Point point)
        {
            EnsureGeometry();
            return PointInTriangle(point, trianglePoints[0], trianglePoints[1], trianglePoints[2]);
        }

        private bool isMouseOnControl = false;

        protected override void OnMouseEnter(EventArgs e)
        {
            isMouseOnControl = true;
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            isMouseOnControl = false;
            base.OnMouseLeave(e);
        }
    }
}