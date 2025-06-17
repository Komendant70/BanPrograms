using System;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;
using System.IO;
using System.Reflection;
using System.Drawing;
using Microsoft.Win32;
using System.Security.Principal;
using Microsoft.Win32.TaskScheduler;
using System.Linq;
using System.Diagnostics;

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
        private bool isAdmin;
        private readonly Logger _logger;

        public MainForm()
        {
            // Вызываем InitializeComponent до любых проверок
            InitializeComponent();

            // Инициализация MaterialSkin
            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            materialSkinManager.ColorScheme = new ColorScheme(Primary.BlueGrey800, Primary.BlueGrey900, Primary.BlueGrey500, Accent.LightBlue200, TextShade.WHITE);
            logger.Log("MaterialSkin initialized successfully.");
            _logger = new Logger();

            // Настройки формы
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

            // Сохраняем исходные параметры окна
            originalFormBorderStyle = this.FormBorderStyle;
            originalWindowState = this.WindowState;
            originalSize = this.Size;
            originalLocation = this.Location;

            // Включаем обработку клавиш
            this.KeyPreview = true;
            this.KeyDown += new KeyEventHandler(MainForm_KeyDown);

            // Подписываемся на событие изменения размера формы
            this.Resize += new EventHandler(MainForm_Resize);

            // Проверка пути к программе
            currentProgramPath = Assembly.GetExecutingAssembly().Location;

            // Проверка прав администратора
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            logger.Log($"User admin status: {isAdmin}");

            // Ограничиваем доступ для не-администраторов
            if (!isAdmin)
            {
                // Отключаем элементы управления
                btnAdd.Enabled = false;
                btnRemove.Enabled = false;
                btnToggle.Enabled = false; // Отключаем переключатель для неадминов
                lstPrograms.Enabled = true; // Только для чтения
                lstPrograms.Columns[0].Text = "Program (Read-Only)";

                // Отключаем переключатель автозагрузки (используем MaterialSwitch)
                if (this.Controls.OfType<MaterialSwitch>().Any(b => b.Name == "btnEnableStartup"))
                {
                    this.Controls.OfType<MaterialSwitch>().First(b => b.Name == "btnEnableStartup").Enabled = false;
                }

                // Отключаем кнопку открытия лог-файла
                if (this.Controls.OfType<MaterialButton>().Any(b => b.Name == "materialButton1"))
                {
                    this.Controls.OfType<MaterialButton>().First(b => b.Name == "materialButton1").Enabled = false;
                }

                // Показываем сообщение
                MessageBox.Show("You are not an administrator. You can only view the list of banned programs.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                // Инициализируем состояние переключателя автозагрузки
                bool isStartupEnabled = IsStartupEnabled();
                btnEnableStartup.Checked = isStartupEnabled;
                materialLabel1.Text = isStartupEnabled ? "Startup: On" : "Startup: Off";
                logger.Log($"Initial startup state: {materialLabel1.Text}");
            }

            // Инициализация после проверки прав
            CheckAdminRights();
            LoadProgramList();
        }

        private bool IsStartupEnabled()
        {
            try
            {
                string appName = "BanPrograms";
                using (TaskService ts = new TaskService())
                {
                    return ts.GetTask(appName) != null;
                }
            }
            catch (Exception ex)
            {
                logger.Log($"Error checking startup state: {ex.Message}");
                return false;
            }
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
            btnToggle.Font = new System.Drawing.Font("Segoe UI", newFontSize, System.Drawing.FontStyle.Regular);
            materialButton1.Font = new System.Drawing.Font("Segoe UI", newFontSize, System.Drawing.FontStyle.Regular);

            btnAdd.Size = new Size((int)(100 * scale), (int)(40 * scale));
            btnRemove.Size = new Size((int)(100 * scale), (int)(40 * scale));
            btnToggle.Size = new Size((int)(40 * scale), (int)(40 * scale));
            lblToggle.Size = new Size((int)(120 * scale), (int)(40 * scale));
            materialLabel1.Size = new Size((int)(120 * scale), (int)(40 * scale));
            materialButton1.Size = new Size((int)(100 * scale), (int)(40 * scale));

            btnRemove.Location = new Point(20, this.ClientSize.Height - btnRemove.Height - 20);
            btnAdd.Location = new Point((this.ClientSize.Width - btnAdd.Width) / 2, this.ClientSize.Height - btnAdd.Height - 20);
            materialButton1.Location = new Point((this.ClientSize.Width - materialButton1.Width) - 20, this.ClientSize.Height - materialButton1.Height - 20);
            btnToggle.Location = new Point((int)(this.ClientSize.Width - btnToggle.Width - 20 * scale), (int)(this.ClientSize.Height - btnToggle.Height - 60 * scale));
            lblToggle.Location = new Point((int)(this.ClientSize.Width - lblToggle.Width - btnToggle.Width - 40 * scale), (int)(this.ClientSize.Height - lblToggle.Height - 60 * scale));

            if (lstPrograms.Columns.Count > 0)
            {
                lstPrograms.Columns[0].Width = (int)(450 * scale);
            }

            System.Diagnostics.Debug.WriteLine($"btnRemove Location: {btnRemove.Location}");
            System.Diagnostics.Debug.WriteLine($"btnAdd Location: {btnAdd.Location}");
            System.Diagnostics.Debug.WriteLine($"materialButton1 Location: {materialButton1.Location}");
            System.Diagnostics.Debug.WriteLine($"btnToggle Location: {btnToggle.Location}");
            System.Diagnostics.Debug.WriteLine($"lblToggle Location: {lblToggle.Location}");
            System.Diagnostics.Debug.WriteLine($"materialLabel1 Location: {materialLabel1.Location}");
            System.Diagnostics.Debug.WriteLine($"ClientSize: {this.ClientSize}");

            this.Refresh();
        }

        private void CheckAdminRights()
        {
            logger.Log("Checking admin rights in CheckAdminRights method.");
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
            // Убираем проверку isAdmin здесь, так как переключатель уже отключён для неадминов
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
            if (!isAdmin)
            {
                logger.Log("Non-admin user attempted to add a program.");
                return;
            }

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

        private void btnEnableStartup_Click(object sender, EventArgs e)
        {
            if (!isAdmin)
            {
                logger.Log("Non-admin user attempted to modify startup settings.");
                return;
            }

            try
            {
                string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string appName = "BanPrograms";

                // Проверяем текущее состояние автозагрузки
                bool isCurrentlyEnabled = IsStartupEnabled();

                // Если переключатель включён, но автозагрузка не настроена, добавляем задачу
                if (btnEnableStartup.Checked && !isCurrentlyEnabled)
                {
                    using (TaskService ts = new TaskService())
                    {
                        TaskDefinition td = ts.NewTask();
                        td.RegistrationInfo.Description = "Launches ProcessLock in background mode on system startup for all users.";
                        td.Principal.RunLevel = TaskRunLevel.Highest;
                        td.Principal.LogonType = TaskLogonType.ServiceAccount;
                        td.Principal.UserId = "SYSTEM";

                        BootTrigger bootTrigger = new BootTrigger();
                        td.Triggers.Add(bootTrigger);

                        td.Actions.Add(new ExecAction(exePath, "--background", null));

                        td.Settings.Enabled = true;
                        td.Settings.DisallowStartIfOnBatteries = false;
                        td.Settings.StopIfGoingOnBatteries = false;
                        td.Settings.RunOnlyIfNetworkAvailable = false;

                        ts.RootFolder.RegisterTaskDefinition(appName, td);
                        logger.Log("Program added to startup via Task Scheduler.");
                        MessageBox.Show("Program added to startup for all users.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    materialLabel1.Text = "Startup: On";
                }
                // Если переключатель выключён, но автозагрузка настроена, удаляем задачу
                else if (!btnEnableStartup.Checked && isCurrentlyEnabled)
                {
                    using (TaskService ts = new TaskService())
                    {
                        if (ts.GetTask(appName) != null)
                        {
                            ts.RootFolder.DeleteTask(appName);
                            logger.Log("Program removed from startup via Task Scheduler.");
                            MessageBox.Show("Program removed from startup.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    materialLabel1.Text = "Startup: Off";
                }
            }
            catch (Exception ex)
            {
                logger.Log($"Failed to modify startup settings: {ex.Message}");
                MessageBox.Show($"Failed to modify startup settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Восстанавливаем состояние переключателя
                btnEnableStartup.Checked = IsStartupEnabled();
                materialLabel1.Text = btnEnableStartup.Checked ? "Startup: On" : "Startup: Off";
            }
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (!isAdmin)
            {
                logger.Log("Non-admin user attempted to remove a program.");
                return;
            }

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
            if (!isAdmin)
            {
                logger.Log("Non-admin user attempted to toggle block system.");
                btnToggle.Checked = manager.LoadList().Enabled; // Сбрасываем состояние
                return;
            }

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
                    monitor.Stop();
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

        private void materialButton1_Click(object sender, EventArgs e)
        {
            if (!isAdmin)
            {
                logger.Log("Non-admin user attempted to open a log file.");
                return;
            }
            string logFilePath = _logger.LogFile; // Получаем путь к лог-файлу

            if (File.Exists(logFilePath))
            {
                // Открываем файл в программе по умолчанию (например, Блокнот)
                Process.Start(new ProcessStartInfo
                {
                    FileName = logFilePath,
                    UseShellExecute = true
                });
            }
            else
            {
                MessageBox.Show("Лог-файл не найден!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}