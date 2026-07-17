# 🎮 GameBarMixr - Audio & Bluetooth Mixer Widget para Xbox Game Bar

**GameBarMixr** é um widget nativo desenvolvido para a **Xbox Game Bar (Windows 10/11)** que resolve a frustração de perder o foco dos jogos ao trocar de fone Bluetooth ou ajustar o volume do áudio.

Com o GameBarMixr, você aciona a Game Bar (`Win + G`) sobre a tela de qualquer jogo (em janela ou tela cheia) e altera seus dispositivos de som e fones Bluetooth em 1 clique.

---

## ✨ Recursos Principais

- 🎧 **Troca Rápida de Fone Bluetooth**: Conecte e desconecte fones, earbuds (AirPods, Galaxy Buds, Sony, etc.) e soundbars sem abrir a janela de Configurações do Windows.
- 🎛️ **Mixer de Áudio de Saída**: Troque instantaneamente o dispositivo padrão de reprodução (Alto-falantes / Headset) e controle o volume master e individual por jogo ou aplicativo (Spotify, Discord, etc.).
- ⚡ **Sem Perda de Foco**: O widget roda como um overlay nativo da Xbox Game Bar, mantendo o jogo ativo.
- 📌 **Suporte a Pinagem (Pin Widget)**: Mantenha o widget visível em um canto da tela durante a jogatina se desejar.
- 🏪 **Microsoft Store Ready**: Estrutura de manifesto MSIX e capacidades prontas para publicação na loja de aplicativos do Windows.

---

## ⚡ Instalação Rápida no Windows (1 Clique)

Para testar e utilizar o widget na sua máquina:

1. Baixe ou clone este repositório:
   ```bash
   git clone https://github.com/seu-usuario/GameBarMixr.git
   cd GameBarMixr
   ```
2. Abra o **PowerShell** no Windows e execute o script de instalação rápida:
   ```powershell
   .\Scripts\install_widget.ps1
   ```
3. Pressione no teclado: **`Win + G`**
4. Abra o menu de Widgets (ícone no topo esquerdo) e clique em **Audio & Bluetooth Mixer (GameBarMixr)**!

---

## 🏪 Como Gerar o Pacote para Publicar na Microsoft Store

Para publicar o **GameBarMixr** no *Microsoft Partner Center*:

1. Execute o script de build MSIX:
   ```powershell
   .\Scripts\build_msix.ps1 -Configuration Release -Platform x64
   ```
2. O script gerará o arquivo `.msixbundle` na pasta `GameBarMixr\bin\MSIX\`.
3. No [Microsoft Partner Center](https://partner.microsoft.com/dashboard), crie um novo aplicativo, envie o arquivo `.msixbundle` gerado e defina a categoria como **Games > Utilities**.

---

## 🌐 Pré-visualização da Interface Web

Para visualizar e testar o layout e a interatividade da interface antes de compilar no Visual Studio:

1. Abra o arquivo [`web_preview/index.html`](file:///Users/joaovictorbatista/GameBarMixr/web_preview/index.html) em qualquer navegador (Edge, Chrome, Firefox).
2. Experimente trocar entre as abas "Mixer de Áudio" e "Fones Bluetooth", ajustar os sliders de volume e conectar fones simulados.

---

## 🛠️ Arquitetura do Projeto

- **Linguagem**: C# / WinUI 3 / UWP (.NET 8)
- **SDK**: `Microsoft.Gaming.XboxGameBar.SDK` (v1.6.2)
- **APIs Nativas**: `Windows.Devices.Bluetooth`, `Windows.Devices.Enumeration`, Windows CoreAudio / MMDeviceApi
- **Manifest**: `Package.appxmanifest` configurado com a extensão `windows.xboxGameBarWidget`

---

## 📜 Licença

Distribuído sob a licença MIT. Veja `LICENSE` para mais detalhes.
