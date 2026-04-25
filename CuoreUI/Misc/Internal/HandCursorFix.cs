using CuoreUI.Helpers;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

// https://stackoverflow.com/a/56768588
public static class HandCursorFix
{
    static HandCursorFix()
    {
        EnableModernCursor();
    }

    public static void EnableModernCursor()
    {
        try
        {
            if (!WindowsHelper.IsInDesignMode())
            {
                Cursor SystemHandCursor = new Cursor(LoadCursor(IntPtr.Zero, 32649 /*IDC_HAND*/));
                FieldInfo handFieldInfo = typeof(Cursors).GetField("hand", BindingFlags.Static | BindingFlags.NonPublic);
                if (handFieldInfo != null)
                {
                    handFieldInfo.SetValue(null, SystemHandCursor);
                }
            }
        }
        catch { }
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);
}
