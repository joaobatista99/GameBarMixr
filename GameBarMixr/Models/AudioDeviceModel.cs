using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GameBarMixr.Models
{
    public class AudioDeviceModel : INotifyPropertyChanged
    {
        private string _id = string.Empty;
        private string _name = "Default Device";
        private bool _isDefault;
        private float _volume = 1.0f; // 0.0 to 1.0
        private bool _isMuted;
        private string _iconGlyph = "\uE7F6"; // Volume icon glyph

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

        public bool IsDefault
        {
            get => _isDefault;
            set { _isDefault = value; OnPropertyChanged(); }
        }

        public float Volume
        {
            get => _volume;
            set { _volume = value; OnPropertyChanged(); }
        }

        public int VolumePercent => (int)(_volume * 100);

        public bool IsMuted
        {
            get => _isMuted;
            set { _isMuted = value; OnPropertyChanged(); }
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
            if (propertyName == nameof(Volume))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VolumePercent)));
            }
        }
    }

    public class AppAudioSessionModel : INotifyPropertyChanged
    {
        private string _id = string.Empty;
        private string _appName = "App";
        private string _processPath = string.Empty;
        private float _volume = 1.0f;
        private bool _isMuted;
        private string _iconGlyph = "\uE735";

        public string Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(); }
        }

        public string AppName
        {
            get => _appName;
            set { _appName = value; OnPropertyChanged(); }
        }

        public string ProcessPath
        {
            get => _processPath;
            set { _processPath = value; OnPropertyChanged(); }
        }

        public float Volume
        {
            get => _volume;
            set { _volume = value; OnPropertyChanged(); }
        }

        public int VolumePercent => (int)(_volume * 100);

        public bool IsMuted
        {
            get => _isMuted;
            set { _isMuted = value; OnPropertyChanged(); }
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
            if (propertyName == nameof(Volume))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VolumePercent)));
            }
        }
    }
}
