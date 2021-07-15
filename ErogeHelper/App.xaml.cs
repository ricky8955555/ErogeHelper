using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using ErogeHelper.Common.Function;
using Ookii.Dialogs.Wpf;
using Serilog;

namespace ErogeHelper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private App()
        {
            SingleInstanceWatcher();

            // Enable Pointer for touch device
            //AppContext.SetSwitch("Switch.System.Windows.Input.Stylus.EnablePointerSupport", true);

            // Set environment to app directory
            var currentDirectory = Path.GetDirectoryName(System.AppContext.BaseDirectory);
            Directory.SetCurrentDirectory(currentDirectory ?? throw new ArgumentNullException(nameof(currentDirectory),
                                                            @"Could not located Eroge Helper's directory"));
            // Set i18n (modernWpf controllers)
            SetLanguageDictionary();

            // Set logger
            CreateLogger();

            // Set thread error handle
            AppDomain.CurrentDomain.UnhandledException += (_, unhandledExceptionArgs) =>
            {
                if (Dispatcher.FromThread(Thread.CurrentThread) is null ||
                    Dispatcher.CurrentDispatcher.Thread == Thread.CurrentThread)
                    return;

                var ex = unhandledExceptionArgs.ExceptionObject as Exception ?? new Exception("???");

                Log.Fatal(ex);
                ShowErrorDialog("Fatal", ex);
            };
            DispatcherUnhandledException += (_, dispatcherUnhandledExceptionEventArgs) =>
            {
                // More friendly
                dispatcherUnhandledExceptionEventArgs.Handled = true;

                var ex = dispatcherUnhandledExceptionEventArgs.Exception;

                Log.Error(ex);
                ShowErrorDialog("UI", ex);
            };
        }

        private static void ShowErrorDialog(string errorLevel, Exception ex)
        {
            using var dialog = new TaskDialog
            {
                WindowTitle = $@"Eroge Helper - {errorLevel} Error",
                MainInstruction = ex.Message,
                Content =
                    @"Eroge Helper run into some trouble. See detail error by click Detail information below.",
                ExpandedInformation = ex.StackTrace,
                Width = 300,
            };

            TaskDialogButton okButton = new(ButtonType.Ok);
            dialog.Buttons.Add(okButton);
            _ = dialog.ShowDialog();
        }

        private static void SetLanguageDictionary()
        {
            Language.Strings.Culture = Thread.CurrentThread.CurrentCulture.ToString() switch
            {
                "zh-Hans" => new System.Globalization.CultureInfo("zh-Hans"),
                "zh" => new System.Globalization.CultureInfo("zh-Hans"),
                "zh-CN" => new System.Globalization.CultureInfo("zh-Hans"),
                "zh-SG" => new System.Globalization.CultureInfo("zh-Hans"),
                // Default english because there can be so many different system language, we rather fallback on 
                // english in this case.
                _ => new System.Globalization.CultureInfo(""),
            };
        }

        private static void CreateLogger()
        {
            Serilog.Log.Logger = new LoggerConfiguration()
#if DEBUG
                .MinimumLevel.Debug()
                // IDE Output View
                .WriteTo.Debug(outputTemplate:
                    "[{Timestamp:MM-dd-yyyy HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}")
#else
                .MinimumLevel.Information()
#endif
                .CreateLogger();
        }

        private const string UniqueEventName = "{d3bcc592-a3bb-4c26-99c4-23c750ddcf77}";
        private EventWaitHandle? _eventWaitHandle;

        private void SingleInstanceWatcher()
        {
            try
            {
                _eventWaitHandle = EventWaitHandle.OpenExisting(UniqueEventName);
                _eventWaitHandle.Set();
                Shutdown();
            }
            catch (WaitHandleCannotBeOpenedException)
            {
                Log.Info("Fine exception WaitHandleCannotBeOpenedException for active singleton app");
                _eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, UniqueEventName);
            }

            new Task(async () =>
            {
                while (_eventWaitHandle.WaitOne())
                {
                    await Current.Dispatcher.InvokeAsync(async () =>
                    {
                        // QUESTION: May I use automation to run PowerShell
                        var result = await new NotificationToast(
                            "ErogeHelper is running!",// , or you can turn ErogeHelper down immediately
                            "15")
                            .ShowAsync();
                        if (result != true)
                            await ModernWpf.MessageBox.ShowAsync("ErogeHelper is running!", "Eroge Helper");
                    });
                }
            }).Start();
        }
    }
}
