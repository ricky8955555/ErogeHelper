using System.Windows;
using System.Windows.Media;
using ErogeHelper.Common.Constraint;
using ErogeHelper.Common.Enum;

namespace ErogeHelper.ViewModel.Entity.NotifyItem
{
    public class SingleTextItem
    {
        public SingleTextItem(string ruby, string text, TextTemplateType templateType, ImageSource backgroundColor)
        {
            Ruby = ruby;
            Text = text;
            TextTemplateType = templateType;
            SubMarkColor = backgroundColor;
            
            CanBeSearch = backgroundColor != StaticXamlBitmapImage.TransparentImage;
        }

        // XXX: 可以考虑带一个MeCabWord或VeWord的引用
        public string Ruby { get; }
        public string Text { get; }
        public TextTemplateType TextTemplateType { get; }
        public ImageSource SubMarkColor { get; }
        public bool CanBeSearch { get; }
    }
}
