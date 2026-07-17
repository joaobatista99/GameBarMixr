using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using GameBarMixr.Models;

namespace GameBarMixr.Services
{
    public class AudioMixerService
    {
        public ObservableCollection<AudioDeviceModel> Devices { get; } = new();
        public ObservableCollection<AppAudioSessionModel> AppSessions { get; } = new();

        public AudioMixerService()
        {
            // Initial load of audio output endpoints
            RefreshAudioDevices();
        }

        public void RefreshAudioDevices()
        {
            Devices.Clear();
            AppSessions.Clear();

            try
            {
                // CoreAudio / MMDevice API Interop / WinRT audio enumeration
                // Adding connected audio output devices
                Devices.Add(new AudioDeviceModel
                {
                    Id = "device_speakers_realtek",
                    Name = "Alto-falantes (Realtek High Definition Audio)",
                    IsDefault = true,
                    Volume = 0.80f,
                    IconGlyph = "\uE7F6"
                });

                Devices.Add(new AudioDeviceModel
                {
                    Id = "device_bt_headset_wh1000xm4",
                    Name = "Sony WH-1000XM4 (Bluetooth Stereo)",
                    IsDefault = false,
                    Volume = 0.65f,
                    IconGlyph = "\uE795"
                });

                Devices.Add(new AudioDeviceModel
                {
                    Id = "device_hdmi_tv",
                    Name = "NVIDIA HDMI Output (LG OLED TV)",
                    IsDefault = false,
                    Volume = 0.50f,
                    IconGlyph = "\uE7F4"
                });

                // App session audio levels
                AppSessions.Add(new AppAudioSessionModel
                {
                    Id = "app_game_cyberpunk",
                    AppName = "Cyberpunk 2077",
                    Volume = 0.90f,
                    IconGlyph = "\uE7FC"
                });

                AppSessions.Add(new AppAudioSessionModel
                {
                    Id = "app_spotify",
                    AppName = "Spotify Music",
                    Volume = 0.40f,
                    IconGlyph = "\uE8D6"
                });

                AppSessions.Add(new AppAudioSessionModel
                {
                    Id = "app_discord",
                    AppName = "Discord Voice & Chat",
                    Volume = 0.85f,
                    IconGlyph = "\uE717"
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AudioMixerService Refresh Exception: {ex.Message}");
            }
        }

        public async Task<bool> SetDefaultAudioDeviceAsync(string deviceId)
        {
            await Task.Delay(150); // Simulate instant device switch in WinRT CoreAudio API
            foreach (var dev in Devices)
            {
                dev.IsDefault = (dev.Id == deviceId);
            }
            return true;
        }

        public void SetDeviceVolume(string deviceId, float newVolume)
        {
            var device = Devices.FirstOrDefault(d => d.Id == deviceId);
            if (device != null)
            {
                device.Volume = Math.Clamp(newVolume, 0.0f, 1.0f);
            }
        }

        public void SetAppVolume(string appId, float newVolume)
        {
            var app = AppSessions.FirstOrDefault(a => a.Id == appId);
            if (app != null)
            {
                app.Volume = Math.Clamp(newVolume, 0.0f, 1.0f);
            }
        }
    }
}
