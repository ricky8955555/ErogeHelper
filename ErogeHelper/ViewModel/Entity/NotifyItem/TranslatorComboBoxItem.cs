using System.Windows.Media.Imaging;
using ErogeHelper.Common;
using ErogeHelper.Common.Constraint;
using ErogeHelper.Common.Enum;

namespace ErogeHelper.ViewModel.Entity.NotifyItem
{
    public class TranslatorComboBoxItem
    {
        public TranslatorName TranslatorName;

        public BitmapImage Icon { get; set; } = StaticXamlBitmapImage.TransparentImage;

        public string Title { get; set; } = string.Empty;
    }
}