using System.Collections.Generic;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace ErogeHelper.Common.Extention
{
    public static class WindowManagerExtension
    {
        public static async Task ShowWindowFromIoCAsync<T>(this IWindowManager windowManager, object? context = null)
            => await windowManager.ShowWindowAsync(IoC.Get<T>(), context).ConfigureAwait(false);

        public static async Task ShowDialogFromIoCAsync<T>(this IWindowManager windowManager, object? context = null)
            => await windowManager.ShowDialogAsync(IoC.Get<T>(), context).ConfigureAwait(false);

        public static async Task ShowPopupFromIoCAsync<T>(this IWindowManager windowManager, object? context = null)
            => await windowManager.ShowPopupAsync(IoC.Get<T>(), context).ConfigureAwait(false);

        public static async Task SilentStartWindowFromIoCAsync<T>(this IWindowManager windowManager, object? context = null)
        {
            // NOTE: 注意如果不关闭所有窗口程序是不会结束的，所以不要在应用顶层使用
            var settings = new Dictionary<string, object>
            {
                // 因为设置了ShowInTaskbar=false，所以不能使用WindowState.Minimized，不然会造成缩小到屏幕左下角的效果
                //{ "WindowState", WindowState.Minimized },
                // NOTE: 设置了这一项之后，在主动Call Show() 方法时似乎会在任务栏再次出现
                { "ShowInTaskbar", false },
                // Collapsed 与 Hidden 似乎也无法启用，所以没办法我只好在两个窗口的构造函数中手动添加Visibility.Collapsed
                //{ "Visibility", Visibility.Hidden }, // or Visibility.Collapsed
                { "Left", -32000 },
                { "Top", -32000 },
            };
            await windowManager.ShowWindowAsync(IoC.Get<T>(), context, settings).ConfigureAwait(false);
        }
    }
}