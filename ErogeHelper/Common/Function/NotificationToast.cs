using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace ErogeHelper.Common.Function
{
    public class NotificationToast
    {
        public NotificationToast(string title, string timeout)
        {
            _baseScript = @$"
            $ErrorActionPreference = 'Stop';
            $notificationTitle = '{title}';
            [Windows.UI.Notifications.ToastNotificationManager, Windows.UI.Notifications, ContentType = WindowsRuntime] > $null;
            $template = [Windows.UI.Notifications.ToastNotificationManager]::GetTemplateContent([Windows.UI.Notifications.ToastTemplateType]::ToastText01);
            $toastXml = [xml] $template.GetXml();
            $toastXml.GetElementsByTagName('text').AppendChild($toastXml.CreateTextNode($notificationTitle)) > $null;
            $xml = New-Object Windows.Data.Xml.Dom.XmlDocument;
            $xml.LoadXml($toastXml.OuterXml);
            $toast = [Windows.UI.Notifications.ToastNotification]::new($xml);
            $toast.Tag = 'eh';
            $toast.Group = 'eh';
            $toast.ExpirationTime = [DateTimeOffset]::Now.AddSeconds({timeout});
            $notifier = [Windows.UI.Notifications.ToastNotificationManager]::CreateToastNotifier('{Notifier}');
            $notifier.Show($toast);".Trim();
        }

        public bool Show()
        {
            if (!CanToast())
                return false;
            RunPowerShellScript(_baseScript);
            return true;
        }

        public async Task<bool> ShowAsync() =>
            await Task.Run(() =>
            {
                if (!CanToast())
                    return false;
                RunPowerShellScript(_baseScript);
                return true;
            });


        private const string Notifier = "Eroge Helper";

        private readonly string _baseScript;

        private const string PowerShellPath = @"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe";
        private static bool CheckPowerShellVersionNumber()
        {
            const string versionScript = "Get-Host | Select-Object Version";
            string masterVersion = RunPowerShellScript(versionScript).Split("\r\n")[3].Split('.')[0];
            return Convert.ToInt16(masterVersion) >= 5;
        }
        private static bool CanToast()
        {
            if (File.Exists(PowerShellPath))
            {
                if (CheckPowerShellVersionNumber())
                    return true;

                Log.Info("PowerShell version is lower than v5.0");
                return false;
            }

            Log.Info("Can't find PowerShell in this computer.");
            return false;
        }
        private static string RunPowerShellScript(string script)
        {
            var powerShellProcess = new Process
            {
                StartInfo = new ProcessStartInfo(PowerShellPath, script)
                {
                    WorkingDirectory = Environment.CurrentDirectory,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                }
            };
            powerShellProcess.Start();

            var reader = powerShellProcess.StandardOutput;
            return reader.ReadToEnd();
        }
    }
}