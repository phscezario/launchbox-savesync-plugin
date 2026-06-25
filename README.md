# 🎮 LaunchBox SaveSync Plugin

[![License](https://img.shields.io/badge/license-GPL--3.0-blue)](LICENSE)
[![Platform](https://img.shields.io/badge/platform-LaunchBox-orange)](https://www.launchbox-app.com)

> Automatically sync your emulator and Windows game saves between your local machine and a backup server.

Sincronize automaticamente os saves dos seus emuladores e jogos Windows entre sua máquina local e um servidor de backup.

---

# 🇺🇸 English

## Overview

The **LaunchBox SaveSync Plugin** replaces manual save backup scripts (batch/AHK) with a fully integrated LaunchBox plugin. It automatically syncs save files for emulators and Windows games to a central backup server.

The plugin is designed to work independently — it does not require AutoHotkey or external batch scripts. It can detect game closures and trigger save uploads automatically.

Supports:
- Emulator save files, savestates, screenshots, NVRAM, and memory cards
- Windows game save files in local folders, `%APPDATA%`, or `%USERPROFILE%`
- Bi-directional sync (upload to server / download from server)
- Network-aware: works with local and network drives

---

## ✨ Main Features

### 📂 Emulator Save Configuration

Configure save folders for each emulator registered in LaunchBox:

- Add custom save paths per emulator (saves, states, screenshots, NVRAM, etc.)
- Each emulator can have multiple save folders with different types
- Automatically syncs all configured folders to your backup server
- Configurations are persisted in JSON files and survive updates

### 🎮 Windows Game Save Configuration

Configure save paths for individual Windows games:

- **Relative path**: saves are stored inside the game folder
- **AppData path**: saves are stored in `%APPDATA%`
- **UserProfile path**: saves are stored in `%USERPROFILE%\Documents` or similar
- Game configurations are linked to LaunchBox game IDs for integrity

### ⚙️ Additional Applications

The plugin automatically adds two **Additional Applications** to every configured game:

| Application | Trigger | Description |
|-------------|---------|-------------|
| `Upload Save (SaveSync)` | After game closes | Uploads saves to the backup server |
| `Download Save (SaveSync)` | Before game starts | Downloads saves from the backup server |

Applications are auto-restored if deleted, ensuring your sync setup is always available.

### 🔄 Save Synchronization

- **Startup sync**: automatically download all saves when LaunchBox starts
- **Game close sync**: automatically upload saves when a game closes
- **Manual sync**: use the Tools menu items for full control

### 🧠 Data Integrity

On startup, the plugin validates all configurations:

- Checks if referenced emulators still exist in LaunchBox
- Checks if referenced games still exist in LaunchBox
- If a configuration references a deleted item, prompts the user to remove or keep it
- Re-applies missing Additional Applications automatically

### 🖥️ Windows Forms Configuration

All settings are configured through WinForms dialogs inside LaunchBox:

- **Settings**: backup server path, startup/game-close sync toggles
- **Emulators**: browse all LaunchBox emulators, configure save folders
- **Windows Games**: browse Windows game platforms, configure save paths

---

## 📦 Installation

### 1. Download the Plugin

Download the latest release from the GitHub Repository.

### 2. Extract Into LaunchBox

Extract the plugin folder into:

```
LaunchBox/Plugins/SaveSync LaunchBox Integration
```

Expected structure:

```
LaunchBox
 └── Plugins
      └── SaveSync LaunchBox Integration
           ├── SaveSyncPlugin.dll
           ├── SaveSyncPlugin.Core.dll
           ├── SaveSyncPlugin.UI.dll
           ├── SaveSyncPlugin.CLI.exe
           └── settings.json
```

### 3. Open LaunchBox

Start LaunchBox normally. The plugin menus will become available automatically under `Tools > SaveSync`.

### 4. Configure the Plugin

Use `Tools > SaveSync: Settings` to set your backup server path.

Then use `Tools > SaveSync: Emulator Configs` and `Tools > SaveSync: Game Configs` to configure save paths.

---

## 🧠 Configuration Files

| File | Description |
|------|-------------|
| `settings.json` | General plugin settings (server path, toggles) |
| `emulators.json` | Emulator save folder configurations |
| `games.json` | Windows game save configurations |

## 🕹️ Menu Items

| Menu Item | Description |
|-----------|-------------|
| `SaveSync: Settings` | Configure server path and sync options |
| `SaveSync: Sync all now` | Run full sync |
| `SaveSync: Upload all to server` | Upload all saves to server |
| `SaveSync: Download all from server` | Download all saves from server |

## ⚠️ Known Limitations

- Requires robocopy.exe (included with Windows)
- Backup server path must be accessible from the machine
- Game closure detection requires LaunchBox/BigBox to be running

---

## 📝 Changelog

### v0.0.1
- Initial release
- Emulator save configuration UI
- Windows game save configuration UI
- Upload/download via robocopy
- Additional Applications auto-management
- Data integrity validation on startup

---

---

# 🇧🇷 Português

## Visão Geral

O **LaunchBox SaveSync Plugin** substitui scripts manuais de backup de save (batch/AHK) por um plugin totalmente integrado ao LaunchBox. Ele sincroniza automaticamente os arquivos de save de emuladores e jogos Windows para um servidor de backup central.

O plugin foi projetado para funcionar de forma independente — não requer AutoHotkey ou scripts batch externos. Ele pode detectar o fechamento de jogos e disparar uploads automaticamente.

Suporta:
- Arquivos de save de emuladores, savestates, screenshots, NVRAM e memory cards
- Arquivos de save de jogos Windows em pastas locais, `%APPDATA%` ou `%USERPROFILE%`
- Sincronização bidirecional (upload para o servidor / download do servidor)
- Consciente de rede: funciona com unidades locais e de rede

---

## ✨ Funcionalidades

### 📂 Configuração de Saves de Emuladores

Configure pastas de save para cada emulador registrado no LaunchBox:

- Adicione caminhos de save personalizados por emulador (saves, states, screenshots, NVRAM, etc.)
- Cada emulador pode ter múltiplas pastas de save com diferentes tipos
- Sincroniza automaticamente todas as pastas configuradas para o servidor de backup
- As configurações são persistidas em arquivos JSON e sobrevivem a atualizações

### 🎮 Configuração de Saves de Jogos Windows

Configure caminhos de save para jogos Windows individuais:

- **Caminho relativo**: saves armazenados dentro da pasta do jogo
- **Caminho AppData**: saves armazenados em `%APPDATA%`
- **Caminho UserProfile**: saves armazenados em `%USERPROFILE%\Documents` ou similar
- Configurações vinculadas aos IDs do LaunchBox para integridade

### ⚙️ Additional Applications

O plugin adiciona automaticamente duas **Additional Applications** a cada jogo configurado:

| Aplicação | Gatilho | Descrição |
|-----------|---------|-----------|
| `Upload Save (SaveSync)` | Após fechar o jogo | Envia saves para o servidor de backup |
| `Download Save (SaveSync)` | Antes de iniciar o jogo | Baixa saves do servidor de backup |

As aplicações são restauradas automaticamente se excluídas, garantindo que sua configuração de sincronização esteja sempre disponível.

### 🔄 Sincronização de Saves

- **Sincronização na inicialização**: baixa automaticamente todos os saves quando o LaunchBox inicia
- **Sincronização ao fechar jogo**: envia automaticamente os saves quando um jogo é fechado
- **Sincronização manual**: use os itens do menu Tools para controle total

### 🧠 Integridade dos Dados

Na inicialização, o plugin valida todas as configurações:

- Verifica se os emuladores referenciados ainda existem no LaunchBox
- Verifica se os jogos referenciados ainda existem no LaunchBox
- Se uma configuração referencia um item deletado, pergunta ao usuário se deseja remover ou manter
- Reaplica automaticamente Additional Applications ausentes

### 🖥️ Configuração via Windows Forms

Todas as configurações são feitas através de diálogos WinForms dentro do LaunchBox:

- **Configurações**: caminho do servidor de backup, ativação de sincronização automática
- **Emuladores**: navegue por todos os emuladores do LaunchBox, configure pastas de save
- **Jogos Windows**: navegue pelas plataformas de jogos Windows, configure caminhos de save

---

## 📦 Instalação

### 1. Baixe o Plugin

Baixe a versão mais recente pelo GitHub Repository.

### 2. Extraia Dentro do LaunchBox

Extraia a pasta do plugin em:

```
LaunchBox/Plugins/SaveSync LaunchBox Integration
```

Estrutura esperada:

```
LaunchBox
 └── Plugins
      └── SaveSync LaunchBox Integration
           ├── SaveSyncPlugin.dll
           ├── SaveSyncPlugin.Core.dll
           ├── SaveSyncPlugin.UI.dll
           ├── SaveSyncPlugin.CLI.exe
           └── settings.json
```

### 3. Abra o LaunchBox

Abra o LaunchBox normalmente. Os menus do plugin ficarão disponíveis automaticamente em `Tools > SaveSync`.

### 4. Configure o Plugin

Use `Tools > SaveSync: Settings` para definir o caminho do servidor de backup.

Depois use `Tools > SaveSync: Emulator Configs` e `Tools > SaveSync: Game Configs` para configurar os caminhos de save.

---

## 🧠 Arquivos de Configuração

| Arquivo | Descrição |
|---------|-----------|
| `settings.json` | Configurações gerais do plugin (servidor, ativações) |
| `emulators.json` | Configurações de pastas de save dos emuladores |
| `games.json` | Configurações de save dos jogos Windows |

## 🕹️ Itens do Menu

| Item do Menu | Descrição |
|--------------|-----------|
| `SaveSync: Settings` | Configurar servidor e opções de sincronização |
| `SaveSync: Sync all now` | Executar sincronização completa |
| `SaveSync: Upload all to server` | Enviar todos os saves para o servidor |
| `SaveSync: Download all from server` | Baixar todos os saves do servidor |

## ⚠️ Limitações Conhecidas

- Requer robocopy.exe (incluído no Windows)
- O caminho do servidor de backup precisa estar acessível pela máquina
- A detecção de fechamento de jogo requer que o LaunchBox/BigBox esteja em execução

---

## 📝 Histórico de Versões

### v0.0.1
- Versão inicial
- Interface de configuração de saves de emuladores
- Interface de configuração de saves de jogos Windows
- Upload/download via robocopy
- Gerenciamento automático de Additional Applications
- Validação de integridade na inicialização

---

## 🤝 Contributing / Contribuições

Contributions are welcome. / Contribuições são bem-vindas.

If you find bugs or want improvements:
- Open an issue
- Submit a pull request

---

## 📄 License / Licença

GPL-3.0 License
