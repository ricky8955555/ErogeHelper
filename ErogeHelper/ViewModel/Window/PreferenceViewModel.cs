using System;
using ErogeHelper.ViewModel.Page;

namespace ErogeHelper.ViewModel.Window
{
    public class PreferenceViewModel
    {
        public PreferenceViewModel(
            GeneralViewModel generalViewModel,
            MeCabViewModel meCabViewModel,
            HookViewModel hookViewModel,
            TransViewModel transViewModel,
            AboutViewModel aboutViewModel)
        {
            GeneralViewModel = generalViewModel;
            MeCabViewModel = meCabViewModel;
            HookViewModel = hookViewModel;
            TransViewModel = transViewModel;
            AboutViewModel = aboutViewModel;
        }

        public GeneralViewModel GeneralViewModel { get; }

        public MeCabViewModel MeCabViewModel { get; }

        public HookViewModel HookViewModel { get; }

        public TransViewModel TransViewModel { get; }

        public AboutViewModel AboutViewModel { get; }
    }
}