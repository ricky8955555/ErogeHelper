using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace ErogeHelper.View.Control
{
    /// <summary>
    /// TextControl.xaml 的交互逻辑
    /// </summary>
    public partial class TextControl
    {
        public TextControl()
        {
            InitializeComponent();
        }

        private void Border_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Border border)
            {
                border.Effect = new DropShadowEffect
                {
                    Color = new Color { A = 255, R = 255, G = 255, B = 0 },
                    Direction = 320,
                    ShadowDepth = 0,
                    Opacity = 1
                };
            }
        }

        private void Border_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is not Border border)
                return;

            border.ClearValue(EffectProperty);
            border.Opacity = 1;
        }

        private void Border_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border)
            {
                border.Opacity = 0.5;
            }
        }

        private void Border_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border)
            {
                border.Opacity = 1;
            }
        }
    }
}
