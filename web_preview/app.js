document.addEventListener('DOMContentLoaded', () => {
  const state = {
    pinned: false,
    audioDevices: [
      { id: 'dev1', name: 'Alto-falantes (Realtek)', isDefault: true, icon: '🔊' },
      { id: 'dev2', name: 'Sony WH-1000XM4', isDefault: false, icon: '🎧' },
      { id: 'dev3', name: 'NVIDIA HDMI Output', isDefault: false, icon: '📺' }
    ],
    apps: [
      { id: 'app1', name: 'Cyberpunk 2077', vol: 90, icon: '🎮' },
      { id: 'app2', name: 'Spotify', vol: 40, icon: '🎵' },
      { id: 'app3', name: 'Discord', vol: 85, icon: '💬' }
    ],
    bluetoothDevices: [
      { id: 'bt1', name: 'Sony WH-1000XM4', status: 'connected', battery: 90 },
      { id: 'bt2', name: 'Galaxy Buds2 Pro', status: 'disconnected', battery: 75 },
      { id: 'bt3', name: 'Xbox Headset', status: 'disconnected', battery: 100 }
    ]
  };

  const pillBtns = document.querySelectorAll('.pill-btn');
  const tabPanes = document.querySelectorAll('.tab-pane');
  const audioDevicesList = document.getElementById('audioDevicesList');
  const appMixerList = document.getElementById('appMixerList');
  const btDevicesList = document.getElementById('btDevicesList');
  const pinBtn = document.getElementById('pinBtn');
  const refreshBtn = document.getElementById('refreshBtn');

  pillBtns.forEach(btn => {
    btn.addEventListener('click', () => {
      pillBtns.forEach(b => b.classList.remove('active'));
      tabPanes.forEach(p => p.classList.remove('active'));
      btn.classList.add('active');
      document.getElementById(`tab-${btn.getAttribute('data-tab')}`).classList.add('active');
    });
  });

  pinBtn.addEventListener('click', () => {
    state.pinned = !state.pinned;
    pinBtn.classList.toggle('active', state.pinned);
  });

  refreshBtn.addEventListener('click', () => {
    refreshBtn.style.transform = 'rotate(360deg)';
    refreshBtn.style.transition = 'transform 0.4s ease';
    render();
    setTimeout(() => { refreshBtn.style.transform = 'none'; }, 400);
  });

  function renderAudioDevices() {
    audioDevicesList.innerHTML = '';
    state.audioDevices.forEach(dev => {
      const item = document.createElement('div');
      item.className = `device-item ${dev.isDefault ? 'active-dev' : ''}`;
      item.innerHTML = `
        <div class="item-left">
          <span class="item-icon">${dev.icon}</span>
          <span class="item-title">${dev.name}</span>
        </div>
        ${dev.isDefault ? '<div class="active-dot"></div>' : ''}
      `;
      item.addEventListener('click', () => {
        state.audioDevices.forEach(d => d.isDefault = false);
        dev.isDefault = true;
        render();
      });
      audioDevicesList.appendChild(item);
    });
  }

  function renderAppMixer() {
    appMixerList.innerHTML = '';
    state.apps.forEach(app => {
      const item = document.createElement('div');
      item.className = 'app-item';
      item.innerHTML = `
        <div class="app-meta">
          <span>${app.icon} ${app.name}</span>
          <span class="app-vol-val" id="vol-${app.id}">${app.vol}%</span>
        </div>
        <div class="slider-row">
          <input type="range" min="0" max="100" value="${app.vol}" id="range-${app.id}">
        </div>
      `;
      appMixerList.appendChild(item);

      const slider = item.querySelector(`#range-${app.id}`);
      slider.addEventListener('input', (e) => {
        app.vol = e.target.value;
        item.querySelector(`#vol-${app.id}`).textContent = `${app.vol}%`;
      });
    });
  }

  function renderBluetoothDevices() {
    btDevicesList.innerHTML = '';
    state.bluetoothDevices.forEach(bt => {
      const item = document.createElement('div');
      item.className = 'bt-item';
      item.innerHTML = `
        <div class="item-left">
          <div class="bt-status-dot ${bt.status === 'connected' ? 'connected' : ''}"></div>
          <div>
            <div class="item-title">${bt.name}</div>
            <div class="bt-sub">${bt.status === 'connected' ? 'Conectado' : 'Desconectado'} ${bt.battery ? `• 🔋${bt.battery}%` : ''}</div>
          </div>
        </div>
        <button class="action-btn ${bt.status === 'connected' ? 'active-conn' : ''}">
          ${bt.status === 'connected' ? 'Desconectar' : 'Conectar'}
        </button>
      `;

      item.querySelector('.action-btn').addEventListener('click', () => {
        if (bt.status === 'connected') {
          bt.status = 'disconnected';
        } else {
          state.bluetoothDevices.forEach(d => d.status = 'disconnected');
          bt.status = 'connected';
          const match = state.audioDevices.find(a => a.name.includes(bt.name.split(' ')[0]));
          if (match) {
            state.audioDevices.forEach(d => d.isDefault = false);
            match.isDefault = true;
          }
        }
        render();
      });

      btDevicesList.appendChild(item);
    });
  }

  function render() {
    renderAudioDevices();
    renderAppMixer();
    renderBluetoothDevices();
  }

  render();
});
