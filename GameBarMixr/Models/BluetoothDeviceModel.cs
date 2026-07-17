using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GameBarMixr.Models
{
    public enum BluetoothConnectionStatus
    {
        Disconnected,
        Connecting,
        Connected,
        Failed
    }

    public class BluetoothDeviceModel : INotifyPropertyChanged
    {
        private string _id = string.Empty;
        private string _name = "Dispositivo Bluetooth";
        private ulong _address;
        private bool _isPaired = true;
        private BluetoothConnectionStatus _status = BluetoothConnectionStatus.Disconnected;
        private int _batteryLevel = -1; // -1 if unknown
        private bool _isAudioDevice = true;
        private string _iconGlyph = "\uE7F6"; // Headphone/Speaker default glyph

        public string Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(); }
        }

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public ulong Address
        {
            get => _address;
            set { _address = value; OnPropertyChanged(); }
        }

        public bool IsPaired
        {
            get => _isPaired;
            set { _isPaired = value; OnPropertyChanged(); }
        }

        public BluetoothConnectionStatus Status
        {
            get => _status;
            set 
            { 
                _status = value; 
                OnPropertyChanged();
                OnPropertyChanged(nameof(StatusText));
                OnPropertyChanged(nameof(IsConnected));
                OnPropertyChanged(nameof(ActionButtonText));
            }
        }

        public bool IsConnected => Status == BluetoothConnectionStatus.Connected;

        public string StatusText => Status switch
        {
            BluetoothConnectionStatus.Connected => "Conectado",
            BluetoothConnectionStatus.Connecting => "Conectando...",
            BluetoothConnectionStatus.Failed => "Falha ao conectar",
            _ => "Desconectado"
        };

        public string ActionButtonText => Status switch
        {
            BluetoothConnectionStatus.Connected => "Desconectar",
            BluetoothConnectionStatus.Connecting => "Aguarde...",
            _ => "Conectar"
        };

        public int BatteryLevel
        {
            get => _batteryLevel;
            set { _batteryLevel = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasBatteryInfo)); }
        }

        public bool HasBatteryInfo => _batteryLevel >= 0;

        public bool IsAudioDevice
        {
            get => _isAudioDevice;
            set { _isAudioDevice = value; OnPropertyChanged(); }
        }

        public string IconGlyph
        {
            get => _iconGlyph;
            set { _iconGlyph = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
