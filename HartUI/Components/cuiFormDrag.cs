using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace HartUI.Components
{
    [Description("Window dragging abilities for FormBorderStyle.None forms")]
    [ToolboxBitmap(typeof(Form))]
    public partial class cuiFormDrag : Component
    {
        public cuiFormDrag(IContainer container)
        {
            container.Add(this);
        }

        private Form privateTargetForm;

        [Category("HartUI")]
        public Form TargetForm
        {
            get
            {
                return privateTargetForm;
            }
            set
            {
                if (privateTargetForm != null)
                {
                    privateTargetForm.MouseMove -= MouseMove;
                }

                privateTargetForm = value;

                if (privateTargetForm != null)
                {
                    privateTargetForm.MouseMove += MouseMove;
                }
            }
        }

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTCAPTION = 0x2;

        private void MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(TargetForm.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
            }
        }
    }
}
