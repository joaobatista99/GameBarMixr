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
        private MMDeviceEnumerator? _enumerator;
        private readonly Dictionary<string, AudioSessionControl> _liveSessions = new();

        public ObservableCollection<AudioDeviceModel>     Devices     { get; } = new();
        public ObservableCollection<AppAudioSessionModel> AppSessions { get; } = new();

        public AudioMixerService()
        {
            try { _enumerator = new MMDeviceEnumerator(); }
            catch { _enumerator = null; }

            RefreshAudioDevices();
        }

        public void RefreshAudioDevices()
        {
            Devices.Clear();
            AppSessions.Clear();
            _liveSessions.Clear();

            // ── Tenta enumerar dispositivos reais via NAudio ─────────────────
            bool gotRealData = false;
            if (_enumerator != null)
            {
                try
                {
                    var defaultDevice = _enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                    var allDevices    = _enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);

                    foreach (var device in allDevices)
                    {
                        Devices.Add(new AudioDeviceModel
                        {
                            Id        = device.ID,
                            Name      = device.FriendlyName,
                            IsDefault = device.ID == defaultDevice.ID,
                            Volume    = device.AudioEndpointVolume.MasterVolumeLevelScalar,
                            IconGlyph = device.FriendlyName.Contains("Bluetooth", StringComparison.OrdinalIgnoreCase)
                                     || device.FriendlyName.Contains("Wireless",  StringComparison.OrdinalIgnoreCase)
                                ? "\uE7F6" : "\uE7F4"
                        });
                    }

                    var sessions = defaultDevice.AudioSessionManager.Sessions;
                    for (int i = 0; i < sessions.Count; i++)
                    {
                        var session = sessions[i];
                        var pid     = session.GetProcessID;
                        var name    = GetProcessName(pid);
                        if (string.IsNullOrWhiteSpace(name) || name == "Idle") continue;

                        var sid = session.GetSessionIdentifier;
                        _liveSessions[sid] = session;

                        AppSessions.Add(new AppAudioSessionModel
                        {
                            Id       = sid,
                            AppName  = name,
                            Volume   = session.SimpleAudioVolume.Volume,
                            IsMuted  = session.SimpleAudioVolume.Mute,
                            IconGlyph = "\uE735"
                        });
                    }

                    gotRealData = Devices.Count > 0;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[AudioMixerService] {ex.Message}");
                }
            }

            // ── Dados de fallback quando NAudio não consegue enumerar ────────
            if (!gotRealData)
            {
                Devices.Add(new AudioDeviceModel
                {
                    Id = "fallback_default", Name = "Alto-falantes (padrão)", IsDefault = true,
                    Volume = 0.75f, IconGlyph = "\uE7F4"
                });
                Devices.Add(new AudioDeviceModel
                {
                    Id = "fallback_hdmi", Name = "HDMI / Display Audio", IsDefault = false,
                    Volume = 1.0f, IconGlyph = "\uE7F4"
                });
            }

            if (AppSessions.Count == 0)
            {
                AppSessions.Add(new AppAudioSessionModel
                {
                    Id = "fallback_system", AppName = "Sons do Sistema",
                    Volume = 0.5f, IsMuted = false, IconGlyph = "\uE7F3"
                });
            }
        }

        private static string GetProcessName(uint pid)
        {
            try   { return System.Diagnostics.Process.GetProcessById((int)pid).ProcessName; }
            catch { return string.Empty; }
        }

        public async Task<bool> SetDefaultAudioDeviceAsync(string deviceId)
        {
            await Task.Delay(80);
            foreach (var dev in Devices) dev.IsDefault = dev.Id == deviceId;
            return true;
        }

        public void SetDeviceVolume(string deviceId, float newVolume)
        {
            try
            {
                if (_enumerator != null && !deviceId.StartsWith("fallback"))
                {
                    var device = _enumerator.GetDevice(deviceId);
                    device.AudioEndpointVolume.MasterVolumeLevelScalar = Math.Clamp(newVolume, 0f, 1f);
                }
                var model = Devices.FirstOrDefault(d => d.Id == deviceId);
                if (model != null) model.Volume = newVolume;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AudioMixerService] SetDeviceVolume: {ex.Message}");
            }
        }

        public void SetAppVolume(string sessionId, float volume)
        {
            try
            {
                volume = Math.Clamp(volume, 0f, 1f);
                var model = AppSessions.FirstOrDefault(a => a.Id == sessionId);
                if (model != null) model.Volume = volume;
                if (_liveSessions.TryGetValue(sessionId, out var session))
                    session.SimpleAudioVolume.Volume = volume;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AudioMixerService] SetAppVolume: {ex.Message}");
            }
        }

        public void Dispose() => _enumerator?.Dispose();
    }
}
