using Caliburn.Micro;
using ErogeHelper.Common.Enum;
using ErogeHelper.Model.Factory.Interface;

namespace ErogeHelper.ViewModel.Entity.NotifyItem
{
    public class TranslatorItem : PropertyChangedBase
    {
        private bool _enable;
        private bool _canBeEnable;

        public bool CanBeEnable
        {
            get => _canBeEnable;
            set { _canBeEnable = value; NotifyOfPropertyChange(() => CanBeEnable); }
        }

        public bool Enable
        {
            get => _enable;
            set
            {
                _enable = value;
                if (TransName != string.Empty)
                {
                    IoC.Get<ITranslatorFactory>().GetTranslator(NameEnum).IsEnable = value;
                }
            }
        }

        public string IconPath { get; set; } = string.Empty;

        public string TransName { get; set; } = string.Empty;

        public TranslatorName NameEnum { get; set; }

        public bool CanEdit { get; set; }
    }
}