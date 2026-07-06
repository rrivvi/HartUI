using System;
using System.Buffers;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;

namespace HartUI.Helpers
{
    public static class BlurHelper
    {
        public static class QuadraticBlur
        {
            public static unsafe void Apply(ref Bitmap bitmap, int radius)
            {
                if (bitmap is null)
                    throw new ArgumentNullException(nameof(bitmap));

                if (!(radius > 0.1f) || float.IsNaN(radius) || float.IsInfinity(radius))
                    return;

                switch (bitmap.PixelFormat)
                {
                    case PixelFormat.Format24bppRgb:
                        Apply24Bpp(ref bitmap, radius);
                        return;

                    case PixelFormat.Format32bppArgb:
                    case PixelFormat.Format32bppPArgb:
                    case PixelFormat.Format32bppRgb:
                        Apply32Bpp(ref bitmap, radius);
                        return;

                    default:
                        throw new NotSupportedException(
                            "QuadraticBlur.Apply currently supports only Format24bppRgb and 32bpp bitmap formats.");
                }
            }

            internal static unsafe void Apply24Bpp(ref Bitmap bitmap, int radius)
            {
                ApplyCore(ref bitmap, radius, bytesPerPixel: 3, preserveAlpha: false);
            }

            internal static unsafe void Apply32Bpp(ref Bitmap bitmap, int radius)
            {
                ApplyCore(ref bitmap, radius, bytesPerPixel: 4, preserveAlpha: true);
            }

            private static unsafe void ApplyCore(ref Bitmap bitmap, int radius, int bytesPerPixel, bool preserveAlpha)
            {
                int width = bitmap.Width;
                int height = bitmap.Height;
                if (width == 0 || height == 0)
                    return;

                int halfKernel = radius;
                double radiusD = radius;
                double invRadiusSq = 1.0 / (radiusD * radiusD);
                double centerShift = halfKernel - radiusD;

                Rectangle rect = new Rectangle(0, 0, width, height);
                BitmapData data = null;
                byte[] scratch = null;

                try
                {
                    data = bitmap.LockBits(rect, ImageLockMode.ReadWrite, bitmap.PixelFormat);

                    int stride = data.Stride;
                    IntPtr srcBasePtr = data.Scan0;

                    if (stride < 0)
                    {
                        stride = -stride;
                        srcBasePtr = IntPtr.Add(srcBasePtr, checked((height - 1) * stride));
                    }

                    int scratchBytes = checked(height * stride);
                    scratch = ArrayPool<byte>.Shared.Rent(scratchBytes);

                    bool parallel = Environment.ProcessorCount > 1 && (long)width * height >= 65536;

                    fixed (byte* tempBase = scratch)
                    {
                        IntPtr tempBasePtr = (IntPtr)tempBase;

                        if (parallel)
                        {
                            Parallel.For(0, height, y =>
                                ProcessHorizontalRow(srcBasePtr, tempBasePtr, width, stride, y, bytesPerPixel, preserveAlpha, halfKernel, invRadiusSq, centerShift));

                            Parallel.For(0, width, x =>
                                ProcessVerticalColumn(srcBasePtr, tempBasePtr, width, height, stride, x, bytesPerPixel, preserveAlpha, halfKernel, invRadiusSq, centerShift));
                        }
                        else
                        {
                            for (int y = 0; y < height; y++)
                                ProcessHorizontalRow(srcBasePtr, tempBasePtr, width, stride, y, bytesPerPixel, preserveAlpha, halfKernel, invRadiusSq, centerShift);

                            for (int x = 0; x < width; x++)
                                ProcessVerticalColumn(srcBasePtr, tempBasePtr, width, height, stride, x, bytesPerPixel, preserveAlpha, halfKernel, invRadiusSq, centerShift);
                        }
                    }
                }
                finally
                {
                    if (data != null)
                        bitmap.UnlockBits(data);

                    if (scratch != null)
                        ArrayPool<byte>.Shared.Return(scratch, clearArray: false);
                }
            }

            private static unsafe void ProcessHorizontalRow(
                IntPtr srcBasePtr,
                IntPtr tempBasePtr,
                int width,
                int stride,
                int y,
                int bytesPerPixel,
                bool preserveAlpha,
                int halfKernel,
                double invRadiusSq,
                double centerShift)
            {
                byte* srcBase = (byte*)srcBasePtr.ToPointer();
                byte* tempBase = (byte*)tempBasePtr.ToPointer();

                byte* srcRow = srcBase + y * stride;
                byte* dstRow = tempBase + y * stride;

                double* local = stackalloc double[9];
                for (int i = 0; i < 9; i++)
                    local[i] = 0.0;

                int left = 0;
                int right = Math.Min(width - 1, halfKernel);

                double count = 0.0;
                double sumX = 0.0;
                double sumX2 = 0.0;

                for (int x = left; x <= right; x++)
                {
                    byte* p = srcRow + x * bytesPerPixel;
                    double xd = x;
                    double xd2 = xd * xd;

                    count += 1.0;
                    sumX += xd;
                    sumX2 += xd2;

                    local[0] += p[0];
                    local[1] += xd * p[0];
                    local[2] += xd2 * p[0];

                    local[3] += p[1];
                    local[4] += xd * p[1];
                    local[5] += xd2 * p[1];

                    local[6] += p[2];
                    local[7] += xd * p[2];
                    local[8] += xd2 * p[2];
                }

                for (int x = 0; x < width; x++)
                {
                    double t = x - centerShift;
                    double a = 1.0 - (t * t) * invRadiusSq;
                    double bCoef = 2.0 * t * invRadiusSq;
                    double cCoef = -invRadiusSq;

                    double denom = a * count + bCoef * sumX + cCoef * sumX2;
                    if (denom <= 0.0)
                        denom = 1.0;

                    double inv = 1.0 / denom;
                    byte* d = dstRow + x * bytesPerPixel;

                    d[0] = ToByte((a * local[0] + bCoef * local[1] + cCoef * local[2]) * inv);
                    d[1] = ToByte((a * local[3] + bCoef * local[4] + cCoef * local[5]) * inv);
                    d[2] = ToByte((a * local[6] + bCoef * local[7] + cCoef * local[8]) * inv);

                    if (preserveAlpha)
                        d[3] = srcRow[x * bytesPerPixel + 3];

                    if (x - halfKernel >= 0)
                    {
                        byte* p = srcRow + left * bytesPerPixel;
                        double xd = left;
                        double xd2 = xd * xd;

                        count -= 1.0;
                        sumX -= xd;
                        sumX2 -= xd2;

                        local[0] -= p[0];
                        local[1] -= xd * p[0];
                        local[2] -= xd2 * p[0];

                        local[3] -= p[1];
                        local[4] -= xd * p[1];
                        local[5] -= xd2 * p[1];

                        local[6] -= p[2];
                        local[7] -= xd * p[2];
                        local[8] -= xd2 * p[2];

                        left++;
                    }

                    if (x + halfKernel + 1 < width)
                    {
                        int add = ++right;
                        byte* p = srcRow + add * bytesPerPixel;
                        double xd = add;
                        double xd2 = xd * xd;

                        count += 1.0;
                        sumX += xd;
                        sumX2 += xd2;

                        local[0] += p[0];
                        local[1] += xd * p[0];
                        local[2] += xd2 * p[0];

                        local[3] += p[1];
                        local[4] += xd * p[1];
                        local[5] += xd2 * p[1];

                        local[6] += p[2];
                        local[7] += xd * p[2];
                        local[8] += xd2 * p[2];
                    }
                }
            }

            private static unsafe void ProcessVerticalColumn(
                IntPtr srcBasePtr,
                IntPtr tempBasePtr,
                int width,
                int height,
                int stride,
                int x,
                int bytesPerPixel,
                bool preserveAlpha,
                int halfKernel,
                double invRadiusSq,
                double centerShift)
            {
                byte* srcBase = (byte*)srcBasePtr.ToPointer();
                byte* tempBase = (byte*)tempBasePtr.ToPointer();

                int xOffset = x * bytesPerPixel;

                double* local = stackalloc double[9];
                for (int i = 0; i < 9; i++)
                    local[i] = 0.0;

                double count = 0.0;
                double sumY = 0.0;
                double sumY2 = 0.0;

                int top = 0;
                int bottom = Math.Min(height - 1, halfKernel);

                for (int y = top; y <= bottom; y++)
                {
                    byte* p = tempBase + y * stride + xOffset;
                    double yd = y;
                    double yd2 = yd * yd;

                    count += 1.0;
                    sumY += yd;
                    sumY2 += yd2;

                    local[0] += p[0];
                    local[1] += yd * p[0];
                    local[2] += yd2 * p[0];

                    local[3] += p[1];
                    local[4] += yd * p[1];
                    local[5] += yd2 * p[1];

                    local[6] += p[2];
                    local[7] += yd * p[2];
                    local[8] += yd2 * p[2];
                }

                for (int y = 0; y < height; y++)
                {
                    double t = y - centerShift;
                    double a = 1.0 - (t * t) * invRadiusSq;
                    double bCoef = 2.0 * t * invRadiusSq;
                    double cCoef = -invRadiusSq;

                    double denom = a * count + bCoef * sumY + cCoef * sumY2;
                    if (denom <= 0.0)
                        denom = 1.0;

                    double inv = 1.0 / denom;
                    byte* d = srcBase + y * stride + xOffset;

                    d[0] = ToByte((a * local[0] + bCoef * local[1] + cCoef * local[2]) * inv);
                    d[1] = ToByte((a * local[3] + bCoef * local[4] + cCoef * local[5]) * inv);
                    d[2] = ToByte((a * local[6] + bCoef * local[7] + cCoef * local[8]) * inv);

                    if (preserveAlpha)
                        d[3] = tempBase[y * stride + xOffset + 3];

                    if (y - halfKernel >= 0)
                    {
                        int removeRow = y - halfKernel;
                        byte* p = tempBase + removeRow * stride + xOffset;
                        double yd = removeRow;
                        double yd2 = yd * yd;

                        count -= 1.0;
                        sumY -= yd;
                        sumY2 -= yd2;

                        local[0] -= p[0];
                        local[1] -= yd * p[0];
                        local[2] -= yd2 * p[0];

                        local[3] -= p[1];
                        local[4] -= yd * p[1];
                        local[5] -= yd2 * p[1];

                        local[6] -= p[2];
                        local[7] -= yd * p[2];
                        local[8] -= yd2 * p[2];
                    }

                    if (y + halfKernel + 1 < height)
                    {
                        int addRow = y + halfKernel + 1;
                        byte* p = tempBase + addRow * stride + xOffset;
                        double yd = addRow;
                        double yd2 = yd * yd;

                        count += 1.0;
                        sumY += yd;
                        sumY2 += yd2;

                        local[0] += p[0];
                        local[1] += yd * p[0];
                        local[2] += yd2 * p[0];

                        local[3] += p[1];
                        local[4] += yd * p[1];
                        local[5] += yd2 * p[1];

                        local[6] += p[2];
                        local[7] += yd * p[2];
                        local[8] += yd2 * p[2];
                    }
                }
            }

            private static byte ToByte(double value)
            {
                if (value <= 0.0) return 0;
                if (value >= 255.0) return 255;
                return (byte)(value + 0.5);
            }
        }
    }
}