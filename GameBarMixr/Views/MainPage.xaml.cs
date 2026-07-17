using Microsoft.Gaming.XboxGameBar;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using GameBarMixr.Models;
using GameBarMixr.Services;

namespace GameBarMixr.Views
{
    public partial class MainPage : Page
    {
        public AudioMixerService AudioService { get; } = new();
        public BluetoothService BluetoothService { get; } = new();
        private XboxGameBarWidget? _widget;

        public MainPage(XboxGameBarWidget? widget)
        {
            this.InitializeComponent();
            _widget = widget;
        }

        private void OnTabAudioClicked(object sender, RoutedEventArgs e)
        {
            AudioSection.Visibility = Visibility.Visible;
            BluetoothSection.Visibility = Visibility.Collapsed;

            TabAudioBtn.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 16, 124, 65));
            TabAudioBtn.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255));

            TabBtBtn.Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
            TabBtBtn.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 140, 140, 140));
        }

        private void OnTabBtClicked(object sender, RoutedEventArgs e)
        {
            AudioSection.Visibility = Visibility.Collapsed;
            BluetoothSection.Visibility = Visibility.Visible;

            TabBtBtn.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 16, 124, 65));
            TabBtBtn.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255));

            TabAudioBtn.Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
            TabAudioBtn.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 140, 140, 140));
        }

        private async void OnAudioDeviceSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AudioDevicesListView.SelectedItem is AudioDeviceModel selectedDevice)
            {
                await AudioService.SetDefaultAudioDeviceAsync(selectedDevice.Id);
            }
        }

        private async void OnBluetoothActionClicked(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is BluetoothDeviceModel device)
            {
                bool success = await BluetoothService.ToggleConnectionAsync(device);
                if (success && device.IsConnected)
                {
                    AudioService.RefreshAudioDevices();
                }
            }
        }

        private async void OnRefreshClicked(object sender, RoutedEventArgs e)
        {
            AudioService.RefreshAudioDevices();
            await BluetoothService.RefreshDevicesAsync();
        }
    }
}
