using HartUI.Helpers;
using System.Windows.Forms;

namespace HartUI.Misc.Internal
{
    public static class InputManager
    {
        private static readonly object lockObj = new object();
        private static bool isFilterInstalled = false;

        public static bool LastInputWasKeyboard
        {
            get
            {
                return InputModalityFilter.LastInputWasKeyboard;
            }
        }

        internal static void TryInstallInputModalityFilter()
        {
            if (WindowsHelper.IsInDesignMode())
            {
                return;
            }

            lock (lockObj)
            {
                if (!isFilterInstalled)
                {
                    Application.AddMessageFilter(new InputModalityFilter());
                    isFilterInstalled = true;
                }
            }
        }

        private sealed class InputModalityFilter : IMessageFilter
        {
            internal static bool LastInputWasKeyboard { get; private set; } = false;

            public bool PreFilterMessage(ref Message m)
            {
                switch ((uint)m.Msg)
                {
                    // Keyboard
                    case 0x0100: // WM_KEYDOWN
                    case 0x0104: // WM_SYSKEYDOWN
                    case 0x0102: // WM_CHAR
                        LastInputWasKeyboard = true;
                        break;

                    // Mouse buttons, wheel, activation
                    case 0x0201: // WM_LBUTTONDOWN
                    case 0x0204: // WM_RBUTTONDOWN
                    case 0x0207: // WM_MBUTTONDOWN
                    case 0x020B: // WM_XBUTTONDOWN
                    case 0x020A: // WM_MOUSEWHEEL
                    case 0x020E: // WM_MOUSEHWHEEL
                    case 0x0021: // WM_MOUSEACTIVATE
                        LastInputWasKeyboard = false;
                        break;

                    // Touch input, Pointer input
                    case 0x0240: // WM_TOUCH
                    case 0x0246: // WM_POINTERDOWN
                        LastInputWasKeyboard = false;
                        break;
                }

                return false;
            }
        }
    }
}
