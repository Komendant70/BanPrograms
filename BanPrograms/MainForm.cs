using System;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;
using System.IO;
using System.Reflection;
using System.Drawing;

namespace BanPrograms
{
    public partial class MainForm : MaterialForm
    {
        private Logger logger = new Logger();
        private ProgramListManager manager = new ProgramListManager();
        private MonitorService monitor = new MonitorService();
        private string currentProgramPath;
        private bool isFullScreen = false;
        private FormBorderStyle originalFormBorderStyle; 
        private FormWindowState originalWindowState; 
        private Size originalSize; 
        private Point originalLocation; 

        public MainForm()
        {
            currentProgramPath = Assembly.GetExecutingAssembly().Location;
            InitializeComponent();
            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            materialSkinManager.ColorScheme = new ColorScheme(Primary.BlueGrey800, Primary.BlueGrey900, Primary.BlueGrey500, Accent.LightBlue200, TextShade.WHITE);
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

            originalFormBorderStyle = this.FormBorderStyle;
            originalWindowState = this.WindowState;
            originalSize = this.Size;
            originalLocation = this.Location;

      
            this.KeyPreview = true;
            this.KeyDown += new KeyEventHandler(MainForm_KeyDown);
            this.Resize += new EventHandler(MainForm_Resize);

            CheckAdminRights();
            LoadProgramList();
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F11)
            {
                ToggleFullScreen();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape && isFullScreen) 
            {
                ToggleFullScreen();
                e.Handled = true;
            }
        }

        private void ToggleFullScreen()
        {
            if (!isFullScreen)
            {
                
                originalFormBorderStyle = this.FormBorderStyle;
                originalWindowState = this.WindowState;
                originalSize = this.Size;
                originalLocation = this.Location;

                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Maximized;
                isFullScreen = true;
            }
            else
            {
              
                this.FormBorderStyle = originalFormBorderStyle;
                this.WindowState = originalWindowState;
                this.Size = originalSize;
                this.Location = originalLocation;
                isFullScreen = false;
            }

     
            ScaleControls();
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
   
            ScaleControls();
        }

        private void ScaleControls()
        {
   
            float scaleX = (float)this.ClientSize.Width / 500f;
            float scaleY = (float)this.ClientSize.Height / 400f;
            float scale = Math.Min(scaleX, scaleY); 

            float newFontSize = 9f * scale; 
            newFontSize = Math.Max(8f, Math.Min(newFontSize, 20f)); 
            this.Font = new System.Drawing.Font("Segoe UI", newFontSize, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            btnAdd.Font = new System.Drawing.Font("Segoe UI", newFontSize, System.Drawing.FontStyle.Regular);
            btnRemove.Font = new System.Drawing.Font("Segoe UI", newFontSize, System.Drawing.FontStyle.Regular);
            lblToggle.Font = new System.Drawing.Font("Segoe UI", newFontSize, System.Drawing.FontStyle.Regular);


            btnAdd.Size = new Size((int)(100 * scale), (int)(40 * scale));
            btnRemove.Size = new Size((int)(100 * scale), (int)(40 * scale));
            btnToggle.Size = new Size((int)(40 * scale), (int)(40 * scale));
            lblToggle.Size = new Size((int)(120 * scale), (int)(40 * scale));

            btnRemove.Location = new Point(20, this.ClientSize.Height - btnRemove.Height - 20); 
            btnAdd.Location = new Point((this.ClientSize.Width - btnAdd.Width) / 2, this.ClientSize.Height - btnAdd.Height - 20); 
            lblToggle.Location = new Point(this.ClientSize.Width - lblToggle.Width - btnToggle.Width - 40, this.ClientSize.Height - lblToggle.Height - 20); 
            btnToggle.Location = new Point(this.ClientSize.Width - btnToggle.Width - 20, this.ClientSize.Height - btnToggle.Height - 20); 

            if (lstPrograms.Columns.Count > 0)
            {
                lstPrograms.Columns[0].Width = (int)(450 * scale);
            }

            System.Diagnostics.Debug.WriteLine($"btnRemove Location: {btnRemove.Location}");
            System.Diagnostics.Debug.WriteLine($"btnAdd Location: {btnAdd.Location}");
            System.Diagnostics.Debug.WriteLine($"lblToggle Location: {lblToggle.Location}");
            System.Diagnostics.Debug.WriteLine($"btnToggle Location: {btnToggle.Location}");
            System.Diagnostics.Debug.WriteLine($"ClientSize: {this.ClientSize}");

            this.Refresh();
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
            System.Diagnostics.Debug.WriteLine($"Loaded {list.Programs.Count} programs from ProgramListManager.");

            list.Programs.RemoveAll(p => p.Path.Equals(currentProgramPath, StringComparison.OrdinalIgnoreCase));
            manager.SaveList(list);

            lstPrograms.Items.Clear();
            foreach (var program in list.Programs)
            {
                System.Diagnostics.Debug.WriteLine($"Adding program: {program.Name} ({program.Path})");
                lstPrograms.Items.Add(new ListViewItem($"{program.Name} ({program.Path})"));
            }

            btnToggle.Checked = list.Enabled;
            lblToggle.Text = btnToggle.Checked ? "Block System: ON" : "Block System: OFF";
            btnToggle.Enabled = list.Programs.Count > 0;
            if (list.Enabled && list.Programs.Count == 0)
            {
                list.Enabled = false;
                manager.SaveList(list);
                monitor.UpdateCache();
                monitor.Stop();
                btnToggle.Checked = false;
                lblToggle.Text = "Block System: OFF";
                System.Diagnostics.Debug.WriteLine("System disabled due to empty program list.");
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    if (ofd.FileName.Equals(currentProgramPath, StringComparison.OrdinalIgnoreCase))
                    {
                        MessageBox.Show("You cannot add this program to the banned list!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

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
                    logger.Log($"Added program: {ofd.FileName}, Hash: {hash}");

                    monitor.CheckAndTerminateRunningProcesses();

                    LoadProgramList();
                }
            }
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (lstPrograms.SelectedIndices.Count > 0)
            {
                var list = manager.LoadList();
                list.Programs.RemoveAt(lstPrograms.SelectedIndices[0]);
                manager.SaveList(list);
                monitor.UpdateCache();
                LoadProgramList();

                if (list.Programs.Count == 0 && btnToggle.Checked)
                {
                    btnToggle.Checked = false;
                }
            }
        }

        private void btnToggle_CheckedChanged(object sender, EventArgs e)
        {
            var list = manager.LoadList();
            list.Enabled = btnToggle.Checked;
            manager.SaveList(list);
            monitor.UpdateCache();

            lblToggle.Text = btnToggle.Checked ? "Block System: ON" : "Block System: OFF";

            if (list.Enabled)
            {
                if (list.Programs.Count == 0)
                {
                    MessageBox.Show("No programs in the banned list. Add programs to block.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    btnToggle.Checked = false;
                    lblToggle.Text = "Block System: OFF";
                }

                else
                {
                    System.Diagnostics.Debug.WriteLine("System enabled, starting monitor...");
                    monitor.Start();
                    monitor.CheckAndTerminateRunningProcesses();
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