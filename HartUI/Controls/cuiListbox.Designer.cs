namespace HartUI.Controls
{
    partial class cuiListbox
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
                hookedParent.BackColorChanged -= HandleParentBackColorChanged;
                hookedParent = null;
            }
            base.Dispose(disposing);
        }

        #region Kod wygenerowany przez Projektanta składników

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // cuiListbox
            // 
            this.MouseLeave += new System.EventHandler(this.cuiListbox_MouseLeave);
            this.ResumeLayout(false);

        }

        #endregion
    }
}
