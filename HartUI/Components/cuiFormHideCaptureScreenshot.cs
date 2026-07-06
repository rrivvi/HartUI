using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace HartUI.Components
{
    [ToolboxBitmap(typeof(MaskedTextBox))]
    [Description("Hides your form from screenshots and screen recordings.")]
    public partial class cuiFormHideCaptureScreenshot : Component
    {
        public cuiFormHideCaptureScreenshot()
        {
            InitializeComponent();
        }

        internal void SetExclusionType(ExclusionTypeEnum targetExclusionType)
        {
            if (!DesignMode)
            {
                if (privateTargetForm != null && !privateTargetForm.IsDisposed)
                {
                    cuiFormRounder formRounder = FormsRegisteredByRounder.GetRounderByForm(privateTargetForm);
                    if (formRounder != null)
                    {
                        Form roundedForm = formRounder.roundedFormObj;
                        if (roundedForm != null && !roundedForm.IsDisposed)
                        {
                            Helpers.WindowsHelper.SetWindowDisplayAffinity(roundedForm.Handle, (uint)ExclusionTypeEnum.None);
                            Helpers.WindowsHelper.SetWindowDisplayAffinity(roundedForm.Handle, (uint)targetExclusionType);
                        }
                    }

                    // Switching from Black -> Disappear or Disappear -> Black didn't always work
                    // This line fixes that issue for some reason..
                    Helpers.WindowsHelper.SetWindowDisplayAffinity(TargetForm.Handle, (uint)ExclusionTypeEnum.None);
                    Helpers.WindowsHelper.SetWindowDisplayAffinity(TargetForm.Handle, (uint)targetExclusionType);
                }
            }
        }

        public enum ExclusionTypeEnum
        {
            None = 0,
            Black = 1,
            Disappear = 17
        }

        private ExclusionTypeEnum privateExclusionType = ExclusionTypeEnum.Disappear;

        [Category("HartUI")]
        public ExclusionTypeEnum ExclusionType
        {
            get
            {
                return privateExclusionType;
            }
            set
            {
                privateExclusionType = value;
                SetExclusionType(value);
            }
        }

        private Form privateTargetForm = null;

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
                    SetExclusionType(ExclusionTypeEnum.None);
                    privateTargetForm.HandleCreated -= RefreshExclusionType;
                    privateTargetForm.Load -= RefreshExclusionType;
                }

                privateTargetForm = value;

                if (privateTargetForm != null)
                {
                    SetExclusionType(ExclusionType);
                    privateTargetForm.HandleCreated += RefreshExclusionType;
                    privateTargetForm.Load += RefreshExclusionType;
                }
            }
        }

        public void RefreshExclusionType(object sender = null, EventArgs e = null)
        {
            SetExclusionType(ExclusionType);
        }
    }
}
