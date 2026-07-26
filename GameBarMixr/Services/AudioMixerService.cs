using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using GameBarMixr.Models;
using NAudio.CoreAudioApi;

namespace GameBarMixr.Services
{
    public class AudioMixerService : IDisposable
    {
        private readonly MMDeviceEnumerator _enumerator;

        public ObservableCollection<AudioDeviceModel> Devices { get; } = new();
        public ObservableCollection<AppAudioSessionModel> AppSessions { get; } = new();

        public AudioMixerService()
        {
            _enumerator = new MMDeviceEnumerator();
            RefreshAudioDevices();
        }

        public void RefreshAudioDevices()
        {
            Devices.Clear();
            AppSessions.Clear();

            try
            {
                // Enumerate all active audio playback endpoints
                var defaultDevice = _enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                var allDevices = _enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);

                foreach (var device in allDevices)
                {
                    var isDefault = device.ID == defaultDevice.ID;
                    var volume = device.AudioEndpointVolume.MasterVolumeLevelScalar;

                    Devices.Add(new AudioDeviceModel
                    {
                        Id = device.ID,
                        Name = device.FriendlyName,
                        IsDefault = isDefault,
                        Volume = volume,
                        IconGlyph = device.FriendlyName.Contains("Bluetooth", StringComparison.OrdinalIgnoreCase)
                                    || device.FriendlyName.Contains("Wireless", StringComparison.OrdinalIgnoreCase)
                            ? "\uE7F6"   // headset glyph
                            : "\uE7F4"  // speaker glyph
                    });
                }

                // Enumerate per-app audio sessions from default device
                var sessionManager = defaultDevice.AudioSessionManager;
                var sessions = sessionManager.Sessions;

                for (int i = 0; i < sessions.Count; i++)
                {
                    var session = sessions[i];
                    var process = GetProcessName(session.GetProcessID);
                    if (string.IsNullOrWhiteSpace(process)) continue;

                    AppSessions.Add(new AppAudioSessionModel
                    {
                        Id = session.GetSessionIdentifier,
                        AppName = process,
                        Volume = session.SimpleAudioVolume.Volume,
                        IsMuted = session.SimpleAudioVolume.Mute,
                        IconGlyph = "\uE735"
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AudioMixerService] Refresh error: {ex.Message}");
            }
        }

        private static string GetProcessName(uint pid)
        {
            try
            {
                var proc = System.Diagnostics.Process.GetProcessById((int)pid);
                return proc.ProcessName;
            }
            catch { return string.Empty; }
        }

        public async Task<bool> SetDefaultAudioDeviceAsync(string deviceId)
        {
            // Windows does not expose a public API to change default audio device.
            // This requires PolicyConfig COM interop (undocumented).
            // For now we simulate the change in UI; full implementation would use
            // the AudioSwitcher library or direct PolicyConfigClient COM call.
            await Task.Delay(100);
            foreach (var dev in Devices)
                dev.IsDefault = dev.Id == deviceId;
            return true;
        }

        public void SetDeviceVolume(string deviceId, float newVolume)
        {
            try
            {
                var device = _enumerator.GetDevice(deviceId);
                device.AudioEndpointVolume.MasterVolumeLevelScalar = Math.Clamp(newVolume, 0f, 1f);
                var model = Devices.FirstOrDefault(d => d.Id == deviceId);
                if (model != null) model.Volume = newVolume;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AudioMixerService] SetVolume error: {ex.Message}");
            }
        }

        public void Dispose()
        {
            _enumerator?.Dispose();
        }
    }
}
