using HartUI.Helpers;
using System.Windows.Forms;

namespace HartUI.Misc.Internal
{
    internal static class PreloadedForms
    {
        static bool mustPreload = true;

        internal static void TryPreloadForms()
        {
            if (WindowsHelper.IsInDesignMode() || !mustPreload)
            {
                return;
            }
            mustPreload = false;

            void PreloadForm(Form f)
            {
                // this workaround gets rid of a white background when later calling .Show() on this form
                f.Location = new System.Drawing.Point(1, 1) - f.Size;
                f.Show();
                f.Hide();
            }

            PreloadForm(ComboBoxDropDownForm);
        }

        internal static Controls.Forms.Internal.ComboBoxDropDownForm ComboBoxDropDownForm { get; private set; } = new Controls.Forms.Internal.ComboBoxDropDownForm();
    }
}
