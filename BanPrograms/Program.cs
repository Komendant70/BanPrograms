using System;
using System.Windows.Forms;
using System.Security.Principal;
using Microsoft.Win32;
using Microsoft.Win32.TaskScheduler;

namespace BanPrograms
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var logger = new Logger();
            logger.Log("Application started.");

            bool isBackgroundMode = args.Length > 0 && args[0] == "--background";
            logger.Log($"Launch mode: {(isBackgroundMode ? "Background" : "Foreground")}");

            RegisterInStartup();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (!isBackgroundMode)
            {
                logger.Log("Starting GUI mode.");
                Application.Run(new MainForm());
            }
            else
            {
                logger.Log("Starting background mode.");
                var monitor = new MonitorService();
                var listManager = new ProgramListManager();
                var list = listManager.LoadList();
                logger.Log($"Background mode: System enabled: {list.Enabled}, Programs count: {list.Programs.Count}");

                try
                {
                    monitor.Start();
                    logger.Log("MonitorService started in background mode.");
                }
                catch (Exception ex)
                {
                    logger.Log($"Failed to start MonitorService in background mode: {ex.Message}");
                }

                System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
            }
        }

        private static void RegisterInStartup()
        {
            try
            {
                string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string appName = "BanPrograms";
                var logger = new Logger();
                logger.Log($"Attempting to register in Task Scheduler. Executable path: {exePath}");

                // Проверяем права администратора
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
                {
                    logger.Log("Cannot register in Task Scheduler: Administrator privileges required.");
                    return;
                }

                // Используем Task Scheduler для создания задачи
                using (TaskService ts = new TaskService())
                {
                    // Проверяем, существует ли уже задача
                    if (ts.GetTask(appName) != null)
                    {
                        logger.Log("Task already exists in Task Scheduler.");
                        return;
                    }

                    // Создаём задачу
                    TaskDefinition td = ts.NewTask();
                    td.RegistrationInfo.Description = "Launches ProcessLock in background mode on system startup for all users.";
                    td.Principal.RunLevel = TaskRunLevel.Highest; // Запуск с правами администратора
                    td.Principal.LogonType = TaskLogonType.ServiceAccount; // Для всех пользователей
                    td.Principal.UserId = "SYSTEM"; // Запуск от имени SYSTEM

                    // Триггер: запуск при старте системы
                    BootTrigger bootTrigger = new BootTrigger();
                    td.Triggers.Add(bootTrigger);

                    // Действие: запуск программы с параметром --background
                    td.Actions.Add(new ExecAction(exePath, "--background", null));

                    // Настройки
                    td.Settings.Enabled = true;
                    td.Settings.DisallowStartIfOnBatteries = false;
                    td.Settings.StopIfGoingOnBatteries = false;
                    td.Settings.RunOnlyIfNetworkAvailable = false;

                    // Регистрируем задачу
                    ts.RootFolder.RegisterTaskDefinition(appName, td);
                    logger.Log("Successfully registered in Task Scheduler.");
                }

                // Удаляем старую запись из реестра
                RemoveOldRegistryEntry();
            }
            catch (Exception ex)
            {
                var logger = new Logger();
                logger.Log($"Failed to register in Task Scheduler: {ex.Message}");
            }
        }

        private static void RemoveOldRegistryEntry()
        {
            try
            {
                string appName = "BanPrograms";
                var logger = new Logger();
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    if (key != null && key.GetValue(appName) != null)
                    {
                        key.DeleteValue(appName);
                        logger.Log("Removed old registry entry for startup.");
                    }
                }
            }
            catch (Exception ex)
            {
                var logger = new Logger();
                logger.Log($"Failed to remove old registry entry: {ex.Message}");
            }
        }
    }
}