using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;

// This class is an almost 1:1 copy of Simon Mourier's stackoverflow answer.
// Find more at https://stackoverflow.com/a/66187224

namespace HartUI
{
    public class OpenFolderDialog : IDisposable
    {
        private IFileOpenDialog dialog = (IFileOpenDialog)new FileOpenDialog();
        public bool IsDisposed { get; private set; }
        private readonly List<string> _resultPaths = new List<string>();
        private readonly List<string> _resultNames = new List<string>();

        public IReadOnlyList<string> FolderNames => _resultPaths;

        public IReadOnlyList<string> SafeFolderNames => _resultNames;

        public string FolderName => FolderNames.FirstOrDefault();

        public string SafeFolderName => SafeFolderNames.FirstOrDefault();

        public string InputPath { get; set; }

        public bool ForceFileSystem { get; set; }

        public bool Multiselect { get; set; }

        public string Title { get; set; }

        public string OkButtonLabel { get; set; }

        public string FileNameLabel { get; set; }

        public DialogResult ShowDialog() => ShowDialog(IntPtr.Zero);

        public DialogResult ShowDialog(IntPtr owner, bool throwOnError = false)
        {
            if (!string.IsNullOrEmpty(InputPath))
            {
                if (CheckHr(SHCreateItemFromParsingName(InputPath, null, typeof(IShellItem).GUID, out var item), throwOnError) != 0)
                    return DialogResult.None;

                dialog.SetFolder(item);
            }

            var options = FOS.FOS_PICKFOLDERS;
            options = (FOS)SetOptions((int)options);
            dialog.SetOptions(options);

            if (Title != null)
            {
                dialog.SetTitle(Title);
            }

            if (OkButtonLabel != null)
            {
                dialog.SetOkButtonLabel(OkButtonLabel);
            }

            if (FileNameLabel != null)
            {
                dialog.SetFileName(FileNameLabel);
            }

            if (owner == IntPtr.Zero)
            {
                owner = Process.GetCurrentProcess().MainWindowHandle;
                if (owner == IntPtr.Zero)
                {
                    owner = GetDesktopWindow();
                }
            }

            var hr = dialog.Show(owner);
            if (hr == unchecked((int)0x800704C7))
                return DialogResult.Cancel;

            if (CheckHr(hr, throwOnError) != 0)
                return DialogResult.None;

            if (CheckHr(dialog.GetResults(out var items), throwOnError) != 0)
                return DialogResult.None;

            items.GetCount(out var count);
            for (var i = 0; i < count; i++)
            {
                items.GetItemAt(i, out var item);
                CheckHr(item.GetDisplayName(SIGDN.SIGDN_DESKTOPABSOLUTEPARSING, out var path), throwOnError);
                CheckHr(item.GetDisplayName(SIGDN.SIGDN_DESKTOPABSOLUTEEDITING, out var name), throwOnError);
                if (path != null || name != null)
                {
                    _resultPaths.Add(path);
                    _resultNames.Add(name);
                }
            }

            return DialogResult.OK;
        }

        protected virtual int SetOptions(int options)
        {
            if (ForceFileSystem)
            {
                options |= (int)FOS.FOS_FORCEFILESYSTEM;
            }

            if (Multiselect)
            {
                options |= (int)FOS.FOS_ALLOWMULTISELECT;
            }
            return options;
        }

        private static int CheckHr(int hr, bool throwOnError)
        {
            if (hr != 0 & throwOnError)
                Marshal.ThrowExceptionForHR(hr);
            return hr;
        }

        [DllImport("shell32.dll")]
        private static extern int SHCreateItemFromParsingName(
          [MarshalAs(UnmanagedType.LPWStr)] string pszPath,
          IBindCtx pbc,
          [MarshalAs(UnmanagedType.LPStruct)] Guid riid,
          out OpenFolderDialog.IShellItem ppv);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDesktopWindow();

        [ComImport, Guid("DC1C5A9C-E88A-4dde-A5A1-60F82A20AEF7")] // CLSID_FileOpenDialog
        private class FileOpenDialog { }

        [ComImport, Guid("d57c7288-d4ad-4768-be02-9d969532d960"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IFileOpenDialog
        {
            [PreserveSig] int Show(IntPtr parent); // IModalWindow
            [PreserveSig] int SetFileTypes();
            [PreserveSig] int SetFileTypeIndex(int iFileType);
            [PreserveSig] int GetFileTypeIndex(out int piFileType);
            [PreserveSig] int Advise();
            [PreserveSig] int Unadvise();
            [PreserveSig] int SetOptions(FOS fos);
            [PreserveSig] int GetOptions(out FOS pfos);
            [PreserveSig] int SetDefaultFolder(IShellItem psi);
            [PreserveSig] int SetFolder(IShellItem psi);
            [PreserveSig] int GetFolder(out IShellItem ppsi);
            [PreserveSig] int GetCurrentSelection(out IShellItem ppsi);
            [PreserveSig] int SetFileName([MarshalAs(UnmanagedType.LPWStr)] string pszName);
            [PreserveSig] int GetFileName([MarshalAs(UnmanagedType.LPWStr)] out string pszName);
            [PreserveSig] int SetTitle([MarshalAs(UnmanagedType.LPWStr)] string pszTitle);
            [PreserveSig] int SetOkButtonLabel([MarshalAs(UnmanagedType.LPWStr)] string pszText);
            [PreserveSig] int SetFileNameLabel([MarshalAs(UnmanagedType.LPWStr)] string pszLabel);
            [PreserveSig] int GetResult(out IShellItem ppsi);
            [PreserveSig] int AddPlace(IShellItem psi, int alignment);
            [PreserveSig] int SetDefaultExtension([MarshalAs(UnmanagedType.LPWStr)] string pszDefaultExtension);
            [PreserveSig] int Close(int hr);
            [PreserveSig] int SetClientGuid();
            [PreserveSig] int ClearClientData();
            [PreserveSig] int SetFilter([MarshalAs(UnmanagedType.IUnknown)] object pFilter);
            [PreserveSig] int GetResults(out IShellItemArray ppenum);
            [PreserveSig] int GetSelectedItems([MarshalAs(UnmanagedType.IUnknown)] out object ppsai);
        }

        [ComImport, Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IShellItem
        {
            [PreserveSig] int BindToHandler();
            [PreserveSig] int GetParent();
            [PreserveSig] int GetDisplayName(SIGDN sigdnName, [MarshalAs(UnmanagedType.LPWStr)] out string ppszName);
            [PreserveSig] int GetAttributes();
            [PreserveSig] int Compare();
        }

        [ComImport, Guid("b63ea76d-1f85-456f-a19c-48159efa858b"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IShellItemArray
        {
            [PreserveSig] int BindToHandler();
            [PreserveSig] int GetPropertyStore();
            [PreserveSig] int GetPropertyDescriptionList();
            [PreserveSig] int GetAttributes();
            [PreserveSig] int GetCount(out int pdwNumItems);
            [PreserveSig] int GetItemAt(int dwIndex, out IShellItem ppsi);
            [PreserveSig] int EnumItems();
        }

        private enum SIGDN : uint
        {
            SIGDN_DESKTOPABSOLUTEEDITING = 0x8004c000,
            SIGDN_DESKTOPABSOLUTEPARSING = 0x80028000,
            SIGDN_FILESYSPATH = 0x80058000,
            SIGDN_NORMALDISPLAY = 0,
            SIGDN_PARENTRELATIVE = 0x80080001,
            SIGDN_PARENTRELATIVEEDITING = 0x80031001,
            SIGDN_PARENTRELATIVEFORADDRESSBAR = 0x8007c001,
            SIGDN_PARENTRELATIVEPARSING = 0x80018001,
            SIGDN_URL = 0x80068000
        }

        [Flags]
        private enum FOS
        {
            FOS_OVERWRITEPROMPT = 0x2,
            FOS_STRICTFILETYPES = 0x4,
            FOS_NOCHANGEDIR = 0x8,
            FOS_PICKFOLDERS = 0x20,
            FOS_FORCEFILESYSTEM = 0x40,
            FOS_ALLNONSTORAGEITEMS = 0x80,
            FOS_NOVALIDATE = 0x100,
            FOS_ALLOWMULTISELECT = 0x200,
            FOS_PATHMUSTEXIST = 0x800,
            FOS_FILEMUSTEXIST = 0x1000,
            FOS_CREATEPROMPT = 0x2000,
            FOS_SHAREAWARE = 0x4000,
            FOS_NOREADONLYRETURN = 0x8000,
            FOS_NOTESTFILECREATE = 0x10000,
            FOS_HIDEMRUPLACES = 0x20000,
            FOS_HIDEPINNEDPLACES = 0x40000,
            FOS_NODEREFERENCELINKS = 0x100000,
            FOS_OKBUTTONNEEDSINTERACTION = 0x200000,
            FOS_DONTADDTORECENT = 0x2000000,
            FOS_FORCESHOWHIDDEN = 0x10000000,
            FOS_DEFAULTNOMINIMODE = 0x20000000,
            FOS_FORCEPREVIEWPANEON = 0x40000000,
            FOS_SUPPORTSTREAMABLEITEMS = unchecked((int)0x80000000)
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                try
                {
                    // Release unmanaged COM resources
                    if (dialog != null)
                    {
                        Marshal.FinalReleaseComObject(dialog);
                        dialog = null;
                    }

                    if (disposing)
                    {
                        // Dispose managed resources
                        _resultPaths.Clear();
                        _resultNames.Clear();
                    }
                }
                finally
                {
                    IsDisposed = true;
                }
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
