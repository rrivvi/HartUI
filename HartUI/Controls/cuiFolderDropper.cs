using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace HartUI.Controls
{
    [Description("Lets the user select a folder or drop it onto the control")]
    [DefaultEvent("FolderDropped")]
    public partial class cuiFolderDropper : cuiFileDropper
    {
        public cuiFolderDropper()
        {
            InitializeComponent();
            AllowDrop = true;
            ForeColor = Color.Gray;
            Cursor = Cursors.Hand;
            Size = new Size(270, 135);
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            NormalContent = "Drop folder here";
        }


        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new string Filter
        {
            get => base.Filter;
            set { }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public new string[] GetExtensionsFromFilter() => Array.Empty<string>();

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new string FileName => base.FileName;

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new string[] FileNames => base.FileNames;

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler<FileDroppedEventArgs> FileDropped
        {
            add { }
            remove { }
        }

        [Category("HartUI")]
        public string FolderName { get; private set; }

        [Category("HartUI")]
        public string[] FolderNames { get; private set; }

        [Category("HartUI")]
        public event EventHandler<FolderDroppedEventArgs> FolderDropped;

        protected override void HandleDroppedPaths(string[] paths)
        {
            // cuiFolderDropper specific
            var validFolders = paths.Where(f => Directory.Exists(f));

            if (validFolders.Count() == 0)
            {
                return;
            }

            FolderNames = validFolders.ToArray();
            FolderName = FolderNames[0];

            if (FolderNames.Length > 1)
            {
                FolderDropped?.Invoke(null, new FolderDroppedEventArgs(FolderNames));
            }
            else
            {
                FolderDropped?.Invoke(null, new FolderDroppedEventArgs(FolderName));
            }
        }

        protected override void PerformUpload()
        {
            if (!UploadWithClick)
            {
                return;
            }

            bool MultiselectNow = Multiselect;

            using (OpenFolderDialog ofd = new OpenFolderDialog() { Multiselect = MultiselectNow })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    FolderName = ofd.FolderName;
                    FolderNames = ofd.FolderNames.ToArray();

                    if (MultiselectNow)
                    {
                        FolderDropped?.Invoke(null, new FolderDroppedEventArgs(FolderNames));
                    }
                    else
                    {
                        FolderDropped?.Invoke(null, new FolderDroppedEventArgs(FolderName));
                    }
                }
            }
        }
    }

    public class FolderDroppedEventArgs : EventArgs
    {
        public FolderDroppedEventArgs(string folderName)
        {
            FolderName = folderName;
            FolderNames = new string[] { folderName };
            OneFolderDropped = true;
        }

        public FolderDroppedEventArgs(string[] folderNames)
        {
            FolderNames = folderNames;
            FolderName = folderNames.FirstOrDefault();

            if (folderNames.Length == 1)
            {
                OneFolderDropped = true;
            }
        }

        public bool OneFolderDropped { get; private set; } = false;
        public string FolderName;
        public string[] FolderNames { get; }
    }
}
