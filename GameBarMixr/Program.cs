using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using GameBarMixr.Views;

namespace GameBarMixr
{
    internal static class Program
    {
        private static NotifyIcon? _trayIcon;
        private static WidgetForm? _form;

        [STAThread]
        static void Main()
        {
            // Garante instância única
            using var mutex = new Mutex(true, "GameBarMixr_SingleInstance", out bool createdNew);
            if (!createdNew)
            {
                // Já está rodando: traz a janela para frente
                NativeMethods.PostMessage(
                    NativeMethods.FindWindow(null, "GameBarMixr"),
                    NativeMethods.WM_SHOWWINDOW, IntPtr.Zero, IntPtr.Zero);
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetHighDpiMode(HighDpiMode.SystemAware);

            _form = new WidgetForm();

            // ── Ícone na bandeja do sistema ──────────────────────────────────
            _trayIcon = new NotifyIcon
            {
                Text    = "GameBarMixr — Audio & Bluetooth Mixer",
                Visible = true,
                Icon    = SystemIcons.Application
            };

            var ctxMenu = new ContextMenuStrip();
            ctxMenu.Items.Add("Abrir GameBarMixr",  null, (s, e) => ShowForm());
            ctxMenu.Items.Add(new ToolStripSeparator());
            ctxMenu.Items.Add("Fechar",              null, (s, e) => ExitApp());
            _trayIcon.ContextMenuStrip  = ctxMenu;
            _trayIcon.DoubleClick       += (s, e) => ShowForm();

            // Mostra a janela logo na inicialização (lançamento pelo Game Bar)
            ShowForm();

            Application.Run();

            _trayIcon.Visible = false;
        }

        private static void ShowForm()
        {
            if (_form == null) return;
            _form.Show();
            _form.WindowState = FormWindowState.Normal;
            _form.Activate();
            _form.BringToFront();
        }

        private static void ExitApp()
        {
            _trayIcon?.Dispose();
            Application.Exit();
        }
    }

    internal static class NativeMethods
    {
        public const int WM_SHOWWINDOW = 0x0018;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string? lpClassName, string lpWindowName);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool PostMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
    }
}
