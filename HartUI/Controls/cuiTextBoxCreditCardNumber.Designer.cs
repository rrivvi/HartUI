namespace HartUI.Controls
{
    partial class cuiTextBoxCreditCardNumber
    {
        /// <summary> 
        /// Wymagana zmienna projektanta.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Wyczyść wszystkie używane zasoby.
        /// </summary>
        /// <param name="disposing">prawda, jeżeli zarządzane zasoby powinny zostać zlikwidowane; Fałsz w przeciwnym wypadku.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Kod wygenerowany przez Projektanta składników

        /// <summary> 
        /// Metoda wymagana do obsługi projektanta — nie należy modyfikować 
        /// jej zawartości w edytorze kodu.
        /// </summary>
        private void InitializeComponent()
        {
            this.panel1 = new System.Windows.Forms.Panel();
            this.placeholderTextField = new System.Windows.Forms.TextBox();
            this.contentTextField = new HartUI.Controls.Internal.DigitsOnlyTextBox();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(0, 0);
            this.panel1.TabIndex = 1;
            // 
            // placeholderTextField
            // 
            this.placeholderTextField.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.placeholderTextField.Dock = System.Windows.Forms.DockStyle.Fill;
            this.placeholderTextField.ForeColor = System.Drawing.Color.LightGray;
            this.placeholderTextField.Location = new System.Drawing.Point(10, 7);
            this.placeholderTextField.Name = "placeholderTextField";
            this.placeholderTextField.ReadOnly = true;
            this.placeholderTextField.Size = new System.Drawing.Size(246, 16);
            this.placeholderTextField.TabIndex = 2;
            this.placeholderTextField.TextChanged += new System.EventHandler(this.textBox2_TextChanged);
            this.placeholderTextField.MouseDown += new System.Windows.Forms.MouseEventHandler(this.textBox2_MouseDown);
            // 
            // contentTextField
            // 
            this.contentTextField.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.contentTextField.Dock = System.Windows.Forms.DockStyle.Fill;
            this.contentTextField.Location = new System.Drawing.Point(10, 7);
            this.contentTextField.Name = "contentTextField";
            this.contentTextField.Size = new System.Drawing.Size(246, 16);
            this.contentTextField.TabIndex = 0;
            this.contentTextField.Click += new System.EventHandler(this.textBox1_Click);
            this.contentTextField.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            this.contentTextField.Enter += new System.EventHandler(this.textBox1_Enter);
            this.contentTextField.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox1_KeyPress);
            this.contentTextField.Leave += new System.EventHandler(this.textBox1_Leave);
            this.contentTextField.MouseEnter += new System.EventHandler(this.textBox1_MouseEnter);
            this.contentTextField.MouseLeave += new System.EventHandler(this.textBox1_MouseLeave);
            // 
            // cuiTextBoxCreditCardNumber
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.placeholderTextField);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.contentTextField);
            this.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "cuiTextBoxCreditCardNumber";
            this.Padding = new System.Windows.Forms.Padding(10, 7, 10, 7);
            this.Size = new System.Drawing.Size(266, 45);
            this.Click += new System.EventHandler(this.cuiTextBox2_Click);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox placeholderTextField;
        internal Controls.Internal.DigitsOnlyTextBox contentTextField;
    }
}
