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

namespace BanPrograms
{
    public partial class MainForm : Form
    {
        private ProgramListManager manager = new ProgramListManager();
        private MonitorService monitor = new MonitorService();

        public MainForm()
        {
            InitializeComponent(); // Этот метод сгенерирован дизайнером
            CheckAdminRights();
            LoadProgramList();
        }

        private void CheckAdminRights()
        {
            if (!manager.IsAdmin())
            {
                MessageBox.Show("Требуются права администратора!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            btnToggle.Text = list.Enabled ? "Enable" : "Disable";
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
                        Name = Path.GetFileName(ofd.FileName),
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

        private void btnToggle_Click(object sender, EventArgs e)
        {
            var list = manager.LoadList();
            list.Enabled = !list.Enabled;
            manager.SaveList(list);
            monitor.UpdateCache();
            if (list.Enabled) monitor.Start(); else monitor.Stop();
            LoadProgramList();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }
    }
}