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
        // ── Win32 ────────────────────────────────────────────────────────────
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
        private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
        private const int DWMWCP_ROUND = 2;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        private const int  HOTKEY_ID  = 9001;
        private const uint MOD_SHIFT  = 0x0004;
        private const uint MOD_WIN    = 0x0008;
        private const uint VK_M       = 0x4D;

        // ── Tema Xbox Dark ───────────────────────────────────────────────────
        private static readonly Color BgColor       = Color.FromArgb(18,  18,  18);
        private static readonly Color CardColor     = Color.FromArgb(30,  30,  30);
        private static readonly Color AccentGreen   = Color.FromArgb(16, 124,  65);
        private static readonly Color TextPrimary   = Color.FromArgb(255, 255, 255);
        private static readonly Color TextSecondary = Color.FromArgb(140, 140, 140);
        private static readonly Color BorderColor   = Color.FromArgb(50,  50,  50);

        // ── Serviços ─────────────────────────────────────────────────────────
        private readonly AudioMixerService _audioService;
        private readonly BluetoothService  _btService;

        // ── Panels ───────────────────────────────────────────────────────────
        private Panel           _pnlAudio     = null!;
        private Panel           _pnlBluetooth = null!;
        private Button          _btnAudio     = null!;
        private Button          _btnBluetooth = null!;
        private FlowLayoutPanel _audioList    = null!;
        private FlowLayoutPanel _appList      = null!;
        private FlowLayoutPanel _btList       = null!;

        // ── Drag ─────────────────────────────────────────────────────────────
        private Point _dragOffset;
        private bool  _dragging;

        public WidgetForm()
        {
            _audioService = new AudioMixerService();
            _btService    = new BluetoothService();
            Build();
        }

        // ════════════════════════════════════════════════════════════════════
        //  BUILD UI
        // ════════════════════════════════════════════════════════════════════
        private void Build()
        {
            SuspendLayout();

            Text            = "GameBarMixr";
            FormBorderStyle = FormBorderStyle.None;
            BackColor       = BgColor;
            TopMost         = true;
            StartPosition   = FormStartPosition.Manual;
            Size            = new Size(330, 480);
            ShowInTaskbar   = false;

            var screen = Screen.PrimaryScreen?.WorkingArea ?? new Rectangle(0, 0, 1920, 1080);
            Location = new Point(screen.Right - Width - 16, screen.Top + 48);

            // ── Root: TableLayoutPanel (3 linhas: header / tabs / conteúdo) ─
            var root = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                RowCount    = 3,
                ColumnCount = 1,
                BackColor   = BgColor,
                Margin      = Padding.Empty,
                Padding     = Padding.Empty
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));  // header
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));  // tabs
            root.RowStyles.Add(new RowStyle(SizeType.Percent,  100)); // content

            root.Controls.Add(BuildHeader(), 0, 0);
            root.Controls.Add(BuildTabs(),   0, 1);

            // ── Conteúdo: dois painéis sobrepostos ───────────────────────────
            var contentHost = new Panel { Dock = DockStyle.Fill, BackColor = BgColor };
            _pnlAudio     = BuildAudioPanel();
            _pnlBluetooth = BuildBluetoothPanel();
            _pnlBluetooth.Visible = false;
            contentHost.Controls.Add(_pnlBluetooth);
            contentHost.Controls.Add(_pnlAudio);

            root.Controls.Add(contentHost, 0, 2);
            Controls.Add(root);

            ResumeLayout(true);

            RenderAudio();
            RenderBluetooth();
        }

        // ── Header ───────────────────────────────────────────────────────────
        private Panel BuildHeader()
        {
            var hdr = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(14, 14, 14) };

            var title = new Label
            {
                Text      = "🎧  GameBarMixr",
                ForeColor = TextPrimary,
                Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
                Location  = new Point(12, 0),
                Size      = new Size(180, 44),
                TextAlign = ContentAlignment.MiddleLeft
            };

            var btnClose = IconBtn("✕", 8,  8, () => Hide());
            var btnRef   = IconBtn("↻", 44, 8, Refresh);

            hdr.Controls.AddRange(new Control[] { title, btnRef, btnClose });
            hdr.MouseDown += HeaderDrag;
            title.MouseDown += HeaderDrag;
            return hdr;
        }

        private Button IconBtn(string text, int rightOffset, int top, Action action)
        {
            var b = new Button
            {
                Text      = text,
                Size      = new Size(28, 28),
                Location  = new Point(330 - rightOffset - 28, top),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = TextSecondary,
                Font      = new Font("Segoe UI", 10f),
                Cursor    = Cursors.Hand,
                Anchor    = AnchorStyles.Top | AnchorStyles.Right,
                FlatAppearance = { BorderSize = 0 }
            };
            b.Click += (s, e) => action();
            return b;
        }

        // ── Tabs ─────────────────────────────────────────────────────────────
        private Panel BuildTabs()
        {
            var pnl = new Panel { Dock = DockStyle.Fill, BackColor = BgColor, Padding = new Padding(10, 6, 10, 4) };

            var pill = new Panel
            {
                Size      = new Size(308, 30),
                Location  = new Point(1, 6),
                BackColor = Color.FromArgb(28, 28, 28)
            };

            _btnAudio = PillBtn("  Áudio  ", true);
            _btnAudio.Location = new Point(3, 3);
            _btnAudio.Click += (s, e) => SwitchTab(true);

            _btnBluetooth = PillBtn("  Bluetooth  ", false);
            _btnBluetooth.Location = new Point(_btnAudio.Width + 5, 3);
            _btnBluetooth.Click += (s, e) => SwitchTab(false);

            pill.Controls.AddRange(new Control[] { _btnAudio, _btnBluetooth });
            pnl.Controls.Add(pill);
            return pnl;
        }

        private static Button PillBtn(string text, bool active) => new Button
        {
            Text      = text,
            AutoSize  = true,
            FlatStyle = FlatStyle.Flat,
            BackColor = active ? AccentGreen : Color.Transparent,
            ForeColor = active ? TextPrimary : TextSecondary,
            Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
            Cursor    = Cursors.Hand,
            MinimumSize = new Size(80, 24),
            FlatAppearance = { BorderSize = 0 }
        };

        // ── Painel Áudio ─────────────────────────────────────────────────────
        private Panel BuildAudioPanel()
        {
            var scroll = new Panel { Dock = DockStyle.Fill, BackColor = BgColor, AutoScroll = true };

            var inner = new FlowLayoutPanel
            {
                Dock          = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents  = false,
                AutoSize      = false,
                BackColor     = BgColor,
                Padding       = new Padding(10, 6, 10, 10)
            };

            inner.Controls.Add(SectionLabel("SAÍDA DE ÁUDIO"));
            _audioList = NewList();
            inner.Controls.Add(_audioList);

            inner.Controls.Add(SectionLabel("APLICATIVOS"));
            _appList = NewList();
            inner.Controls.Add(_appList);

            scroll.Controls.Add(inner);
            return scroll;
        }

        // ── Painel Bluetooth ─────────────────────────────────────────────────
        private Panel BuildBluetoothPanel()
        {
            var scroll = new Panel { Dock = DockStyle.Fill, BackColor = BgColor, AutoScroll = true };

            var inner = new FlowLayoutPanel
            {
                Dock          = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents  = false,
                AutoSize      = false,
                BackColor     = BgColor,
                Padding       = new Padding(10, 6, 10, 10)
            };

            inner.Controls.Add(SectionLabel("DISPOSITIVOS EMPARELHADOS"));
            _btList = NewList();
            inner.Controls.Add(_btList);

            scroll.Controls.Add(inner);
            return scroll;
        }

        private static FlowLayoutPanel NewList() => new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            WrapContents  = false,
            AutoSize      = true,
            Width         = 306,
            BackColor     = Color.Transparent
        };

        // ════════════════════════════════════════════════════════════════════
        //  RENDER
        // ════════════════════════════════════════════════════════════════════
        private void RenderAudio()
        {
            _audioList.Controls.Clear();
            _appList.Controls.Clear();

            foreach (var d in _audioService.Devices)    _audioList.Controls.Add(DeviceCard(d));
            foreach (var a in _audioService.AppSessions) _appList.Controls.Add(AppCard(a));
        }

        private void RenderBluetooth()
        {
            _btList.Controls.Clear();
            foreach (var b in _btService.PairedDevices) _btList.Controls.Add(BtCard(b));
        }

        // ── Cards ─────────────────────────────────────────────────────────────
        private Panel DeviceCard(AudioDeviceModel dev)
        {
            var bg = dev.IsDefault ? Color.FromArgb(20, 60, 35) : CardColor;
            var card = Card(308, 44, bg);

            var dot = new Panel
            {
                Size      = new Size(8, 8),
                Location  = new Point(12, 18),
                BackColor = dev.IsDefault ? AccentGreen : Color.FromArgb(55, 55, 55)
            };

            var lbl = new Label
            {
                Text      = dev.Name,
                ForeColor = dev.IsDefault ? TextPrimary : TextSecondary,
                Font      = new Font("Segoe UI", 9.5f, dev.IsDefault ? FontStyle.Bold : FontStyle.Regular),
                Location  = new Point(28, 0),
                Size      = new Size(264, 44),
                TextAlign = ContentAlignment.MiddleLeft
            };

            card.Controls.AddRange(new Control[] { dot, lbl });

            async void Click(object? s, EventArgs e)
            {
                await _audioService.SetDefaultAudioDeviceAsync(dev.Id);
                RenderAudio();
            }
            card.Click += Click;
            lbl.Click  += Click;
            card.Cursor = Cursors.Hand;
            lbl.Cursor  = Cursors.Hand;
            return card;
        }

        private Panel AppCard(AppAudioSessionModel app)
        {
            var card = Card(308, 56, Color.FromArgb(24, 24, 24));

            var lblName = new Label
            {
                Text      = app.AppName,
                ForeColor = TextPrimary,
                Font      = new Font("Segoe UI", 9f),
                Location  = new Point(10, 4),
                Size      = new Size(210, 22),
                TextAlign = ContentAlignment.MiddleLeft
            };

            var lblPct = new Label
            {
                Text      = $"{app.VolumePercent}%",
                ForeColor = TextSecondary,
                Font      = new Font("Segoe UI", 8.5f),
                Location  = new Point(240, 4),
                Size      = new Size(58, 22),
                TextAlign = ContentAlignment.MiddleRight
            };

            var slider = new TrackBar
            {
                Minimum   = 0,
                Maximum   = 100,
                Value     = app.VolumePercent,
                TickStyle = TickStyle.None,
                Location  = new Point(6, 28),
                Size      = new Size(292, 24),
                BackColor = Color.FromArgb(24, 24, 24)
            };
            slider.ValueChanged += (s, e) =>
            {
                lblPct.Text = $"{slider.Value}%";
                _audioService.SetAppVolume(app.Id, slider.Value / 100f);
            };

            card.Controls.AddRange(new Control[] { lblName, lblPct, slider });
            return card;
        }

        private Panel BtCard(BluetoothDeviceModel bt)
        {
            var card = Card(308, 54, CardColor);

            var dot = new Panel
            {
                Size      = new Size(8, 8),
                Location  = new Point(12, 23),
                BackColor = bt.IsConnected ? Color.FromArgb(78, 202, 132) : Color.FromArgb(55, 55, 55)
            };

            var lblName = new Label
            {
                Text      = bt.Name,
                ForeColor = TextPrimary,
                Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Location  = new Point(28, 6),
                Size      = new Size(180, 20),
                TextAlign = ContentAlignment.MiddleLeft
            };

            var batt  = bt.HasBatteryInfo ? $" · 🔋{bt.BatteryLevel}%" : "";
            var lblSub = new Label
            {
                Text      = (bt.IsConnected ? "Conectado" : "Desconectado") + batt,
                ForeColor = bt.IsConnected ? Color.FromArgb(78, 202, 132) : TextSecondary,
                Font      = new Font("Segoe UI", 8f),
                Location  = new Point(28, 28),
                Size      = new Size(180, 18),
                TextAlign = ContentAlignment.MiddleLeft
            };

            var actionBtn = new Button
            {
                Text      = bt.IsConnected ? "Desconectar" : "Conectar",
                ForeColor = bt.IsConnected ? Color.FromArgb(255, 100, 90) : TextPrimary,
                BackColor = Color.FromArgb(38, 38, 38),
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Size      = new Size(90, 28),
                Location  = new Point(210, 13),
                Cursor    = Cursors.Hand,
                FlatAppearance = { BorderColor = BorderColor, BorderSize = 1 }
            };
            actionBtn.Click += async (s, e) =>
            {
                actionBtn.Enabled = false;
                actionBtn.Text    = "Aguarde...";
                await _btService.ToggleConnectionAsync(bt);
                if (bt.IsConnected) _audioService.RefreshAudioDevices();
                RenderAudio();
                RenderBluetooth();
            };

            card.Controls.AddRange(new Control[] { dot, lblName, lblSub, actionBtn });
            return card;
        }

        // ── Helpers ───────────────────────────────────────────────────────────
        private static Panel Card(int w, int h, Color bg)
        {
            var p = new Panel
            {
                Width     = w,
                Height    = h,
                BackColor = bg,
                Margin    = new Padding(0, 0, 0, 5)
            };
            p.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var pen = new Pen(BorderColor, 1);
                var path = Rounded(new Rectangle(0, 0, p.Width - 1, p.Height - 1), 8);
                e.Graphics.DrawPath(pen, path);
            };
            return p;
        }

        private static Label SectionLabel(string text) => new Label
        {
            Text      = text,
            ForeColor = Color.FromArgb(85, 85, 85),
            Font      = new Font("Segoe UI", 7.5f, FontStyle.Bold),
            Size      = new Size(306, 20),
            TextAlign = ContentAlignment.MiddleLeft,
            Margin    = new Padding(0, 4, 0, 2)
        };

        private static GraphicsPath Rounded(Rectangle b, int r)
        {
            var p = new GraphicsPath();
            p.AddArc(b.X,          b.Y,           r*2, r*2, 180, 90);
            p.AddArc(b.Right-r*2,  b.Y,           r*2, r*2, 270, 90);
            p.AddArc(b.Right-r*2,  b.Bottom-r*2,  r*2, r*2,   0, 90);
            p.AddArc(b.X,          b.Bottom-r*2,  r*2, r*2,  90, 90);
            p.CloseFigure();
            return p;
        }

        // ── Tab switch ────────────────────────────────────────────────────────
        private void SwitchTab(bool audio)
        {
            _pnlAudio.Visible     = audio;
            _pnlBluetooth.Visible = !audio;
            _btnAudio.BackColor     = audio  ? AccentGreen       : Color.Transparent;
            _btnAudio.ForeColor     = audio  ? TextPrimary       : TextSecondary;
            _btnBluetooth.BackColor = !audio ? AccentGreen       : Color.Transparent;
            _btnBluetooth.ForeColor = !audio ? TextPrimary       : TextSecondary;
        }

        // ── Refresh ───────────────────────────────────────────────────────────
        private new void Refresh()
        {
            _audioService.RefreshAudioDevices();
            RenderAudio();
            RenderBluetooth();
        }

        // ── Drag ──────────────────────────────────────────────────────────────
        private void HeaderDrag(object? s, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) { _dragging = true; _dragOffset = e.Location; }
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_dragging && e.Button == MouseButtons.Left)
                Location = new Point(Location.X + e.X - _dragOffset.X, Location.Y + e.Y - _dragOffset.Y);
        }
        protected override void OnMouseUp(MouseEventArgs e) { _dragging = false; }

        // ── Win32 hooks ───────────────────────────────────────────────────────
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            int r = DWMWCP_ROUND;
            DwmSetWindowAttribute(Handle, DWMWA_WINDOW_CORNER_PREFERENCE, ref r, sizeof(int));
            RegisterHotKey(Handle, HOTKEY_ID, MOD_WIN | MOD_SHIFT, VK_M);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing) { e.Cancel = true; Hide(); return; }
            UnregisterHotKey(Handle, HOTKEY_ID);
            base.OnFormClosing(e);
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_HOTKEY = 0x0312;
            if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == HOTKEY_ID)
            {
                if (Visible) Hide(); else { Show(); Activate(); BringToFront(); }
            }
            base.WndProc(ref m);
        }
    }
}
