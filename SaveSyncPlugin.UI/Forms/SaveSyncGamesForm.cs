using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using SaveSyncPlugin.Core.Models;
using SaveSyncPlugin.Core.Storage;
using Unbroken.LaunchBox.Plugins;
using Unbroken.LaunchBox.Plugins.Data;

namespace SaveSyncPlugin.UI.Forms
{
    public partial class SaveSyncGamesForm : Form
    {
        private List<GameSaveConfig> _configs;
        private List<IGame> _allGames;
        private List<IPlatform> _platforms;

        public SaveSyncGamesForm(List<GameSaveConfig> configs)
        {
            _configs = configs ?? new List<GameSaveConfig>();
            InitializeComponent();
            LoadPlatforms();
        }

        private void CmbPlatform_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectPlatform();
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            FilterGames();
        }

        private void LstGames_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectGame();
        }

        private void CmbSaveType_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdatePathHint();
        }

        private void BtnSaveAll_Click(object sender, EventArgs e)
        {
            SaveCurrentConfig();
            SaveSyncStorage.SaveGameConfigs(_configs);
            DialogResult = DialogResult.OK;
            Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private IGame _currentGame;
        private string _currentPlatform;

        private void LoadPlatforms()
        {
            try
            {
                var all = PluginHelper.DataManager.GetAllPlatforms();
                if (all == null) return;

                _platforms = all
                    .Where(p => p.Name != null &&
                        p.Name.Contains("Windows", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(p => p.Name)
                    .ToList();

                cmbPlatform.Items.Clear();
                foreach (var p in _platforms)
                    cmbPlatform.Items.Add(p.Name);

                if (cmbPlatform.Items.Count > 0)
                    cmbPlatform.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                lstGames.Items.Add("(error loading platforms: " + ex.Message + ")");
            }
        }

        private void SelectPlatform()
        {
            _currentPlatform = cmbPlatform.SelectedItem as string;
            LoadGamesForPlatform();
        }

        private void LoadGamesForPlatform()
        {
            try
            {
                if (string.IsNullOrEmpty(_currentPlatform))
                {
                    _allGames = null;
                    lstGames.Items.Clear();
                    return;
                }

                _allGames = PluginHelper.DataManager.GetAllGames()
                    ?.Where(g => g.Platform != null &&
                        g.Platform.Equals(_currentPlatform, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            catch (Exception ex)
            {
                _allGames = null;
                lstGames.Items.Clear();
                lstGames.Items.Add("(error: " + ex.Message + ")");
                return;
            }

            FilterGames();
        }

        private void FilterGames()
        {
            lstGames.Items.Clear();
            if (_allGames == null) return;

            var filter = txtSearch.Text?.Trim() ?? "";

            var filtered = string.IsNullOrEmpty(filter)
                ? _allGames
                : _allGames.Where(g => g.Title != null &&
                    g.Title.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

            foreach (var g in filtered)
                lstGames.Items.Add(g);
        }

        private void SelectGame()
        {
            SaveCurrentConfig();

            _currentGame = lstGames.SelectedItem as IGame;
            cmbSaveType.SelectedIndex = 0;
            txtFolderName.Text = "";
            txtRelativePaths.Text = "";
            lblPathHint.Text = "";
            lblStatus.Text = "";

            if (_currentGame == null) return;

            var config = _configs.Find(c => c.GameId == _currentGame.Id)
                ?? _configs.Find(c => c.Title != null &&
                    _currentGame.Title.IndexOf(c.Title, StringComparison.OrdinalIgnoreCase) >= 0);

            if (config != null)
            {
                if (string.IsNullOrEmpty(config.GameId))
                    config.GameId = _currentGame.Id;
                config.Title = _currentGame.Title;
                switch (config.SaveType?.ToLower())
                {
                    case "relative": cmbSaveType.SelectedIndex = 0; break;
                    case "appdata": cmbSaveType.SelectedIndex = 1; break;
                    case "userprofile": cmbSaveType.SelectedIndex = 2; break;
                    case "public": cmbSaveType.SelectedIndex = 3; break;
                }
                txtFolderName.Text = config.FolderName ?? "";
                txtRelativePaths.Text = string.Join("\r\n", config.RelativePaths ?? new List<string>());
                lblStatus.Text = $"Config: Active [{_currentPlatform}]";
            }
            else
            {
                var defaults = SaveSyncStorage.LoadGameDefaults();
                var def = defaults.Find(d =>
                    d.Title != null &&
                    _currentGame.Title.IndexOf(d.Title, StringComparison.OrdinalIgnoreCase) >= 0);
                if (def != null)
                {
                    switch (def.SaveType?.ToLower())
                    {
                        case "relative": cmbSaveType.SelectedIndex = 0; break;
                        case "appdata": cmbSaveType.SelectedIndex = 1; break;
                        case "userprofile": cmbSaveType.SelectedIndex = 2; break;
                        case "public": cmbSaveType.SelectedIndex = 3; break;
                    }
                    txtFolderName.Text = def.FolderName ?? "";
                    txtRelativePaths.Text = string.Join("\r\n", def.RelativePaths ?? new List<string>());
                    lblStatus.Text = $"Default loaded [{_currentPlatform}]";
                }
                else
                {
                    txtFolderName.Text = _currentGame.Title ?? "";
                    lblStatus.Text = $"[{_currentPlatform}]";
                }
            }
            UpdatePathHint();
        }

        private void UpdatePathHint()
        {
            var saveType = cmbSaveType.SelectedItem as string;
            switch (saveType)
            {
                case "relative":
                    lblPathHint.Text = "Paths relative to game folder. Files: gm.dat  Folders: savedata";
                    break;
                case "appdata":
                    lblPathHint.Text = "Subpaths under %APPDATA%, one per line";
                    break;
                case "userprofile":
                    lblPathHint.Text = "Subpaths under %USERPROFILE%, one per line";
                    break;
                case "public":
                    lblPathHint.Text = "Subpaths under %PUBLIC%\\Documents, one per line";
                    break;
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (_currentGame == null) return;
            SaveCurrentConfig();
            SaveSyncStorage.SaveGameConfigs(_configs);
            lblStatus.Text = "Saved!";
        }

        private void SaveCurrentConfig()
        {
            if (_currentGame == null) return;

            var saveType = cmbSaveType.SelectedItem as string;
            var folderName = txtFolderName.Text.Trim();
            var paths = txtRelativePaths.Text
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .Where(p => p.Length > 0)
                .ToList();

            if (string.IsNullOrWhiteSpace(folderName) && paths.Count == 0)
            {
                _configs.RemoveAll(c => c.GameId == _currentGame.Id);
                return;
            }

            var existing = _configs.Find(c => c.GameId == _currentGame.Id);
            if (existing == null)
            {
                existing = new GameSaveConfig { GameId = _currentGame.Id };
                _configs.Add(existing);
            }

            existing.Title = _currentGame.Title;
            existing.FolderName = string.IsNullOrWhiteSpace(folderName) ? _currentGame.Title : folderName;
            existing.SaveType = saveType;
            existing.RelativePaths = paths;
        }

        private void BtnRemove_Click(object sender, EventArgs e)
        {
            if (_currentGame == null) return;

            var config = _configs.Find(c => c.GameId == _currentGame.Id);
            if (config != null)
            {
                _configs.Remove(config);
                SaveSyncStorage.SaveGameConfigs(_configs);
                txtFolderName.Text = "";
                txtRelativePaths.Text = "";
                lblStatus.Text = "Config removed";
            }
        }
    }
}