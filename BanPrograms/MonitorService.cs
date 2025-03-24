using System;
using System.Management;
using System.Diagnostics;
using System.Windows.Forms;
using System.Security.Principal;

namespace BanPrograms
{
    public class MonitorService
    {
        private ProgramListManager manager = new ProgramListManager();
        private Logger logger = new Logger();
        private ManagementEventWatcher watcher;
        private ProgramList cachedList;

        public MonitorService()
        {
            cachedList = manager.LoadList();
        }

        public void Start()
        {
            if (!cachedList.Enabled)
            {
                logger.Log($"System is disabled on startup. Enabled: {cachedList.Enabled}, Banned Programs Count: {cachedList.Programs.Count}");
                return;
            }

            WqlEventQuery query = new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace");
            watcher = new ManagementEventWatcher(query);
            watcher.EventArrived += ProcessStarted;
            watcher.Start();
            logger.Log($"MonitorService started. Enabled: {cachedList.Enabled}, Banned Programs Count: {cachedList.Programs.Count}");
        }

        public void Stop()
        {
            watcher?.Stop();
            logger.Log($"MonitorService stopped. Enabled: {cachedList.Enabled}, Banned Programs Count: {cachedList.Programs.Count}");
        }

        public void UpdateCache()
        {
            cachedList = manager.LoadList();
            logger.Log($"Cache updated. Enabled: {cachedList.Enabled}, Banned Programs Count: {cachedList.Programs.Count}");
        }

        private string GetProcessUser(int processId)
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher($"SELECT * FROM Win32_Process WHERE ProcessId = {processId}"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string[] owner = new string[2];
                        obj.InvokeMethod("GetOwner", owner);
                        return $"{owner[1]}\\{owner[0]}"; // Формат: Domain\User
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log($"Failed to get user for process ID {processId}: {ex.Message}");
            }
            return "Unknown";
        }

        private void ProcessStarted(object sender, EventArrivedEventArgs e)
        {
            string processName = e.NewEvent.Properties["ProcessName"].Value.ToString();
            string processId = e.NewEvent.Properties["ProcessID"].Value.ToString();

            if (!cachedList.Enabled)
            {
                logger.Log($"System is disabled, skipping: {processName} (ID: {processId})");
                return;
            }

            try
            {
                bool isBanned = false;
                string filePath = null;
                string hash = null;
                string reason = null;
                string user = GetProcessUser(int.Parse(processId));

                // Проверяем по имени процесса
                foreach (var program in cachedList.Programs)
                {
                    if (program.Name.Equals(processName, StringComparison.OrdinalIgnoreCase))
                    {
                        isBanned = true;
                        reason = "Name match";
                        break;
                    }
                }

                // Если процесс помечен как запрещённый по имени
                if (isBanned)
                {
                    try
                    {
                        Process process = Process.GetProcessById(int.Parse(processId));
                        filePath = process.MainModule.FileName;
                        hash = manager.CalculateHash(filePath);

                        foreach (var program in cachedList.Programs)
                        {
                            if (program.Path == filePath)
                            {
                                reason = "Path match";
                                process.Kill();
                                logger.Log($"Blocked: {processName} (ID: {processId}, Path: {filePath}, Hash: {hash}, User: {user}, Reason: {reason})");
                                MessageBox.Show(
                                    $"Запуск программы {processName} запрещён!\nПуть: {filePath}",
                                    "Запрещено",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error
                                );
                                return;
                            }
                            if (program.Hash == hash)
                            {
                                reason = "Hash match";
                                process.Kill();
                                logger.Log($"Blocked: {processName} (ID: {processId}, Path: {filePath}, Hash: {hash}, User: {user}, Reason: {reason})");
                                MessageBox.Show(
                                    $"Запуск программы {processName} запрещён!\nПуть: {filePath}",
                                    "Запрещено",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error
                                );
                                return;
                            }
                        }
                    }
                    catch (ArgumentException)
                    {
                        logger.Log($"Blocked: {processName} (ID: {processId}, User: {user}, Reason: {reason}) - Process terminated before inspection.");
                        MessageBox.Show(
                            $"Попытка запуска программы {processName} была заблокирована, но процесс завершился слишком быстро.",
                            "Предупреждение",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                        );
                        return;
                    }
                }

                // Проверяем по пути и хэшу
                try
                {
                    Process process = Process.GetProcessById(int.Parse(processId));
                    filePath = process.MainModule.FileName;
                    hash = manager.CalculateHash(filePath);

                    foreach (var program in cachedList.Programs)
                    {
                        if (program.Path == filePath)
                        {
                            reason = "Path match";
                            process.Kill();
                            logger.Log($"Blocked: {processName} (ID: {processId}, Path: {filePath}, Hash: {hash}, User: {user}, Reason: {reason})");
                            MessageBox.Show(
                                $"Запуск программы {processName} запрещён!\nПуть: {filePath}",
                                "Запрещено",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error
                            );
                            return;
                        }
                        if (program.Hash == hash)
                        {
                            reason = "Hash match";
                            process.Kill();
                            logger.Log($"Blocked: {processName} (ID: {processId}, Path: {filePath}, Hash: {hash}, User: {user}, Reason: {reason})");
                            MessageBox.Show(
                                $"Запуск программы {processName} запрещён!\nПуть: {filePath}",
                                "Запрещено",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error
                            );
                            return;
                        }
                    }
                    // Не логируем разрешённые программы
                }
                catch (ArgumentException)
                {
                    // Не логируем, если процесс завершился, но не был заблокирован
                }
            }
            catch (Exception ex)
            {
                logger.Log($"Error blocking {processName} (ID: {processId}): {ex.Message}, StackTrace: {ex.StackTrace}");
            }
        }
    }
}