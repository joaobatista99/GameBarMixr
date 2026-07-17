using Microsoft.Gaming.XboxGameBar;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using GameBarMixr.Views;

namespace GameBarMixr
{
    public partial class App : Application
    {
        private XboxGameBarWidget? _widget = null;

        public App()
        {
            this.InitializeComponent();
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            var window = new Window();
            window.Content = new MainPage(null);
            window.Activate();
        }

        // Activator para quando a app é chamada diretamente pelo Xbox Game Bar (Win + G)
        public void OnXboxGameBarWidgetActivated(XboxGameBarWidget widget)
        {
            _widget = widget;
            var window = new Window();
            window.Content = new MainPage(_widget);
            window.Activate();
        }
    }
}
