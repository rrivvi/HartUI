using HartUI.Components.Forms;
using HartUI.Helpers;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Color = System.Drawing.Color;
using Task = System.Threading.Tasks.Task;

namespace HartUI.Components
{
    [Description("(Experimental) Rounded corners for Windows 10 and below")]
    [ToolboxBitmap(typeof(Form))]
    public partial class cuiFormRounder : Component
    {
        public cuiFormRounder()
        {
        }

        internal RoundedForm roundedFormObj;

        private Form privateTargetForm;

        [Category("HartUI")]
        public Form TargetForm
        {
            get => privateTargetForm;
            set
            {
                if (privateTargetForm != null)
                {
                    FormsRegisteredByRounder.RemoveByForm(privateTargetForm);

                    // cuiFormRounder sets a custom region to the TargetForm
                    // to avoid visual artifacts, the region is reset to normal here
                    if (value == null && !privateTargetForm.IsDisposed)
                    {
                        privateTargetForm.Region?.Dispose();
                        privateTargetForm.Region = null;
                    }

                    if (privateTargetForm != value)
                    {
                        privateTargetForm.Load -= TargetForm_Load;
                        privateTargetForm.Resize -= TargetForm_Resize;
                        privateTargetForm.LocationChanged -= TargetForm_LocationChanged;
                        privateTargetForm.FormClosing -= TargetForm_FormClosing;
                        privateTargetForm.VisibleChanged -= TargetForm_VisibleChanged;
                        privateTargetForm.Activated -= TargetForm_Activated;
                        privateTargetForm.HandleCreated -= TargetForm_HandleCreated;
                        privateTargetForm.ResizeEnd -= TargetForm_ResizeEnd;
                    }
                }

                privateTargetForm = value;

                if (value == null)
                {
                    roundedFormObj?.UpdBitmap();
                    roundedFormObj?.Hide();
                    return;
                }

                if (FormsRegisteredByRounder.AddByForm(value, this) == false)
                {
                    throw new Exception("This form already has a cuiformRounder");
                }

                TargetForm.Load += TargetForm_Load;
                TargetForm.Resize += TargetForm_Resize;
                TargetForm.LocationChanged += TargetForm_LocationChanged;
                TargetForm.FormClosing += TargetForm_FormClosing;
                TargetForm.VisibleChanged += TargetForm_VisibleChanged;
                TargetForm.Activated += TargetForm_Activated;
                TargetForm.HandleCreated += TargetForm_HandleCreated;
                TargetForm.ResizeEnd += TargetForm_ResizeEnd;

                if (roundedFormObj != null && roundedFormObj.IsDisposed == false && !DesignMode) // if for some reason you want to toggle between a form and 'null'
                {
                    UpdateRoundedFormRegion();

                    TargetForm_LocationChanged(this, EventArgs.Empty);
                    TargetForm_Resize(this, EventArgs.Empty);

                    roundedFormObj?.Show();
                }
            }
        }

        private EventHandler tenFramesDrawnHandler;


        private void TargetForm_ResizeEnd(object sender, EventArgs e)
        {
            UpdateExperimentalBitmap();
        }

        private void TargetForm_HandleCreated(object sender, EventArgs e)
        {
            FormsRegisteredByRounder.AddByForm(TargetForm, this);

            if (!DesignMode && TargetForm != null)
            {
                int disable = 1;
                DwmSetWindowAttribute(TargetForm.Handle, -3, ref disable, sizeof(int));
            }
        }

        bool targetFormActivating = false;

        private void TargetForm_Activated(object sender, EventArgs e)
        {
            if (shouldCloseDown)
            {
                return;
            }

            if (!targetFormActivating)
            {
                targetFormActivating = true;
                FakeForm_Activated(sender, e);
            }
        }

        private void TargetForm_VisibleChanged(object sender, EventArgs e)
        {
            if (shouldCloseDown)
            {
                return;
            }

            if (!DesignMode && roundedFormObj != null && !wasFormClosingCalled && roundedFormObj.IsDisposed == false)
            {
                roundedFormObj.Visible = TargetForm.Visible;
                roundedFormObj.Tag = TargetForm.Opacity;
                roundedFormObj.InvalidateNextDrawCall = true;
            }
        }

        private bool shouldCloseDown = false;
        private bool wasFormClosingCalled = false;

        private void TargetForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // causes the other methods inside this class to not run
            // that stops any exceptions in the code trying to access disposed or null stuff 
            shouldCloseDown = true;


            if (tenFramesDrawnHandler != null)
            {
                DrawingHelper.TenFramesDrawn -= tenFramesDrawnHandler;
                tenFramesDrawnHandler = null;
            }

            if (!wasFormClosingCalled && TargetForm != null)
            {
                wasFormClosingCalled = true;

                // detach events
                privateTargetForm.Load -= TargetForm_Load;
                privateTargetForm.Resize -= TargetForm_Resize;
                privateTargetForm.LocationChanged -= TargetForm_LocationChanged;
                privateTargetForm.FormClosing -= TargetForm_FormClosing;
                privateTargetForm.VisibleChanged -= TargetForm_VisibleChanged;
                privateTargetForm.Activated -= TargetForm_Activated;
                privateTargetForm.HandleCreated -= TargetForm_HandleCreated;
                privateTargetForm.ResizeEnd -= TargetForm_ResizeEnd;

                // clean controls from targetform
                TargetForm.Controls.Clear();

                // send close message to all forms
                TryCloseForm(TargetForm);
                TryCloseForm(roundedFormObj);
            }
        }

        void TryCloseForm(Form f)
        {
            if (f != null && !f.IsDisposed && f.IsHandleCreated)
            {
                Helpers.WindowsHelper.SendMessage(f.Handle, 0x0010, IntPtr.Zero, IntPtr.Zero);
            }
        }

        public void FakeForm_Activated(object sender, EventArgs e)
        {
            if (DesignMode || shouldCloseDown || wasFormClosingCalled || DesignMode || TargetForm == null || TargetForm.IsDisposed)
            {
                return;
            }

            if (roundedFormObj != null && !roundedFormObj.IsDisposed)
            {
                roundedFormObj.Tag = TargetForm.Opacity;
                roundedFormObj.InvalidateNextDrawCall = true;

                // https://github.com/1Kxhu/HartUI/issues/11 fix #3
                if (roundedFormObj.WindowState != FormWindowState.Minimized)
                {
                    if (!wasFormClosingCalled && !shouldCloseDown)
                    {
                        if (TargetForm.WindowState != FormWindowState.Minimized)
                        {
                            SetWindowPos(roundedFormObj.Handle, HWND_TOP, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);
                            SetWindowPos(TargetForm.Handle, HWND_TOP, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);
                        }

                        TargetForm.BringToFront();
                    }
                }
            }

            targetFormActivating = false;
        }

        private static Point PointSubtract(Point p1, Point p2)
        {
            return new Point(p1.X - p2.X, p1.Y - p2.Y);
        }

        private int privateRounding = 8;

        [Category("HartUI")]
        public int Rounding
        {
            get => privateRounding;
            set
            {
                privateRounding = value;
                if (shouldCloseDown)
                {
                    return;
                }
                if (roundedFormObj != null && roundedFormObj.IsDisposed == false)
                {
                    roundedFormObj.Rounding = value;
                    if (TargetForm != null)
                    {
                        UpdateTargetFormRegion();
                        UpdateRoundedFormRegion();
                        roundedFormObj?.Invalidate();
                    }
                }
            }
        }

        private async void TargetForm_LocationChanged(object sender, EventArgs e)
        {
            // inlined not to repeat ourselves later
            bool IsSafeToEditRoundedForm() =>
                !shouldCloseDown &&
                !DesignMode &&
                roundedFormObj != null &&
                TargetForm != null &&
                !roundedFormObj.IsDisposed && roundedFormObj.initialized;

            if (IsSafeToEditRoundedForm())
            {
                roundedFormObj.Location = PointSubtract(TargetForm.Location, new Point(1, 1));
                UpdateRoundedFormRegion();

                // https://github.com/1Kxhu/HartUI/issues/11 fix #2
                if (TargetForm.WindowState == FormWindowState.Minimized)
                {
                    roundedFormObj?.Hide();
                }
                else
                {
                    await Task.Delay(1000 / DrawingHelper.GetHighestRefreshRate());

                    // check same flags again because some time had passed
                    if (IsSafeToEditRoundedForm())
                    {
                        roundedFormObj?.Show();
                    }
                }
            }
        }

        private Color privateOutlineColor = Color.FromArgb(32, 128, 128, 128);

        [Category("HartUI")]
        public Color OutlineColor
        {
            get => privateOutlineColor;
            set
            {
                privateOutlineColor = value;
                if (shouldCloseDown)
                {
                    return;
                }

                if (roundedFormObj != null && roundedFormObj.IsDisposed == false)
                {
                    roundedFormObj?.Invalidate();
                }
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOACTIVATE = 0x0010;
        private static readonly IntPtr HWND_TOP = new IntPtr(0);

        private void UpdateTargetFormRegion()
        {
            IntPtr hrgn = CreateRoundRectRgn(0, 0, TargetForm.Width, TargetForm.Height, (int)(Rounding * 2f), (int)(Rounding * 2f));

            try
            {
                Region newRegion = Region.FromHrgn(hrgn);

                TargetForm.Region?.Dispose();
                TargetForm.Region = newRegion;
            }
            finally
            {
                DeleteObject(hrgn);
            }
        }

        public void UpdateRoundedFormRegion()
        {
            if (roundedFormObj == null || roundedFormObj.IsDisposed)
            {
                return;
            }

            Region newRegion = null;

            try
            {
                if (!DesignMode || TargetForm?.Opacity != 1)
                {
                    newRegion = new Region(roundedFormObj.ClientRectangle);

                    using (Region offsetRegion = TargetForm.Region?.Clone())
                    {
                        if (offsetRegion != null)
                        {
                            offsetRegion.Translate(1, 1);
                            newRegion.Exclude(offsetRegion);
                        }
                    }
                }
                else
                {
                    newRegion = new Region(roundedFormObj.ClientRectangle);
                }

                roundedFormObj.Region?.Dispose();
                roundedFormObj.Region = newRegion;
                newRegion = null;
            }
            finally
            {
                newRegion?.Dispose();
            }
        }

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int dwAttribute, ref int pvAttribute, int cbAttribute);

        private void TargetForm_Load(object sender, EventArgs e)
        {
            // initialize rounding
            FakeForm_Activated(sender, e);

            TargetForm.FormBorderStyle = FormBorderStyle.None;

            if (roundedFormObj != null)
            {
                roundedFormObj.Activated -= FakeForm_Activated;
                roundedFormObj.Dispose();
            }

            roundedFormObj = new RoundedForm(TargetForm.BackColor, OutlineColor, ref privateRounding);
            roundedFormObj.TargetForm = TargetForm;
            roundedFormObj.Tag = TargetForm.Opacity;

            UpdateTargetFormRegion();

            roundedFormObj?.Show();

            //FakeForm.Show();

            roundedFormObj.Activated += FakeForm_Activated;
            TargetForm_Resize(this, EventArgs.Empty);
            TargetForm_LocationChanged(this, EventArgs.Empty);

            TargetForm.FormBorderStyle = FormBorderStyle.None;

            // fix for the program not appearing in the taskbar until you'd interacted with the window
            if (TargetForm.ShowInTaskbar)
            {
                TargetForm.BringToFront();
                TargetForm.Activate();
                TargetForm.Focus();
                TargetForm.Location = TargetForm.Location;

                if (!DesignMode && TargetForm != null)
                {
                    int enable = 0;
                    DwmSetWindowAttribute(TargetForm.Handle, -3, ref enable, sizeof(int));
                }
            }

            // Drawing.TenFramesDrawn is called every 10000/hz milliseconds
            // where hz stands for the maximum refresh rate recorded from all display devices
            tenFramesDrawnHandler = DrawingHelper_TenFramesDrawn;
            DrawingHelper.TenFramesDrawn += tenFramesDrawnHandler;
        }

        private void DrawingHelper_TenFramesDrawn(object sender, EventArgs e)
        {
            if (roundedFormObj != null && shouldCloseDown == false && roundedFormObj.IsDisposed == false)
            {
                roundedFormObj.Tag = TargetForm.Opacity;
                roundedFormObj.InvalidateNextDrawCall = true;

                // TopMost smooth corners n border disappear fix
                if (TargetForm != null && TargetForm.IsDisposed == false && TargetForm.TopMost != roundedFormObj.TopMost)
                {
                    roundedFormObj.TopMost = TargetForm.TopMost;
                }
            }
            else if (roundedFormObj?.initialized == true)
            {
                // either roundedFormObj is null or "stop" is true
                // stop is true when the form had announced it wants to close (see TargetForm_FormClosing)
                Dispose();
            }
        }

        Bitmap experimentalBitmap
        {
            get; set;
        }

        // truly the smooth corner experience (tears of joy as of writing this)
        private void UpdateExperimentalBitmap()
        {
            if (DesignMode ||
                TargetForm == null || TargetForm.IsDisposed ||
                shouldCloseDown ||
                roundedFormObj == null || roundedFormObj.IsDisposed ||
                !roundedFormObj.initialized)
            {
                return;
            }

            if (Rounding == 0)
            {
                experimentalBitmap?.Dispose();
                experimentalBitmap = null;
                roundedFormObj.UpdBitmap();
                return;
            }

            int targetWidth = Math.Max(TargetForm.Width + 1, 1);
            int targetHeight = Math.Max(TargetForm.Height + 1, 1);

            Bitmap newBitmap = new Bitmap(targetWidth, targetHeight);

            using (Graphics g = Graphics.FromImage(newBitmap))
            {
                g.Clear(Color.Transparent);
                TargetForm.DrawToBitmap(newBitmap, new Rectangle(0, 0, TargetForm.Width, TargetForm.Height));
            }

            experimentalBitmap?.Dispose();
            experimentalBitmap = newBitmap;

            roundedFormObj.UpdBitmap(newBitmap);
        }

        FormWindowState lastState;

        private void TargetForm_Resize(object sender, EventArgs e)
        {
            if (DesignMode || TargetForm == null || TargetForm.IsDisposed || shouldCloseDown || roundedFormObj == null || roundedFormObj.IsDisposed || !roundedFormObj.initialized)
            {
                return;
            }

            // If windowstate has changed, set said windowstate value to all the other forms
            if (TargetForm.WindowState != lastState)
            {
                lastState = TargetForm.WindowState;
                roundedFormObj.WindowState = TargetForm.WindowState;
            }

            if (TargetForm.WindowState != FormWindowState.Minimized)
            {
                // Related to how RoundedForm is drawn
                // Updates rounding if needed, too
                roundedFormObj.Size = System.Drawing.Size.Add(TargetForm.Size, new Size(2, 2));
                UpdateRoundedFormRegion();
                UpdateExperimentalBitmap();

                roundedFormObj.InvalidateNextDrawCall = true;

                if (TargetForm.WindowState == FormWindowState.Normal)
                {
                    UpdateTargetFormRegion();
                }
                else
                {
                    TargetForm.Region?.Dispose();
                    TargetForm.Region = null;
                }
            }

            targetFormActivating = false;
        }
    }
}
