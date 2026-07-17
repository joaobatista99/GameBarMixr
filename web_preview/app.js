document.addEventListener('DOMContentLoaded', () => {
  // State
  const state = {
    pinned: false,
    activeTab: 'audio',
    audioDevices: [
      { id: 'dev1', name: 'Alto-falantes (Realtek Audio)', isDefault: true, vol: 80, icon: '🔊' },
      { id: 'dev2', name: 'Sony WH-1000XM4 (Bluetooth)', isDefault: false, vol: 65, icon: '🎧' },
      { id: 'dev3', name: 'NVIDIA HDMI Output (LG TV)', isDefault: false, vol: 50, icon: '📺' }
    ],
    apps: [
      { id: 'app1', name: 'Cyberpunk 2077', vol: 90, icon: '🎮' },
      { id: 'app2', name: 'Spotify Music', vol: 40, icon: '🎵' },
      { id: 'app3', name: 'Discord Voice', vol: 85, icon: '💬' }
    ],
    bluetoothDevices: [
      { id: 'bt1', name: 'Sony WH-1000XM4', status: 'connected', battery: 90, icon: '🎧' },
      { id: 'bt2', name: 'Galaxy Buds2 Pro', status: 'disconnected', battery: 75, icon: '🎧' },
      { id: 'bt3', name: 'Xbox Wireless Headset', status: 'disconnected', battery: 100, icon: '🎧' },
      { id: 'bt4', name: 'JBL Flip 6 Speaker', status: 'disconnected', battery: null, icon: '📻' }
    ]
  };

  // DOM Elements
  const tabBtns = document.querySelectorAll('.tab-btn');
  const tabPanes = document.querySelectorAll('.tab-pane');
  const audioDevicesList = document.getElementById('audioDevicesList');
  const appMixerList = document.getElementById('appMixerList');
  const btDevicesList = document.getElementById('btDevicesList');
  const pinBtn = document.getElementById('pinBtn');
  const refreshBtn = document.getElementById('refreshBtn');
  const btConnectedBadge = document.getElementById('btConnectedBadge');

  // Tab switching
  tabBtns.forEach(btn => {
    btn.addEventListener('click', () => {
      tabBtns.forEach(b => b.classList.remove('active'));
      tabPanes.forEach(p => p.classList.remove('active'));
      btn.classList.add('active');
      const tabId = btn.getAttribute('data-tab');
      document.getElementById(`tab-${tabId}`).classList.add('active');
    });
  });

  // Pin Toggle
  pinBtn.addEventListener('click', () => {
    state.pinned = !state.pinned;
    pinBtn.classList.toggle('active', state.pinned);
    pinBtn.setAttribute('title', state.pinned ? 'Widget Fixado na Tela' : 'Fixar Widget na Tela (Pin)');
  });

  // Refresh Animation
  refreshBtn.addEventListener('click', () => {
    refreshBtn.style.transform = 'rotate(360deg)';
    refreshBtn.style.transition = 'transform 0.5s ease';
    render();
    setTimeout(() => { refreshBtn.style.transform = 'none'; }, 500);
  });

  // Render Functions
  function renderAudioDevices() {
    audioDevicesList.innerHTML = '';
    state.audioDevices.forEach(dev => {
      const card = document.createElement('div');
      card.className = `device-card ${dev.isDefault ? 'default' : ''}`;
      card.innerHTML = `
        <div class="device-info">
          <span class="device-icon">${dev.icon}</span>
          <div>
            <div class="device-name">${dev.name}</div>
            <div class="device-sub">Volume Padrão: ${dev.vol}%</div>
          </div>
        </div>
        ${dev.isDefault ? '<span class="active-pill">ATIVO</span>' : ''}
      `;
      card.addEventListener('click', () => {
        state.audioDevices.forEach(d => d.isDefault = false);
        dev.isDefault = true;
        render();
      });
      audioDevicesList.appendChild(card);
    });
  }

  function renderAppMixer() {
    appMixerList.innerHTML = '';
    state.apps.forEach(app => {
      const card = document.createElement('div');
      card.className = 'app-card';
      card.innerHTML = `
        <div class="app-header">
          <span>${app.icon} ${app.name}</span>
          <span class="vol-percent" id="vol-val-${app.id}">${app.vol}%</span>
        </div>
        <div class="slider-container">
          <input type="range" min="0" max="100" value="${app.vol}" id="slider-${app.id}">
        </div>
      `;
      appMixerList.appendChild(card);

      const slider = card.querySelector(`#slider-${app.id}`);
      slider.addEventListener('input', (e) => {
        app.vol = e.target.value;
        card.querySelector(`#vol-val-${app.id}`).textContent = `${app.vol}%`;
      });
    });
  }

  function renderBluetoothDevices() {
    btDevicesList.innerHTML = '';
    let connectedCount = 0;

    state.bluetoothDevices.forEach(bt => {
      if (bt.status === 'connected') connectedCount++;

      const card = document.createElement('div');
      card.className = 'bt-card';
      card.innerHTML = `
        <div class="bt-info">
          <span class="device-icon">${bt.icon}</span>
          <div>
            <div class="device-name">${bt.name}</div>
            <div class="bt-status ${bt.status === 'connected' ? 'connected' : ''}">
              <span>${bt.status === 'connected' ? '🟢 Conectado' : '⚪ Desconectado'}</span>
              ${bt.battery ? `<span>• 🔋 ${bt.battery}%</span>` : ''}
            </div>
          </div>
        </div>
        <button class="bt-action-btn ${bt.status === 'connected' ? 'disconnect' : ''}">
          ${bt.status === 'connected' ? 'Desconectar' : 'Conectar'}
        </button>
      `;

      const btn = card.querySelector('.bt-action-btn');
      btn.addEventListener('click', (e) => {
        e.stopPropagation();
        if (bt.status === 'connected') {
          bt.status = 'disconnected';
        } else {
          // Disconnect any other Bluetooth device, then connect this one
          state.bluetoothDevices.forEach(d => d.status = 'disconnected');
          bt.status = 'connected';
          // Auto switch default audio device to this Bluetooth headphone
          const matchedAudio = state.audioDevices.find(a => a.name.includes(bt.name.split(' ')[0]));
          if (matchedAudio) {
            state.audioDevices.forEach(d => d.isDefault = false);
            matchedAudio.isDefault = true;
          }
        }
        render();
      });

      btDevicesList.appendChild(card);
    });

    btConnectedBadge.textContent = connectedCount;
  }

  function render() {
    renderAudioDevices();
    renderAppMixer();
    renderBluetoothDevices();
  }

  // Initial render
  render();
});
