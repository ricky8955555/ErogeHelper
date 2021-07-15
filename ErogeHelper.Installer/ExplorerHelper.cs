using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace ErogeHelper.Installer
{
    class ExplorerHelper
    {
        public static List<string> Paths = new();

        #region Win32 Apis
        private delegate bool CallBack(int hwnd, int y);

        [DllImport("user32.dll")]
        private static extern int EnumWindows(CallBack x, int y);

        [DllImport("user32.dll")]
        private static extern int GetWindowText(int hwnd, StringBuilder lptrString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern int GetParent(int hwnd);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(int hwnd);

        [DllImport("User32.Dll ")]
        private static extern void GetClassName(IntPtr hwnd, StringBuilder s, int nMaxCount);

        [DllImport("User32.dll ")]
        private static extern IntPtr FindWindowEx(IntPtr parent, IntPtr childe, string strclass, string? FrmText);

        private string GetFormClassName(IntPtr ptr)
        {
            StringBuilder nameBiulder = new StringBuilder(255);
            GetClassName(ptr, nameBiulder, 255);
            return nameBiulder.ToString();
        }

        private string GetFormTitle(IntPtr ptr)
        {
            StringBuilder titleBiulder = new StringBuilder(255);
            GetWindowText((int)ptr, titleBiulder, 255);
            return titleBiulder.ToString();
        }

        private bool Report(int hwnd, int lParam)
        {
            int pHwnd = GetParent(hwnd);
            if (pHwnd == 0 && IsWindowVisible(hwnd) == true)
            {
                IntPtr cabinetWClassIntPtr = new IntPtr(hwnd);
                string cabinetWClassName = GetFormClassName(cabinetWClassIntPtr);
                if (cabinetWClassName.Equals("CabinetWClass", StringComparison.OrdinalIgnoreCase))
                {
                    IntPtr workerWIntPtr = FindWindowEx(cabinetWClassIntPtr, IntPtr.Zero, "WorkerW", null);
                    IntPtr reBarWindow32IntPtr = FindWindowEx(workerWIntPtr, IntPtr.Zero, "ReBarWindow32", null);
                    IntPtr addressBandRootIntPtr = FindWindowEx(reBarWindow32IntPtr, IntPtr.Zero, "Address Band Root", null);
                    IntPtr msctls_progress32IntPtr = FindWindowEx(addressBandRootIntPtr, IntPtr.Zero, "msctls_progress32", null);
                    IntPtr breadcrumbParentIntPtr = FindWindowEx(msctls_progress32IntPtr, IntPtr.Zero, "Breadcrumb Parent", null);
                    IntPtr toolbarWindow32IntPtr = FindWindowEx(breadcrumbParentIntPtr, IntPtr.Zero, "ToolbarWindow32", null);


                    string title = GetFormTitle(toolbarWindow32IntPtr);
                    int index = title.IndexOf(':');
                    index++;
                    string path = title.Substring(index, title.Length - index);
                    Paths.Add(path);
                }
            }
            return true;
        }
        #endregion

        public void CollectDir()
        {
            EnumWindows(Report, 0);
        }

        public void KillExplorer()
        {
            Process[] prcChecker = Process.GetProcessesByName("explorer");
            if (prcChecker.Length > 0)
            {
                foreach (Process p in prcChecker)
                {
                    p.Kill();
                }
            }
        }

        public void ReOpenDirs()
        {
            foreach (var dir in Paths)
            {
                Process.Start(new ProcessStartInfo()
                {
                    FileName = "explorer",
                    Arguments = dir
                });
            }
            Paths.Clear();
        }
    }
}
