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
            logger.Log($"Process started: {processName} (ID: {processId})");

            if (!cachedList.Enabled)
            {
                logger.Log($"System is disabled, skipping: {processName} (ID: {processId})");
                return;
            }

            foreach (var banned in cachedList.Programs)
            {
                if (processName.Equals(banned.Name, StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        Process process = Process.GetProcessById(int.Parse(processId));
                        if (!process.HasExited) // Проверяем, не завершился ли процесс
                        {
                            process.Kill();
                            string path = process.MainModule.FileName;
                            string hash = manager.CalculateHash(path);
                            string user = process.StartInfo.UserName;
                            string reason = banned.Hash == hash ? "Hash match" : "Name match";
                            MessageBox.Show($"The launch of {processName} is prohibited!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            logger.Log($"Blocked: {processName} (ID: {processId}, Path: {path}, Hash: {hash}, User: {user}, Reason: {reason})");
                        }
                        else
                        {
                            logger.Log($"Process {processName} (ID: {processId}) already exited before termination.");
                        }
                    }
                    catch (Exception ex)
                    {
                        // Логируем ошибку, но не показываем MessageBox
                        logger.Log($"Failed to terminate {processName} (ID: {processId}): {ex.Message}");
                    }
                    break;
                }
            }
        }
    }
}