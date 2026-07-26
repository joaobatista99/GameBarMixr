using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using GameBarMixr.Services;
using GameBarMixr.Models;

namespace GameBarMixr.Views
{
    public class WidgetForm : Form
    {
        // ── Win32 para bordas arredondadas e always-on-top ──────────────────
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
        private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
        private const int DWMWCP_ROUND = 2;

        // ── Cores do tema Xbox Dark ─────────────────────────────────────────
        private static readonly Color BgColor       = Color.FromArgb(18,  18,  18);
        private static readonly Color CardColor     = Color.FromArgb(30,  30,  30);
        private static readonly Color CardHover     = Color.FromArgb(45,  45,  45);
        private static readonly Color AccentGreen   = Color.FromArgb(16, 124,  65);
        private static readonly Color TextPrimary   = Color.FromArgb(255, 255, 255);
        private static readonly Color TextSecondary = Color.FromArgb(140, 140, 140);
        private static readonly Color BorderColor   = Color.FromArgb(45,  45,  45);

        // ── Serviços ────────────────────────────────────────────────────────
        private readonly AudioMixerService  _audioService;
        private readonly BluetoothService   _btService;

        // ── Controles ───────────────────────────────────────────────────────
        private Panel      _pnlHeader    = null!;
        private Button     _btnAudio     = null!;
        private Button     _btnBluetooth = null!;
        private Button     _btnRefresh   = null!;
        private Button     _btnClose     = null!;
        private Panel      _pnlAudio     = null!;
        private Panel      _pnlBluetooth = null!;
        private FlowLayoutPanel _audioDevicesPanel  = null!;
        private FlowLayoutPanel _appSessionsPanel   = null!;
        private FlowLayoutPanel _btDevicesPanel     = null!;
        private bool _audioTabActive = true;

        public WidgetForm()
        {
            _audioService = new AudioMixerService();
            _btService    = new BluetoothService();

            InitializeComponent();
            ApplyRoundedCorners();
            RenderAudio();
            RenderBluetooth();
        }

        private void InitializeComponent()
        {
            Text            = "GameBarMixr";
            Size            = new Size(340, 470);
            FormBorderStyle = FormBorderStyle.None;
            BackColor       = BgColor;
            TopMost         = true;
            StartPosition   = FormStartPosition.Manual;

            // Posicionar no canto superior direito
            var screen = Screen.PrimaryScreen!.WorkingArea;
            Location = new Point(screen.Right - Width - 16, screen.Top + 60);

            // ── Header ──────────────────────────────────────────────────────
            _pnlHeader = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 46,
                BackColor = Color.FromArgb(14, 14, 14)
            };

            // Label GameBarMixr
            var lblTitle = new Label
            {
                Text      = "🎧  GameBarMixr",
                ForeColor = TextPrimary,
                Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
                AutoSize  = false,
                Size      = new Size(180, 46),
                Location  = new Point(12, 0),
                TextAlign = ContentAlignment.MiddleLeft
            };

            // Botão Fechar
            _btnClose = MakeIconButton("✕", 32, 7, () => this.Hide());

            // Botão Refresh
            _btnRefresh = MakeIconButton("↻", 32 + 36, 7, OnRefresh);

            _pnlHeader.Controls.AddRange(new Control[] { lblTitle, _btnRefresh, _btnClose });

            // ── Segmented Pill ───────────────────────────────────────────────
            var pnlTabs = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 42,
                BackColor = BgColor,
                Padding   = new Padding(10, 6, 10, 0)
            };

            var pillBg = new Panel
            {
                BackColor = Color.FromArgb(28, 28, 28),
                Size      = new Size(296, 30),
                Location  = new Point(10, 6)
            };
            pillBg.Paint += (s, e) =>
            {
                var path = RoundedRect(new Rectangle(0, 0, pillBg.Width - 1, pillBg.Height - 1), 15);
                e.Graphics.FillPath(new SolidBrush(pillBg.BackColor), path);
            };

            _btnAudio = MakePillButton("Áudio", true);
            _btnAudio.Location = new Point(3, 3);
            _btnAudio.Click += (s, e) => SwitchTab(true);

            _btnBluetooth = MakePillButton("Bluetooth", false);
            _btnBluetooth.Location = new Point(3 + _btnAudio.Width + 2, 3);
            _btnBluetooth.Click += (s, e) => SwitchTab(false);

            pillBg.Controls.AddRange(new Control[] { _btnAudio, _btnBluetooth });
            pnlTabs.Controls.Add(pillBg);

            // ── Painel ÁUDIO ─────────────────────────────────────────────────
            _pnlAudio = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = BgColor,
                AutoScroll = true,
                Padding   = new Padding(10, 4, 10, 10)
            };

            var lblDevices = MakeSectionLabel("SAÍDA DE ÁUDIO");
            lblDevices.Location = new Point(0, 4);

            _audioDevicesPanel = new FlowLayoutPanel
            {
                AutoSize   = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Location   = new Point(0, 24),
                Width      = 310
            };

            var lblApps = MakeSectionLabel("APLICATIVOS");
            lblApps.Location = new Point(0, 0);

            _appSessionsPanel = new FlowLayoutPanel
            {
                AutoSize      = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents  = false,
                Width         = 310
            };

            // Wrapper para Apps com margin top
            var pnlAppsWrapper = new Panel { AutoSize = true, Width = 310 };
            pnlAppsWrapper.Controls.Add(lblApps);
            pnlAppsWrapper.Controls.Add(_appSessionsPanel);
            _appSessionsPanel.Location = new Point(0, 24);

            _pnlAudio.Controls.Add(lblDevices);
            _pnlAudio.Controls.Add(_audioDevicesPanel);
            _pnlAudio.Controls.Add(pnlAppsWrapper);

            // ── Painel BLUETOOTH ─────────────────────────────────────────────
            _pnlBluetooth = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = BgColor,
                AutoScroll = true,
                Padding   = new Padding(10, 4, 10, 10),
                Visible   = false
            };

            var lblBt = MakeSectionLabel("DISPOSITIVOS EMPARELHADOS");
            lblBt.Location = new Point(0, 4);

            _btDevicesPanel = new FlowLayoutPanel
            {
                AutoSize      = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents  = false,
                Location      = new Point(0, 24),
                Width         = 310
            };

            _pnlBluetooth.Controls.Add(lblBt);
            _pnlBluetooth.Controls.Add(_btDevicesPanel);

            // ── Drag para mover a janela ─────────────────────────────────────
            _pnlHeader.MouseDown += StartDrag;
            lblTitle.MouseDown   += StartDrag;

            // ── Layout ───────────────────────────────────────────────────────
            Controls.Add(_pnlAudio);
            Controls.Add(_pnlBluetooth);
            Controls.Add(pnlTabs);
            Controls.Add(_pnlHeader);

            // BringToFront garante a ordem de dock
            _pnlHeader.BringToFront();
            pnlTabs.BringToFront();
        }

        // ── Render ──────────────────────────────────────────────────────────
        private void RenderAudio()
        {
            _audioDevicesPanel.Controls.Clear();
            _appSessionsPanel.Controls.Clear();

            foreach (var dev in _audioService.Devices)
                _audioDevicesPanel.Controls.Add(MakeDeviceCard(dev));

            foreach (var app in _audioService.AppSessions)
                _appSessionsPanel.Controls.Add(MakeAppCard(app));
        }

        private void RenderBluetooth()
        {
            _btDevicesPanel.Controls.Clear();
            foreach (var bt in _btService.PairedDevices)
                _btDevicesPanel.Controls.Add(MakeBtCard(bt));
        }

        // ── Cards ────────────────────────────────────────────────────────────
        private Panel MakeDeviceCard(AudioDeviceModel dev)
        {
            var card = new Panel
            {
                Width     = 308,
                Height    = 42,
                BackColor = dev.IsDefault ? Color.FromArgb(20, 60, 35) : CardColor,
                Cursor    = Cursors.Hand,
                Margin    = new Padding(0, 0, 0, 5)
            };
            DrawRoundedBorder(card, dev.IsDefault ? AccentGreen : BorderColor);

            var dot = new Panel
            {
                Size      = new Size(7, 7),
                BackColor = dev.IsDefault ? AccentGreen : Color.FromArgb(60, 60, 60),
                Location  = new Point(10, 17)
            };
            DrawRoundedBorder(dot, dot.BackColor);

            var lbl = new Label
            {
                Text      = dev.Name,
                ForeColor = dev.IsDefault ? TextPrimary : TextSecondary,
                Font      = new Font("Segoe UI", 9.5f, dev.IsDefault ? FontStyle.Bold : FontStyle.Regular),
                AutoSize  = false,
                Size      = new Size(260, 42),
                Location  = new Point(26, 0),
                TextAlign = ContentAlignment.MiddleLeft
            };
            lbl.TextAlign = ContentAlignment.MiddleLeft;

            card.Controls.AddRange(new Control[] { dot, lbl });

            // Handler extraído para poder ser reutilizado no label
            async void OnCardClick(object? s, EventArgs e)
            {
                await _audioService.SetDefaultAudioDeviceAsync(dev.Id);
                RenderAudio();
            }
            card.Click += OnCardClick;
            lbl.Click  += OnCardClick;
            return card;
        }

        private Panel MakeAppCard(AppAudioSessionModel app)
        {
            var card = new Panel
            {
                Width     = 308,
                Height    = 52,
                BackColor = Color.FromArgb(24, 24, 24),
                Margin    = new Padding(0, 0, 0, 5)
            };
            DrawRoundedBorder(card, BorderColor);

            var lbl = new Label
            {
                Text      = app.AppName,
                ForeColor = TextPrimary,
                Font      = new Font("Segoe UI", 9f, FontStyle.Regular),
                AutoSize  = false,
                Size      = new Size(220, 22),
                Location  = new Point(10, 4),
                TextAlign = ContentAlignment.MiddleLeft
            };

            var lblPct = new Label
            {
                Text      = $"{app.VolumePercent}%",
                ForeColor = TextSecondary,
                Font      = new Font("Segoe UI", 8.5f),
                AutoSize  = false,
                Size      = new Size(60, 22),
                Location  = new Point(240, 4),
                TextAlign = ContentAlignment.MiddleRight
            };

            var slider = new TrackBar
            {
                Minimum   = 0,
                Maximum   = 100,
                Value     = app.VolumePercent,
                TickStyle = TickStyle.None,
                Size      = new Size(288, 24),
                Location  = new Point(6, 26),
                BackColor = Color.FromArgb(24, 24, 24)
            };
            slider.ValueChanged += (s, e) =>
            {
                app.Volume = slider.Value / 100f;
                lblPct.Text = $"{slider.Value}%";
                _audioService.SetAppVolume(app.Id, app.Volume);
            };

            card.Controls.AddRange(new Control[] { lbl, lblPct, slider });
            return card;
        }

        private Panel MakeBtCard(BluetoothDeviceModel bt)
        {
            var card = new Panel
            {
                Width     = 308,
                Height    = 52,
                BackColor = CardColor,
                Margin    = new Padding(0, 0, 0, 6)
            };
            DrawRoundedBorder(card, BorderColor);

            var dot = new Panel
            {
                Size      = new Size(8, 8),
                BackColor = bt.IsConnected ? Color.FromArgb(78, 202, 132) : Color.FromArgb(60, 60, 60),
                Location  = new Point(10, 22)
            };

            var lblName = new Label
            {
                Text      = bt.Name,
                ForeColor = TextPrimary,
                Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                AutoSize  = false,
                Size      = new Size(190, 20),
                Location  = new Point(26, 6),
                TextAlign = ContentAlignment.MiddleLeft
            };

            var status = bt.IsConnected ? "Conectado" : "Desconectado";
            var battery = bt.HasBatteryInfo ? $" · 🔋{bt.BatteryLevel}%" : "";
            var lblSub = new Label
            {
                Text      = status + battery,
                ForeColor = bt.IsConnected ? Color.FromArgb(78, 202, 132) : TextSecondary,
                Font      = new Font("Segoe UI", 8f),
                AutoSize  = false,
                Size      = new Size(190, 18),
                Location  = new Point(26, 28),
                TextAlign = ContentAlignment.MiddleLeft
            };

            var btn = new Button
            {
                Text      = bt.IsConnected ? "Desconectar" : "Conectar",
                ForeColor = bt.IsConnected ? Color.FromArgb(255, 100, 90) : TextPrimary,
                BackColor = Color.FromArgb(38, 38, 38),
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Size      = new Size(88, 26),
                Location  = new Point(212, 13),
                Cursor    = Cursors.Hand
            };
            btn.FlatAppearance.BorderColor = Color.FromArgb(55, 55, 55);
            btn.FlatAppearance.BorderSize  = 1;

            btn.Click += async (s, e) =>
            {
                btn.Enabled = false;
                btn.Text    = "Aguarde...";
                await _btService.ToggleConnectionAsync(bt);
                if (bt.IsConnected) _audioService.RefreshAudioDevices();
                RenderAudio();
                RenderBluetooth();
                btn.Enabled = true;
            };

            card.Controls.AddRange(new Control[] { dot, lblName, lblSub, btn });
            return card;
        }

        // ── Helpers visuais ──────────────────────────────────────────────────
        private static Label MakeSectionLabel(string text) => new Label
        {
            Text      = text,
            ForeColor = Color.FromArgb(90, 90, 90),
            Font      = new Font("Segoe UI", 7.5f, FontStyle.Bold),
            AutoSize  = false,
            Size      = new Size(300, 18),
            TextAlign = ContentAlignment.MiddleLeft
        };

        private static Button MakePillButton(string text, bool active) => new Button
        {
            Text      = text,
            Size      = new Size(142, 24),
            FlatStyle = FlatStyle.Flat,
            BackColor = active ? AccentGreen : Color.Transparent,
            ForeColor = active ? TextPrimary : TextSecondary,
            Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
            Cursor    = Cursors.Hand,
            FlatAppearance = { BorderSize = 0 }
        };

        private Button MakeIconButton(string icon, int rightOffset, int top, Action onClick)
        {
            var btn = new Button
            {
                Text      = icon,
                Size      = new Size(28, 28),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = TextSecondary,
                Font      = new Font("Segoe UI", 10f),
                Cursor    = Cursors.Hand,
                Anchor    = AnchorStyles.Top | AnchorStyles.Right,
                FlatAppearance = { BorderSize = 0 }
            };
            btn.Location = new Point(Width - rightOffset - btn.Width - 4, top);
            btn.Click += (s, e) => onClick();
            return btn;
        }

        private static void DrawRoundedBorder(Control ctrl, Color borderColor)
        {
            ctrl.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var pen = new Pen(borderColor, 1);
                var path = RoundedRect(new Rectangle(0, 0, ctrl.Width - 1, ctrl.Height - 1), 8);
                e.Graphics.DrawPath(pen, path);
            };
        }

        private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            var path = new GraphicsPath();
            path.AddArc(bounds.X, bounds.Y, radius * 2, radius * 2, 180, 90);
            path.AddArc(bounds.Right - radius * 2, bounds.Y, radius * 2, radius * 2, 270, 90);
            path.AddArc(bounds.Right - radius * 2, bounds.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void ApplyRoundedCorners()
        {
            int pref = DWMWCP_ROUND;
            DwmSetWindowAttribute(Handle, DWMWA_WINDOW_CORNER_PREFERENCE, ref pref, sizeof(int));
        }

        // ── Tab switch ───────────────────────────────────────────────────────
        private void SwitchTab(bool audioTab)
        {
            _audioTabActive = audioTab;
            _pnlAudio.Visible     = audioTab;
            _pnlBluetooth.Visible = !audioTab;
            _btnAudio.BackColor     = audioTab  ? AccentGreen : Color.Transparent;
            _btnAudio.ForeColor     = audioTab  ? TextPrimary : TextSecondary;
            _btnBluetooth.BackColor = !audioTab ? AccentGreen : Color.Transparent;
            _btnBluetooth.ForeColor = !audioTab ? TextPrimary : TextSecondary;
        }

        // ── Drag ─────────────────────────────────────────────────────────────
        private Point _dragStart;
        private void StartDrag(object? s, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                _dragStart = e.Location;
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                Location = new Point(
                    Location.X + e.X - _dragStart.X,
                    Location.Y + e.Y - _dragStart.Y);
        }

        // ── Refresh ──────────────────────────────────────────────────────────
        private void OnRefresh()
        {
            _audioService.RefreshAudioDevices();
            RenderAudio();
            RenderBluetooth();
        }
    }
}
