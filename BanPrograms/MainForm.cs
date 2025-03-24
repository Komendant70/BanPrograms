using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MetroFramework.Forms;

namespace BanPrograms
{
    public partial class MainForm : MetroForm
    {
        private ProgramListManager manager = new ProgramListManager();
        private MonitorService monitor = new MonitorService();

        public MainForm()
        {
            InitializeComponent();
            CheckAdminRights();
            LoadProgramList();
        }

        private void CheckAdminRights()
        {
            if (!manager.IsAdmin())
            {
                MessageBox.Show("Administrator rights required!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnAdd.Enabled = btnRemove.Enabled = btnToggle.Enabled = false;
            }
        }

        private void LoadProgramList()
        {
            var list = manager.LoadList();
            lstPrograms.Items.Clear();
            foreach (var program in list.Programs)
            {
                lstPrograms.Items.Add($"{program.Name} ({program.Path})");
            }
            btnToggle.Checked = list.Enabled;
            btnToggle.Enabled = list.Programs.Count > 0;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    var list = manager.LoadList();
                    string hash = manager.CalculateHash(ofd.FileName);
                    list.Programs.Add(new ProgramInfo
                    {
                        Name = System.IO.Path.GetFileName(ofd.FileName),
                        Path = ofd.FileName,
                        Hash = hash
                    });
                    manager.SaveList(list);
                    monitor.UpdateCache();
                    LoadProgramList();
                }
            }
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (lstPrograms.SelectedIndex >= 0)
            {
                var list = manager.LoadList();
                list.Programs.RemoveAt(lstPrograms.SelectedIndex);
                manager.SaveList(list);
                monitor.UpdateCache();
                LoadProgramList();
            }
        }

        private void btnToggle_CheckedChanged(object sender, EventArgs e)
        {
            var list = manager.LoadList();
            list.Enabled = btnToggle.Checked;
            manager.SaveList(list);
            monitor.UpdateCache();

            if (list.Enabled)
            {
                if (list.Programs.Count == 0)
                {
                    MessageBox.Show("No programs in the banned list. Add programs to block.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("System enabled, starting monitor...");
                    monitor.Start();
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("System disabled, stopping monitor...");
                monitor.Stop();
            }
            LoadProgramList();
        }

        
    }
}