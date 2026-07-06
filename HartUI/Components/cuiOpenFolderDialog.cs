using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace HartUI.Components
{
    [Description("The proper OpenFolderDialog, because of the outdated FolderBrowserDialog")]
    [ToolboxBitmap(typeof(FolderBrowserDialog))]
    public partial class cuiOpenFolderDialog : Component
    {
        public cuiOpenFolderDialog()
        {
            InitializeComponent();
        }

        [Category("HartUI")]
        [Description("Leave empty for default system translation.")]
        public string Title
        {
            get;
            set;
        }

        [Category("HartUI")]
        [Description("Controls whether multiple folders can be selected in the dialog.")]
        public bool Multiselect
        {
            get;
            set;
        }

        [Category("HartUI")]
        [Description("Leave empty for default system translation.")]
        public string OkButtonLabel
        {
            get;
            set;
        }

        [Category("HartUI")]
        [Browsable(false)]
        public string FolderName
        {
            get;
            private set;
        }

        [Category("HartUI")]
        [Browsable(false)]
        public IReadOnlyList<string> FolderNames
        {
            get;
            private set;
        }

        public DialogResult ShowDialog()
        {
            using (OpenFolderDialog ofd = new OpenFolderDialog())
            {
                ofd.Title = Title;
                ofd.Multiselect = Multiselect;
                ofd.OkButtonLabel = OkButtonLabel;

                DialogResult ofdResult = ofd.ShowDialog();

                if (ofdResult != DialogResult.Cancel)
                {
                    FolderName = ofd.FolderName;
                    FolderNames = ofd.FolderNames;
                }

                return ofdResult;
            }
        }
    }
}
