using Caliburn.Micro;
using ErogeHelper.Common;
using ErogeHelper.Common.Comparer;
using ErogeHelper.Model.Service.Interface;
using ErogeHelper.ViewModel.Entity.NotifyItem;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ErogeHelper.Model.Service
{
    public class SelectProcessDataService : ISelectProcessDataService
    {
        public Task RefreshBindableProcComboBoxAsync(BindableCollection<ProcComboBoxItem> refData, bool allApp) => Task.Run(() =>
        {
            BindableCollection<ProcComboBoxItem> tmpCollection = new();

            foreach (Process proc in ProcessEnumerable())
            {
                try
                {
                    var icon = Utils.PeIcon2BitmapImage(proc.MainModule?.FileName ?? string.Empty, allApp);
                    if (icon is null)
                        continue;
                    var item = new ProcComboBoxItem
                    {
                        Proc = proc,
                        Title = proc.MainWindowTitle,
                        Icon = icon
                    };
                    tmpCollection.Add(item);

                    if (refData.Contains(item, new ProcComboBoxItemComparer()))
                        continue;

                    refData.Add(item);
                }
                catch (Win32Exception ex)
                {
                    // Access Denied. 64bit -> 32bit module
                    Log.Debug(ex.Message);
                }
            }

            var redundantItems = refData.ToList()
                .Where(i => !tmpCollection.Contains(i, new ProcComboBoxItemComparer()));
            foreach (var i in redundantItems)
            {
                refData.Remove(i);
            }
        });

        private IEnumerable<Process> ProcessEnumerable() => Process.GetProcesses()
            .Where(proc =>
            proc.MainWindowHandle != IntPtr.Zero &&
            proc.MainWindowTitle != string.Empty &&
            !_uselessProcess.Contains(proc.ProcessName));

        private static readonly string[] _uselessProcess = new string[] {
            "SystemSettings", "TextInputHost", "ApplicationFrameHost", "Calculator", "Video.UI", "WinStore.App",
            "PaintStudio.View", "ShellExperienceHost", "commsapps", "Music.UI", "HxOutlook", "Maps", "WhiteboardWRT",
            "PeopleApp", "RtkUWP", "HxCalendarAppImm", "Cortana", "explorer"
        };
    }
}