using CuoreUI.Misc.Internal;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace CuoreUI.Helpers
{
    public static class DrawingHelper
    {
        public static readonly Color PrimaryColor = Color.FromArgb(255, 106, 0);
        public static Color TranslucentPrimaryColor => Color.FromArgb(192, PrimaryColor);

        private static Timer refreshRateTimer;
        public static event EventHandler FrameDrawn;
        public static event EventHandler TenFramesDrawn;

        static DrawingHelper()
        {
            PreloadedForms.TryPreloadForms();
            HandCursorFix.EnableModernCursor();
            refreshRateTimer = new Timer();
            SetTimerRefreshRate();
        }

        public static int[] GetRefreshRates() => WindowsHelper.GetRefreshRates();

        // 1000 max, 1 minimum so as to prevent crashes
        // (timers with Interval = 0 throw exceptions, and the library has timers which rely on this)
        public static int GetHighestRefreshRate() => Math.Min(Math.Max(1, WindowsHelper.GetRefreshRate()), 1000);

        public static int LazyTimeDelta => 1000 / GetHighestRefreshRate();

        static Timer refreshRateTimerIntervalUpdaterTimer = new Timer();
        static byte frame = 0;

        private static void SetTimerRefreshRate()
        {
            refreshRateTimer.Interval = 1000 / GetHighestRefreshRate();

            if (!refreshRateTimer.Enabled)
            {
                refreshRateTimer.Start();
                refreshRateTimer.Tick += (sender, args) =>
                {
                    FrameDrawn?.Invoke(null, EventArgs.Empty);
                    frame++;
                    if (frame >= 10)
                    {
                        frame = 0;
                        TenFramesDrawn?.Invoke(null, EventArgs.Empty);
                    }
                };
            }

            if (!refreshRateTimerIntervalUpdaterTimer.Enabled)
            {
                refreshRateTimerIntervalUpdaterTimer.Interval = 5000;
                refreshRateTimerIntervalUpdaterTimer.Start();
                refreshRateTimerIntervalUpdaterTimer.Tick += (sender, args) =>
                {
                    refreshRateTimer.Interval = 1000 / GetHighestRefreshRate();
                };
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int ClampColor(int i)
        {
            return i < 0 ? 0 : (i > 255 ? 255 : i);
        }

        public static void ColorToHSV(Color color, out double hue, out double saturation, out double value)
        {
            int r = color.R;
            int g = color.G;
            int b = color.B;

            int max = r > g ? (r > b ? r : b) : (g > b ? g : b);
            int min = r < g ? (r < b ? r : b) : (g < b ? g : b);

            value = max / 255d;

            if (max == 0)
            {
                saturation = 0d;
                hue = 0d;
                return;
            }

            saturation = (max - min) / (double)max;

            if (max == min)
            {
                hue = 0d;
                return;
            }

            double delta = max - min;

            if (max == r)
            {
                hue = 60d * ((g - b) / delta);
                if (hue < 0d) hue += 360d;
            }
            else if (max == g)
            {
                hue = 60d * ((b - r) / delta + 2d);
            }
            else
            {
                hue = 60d * ((r - g) / delta + 4d);
            }
        }

        public static Color ColorFromHSV(double hue, double saturation, double value, byte alpha = 255)
        {
            if (saturation <= 0d)
            {
                int gray = ClampColor((int)Math.Round(value * 255d));
                return Color.FromArgb(alpha, gray, gray, gray);
            }

            hue %= 360d;
            if (hue < 0d) hue += 360d;

            double h = hue / 60d;
            int sector = (int)h;
            double f = h - sector;

            double scaled = value * 255d;
            double p = scaled * (1d - saturation);
            double q = scaled * (1d - saturation * f);
            double t = scaled * (1d - saturation * (1d - f));

            int v = ClampColor((int)Math.Round(scaled));
            int pi = ClampColor((int)Math.Round(p));
            int qi = ClampColor((int)Math.Round(q));
            int ti = ClampColor((int)Math.Round(t));

            switch (sector)
            {
                case 0: return Color.FromArgb(alpha, v, ti, pi);
                case 1: return Color.FromArgb(alpha, qi, v, pi);
                case 2: return Color.FromArgb(alpha, pi, v, ti);
                case 3: return Color.FromArgb(alpha, pi, qi, v);
                case 4: return Color.FromArgb(alpha, ti, pi, v);
                default: return Color.FromArgb(alpha, v, pi, qi);
            }
        }

        public static class Imaging
        {
            public static Bitmap TintBitmap(Bitmap originalBitmap, Color tintColor)
            {
                if (originalBitmap == null)
                {
                    return null;
                }

                Bitmap tintedBitmap = new Bitmap(originalBitmap.Width, originalBitmap.Height);

                using (Graphics graphics = Graphics.FromImage(tintedBitmap))
                {
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    float r = tintColor.R / 255f;
                    float g = tintColor.G / 255f;
                    float b = tintColor.B / 255f;
                    float a = tintColor.A / 255f;

                    ColorMatrix colorMatrix = new ColorMatrix(new float[][]
                    {
                    new float[]{ r, 0, 0, 0, 0 },
                      new float[]{0, g, 0, 0, 0 },
                      new float[]{0, 0, b, 0, 0 },
                      new float[]{0, 0, 0, a, 0 },
                      new float[]{0, 0, 0, 0, 1 }
                    });

                    using (ImageAttributes imageAttributes = new ImageAttributes())
                    {
                        imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                        graphics.DrawImage(originalBitmap, new Rectangle(0, 0, originalBitmap.Width, originalBitmap.Height),
                                    0, 0, originalBitmap.Width, originalBitmap.Height,
                                    GraphicsUnit.Pixel, imageAttributes);
                    }
                }

                return tintedBitmap;
            }

            public static class ImageBlurs
            {
                public static class QuadraticBlur
                {
                    public unsafe static void Apply(ref Bitmap bitmap, float radius)
                    {
                        BlurHelper.QuadraticBlur.Apply(ref bitmap, radius);
                    }
                }
            }
        }

        public enum EasingTypes
        {
            QuadIn,
            QuadOut,
            QuadInOut,
            QuartIn,
            QuartOut,
            QuartInOut,
            QuintIn,
            QuintOut,
            QuintInOut,
            ExpoIn,
            ExpoOut,
            ExpoInOut,
            BackIn,
            BackOut,
            BackInOut,
            SextIn,
            SextOut,
            SextInOut,
        }

        public static class EasingFunctions
        {
            public static double FromEasingType(EasingTypes easingType, double time, double duration = 1, double backOvershoot = 1.70158)
            {
                switch (easingType)
                {
                    case EasingTypes.QuadIn:
                        return Quad.In(time, duration);
                    case EasingTypes.QuadOut:
                        return Quad.Out(time, duration);
                    case EasingTypes.QuadInOut:
                        return Quad.InOut(time, duration);

                    case EasingTypes.QuartIn:
                        return Quart.In(time, duration);
                    case EasingTypes.QuartOut:
                        return Quart.Out(time, duration);
                    case EasingTypes.QuartInOut:
                        return Quart.InOut(time, duration);

                    case EasingTypes.QuintIn:
                        return Quint.In(time, duration);
                    case EasingTypes.QuintOut:
                        return Quint.Out(time, duration);
                    case EasingTypes.QuintInOut:
                        return Quint.InOut(time, duration);

                    case EasingTypes.ExpoIn:
                        return Expo.In(time, duration);
                    case EasingTypes.ExpoOut:
                        return Expo.Out(time, duration);
                    case EasingTypes.ExpoInOut:
                        return Expo.InOut(time, duration);

                    case EasingTypes.BackIn:
                        return Back.In(time, duration, backOvershoot);
                    case EasingTypes.BackOut:
                        return Back.Out(time, duration, backOvershoot);
                    case EasingTypes.BackInOut:
                        return Back.InOut(time, duration, backOvershoot);

                    case EasingTypes.SextIn:
                        return Sext.In(time, duration);
                    case EasingTypes.SextOut:
                        return Sext.Out(time, duration);
                    case EasingTypes.SextInOut:
                        return Sext.InOut(time, duration);
                    default:
                        throw new ArgumentOutOfRangeException(nameof(easingType), easingType, null);
                }
            }

            public static class Quad
            {
                public static double In(double time, double duration = 1)
                {
                    return time * time / duration;
                }

                public static double Out(double time, double duration = 1)
                {
                    return time * (2 - time) / duration;
                }

                public static double InOut(double time, double duration = 1)
                {
                    if (time < 0.5)
                    {
                        return 2 * time * time / duration;
                    }
                    return (-1 + (4 - 2 * time) * time) / duration;
                }
            }

            public static class Quart
            {
                public static double In(double time, double duration = 1)
                {
                    return time * time * time * time / duration;
                }

                public static double Out(double time, double duration = 1)
                {
                    return (1 - Math.Pow(1 - time, 4)) / duration;
                }

                public static double InOut(double time, double duration = 1)
                {
                    if (time < 0.5)
                    {
                        return 8 * time * time * time * time / duration;
                    }
                    return (1 - Math.Pow(-2 * time + 2, 4) / 2) / duration;
                }
            }

            public static class Quint
            {
                public static double In(double time, double duration = 1)
                {
                    return time * time * time * time * time / duration;
                }

                public static double Out(double time, double duration = 1)
                {
                    return (1 - Math.Pow(1 - time, 5)) / duration;
                }

                public static double InOut(double time, double duration = 1)
                {
                    if (time < 0.5)
                    {
                        return 16 * time * time * time * time * time / duration;
                    }
                    return (1 - Math.Pow(-2 * time + 2, 5) / 2) / duration;
                }
            }

            public static class Sext
            {
                public static double In(double time, double duration = 1)
                {
                    return Math.Pow(time, 6) / duration;
                }

                public static double Out(double time, double duration = 1)
                {
                    return (1 - Math.Pow(1 - time, 6)) / duration;
                }

                public static double InOut(double time, double duration = 1)
                {
                    if (time < 0.5)
                    {
                        return 32 * Math.Pow(time, 6) / duration;
                    }
                    return (1 - Math.Pow(-2 * time + 2, 6) / 2) / duration;
                }
            }

            public static class Expo
            {
                public static double In(double time, double duration = 1)
                {
                    return (time == 0 ? 0 : Math.Pow(2, 10 * time - 10)) / duration;
                }

                public static double Out(double time, double duration = 1)
                {
                    return (time == 1 ? 1 : 1 - Math.Pow(2, -10 * time)) / duration;
                }

                public static double InOut(double time, double duration = 1)
                {
                    if (time == 0)
                    {
                        return 0;
                    }
                    if (time == 1)
                    {
                        return 1;
                    }
                    if (time < 0.5)
                    {
                        return Math.Pow(2, 20 * time - 10) / 2 / duration;
                    }
                    return (2 - Math.Pow(2, -20 * time + 10)) / 2 / duration;
                }
            }

            public static class Back
            {
                public static double c1 = 1.70158;
                private static double c2 = c1 * 1.525;
                private static double c3 = c1 + 1;

                private static void RecalculateOvershoot(double backOvershoot)
                {
                    if (c1 != backOvershoot)
                    {
                        c1 = backOvershoot;
                        c2 = c1 * 1.525;
                        c3 = c1 + 1;
                    }
                }

                public static double In(double time, double duration = 1, double backOvershoot = 1.70158)
                {
                    RecalculateOvershoot(backOvershoot);
                    return (c3 * time * time * time - c1 * time * time) / duration;
                }

                public static double Out(double time, double duration = 1, double backOvershoot = 1.70158)
                {
                    RecalculateOvershoot(backOvershoot);
                    return (1 + c3 * Math.Pow(time - 1, 3) + c1 * Math.Pow(time - 1, 2)) / duration;
                }

                public static double InOut(double time, double duration = 1, double backOvershoot = 1.70158)
                {
                    RecalculateOvershoot(backOvershoot);
                    if (time < 0.5)
                    {
                        return Math.Pow(2 * time, 2) * ((c2 + 1) * 2 * time - c2) / 2 / duration;
                    }
                    return (Math.Pow(2 * time - 2, 2) * ((c2 + 1) * (time * 2 - 2) + c2) + 2) / 2 / duration;
                }
            }
        }
    }
}
