using HartUI.Components.Forms;
using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HartUI.Components
{
    [Description("Modern dialog which disables interaction with the form until option is chosen")]
    public partial class cuiMessageDialog : Component
    {
        public cuiMessageDialog()
        {
            InitializeComponent();
        }

        [Category("HartUI Dimmer Colors")]
        public Color DimColor { get; set; } = Color.FromArgb(160, 0, 0, 0);

        [Category("HartUI Dialog")]
        public Size ButtonSize { get; set; } = new Size(80, 32);

        [Category("HartUI Dialog")]
        public Padding DialogPadding { get; set; } = new Padding(20);

        private DialogResult result = DialogResult.None;

        [Category("HartUI Dialog Colors")]
        public Color BackColor { get; set; } = Color.White;

        [Category("HartUI Dialog Colors")]
        public Color ForeColor { get; set; } = Color.Black;

        [Category("HartUI Dialog")]
        [Description("Height automatically adjusts if the text is too big.")]
        public Size DialogSize { get; set; } = new Size(300, 200);

        [Category("HartUI Dialog")]
        [Description("How rounded should the dialog box be?")]
        public int Rounding { get; set; } = 8;

        //

        [Category("HartUI Dialog Text")]
        public string OKText { get; set; } = "OK";

        [Category("HartUI Dialog Text")]
        public string YesText { get; set; } = "Yes";

        [Category("HartUI Dialog Text")]
        public string NoText { get; set; } = "No";

        [Category("HartUI Dialog Text")]
        public string CancelText { get; set; } = "Cancel";

        //

        public async Task<DialogResult> ShowDialog(Form parentForm, string description, string title = "")
        {
            return await ShowDialog(parentForm, description, title, MessageBoxButtons.OK);
        }

        public async Task<DialogResult> ShowDialog(Form parentForm, string description, string title, MessageBoxButtons messageBoxButtons)
        {
            using (MessageDialog md = new MessageDialog())
            {
                md.OKText = OKText;
                md.CancelText = CancelText;
                md.YesText = YesText;
                md.NoText = NoText;

                md.Rounding = Rounding;
                md.Size = DialogSize;
                md.DimColor = DimColor;
                md.DialogPadding = DialogPadding;
                md.DialogResult = result;
                md.BackColor = BackColor;
                md.ForeColor = ForeColor;

                return await md.ShowDialog(parentForm, description, title, messageBoxButtons, DialogSize, ButtonSize);
            }
        }
    }
}
