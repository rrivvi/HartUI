using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace HartUI.Helpers
{
    internal static class WindowsHelper
    {
        public static bool IsInDesignMode() => PerPixelAlphaBlend.Win32.IsDesignHost();

        [StructLayout(LayoutKind.Sequential)]
        struct OSVERSIONINFOEX
        {
            public int dwOSVersionInfoSize;
            public int dwMajorVersion;
            public int dwMinorVersion;
            public int dwBuildNumber;
            public int dwPlatformId;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szCSDVersion;
            public ushort wServicePackMajor;
            public ushort wServicePackMinor;
            public ushort wSuiteMask;
            public byte wProductType;
            public byte wReserved;
        }

        [DllImport("ntdll.dll", SetLastError = true)]
        static extern int RtlGetVersion(ref OSVERSIONINFOEX versionInfo);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowDisplayAffinity(IntPtr hWnd, uint dwAffinity);

        public static bool IsWindows11()
        {
            var osVersion = new OSVERSIONINFOEX();
            osVersion.dwOSVersionInfoSize = Marshal.SizeOf(osVersion);
            int status = RtlGetVersion(ref osVersion);

            if (status == 0)
            {
                return osVersion.dwMajorVersion == 10 && osVersion.dwBuildNumber >= 22000;
            }
            else
            {
                return false;
            }
        }

        public const uint FLASHW_ALL = 1 | 2;
        public const uint FLASHW_TIMERNOFG = 12;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        [StructLayout(LayoutKind.Sequential)]
        public struct FLASHWINFO
        {
            public uint cbSize;
            public IntPtr hwnd;
            public uint dwFlags;
            public uint uCount;
            public uint dwTimeout;
        }

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        public const int WM_SYSCOMMAND = 0x0112;
        public const int SC_CLOSE = 0xF060;

        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_TRANSPARENT = 0x00000020;
        public const int WS_EX_LAYERED = 0x00080000;
        public const int WS_EX_NOACTIVATE = 0x08000000;

        [StructLayout(LayoutKind.Sequential)]
        public struct DEVMODE
        {
            private const int CCHDEVICENAME = 32;
            private const int CCHFORMNAME = 32;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public int dmDisplayOrientation;
            public int dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHFORMNAME)]
            public string dmFormName;
            public short dmLogPixels;
            public int dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct DISPLAY_DEVICE
        {
            public int cb;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceString;
            public int StateFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceKey;
        }

        [DllImport("user32.dll")]
        private static extern bool EnumDisplaySettings(string lpszDeviceName, int iModeNum, ref DEVMODE lpDevMode);

        [DllImport("user32.dll")]
        private static extern bool EnumDisplayDevices(string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);

        public static bool REFRESH_RATE_OVERRIDE = false;
        public static int SPOOFED_REFRESH_RATE = 60;

        public static int GetRefreshRate()
        {
            if (REFRESH_RATE_OVERRIDE)
            {
                return SPOOFED_REFRESH_RATE;
            }

            DISPLAY_DEVICE d = new DISPLAY_DEVICE();
            d.cb = Marshal.SizeOf(d);
            DEVMODE vDevMode = new DEVMODE();
            vDevMode.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));

            uint deviceIndex = 0;
            int maxRefreshRate = 1;

            while (EnumDisplayDevices(null, deviceIndex, ref d, 0))
            {
                if (EnumDisplaySettings(d.DeviceName, -1, ref vDevMode))
                {
                    int refreshRate = vDevMode.dmDisplayFrequency;
                    if (refreshRate > maxRefreshRate)
                    {
                        maxRefreshRate = refreshRate;
                    }
                }
                deviceIndex++;
            }

            return maxRefreshRate;
        }

        public static int[] GetRefreshRates()
        {
            List<int> refreshRates = new List<int>();
            DISPLAY_DEVICE d = new DISPLAY_DEVICE();
            d.cb = Marshal.SizeOf(d);
            DEVMODE vDevMode = new DEVMODE();
            vDevMode.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));

            uint deviceIndex = 0;
            while (EnumDisplayDevices(null, deviceIndex, ref d, 0))
            {
                if (EnumDisplaySettings(d.DeviceName, -1, ref vDevMode))
                {
                    refreshRates.Add(vDevMode.dmDisplayFrequency);
                }
                deviceIndex++;
            }

            return refreshRates.ToArray();
        }

        internal static class NativeMethods
        {
            public const int GWL_EXSTYLE = -20;
            public const int WS_EX_LAYERED = 0x80000;

            [DllImport("user32.dll")]
            public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

            [DllImport("user32.dll")]
            public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

            [DllImport("user32.dll")]
            public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

            public const int SW_SHOWNOACTIVATE = 4;
            public const int SW_HIDE = 0;
        }

        internal static class PerPixelAlphaBlend
        {
            public unsafe static void SetBitmap(Bitmap bitmap, byte opacity, int left, int top, IntPtr handle)
            {
                if (bitmap.PixelFormat != PixelFormat.Format32bppArgb)
                    throw new ApplicationException("The bitmap must be 32ppp with alpha-channel.");

                IntPtr screenDc = Win32.GetDC(IntPtr.Zero);
                IntPtr memDc = Win32.CreateCompatibleDC(screenDc);
                IntPtr hBitmap = IntPtr.Zero;
                IntPtr oldBitmap = IntPtr.Zero;

                try
                {
                    hBitmap = bitmap.GetHbitmap(Color.FromArgb(0));
                    oldBitmap = Win32.SelectObject(memDc, hBitmap);

                    Win32.Size size = new Win32.Size(bitmap.Width, bitmap.Height);
                    Win32.Point topPos = new Win32.Point(left, top);
                    Win32.Point pointSource = new Win32.Point(0, 0);
                    Win32.BLENDFUNCTION blend = new Win32.BLENDFUNCTION
                    {
                        BlendOp = Win32.AC_SRC_OVER,
                        BlendFlags = 0,
                        SourceConstantAlpha = opacity,
                        AlphaFormat = Win32.AC_SRC_ALPHA
                    };

                    Win32.UpdateLayeredWindow(handle, screenDc, ref topPos, ref size, memDc, ref pointSource, 0, ref blend, Win32.ULW_ALPHA);

                }
                finally
                {
                    Win32.ReleaseDC(IntPtr.Zero, screenDc);
                    if (hBitmap != IntPtr.Zero)
                    {
                        Win32.SelectObject(memDc, oldBitmap);
                        Win32.DeleteObject(hBitmap);
                    }

                    Win32.DeleteDC(memDc);
                }
            }

            internal static class Win32
            {
                public enum Bool
                {
                    False = 0,
                    True
                };

                [StructLayout(LayoutKind.Sequential)]
                public struct Point
                {
                    public int x;
                    public int y;

                    public Point(int x, int y)
                    {
                        this.x = x;
                        this.y = y;
                    }
                }

                [StructLayout(LayoutKind.Sequential)]
                public struct Size
                {
                    public int cx;
                    public int cy;

                    public Size(int cx, int cy)
                    {
                        this.cx = cx;
                        this.cy = cy;
                    }
                }

                [StructLayout(LayoutKind.Sequential, Pack = 1)]
                public struct BLENDFUNCTION
                {
                    public byte BlendOp;
                    public byte BlendFlags;
                    public byte SourceConstantAlpha;
                    public byte AlphaFormat;
                }

                public const int ULW_ALPHA = 0x00000002;

                public const byte AC_SRC_OVER = 0x00;
                public const byte AC_SRC_ALPHA = 0x01;

                internal const int GWL_EXSTYLE = -20;
                internal const int WS_EX_LAYERED = 0x00080000;
                internal const int WS_EX_TOOLWINDOW = 0x00000080;

                internal const uint SWP_NOSIZE = 0x0001;
                internal const uint SWP_NOMOVE = 0x0002;
                internal const uint SWP_NOZORDER = 0x0004;
                internal const uint SWP_NOACTIVATE = 0x0010;

                [StructLayout(LayoutKind.Sequential)]
                internal struct NativePoint
                {
                    public int X;
                    public int Y;

                    public NativePoint(int x, int y)
                    {
                        X = x;
                        Y = y;
                    }
                }

                [StructLayout(LayoutKind.Sequential)]
                internal struct NativeSize
                {
                    public int CX;
                    public int CY;

                    public NativeSize(int cx, int cy)
                    {
                        CX = cx;
                        CY = cy;
                    }
                }

                [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
                internal static extern IntPtr GetDC(IntPtr hWnd);

                [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
                internal static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

                [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
                internal static extern IntPtr CreateCompatibleDC(IntPtr hDC);

                [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
                internal static extern bool DeleteDC(IntPtr hdc);

                [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
                internal static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

                [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
                internal static extern bool DeleteObject(IntPtr hObject);

                internal static bool IsDesignHost()
                {
                    if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                    {
                        return true;
                    }

                    try
                    {
                        string processName = Process.GetCurrentProcess().ProcessName;
                        return processName.IndexOf("devenv", StringComparison.OrdinalIgnoreCase) >= 0 ||
                               processName.IndexOf("designtoolsserver", StringComparison.OrdinalIgnoreCase) >= 0;
                    }
                    catch
                    {
                        return false;
                    }
                }

                [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
                public static extern Bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst, ref Point pptDst, ref Size psize, IntPtr hdcSrc, ref Point pprSrc, int crKey, ref BLENDFUNCTION pblend, int dwFlags);
            }
        }
    }
}
