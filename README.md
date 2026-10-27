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

#### Emulators launched through a wrapper

When the emulator entry points at a wrapper (for example
`Vita3KWrapper.exe`), the paths are relative to the **wrapper folder** by default.
If the emulator data lives somewhere else, set `LocalBase` (field *Emulator
directory* in the UI) — it is relative to the LaunchBox folder, so the whole tree
stays portable:

```json
{
  "Title": "Vita3K",
  "FolderName": "PSVita",
  "LocalBase": "..\\Emuladores\\PSVita\\portable\\fs\\ux0\\user\\00",
  "RelativePaths": [ "savedata" ]
}
```

- local: `D:\Jogos\LaunchBox` + `LocalBase` + `savedata`
- server: `<ServerBasePath>\PSVita\savedata`

The server side is always resolved **inside** `ServerBasePath`: a relative path
that would escape it (like `..\..\..\Emuladores\...`) is refused and logged as
`SKIP` instead of writing outside the backup folder.

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

#### Non-destructive by default

Every sync runs robocopy with `/E /XO`:

- files are copied in both directions, so a save that exists on only one side is kept;
- `/XO` means the **newest** copy wins — a file is never overwritten by an older one;
- **nothing is deleted.** Older builds passed `/PURGE` on every call, so a save made
  while the server folder was offline (or not yet synced) was silently deleted on the
  next LaunchBox startup.

Mirror mode (`DeleteExtraneous: true` in `settings.json`, or the checkbox in
`Tools > SaveSync: Settings`) re-enables `/PURGE`. Even then it is refused when the
source is empty or the destination is not an absolute path — that guard is what stops
an empty/unmounted server folder from wiping your saves.

#### Conflict resolution vs. deletion

Two independent things, often confused:

| Setting | Decides |
|---|---|
| `AlwaysKeepNewer` (default `true`) | **direction**: the newest copy wins, no prompt |
| `AskOnConflict` (default `true`) | **direction**: ask which side to keep (only when `AlwaysKeepNewer` is off) |
| `DeleteExtraneous` (default `false`) | **whether anything may be deleted** (`/PURGE`) |

So resolving a conflict never deletes the losing file from the other side unless
mirror mode is on — the older copy is simply overwritten in the winning direction.

Every robocopy call is appended to `Plugins\SaveSync LaunchBox Integration\sync.log`
(command line + exit code), together with `LAUNCH`/`EXIT` lines for the game
lifecycle and `SKIP` lines for refused paths, so the sync can be audited.

### 🧠 Data Integrity

On startup, the plugin validates all configurations:

- Checks if referenced emulators still exist in LaunchBox
- Checks if referenced games still exist in LaunchBox
- If a configuration references a deleted item, prompts the user to remove or keep it
- Re-applies missing Additional Applications automatically

### 🖥️ Windows Forms Configuration

All settings are configured through WinForms dialogs inside LaunchBox:

- **Settings**: backup server path, startup/game-close sync toggles, conflict resolution
  (`AlwaysKeepNewer`/`AskOnConflict`) and mirror mode (`DeleteExtraneous`)
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
- "Newest wins" compares file timestamps: two PCs with very different clocks can make an
  older save look newer. Keep the clocks in sync (Windows time service) if you play on both.

---

## 📝 Changelog

### Unreleased

- **fix:** emulator paths for wrapped emulators. The same relative path was applied to
  the local base (the wrapper folder) and to the server base, so `..\..\..\Emuladores\...`
  escaped `ServerBasePath` and the "backup" was written to `C:\Drive\Emuladores` instead
  of `SavesBackup`. New optional `LocalBase` (field *Emulator directory*) sets the local
  base independently, relative to the LaunchBox folder; server paths are resolved inside
  `ServerBasePath` and refused (`SKIP`) when they would escape it.
- **fix:** local paths are resolved against the LaunchBox folder instead of the process
  working directory.
- **feat:** `sync.log` now records `LAUNCH`/`EXIT` (game, emulator, application) and
  `SKIP` reasons, plus a warning when an emulator uses a wrapper and `LocalBase` is empty.
- **fix:** `SyncByEmulator` forwards progress to the UI; `OnGameExited` falls back to the
  per-game config when no emulator is resolved.
- **test:** `PathResolverTests` (11 tests) covering the escape regression.
- **fix:** sync is non-destructive by default. `/PURGE` was passed on every robocopy
  call, so a save that existed on only one side (offline play, power loss, new save
  slot, server folder not yet synced) was deleted by the other side. The default is now
  `/E /XO` (copy both ways, newest wins, nothing deleted).
- **feat:** mirror mode is opt-in via `DeleteExtraneous` in `settings.json` (checkbox in
  the Settings dialog, with a confirmation prompt) and is refused for empty sources or
  relative destinations.
- **fix:** conflict resolution from v0.0.1 (`AlwaysKeepNewer`/`AskOnConflict`,
  `ConflictForm`) is kept and now only decides the **direction** of the copy. The
  previous flow passed `/PURGE` on every conflict, which deleted the losing file from
  the other side; `DeleteExtraneous` is now the single switch that can delete anything.
  Conflicts are logged as `CONFLICT` in `sync.log`.
- **feat:** every robocopy call is logged to `sync.log` (command line + exit code).
- **refactor:** robocopy argument building moved to `SaveSyncPlugin.Core.Sync.RobocopyRunner`,
  shared by the plugin and the CLI (it was duplicated).
- **test:** new `SaveSyncPlugin.Tests` project covering the deletion scenarios.

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

#### Emuladores iniciados por um wrapper

Quando a entrada do emulador aponta para um wrapper (por exemplo
`Vita3KWrapper.exe`), os caminhos são relativos à **pasta do wrapper** por padrão.
Se os dados do emulador ficam em outro lugar, preencha `LocalBase` (campo
*Emulator directory* na interface) — ele é relativo à pasta do LaunchBox, então
a árvore continua portátil:

```json
{
  "Title": "Vita3K",
  "FolderName": "PSVita",
  "LocalBase": "..\\Emuladores\\PSVita\\portable\\fs\\ux0\\user\\00",
  "RelativePaths": [ "savedata" ]
}
```

- local: `D:\Jogos\LaunchBox` + `LocalBase` + `savedata`
- servidor: `<ServerBasePath>\PSVita\savedata`

O lado do servidor é sempre resolvido **dentro** de `ServerBasePath`: um caminho
relativo que escaparia dele (como `..\..\..\Emuladores\...`) é recusado e registrado
como `SKIP`, em vez de gravar fora da pasta de backup.

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

#### Não destrutiva por padrão

Toda sincronização roda robocopy com `/E /XO`:

- os arquivos são copiados nos dois sentidos, então um save que existe só de um lado é mantido;
- `/XO` significa que a cópia **mais nova** vence — um arquivo nunca é sobrescrito por um mais antigo;
- **nada é apagado.** Versões anteriores passavam `/PURGE` em toda chamada, então um save feito
  enquanto a pasta do servidor estava offline (ou ainda não sincronizada) era apagado em silêncio
  na próxima inicialização do LaunchBox.

O modo espelho (`DeleteExtraneous: true` no `settings.json`, ou a caixa em
`Tools > SaveSync: Settings`) reativa o `/PURGE`. Mesmo assim ele é recusado quando a origem
está vazia ou o destino não é um caminho absoluto — essa proteção é o que impede uma pasta de
servidor vazia/desmontada de apagar seus saves.

#### Conflito x exclusão

Duas coisas independentes, que costumam ser confundidas:

| Opção | Decide |
|---|---|
| `AlwaysKeepNewer` (padrão `true`) | **direção**: a cópia mais nova vence, sem perguntar |
| `AskOnConflict` (padrão `true`) | **direção**: pergunta qual lado manter (só quando `AlwaysKeepNewer` está desligado) |
| `DeleteExtraneous` (padrão `false`) | **se algo pode ser apagado** (`/PURGE`) |

Ou seja: resolver um conflito nunca apaga o arquivo perdedor do outro lado, a menos que o
modo espelho esteja ligado — a cópia mais antiga apenas é sobrescrita na direção vencedora.

Toda chamada de robocopy é registrada em `Plugins\SaveSync LaunchBox Integration\sync.log`
(linha de comando + código de saída), junto com linhas `LAUNCH`/`EXIT` do ciclo de vida do
jogo e `SKIP` para caminhos recusados, para auditoria.

### 🧠 Integridade dos Dados

Na inicialização, o plugin valida todas as configurações:

- Verifica se os emuladores referenciados ainda existem no LaunchBox
- Verifica se os jogos referenciados ainda existem no LaunchBox
- Se uma configuração referencia um item deletado, pergunta ao usuário se deseja remover ou manter
- Reaplica automaticamente Additional Applications ausentes

### 🖥️ Configuração via Windows Forms

Todas as configurações são feitas através de diálogos WinForms dentro do LaunchBox:

- **Configurações**: caminho do servidor de backup, ativação de sincronização automática,
  resolução de conflito (`AlwaysKeepNewer`/`AskOnConflict`) e modo espelho (`DeleteExtraneous`)
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
- "Mais novo vence" compara o horário dos arquivos: dois PCs com relógios muito diferentes
  podem fazer um save antigo parecer mais novo. Mantenha os relógios sincronizados (serviço
  de hora do Windows) se você joga nos dois.

---

## 📝 Histórico de Versões

### Não lançado

- **fix:** sincronização não destrutiva por padrão. O `/PURGE` era passado em toda
  chamada de robocopy, então um save que existia só de um lado (jogo offline, queda de
  energia, novo slot, pasta do servidor ainda não sincronizada) era apagado pelo outro
  lado. O padrão agora é `/E /XO` (copia nos dois sentidos, mais novo vence, nada é apagado).
- **feat:** modo espelho é opt-in via `DeleteExtraneous` no `settings.json` (caixa no
  diálogo de Configurações, com confirmação) e é recusado para origem vazia ou destino
  relativo.
- **fix:** a detecção de conflito da v0.0.1 (`AlwaysKeepNewer`/`AskOnConflict`,
  `ConflictForm`) foi mantida e agora decide apenas a **direção** da cópia. O fluxo
  anterior passava `/PURGE` em todo conflito, apagando o arquivo perdedor do outro lado;
  `DeleteExtraneous` passou a ser o único interruptor que permite apagar algo. Conflitos
  são registrados como `CONFLICT` no `sync.log`.
- **feat:** toda chamada de robocopy é registrada em `sync.log` (linha de comando + código
  de saída).
- **refactor:** a montagem dos argumentos do robocopy foi para
  `SaveSyncPlugin.Core.Sync.RobocopyRunner`, compartilhada entre plugin e CLI (estava duplicada).
- **fix:** caminhos de emuladores iniciados por wrapper. A mesma string relativa era
  aplicada na base local (pasta do wrapper) e na base do servidor, então
  `..\..\..\Emuladores\...` escapava do `ServerBasePath` e o "backup" ia para
  `C:\Drive\Emuladores` em vez de `SavesBackup`. Novo campo opcional `LocalBase`
  (*Emulator directory*) define a base local, relativa à pasta do LaunchBox; caminhos do
  servidor são resolvidos dentro de `ServerBasePath` e recusados (`SKIP`) quando escapam.
- **fix:** caminhos locais resolvidos contra a pasta do LaunchBox, não contra o diretório
  de trabalho do processo.
- **feat:** o `sync.log` agora registra `LAUNCH`/`EXIT` (jogo, emulador, aplicativo) e o
  motivo dos `SKIP`, além de um aviso quando o emulador usa wrapper e `LocalBase` está vazio.
- **fix:** `SyncByEmulator` encaminha o progresso para a UI; `OnGameExited` cai na config
  por jogo quando nenhum emulador é resolvido.
- **test:** `PathResolverTests` (11 testes) cobrindo a regressão do escape.
- **test:** novo projeto `SaveSyncPlugin.Tests` cobrindo os cenários de exclusão.

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
