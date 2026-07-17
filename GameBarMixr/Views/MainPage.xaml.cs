using System.Threading.Tasks;
using Microsoft.Gaming.XboxGameBar;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
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
                    // Automatic sync: when Bluetooth device connects, set as default audio output
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
