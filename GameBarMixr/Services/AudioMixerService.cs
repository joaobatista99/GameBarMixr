using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameBarMixr.Models;
using NAudio.CoreAudioApi;

namespace GameBarMixr.Services
{
    public class AudioMixerService : IDisposable
    {
        private readonly MMDeviceEnumerator _enumerator;
        // Guarda referências vivas das sessões para controle de volume real
        private readonly Dictionary<string, AudioSessionControl> _liveSessions = new();

        public ObservableCollection<AudioDeviceModel>   Devices     { get; } = new();
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
            _liveSessions.Clear();

            try
            {
                var defaultDevice = _enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                var allDevices    = _enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);

                foreach (var device in allDevices)
                {
                    Devices.Add(new AudioDeviceModel
                    {
                        Id       = device.ID,
                        Name     = device.FriendlyName,
                        IsDefault = device.ID == defaultDevice.ID,
                        Volume   = device.AudioEndpointVolume.MasterVolumeLevelScalar,
                        IconGlyph = device.FriendlyName.Contains("Bluetooth", StringComparison.OrdinalIgnoreCase)
                                 || device.FriendlyName.Contains("Wireless", StringComparison.OrdinalIgnoreCase)
                            ? "\uE7F6" : "\uE7F4"
                    });
                }

                // Sessões de áudio por aplicativo
                var sessions = defaultDevice.AudioSessionManager.Sessions;
                for (int i = 0; i < sessions.Count; i++)
                {
                    var session = sessions[i];
                    var pid     = session.GetProcessID;
                    var name    = GetProcessName(pid);
                    if (string.IsNullOrWhiteSpace(name)) continue;

                    var sessionId = session.GetSessionIdentifier;
                    _liveSessions[sessionId] = session;

                    AppSessions.Add(new AppAudioSessionModel
                    {
                        Id      = sessionId,
                        AppName = name,
                        Volume  = session.SimpleAudioVolume.Volume,
                        IsMuted = session.SimpleAudioVolume.Mute,
                        IconGlyph = "\uE735"
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AudioMixerService] Refresh: {ex.Message}");
            }
        }

        private static string GetProcessName(uint pid)
        {
            try   { return System.Diagnostics.Process.GetProcessById((int)pid).ProcessName; }
            catch { return string.Empty; }
        }

        public async Task<bool> SetDefaultAudioDeviceAsync(string deviceId)
        {
            // A API pública do Windows não expõe mudança de dispositivo padrão.
            // Atualiza apenas o modelo de UI; implementação completa requer PolicyConfigClient COM.
            await Task.Delay(80);
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
                System.Diagnostics.Debug.WriteLine($"[AudioMixerService] SetDeviceVolume: {ex.Message}");
            }
        }

        // Corrige CS1061: método ausente que o WidgetForm chama
        public void SetAppVolume(string sessionId, float volume)
        {
            try
            {
                volume = Math.Clamp(volume, 0f, 1f);
                // Atualiza modelo de UI
                var model = AppSessions.FirstOrDefault(a => a.Id == sessionId);
                if (model != null) model.Volume = volume;
                // Atualiza volume real via NAudio se a sessão ainda estiver viva
                if (_liveSessions.TryGetValue(sessionId, out var session))
                    session.SimpleAudioVolume.Volume = volume;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AudioMixerService] SetAppVolume: {ex.Message}");
            }
        }

        public void Dispose()
        {
            _enumerator?.Dispose();
        }
    }
}
