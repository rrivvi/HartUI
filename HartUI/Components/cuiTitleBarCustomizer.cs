using HartUI.Helpers;
using HartUI.Misc.Internal;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace HartUI.Components
{
    [Designer(typeof(DesignerIntegration.HartComponentDesigner))]
    [Description("Toggles a form's title bar between light and dark mode")]
    public partial class cuiTitleBarCustomizer : Component
    {
        public cuiTitleBarCustomizer()
        {
            InitializeComponent();
        }

        public cuiTitleBarCustomizer(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        private Form privateTargetForm;

        [Category("HartUI")]
        public Form TargetForm
        {
            get => privateTargetForm;
            set
            {
                if (privateTargetForm != null)
                {
                    privateTargetForm.HandleCreated -= TargetForm_HandleCreated;
                }

                privateTargetForm = value;

                if (privateTargetForm != null)
                {
                    privateTargetForm.HandleCreated += TargetForm_HandleCreated;

                    if (privateTargetForm.IsHandleCreated)
                    {
                        ApplyTitleBarStyle();
                    }
                }
            }
        }

        private void TargetForm_HandleCreated(object sender, System.EventArgs e)
        {
            ApplyTitleBarStyle();
        }

        private bool privateDarkMode = false;

        [Category("HartUI")]
        [Description("Whether the title bar should use dark mode.")]
        public bool DarkMode
        {
            get => privateDarkMode;
            set
            {
                privateDarkMode = value;
                ApplyTitleBarStyle();
            }
        }

        private Color privateTitleBarBackColor = Color.Empty;

        [Category("HartUI - Windows 11 ONLY")]
        [DisplayName("(Win11) Background Color")]
        [Description("The background color of the title bar. Leave empty for the default color (let Windows decide).")]
        public Color TitleBarBackColor
        {
            get => privateTitleBarBackColor;
            set
            {
                privateTitleBarBackColor = value;
                ApplyTitleBarStyle();
            }
        }

        private Color privateTitleBarTextColor = Color.Empty;

        [Category("HartUI - Windows 11 ONLY")]
        [DisplayName("(Win11) Text Color")]
        [Description("The color of the title bar text. Leave empty for the default color (let Windows decide).")]
        public Color TitleBarTextColor
        {
            get => privateTitleBarTextColor;
            set
            {
                privateTitleBarTextColor = value;
                ApplyTitleBarStyle();
            }
        }

        private void ApplyTitleBarStyle()
        {
            if (DesignMode ||
                TargetForm == null ||
                TargetForm.IsDisposed ||
                !TargetForm.IsHandleCreated)
            {
                return;
            }

            WindowsHelper.SetTitleBarDark(TargetForm, privateDarkMode);

            if (WindowsHelper.IsWindows11())
            {
                WindowsHelper.SetTitleBarBackColor(TargetForm, privateTitleBarBackColor);
                WindowsHelper.SetTitleBarTextColor(TargetForm, privateTitleBarTextColor);
            }
        }
    }
}