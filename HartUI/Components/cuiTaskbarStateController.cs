using Microsoft.WindowsAPICodePack.Taskbar;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static HartUI.Helpers.WindowsHelper;

namespace HartUI.Components
{
    [Description("Controls the form's taskbar icon's look")]
    public partial class cuiTaskbarStateController : Component
    {
        private Form privateTargetForm;

        [Category("HartUI")]
        public Form TargetForm
        {
            get => privateTargetForm;
            set
            {
                if (privateTargetForm != null)
                {
                    privateTargetForm.Shown -= TargetForm_Shown;
                }
                privateTargetForm = value;
                if (privateTargetForm != null)
                {
                    privateTargetForm.Shown += TargetForm_Shown;
                }
            }
        }

        private void TargetForm_Shown(object sender, EventArgs e)
        {
            UpdateTaskbarState();
        }

        public cuiTaskbarStateController()
        {
            InitializeComponent();
            MaxValue = 100;
        }

        public cuiTaskbarStateController(IContainer container)
        {
            container.Add(this);
            InitializeComponent();
            MaxValue = 100;
        }

        public enum TaskbarStates
        {
            Progress = 0,
            Paused = 1,
            Error = 2,
            Default = 3,
        }

        private TaskbarStates privateState = TaskbarStates.Default;

        [Category("HartUI")]
        public TaskbarStates State
        {
            get
            {
                return privateState;
            }
            set
            {
                privateState = value;
                UpdateTaskbarState();
            }
        }

        private int privateProgressValue = 50;

        [Category("HartUI")]
        public int ProgressValue
        {
            get
            {
                return privateProgressValue;
            }
            set
            {
                privateProgressValue = value;
                UpdateTaskbarState();
            }
        }

        private int privateMaxValue = 100;

        [Category("HartUI")]
        public int MaxValue
        {
            get
            {
                return privateMaxValue;
            }
            set
            {
                privateMaxValue = value;
                UpdateTaskbarState();
            }
        }

        private void UpdateTaskbarState()
        {
            if (privateTargetForm == null || DesignMode || privateTargetForm.Handle == IntPtr.Zero)
            {
                return;
            }

            var taskbar = TaskbarManager.Instance;

            switch (privateState)
            {
                case TaskbarStates.Progress:
                    taskbar.SetProgressState(TaskbarProgressBarState.Normal, privateTargetForm.Handle);
                    break;
                case TaskbarStates.Paused:
                    taskbar.SetProgressState(TaskbarProgressBarState.Paused, privateTargetForm.Handle);
                    break;
                case TaskbarStates.Error:
                    taskbar.SetProgressState(TaskbarProgressBarState.Error, privateTargetForm.Handle);
                    break;
                case TaskbarStates.Default:
                default:
                    taskbar.SetProgressState(TaskbarProgressBarState.NoProgress, privateTargetForm.Handle);
                    taskbar.SetProgressValue(0, 0, privateTargetForm.Handle);
                    return;
            }

            taskbar.SetProgressValue(privateProgressValue, privateMaxValue, privateTargetForm.Handle);
        }

        public void Flash()
        {
            if (privateTargetForm == null || DesignMode || privateTargetForm.Handle == IntPtr.Zero)
                return;

            var fInfo = new FLASHWINFO
            {
                cbSize = (uint)Marshal.SizeOf(typeof(FLASHWINFO)),
                hwnd = privateTargetForm.Handle,
                dwFlags = FLASHW_ALL | FLASHW_TIMERNOFG,
                uCount = uint.MaxValue,
                dwTimeout = 0
            };

            FlashWindowEx(ref fInfo);
        }
    }
}
