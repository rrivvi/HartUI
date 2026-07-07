using HartUI.Helpers;

namespace HartUI.Components
{
    partial class cuiFormRounder
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                TargetForm = null;

                if (tenFramesDrawnHandler != null)
                {
                    DrawingHelper.TenFramesDrawn -= tenFramesDrawnHandler;
                    tenFramesDrawnHandler = null;
                }

                experimentalBitmap?.Dispose();
                experimentalBitmap = null;

                if (roundedFormObj != null)
                {
                    roundedFormObj.Activated -= FakeForm_Activated;
                    roundedFormObj.Dispose();
                    roundedFormObj = null;
                }

                components?.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
        }

        #endregion
    }
}
