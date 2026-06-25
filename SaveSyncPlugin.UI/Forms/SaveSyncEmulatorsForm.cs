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
            SaveCurrentConfig();
            SaveSyncStorage.SaveEmulatorConfigs(_configs);
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
            SaveCurrentConfig();

            var item = lstEmulators.SelectedItem as EmulatorListItem;
            _currentEmulator = item?.Emulator;
            _selected = null;
            txtFolderName.Text = "";
            txtPaths.Text = "";

            if (_currentEmulator == null) return;

            _selected = _configs.Find(c => c.EmulatorId == _currentEmulator.Id);
            if (_selected != null)
            {
                _selected.Title = _currentEmulator.Title ?? _selected.Title;
                txtFolderName.Text = _selected.FolderName ?? "";
                txtPaths.Text = string.Join("\r\n", _selected.RelativePaths);
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
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (_currentEmulator == null) return;
            SaveCurrentConfig();
            SaveSyncStorage.SaveEmulatorConfigs(_configs);

            MessageBox.Show($"Config saved for '{_currentEmulator.Title}'.",
                "SaveSync", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void SaveCurrentConfig()
        {
            if (_currentEmulator == null) return;

            var folderName = txtFolderName.Text.Trim();
            var paths = txtPaths.Text
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .Where(p => p.Length > 0)
                .ToList();

            if (string.IsNullOrWhiteSpace(folderName) && paths.Count == 0)
            {
                _configs.RemoveAll(c => c.EmulatorId == _currentEmulator.Id);
                return;
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
            existing.RelativePaths = paths;
        }
    }
}
