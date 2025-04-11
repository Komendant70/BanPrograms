using System;
using System.Management;
using System.Diagnostics;
using System.Windows.Forms;
using System.Security.Principal;
using System.Threading.Tasks;

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
                        return $"{owner[1]}\\{owner[0]}"; 
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

            try
            {
                Process process = Process.GetProcessById(int.Parse(processId));
                if (process.HasExited)
                {
                    logger.Log($"Process {processName} (ID: {processId}) already exited.");
                    return;
                }

                string filePath = string.Empty;
                string currentHash = string.Empty;
                try
                {
                    filePath = process.MainModule.FileName;
                    currentHash = manager.CalculateHash(filePath);
                    logger.Log($"Process info: Path={filePath}, Hash={currentHash}");
                }
                catch (Exception ex)
                {
                    logger.Log($"Failed to get MainModule or hash for {processName} (ID: {processId}): {ex.Message}");
                }

                logger.Log($"Checking against {cachedList.Programs.Count} banned programs");
                foreach (var banned in cachedList.Programs)
                {
                    bool isNameMatch = processName.Equals(banned.Name, StringComparison.OrdinalIgnoreCase);
                    bool isPathMatch = !string.IsNullOrEmpty(filePath) && filePath.Equals(banned.Path, StringComparison.OrdinalIgnoreCase);
                    bool isHashMatch = !string.IsNullOrEmpty(currentHash) && currentHash.Equals(banned.Hash, StringComparison.OrdinalIgnoreCase);

                    logger.Log($"Comparing with banned: Name={banned.Name}, Path={banned.Path}, Hash={banned.Hash}, Matches: Name={isNameMatch}, Path={isPathMatch}, Hash={isHashMatch}");

                    if (isNameMatch || isPathMatch || isHashMatch)
                    {
                        try
                        {
                            if (!process.HasExited)
                            {
                                process.Kill();
                                logger.Log($"Blocked: {processName} (ID: {processId}, Path={filePath}, Hash={currentHash}, Reason={(isNameMatch ? "Name" : isPathMatch ? "Path" : "Hash")})");

                                Task.Run(() =>
                                {
                                    try
                                    {
                                        Application.OpenForms[0]?.Invoke((Action)(() =>
                                        {
                                            MessageBox.Show($"The launch of {processName} is prohibited!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                        }));
                                    }
                                    catch (Exception ex)
                                    {
                                        logger.Log($"Error showing MessageBox for {processName}: {ex.Message}");
                                    }
                                });
                            }
                            else
                            {
                                logger.Log($"Process {processName} (ID: {processId}) already exited before termination.");
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Log($"Error terminating {processName} (ID: {processId}): {ex.Message}");
                        }
                        return;
                    }
                }
                logger.Log($"No match found for {processName}");
            }
            catch (Exception ex)
            {
                logger.Log($"Error processing {processName} (ID: {processId}): {ex.Message}");
            }
        }
        public void CheckAndTerminateRunningProcesses()
        {
            if (!cachedList.Enabled)
            {
                logger.Log("System is disabled, skipping check for running processes.");
                return;
            }

            logger.Log("Checking for already running banned processes...");

            Process[] runningProcesses = Process.GetProcesses();
            foreach (var process in runningProcesses)
            {
                try
                {
                    if (process.Id == Process.GetCurrentProcess().Id || string.IsNullOrEmpty(process.ProcessName))
                    {
                        continue;
                    }

                    string processName = process.ProcessName + ".exe";
                    string filePath = string.Empty;
                    string currentHash = string.Empty;

                    try
                    {
                        filePath = process.MainModule.FileName;
                        currentHash = manager.CalculateHash(filePath);
                        logger.Log($"Running process: {processName} (ID: {process.Id}), Path={filePath}, Hash={currentHash}");
                    }
                    catch (Exception ex)
                    {
                        logger.Log($"Failed to get MainModule for running process {processName} (ID: {process.Id}): {ex.Message}");

                        try
                        {
                            using (var searcher = new ManagementObjectSearcher($"SELECT ExecutablePath FROM Win32_Process WHERE ProcessId = {process.Id}"))
                            {
                                foreach (ManagementObject obj in searcher.Get())
                                {
                                    filePath = obj["ExecutablePath"]?.ToString();
                                    if (!string.IsNullOrEmpty(filePath))
                                    {
                                        currentHash = manager.CalculateHash(filePath);
                                        logger.Log($"Running process (via WMI): {processName} (ID: {process.Id}), Path={filePath}, Hash={currentHash}");
                                    }
                                    break;
                                }
                            }
                        }
                        catch (Exception wmiEx)
                        {
                            logger.Log($"Failed to get path via WMI for running process {processName} (ID: {process.Id}): {wmiEx.Message}");
                        }
                    }


                    foreach (var banned in cachedList.Programs)
                    {
                        bool isNameMatch = processName.Equals(banned.Name, StringComparison.OrdinalIgnoreCase);
                        bool isPathMatch = !string.IsNullOrEmpty(filePath) && filePath.Equals(banned.Path, StringComparison.OrdinalIgnoreCase);
                        bool isHashMatch = !string.IsNullOrEmpty(currentHash) && currentHash.Equals(banned.Hash, StringComparison.OrdinalIgnoreCase);

                        if (isNameMatch || isPathMatch || isHashMatch)
                        {
                            try
                            {
                                if (!process.HasExited)
                                {
                                    process.Kill();
                                    logger.Log($"Terminated running process: {processName} (ID: {process.Id}, Path={filePath}, Hash={currentHash}, Reason={(isNameMatch ? "Name" : isPathMatch ? "Path" : "Hash")})");


                                    Task.Run(() =>
                                    {
                                        try
                                        {
                                            Application.OpenForms[0]?.Invoke((Action)(() =>
                                            {
                                                MessageBox.Show($"The running process {processName} was terminated as it is prohibited!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                            }));
                                        }
                                        catch (Exception ex)
                                        {
                                            logger.Log($"Error showing MessageBox for {processName}: {ex.Message}");
                                        }
                                    });
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.Log($"Error terminating running process {processName} (ID: {process.Id}): {ex.Message}");
                            }
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Log($"Error checking running process {process.ProcessName} (ID: {process.Id}): {ex.Message}");
                }
            }

            logger.Log("Finished checking running processes.");
        }

    }
}