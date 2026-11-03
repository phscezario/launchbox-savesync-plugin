using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using SaveSyncPlugin.Core.Models;
using SaveSyncPlugin.Core.Storage;
using SaveSyncPlugin.Core.Sync;
using Unbroken.LaunchBox.Plugins;
using Unbroken.LaunchBox.Plugins.Data;

namespace SaveSyncPlugin.UI.Forms
{
    public partial class SaveSyncEmulatorsForm : Form
    {
        private List<EmulatorSaveConfig> _configs;

        public SaveSyncEmulatorsForm(List<EmulatorSaveConfig> configs)
        {
            _configs = configs ?? new List<EmulatorSaveConfig>();
            InitializeComponent();
            LoadEmulatorList();
        }

        private class EmulatorListItem
        {
            public IEmulator Emulator { get; }
            public EmulatorListItem(IEmulator emulator) => Emulator = emulator;
            public override string ToString() => Emulator.Title ?? "(untitled)";
        }

        private void LstEmulators_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectEmulator();
        }

        private void BtnSaveAll_Click(object sender, EventArgs e)
        {
            if (!SaveCurrentConfig())
                return;

            if (!TrySaveAll())
                return;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private IEmulator _currentEmulator;
        private EmulatorSaveConfig _selected;

        private void LoadEmulatorList()
        {
            lstEmulators.Items.Clear();
            try
            {
                var emulators = PluginHelper.DataManager.GetAllEmulators();
                if (emulators == null) return;

                foreach (IEmulator emu in emulators.OrderBy(e => e.Title ?? ""))
                    lstEmulators.Items.Add(new EmulatorListItem(emu));
            }
            catch (Exception ex)
            {
                lstEmulators.Items.Add("(error: " + ex.Message + ")");
            }
        }

        private void SelectEmulator()
        {
            // Never persist an invalid value, but do not block switching either:
            // the old (valid) config stays untouched until the user fixes the field.
            SaveCurrentConfig(showErrors: false);

            var item = lstEmulators.SelectedItem as EmulatorListItem;
            _currentEmulator = item?.Emulator;
            _selected = null;
            txtAppPath.Text = "";
            txtLocalBase.Text = "";
            txtFolderName.Text = "";
            txtPaths.Text = "";

            if (_currentEmulator == null)
            {
                UpdateLocalBaseHint();
                return;
            }

            txtAppPath.Text = _currentEmulator.ApplicationPath ?? "";

            _selected = _configs.Find(c => c.EmulatorId == _currentEmulator.Id);
            if (_selected != null)
            {
                _selected.Title = _currentEmulator.Title ?? _selected.Title;
                txtLocalBase.Text = _selected.LocalBase ?? "";
                txtFolderName.Text = _selected.FolderName ?? "";
                txtPaths.Text = string.Join("\r\n", _selected.RelativePaths ?? new List<string>());
            }
            else
            {
                var defaults = SaveSyncStorage.LoadEmulatorDefaults();
                var def = defaults.Find(d =>
                    !string.IsNullOrEmpty(d.Title) &&
                    _currentEmulator.Title != null &&
                    _currentEmulator.Title.IndexOf(d.Title, StringComparison.OrdinalIgnoreCase) >= 0);
                if (def != null)
                {
                    txtFolderName.Text = def.FolderName ?? "";
                    txtPaths.Text = string.Join("\r\n", def.RelativePaths ?? new List<string>());
                }
                else
                {
                    txtFolderName.Text = _currentEmulator.Title ?? "";
                }
            }

            UpdateLocalBaseHint();
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (_currentEmulator == null) return;

            if (!SaveCurrentConfig())
                return;

            if (!TrySaveAll())
                return;

            MessageBox.Show($"Config saved for '{_currentEmulator.Title}'.",
                "SaveSync", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private bool TrySaveAll()
        {
            try
            {
                SaveSyncStorage.SaveEmulatorConfigs(_configs);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "SaveSync", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
        }

        // ---------------- local base helpers ----------------

        private void TxtLocalBase_TextChanged(object sender, EventArgs e)
        {
            UpdateLocalBaseHint();
        }

        private void BtnLocalDefault_Click(object sender, EventArgs e)
        {
            // Empty means "the folder of the emulator application", which is the
            // portable default and works on any machine.
            txtLocalBase.Text = string.Empty;
        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            var root = PluginPaths.LaunchBoxRoot;
            if (string.IsNullOrWhiteSpace(root))
            {
                MessageBox.Show(
                    "LaunchBox folder not found; the folder picker is unavailable. " +
                    "Type the path relative to the LaunchBox folder instead.",
                    "SaveSync", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description =
                    "Pick the emulator folder. It is stored relative to the LaunchBox folder, " +
                    "so the setup keeps working when the tree is moved to another machine.";
                dialog.ShowNewFolderButton = false;

                var current = PathResolver.ResolveLocalBase(
                    root, _currentEmulator?.ApplicationPath, txtLocalBase.Text.Trim());

                dialog.SelectedPath = !string.IsNullOrWhiteSpace(current) && Directory.Exists(current)
                    ? current
                    : root;

                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                string relative;
                try
                {
                    relative = PathResolver.Relativize(root, dialog.SelectedPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "SaveSync", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (relative == null)
                {
                    MessageBox.Show(
                        $"The folder must be inside the LaunchBox folder:\r\n{root}\r\n\r\n" +
                        "Absolute paths are not stored: they break the setup on another machine.",
                        "SaveSync", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (PathResolver.IsInsideHostFolders(dialog.SelectedPath, root))
                {
                    MessageBox.Show(
                        "The folder is inside the LaunchBox program folders (Core/Plugins). " +
                        "Pick the emulator folder instead.",
                        "SaveSync", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                txtLocalBase.Text = relative;
            }
        }

        private void UpdateLocalBaseHint()
        {
            if (lblLocalBaseHint == null) return;

            var root = PluginPaths.LaunchBoxRoot;
            var appPath = _currentEmulator?.ApplicationPath;
            var localBase = txtLocalBase.Text.Trim();

            var error = SaveSyncStorage.ValidateLocalBase(localBase);
            if (error != null)
            {
                SetHint(error, warning: true);
                return;
            }

            var resolved = PathResolver.ResolveLocalBase(root, appPath, localBase);
            if (resolved == null)
            {
                SetHint(
                    string.IsNullOrWhiteSpace(root)
                        ? "LaunchBox folder not found, so the local folder cannot be resolved."
                        : "Could not resolve the local folder (check the emulator application path).",
                    warning: true);
                return;
            }

            if (string.IsNullOrWhiteSpace(localBase))
            {
                if (PathResolver.UsesWrapper(appPath))
                {
                    SetHint(
                        "empty = folder of the emulator application. This emulator runs through a wrapper, " +
                        "so set the real emulator folder here, otherwise the sync is skipped.",
                        warning: true);
                }
                else
                {
                    SetHint("empty = folder of the emulator application (default): " + resolved,
                        warning: false);
                }

                return;
            }

            SetHint("resolves to: " + resolved, warning: false);
        }

        private void SetHint(string text, bool warning)
        {
            lblLocalBaseHint.Text = text;
            lblLocalBaseHint.ForeColor = warning
                ? System.Drawing.Color.Firebrick
                : System.Drawing.Color.Gray;
        }

        // ---------------- persistence ----------------

        /// <summary>
        /// Reads the form into the in-memory config. Returns <c>false</c> when the
        /// values are not portable (absolute path), in which case nothing is written.
        /// </summary>
        private bool SaveCurrentConfig(bool showErrors = true)
        {
            if (_currentEmulator == null) return true;

            var folderName = txtFolderName.Text.Trim();
            var localBase = txtLocalBase.Text.Trim();
            var paths = txtPaths.Text
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .Where(p => p.Length > 0)
                .ToList();

            var localBaseError = SaveSyncStorage.ValidateLocalBase(localBase);
            if (localBaseError != null)
            {
                if (showErrors)
                    MessageBox.Show(localBaseError, "SaveSync", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            var pathsError = SaveSyncStorage.ValidateRelativePaths(paths);
            if (pathsError != null)
            {
                if (showErrors)
                    MessageBox.Show(pathsError, "SaveSync", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(folderName) && paths.Count == 0)
            {
                _configs.RemoveAll(c => c.EmulatorId == _currentEmulator.Id);
                return true;
            }

            var existing = _configs.Find(c => c.EmulatorId == _currentEmulator.Id);
            if (existing == null)
            {
                existing = new EmulatorSaveConfig
                {
                    EmulatorId = _currentEmulator.Id,
                    Title = _currentEmulator.Title ?? folderName
                };
                _configs.Add(existing);
            }

            existing.Title = _currentEmulator.Title ?? folderName;
            existing.FolderName = string.IsNullOrWhiteSpace(folderName) ? existing.Title : folderName;
            existing.LocalBase = string.IsNullOrWhiteSpace(localBase) ? null : localBase;
            existing.RelativePaths = paths;

            return true;
        }
    }
}
