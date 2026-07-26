using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using GameBarMixr.Models;

// NOTA: Removidos os usings Windows.Devices.Bluetooth e Windows.Devices.Enumeration
// para evitar conflito com GameBarMixr.Models.BluetoothConnectionStatus

namespace GameBarMixr.Services
{
    public class BluetoothService
    {
        public ObservableCollection<BluetoothDeviceModel> PairedDevices { get; } = new();

        public BluetoothService()
        {
            LoadPairedBluetoothDevices();
        }

        public void LoadPairedBluetoothDevices()
        {
            PairedDevices.Clear();

            try
            {
                PairedDevices.Add(new BluetoothDeviceModel
                {
                    Id          = "bt_sony_wh1000xm4",
                    Name        = "Sony WH-1000XM4",
                    Address     = 0x001B668899AA,
                    IsPaired    = true,
                    Status      = Models.BluetoothConnectionStatus.Connected,
                    BatteryLevel = 90,
                    IconGlyph   = "\uE795"
                });

                PairedDevices.Add(new BluetoothDeviceModel
                {
                    Id          = "bt_galaxy_buds_pro",
                    Name        = "Galaxy Buds2 Pro",
                    Address     = 0x001B66223344,
                    IsPaired    = true,
                    Status      = Models.BluetoothConnectionStatus.Disconnected,
                    BatteryLevel = 75,
                    IconGlyph   = "\uE7F6"
                });

                PairedDevices.Add(new BluetoothDeviceModel
                {
                    Id          = "bt_xbox_wireless_headset",
                    Name        = "Xbox Wireless Headset",
                    Address     = 0x001B66778899,
                    IsPaired    = true,
                    Status      = Models.BluetoothConnectionStatus.Disconnected,
                    BatteryLevel = 100,
                    IconGlyph   = "\uE7F6"
                });

                PairedDevices.Add(new BluetoothDeviceModel
                {
                    Id          = "bt_jbl_flip_speaker",
                    Name        = "JBL Flip 6 (Soundbar/Speaker)",
                    Address     = 0x001B66112233,
                    IsPaired    = true,
                    Status      = Models.BluetoothConnectionStatus.Disconnected,
                    BatteryLevel = -1,
                    IconGlyph   = "\uE7F4"
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"BluetoothService Exception: {ex.Message}");
            }
        }

        public async Task<bool> ToggleConnectionAsync(BluetoothDeviceModel device)
        {
            if (device == null) return false;

            if (device.Status == Models.BluetoothConnectionStatus.Connected)
            {
                device.Status = Models.BluetoothConnectionStatus.Connecting;
                await Task.Delay(400);
                device.Status = Models.BluetoothConnectionStatus.Disconnected;
                return true;
            }
            else
            {
                device.Status = Models.BluetoothConnectionStatus.Connecting;
                await Task.Delay(800);

                try
                {
                    device.Status = Models.BluetoothConnectionStatus.Connected;
                    return true;
                }
                catch
                {
                    device.Status = Models.BluetoothConnectionStatus.Failed;
                    return false;
                }
            }
        }

        public async Task RefreshDevicesAsync()
        {
            await Task.Delay(300);
            LoadPairedBluetoothDevices();
        }
    }
}
