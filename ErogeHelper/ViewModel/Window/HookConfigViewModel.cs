using ErogeHelper.ViewModel.Page;

namespace ErogeHelper.ViewModel.Window
{
    public class HookConfigViewModel
    {
        public HookConfigViewModel(HookViewModel hookViewModel)
        {
            HookPageFrame = hookViewModel;
        }

        public HookViewModel HookPageFrame { get; }
    }
}