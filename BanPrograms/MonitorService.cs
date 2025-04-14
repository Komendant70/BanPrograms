using System;
using System.Management;
using System.Diagnostics;
using System.Windows.Forms;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Timers;

namespace BanPrograms
{
    public class MonitorService
    {
        private ProgramListManager manager = new ProgramListManager();
        private Logger logger = new Logger();
        private ManagementEventWatcher watcher;
        private ProgramList cachedList;
        private System.Timers.Timer pollingTimer;

        public MonitorService()
        {
            cachedList = manager.LoadList();
            logger.Log($"MonitorService initialized. System enabled: {cachedList.Enabled}, Banned programs count: {cachedList.Programs.Count}");
        }

        public void Start()
        {
            try
            {
                if (!cachedList.Enabled)
                {
                    logger.Log($"System is disabled on startup. Enabled: {cachedList.Enabled}, Banned Programs Count: {cachedList.Programs.Count}");
                    return;
                }

                if (watcher != null || pollingTimer != null)
                {
                    logger.Log("MonitorService is already running.");
                    return;
                }

                logger.Log("Starting MonitorService...");
                WqlEventQuery query = new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace");
                watcher = new ManagementEventWatcher(query);
                watcher.EventArrived += ProcessStarted;
                watcher.Start();
                logger.Log($"MonitorService started successfully using WMI. System enabled: {cachedList.Enabled}, Banned programs count: {cachedList.Programs.Count}");

                // Запускаем таймер как резервный механизм
                pollingTimer = new System.Timers.Timer(5000); // Проверяем каждые 5 секунд
                pollingTimer.Elapsed += PollRunningProcesses;
                pollingTimer.AutoReset = true;
                pollingTimer.Start();
                logger.Log("Polling timer started as a fallback mechanism.");
            }
            catch (Exception ex)
            {
                logger.Log($"Failed to start MonitorService using WMI: {ex.Message}");

                // Если WMI не работает, переходим только на опрос
                pollingTimer = new System.Timers.Timer(5000);
                pollingTimer.Elapsed += PollRunningProcesses;
                pollingTimer.AutoReset = true;
                pollingTimer.Start();
                logger.Log("Falling back to polling mechanism due to WMI failure.");
            }
        }

        public void Stop()
        {
            try
            {
                if (watcher != null)
                {
                    watcher.Stop();
                    watcher.Dispose();
                    watcher = null;
                    logger.Log("MonitorService WMI watcher stopped.");
                }

                if (pollingTimer != null)
                {
                    pollingTimer.Stop();
                    pollingTimer.Dispose();
                    pollingTimer = null;
                    logger.Log("Polling timer stopped.");
                }

                logger.Log($"MonitorService stopped. Enabled: {cachedList.Enabled}, Banned Programs Count: {cachedList.Programs.Count}");
            }
            catch (Exception ex)
            {
                logger.Log($"Error stopping MonitorService: {ex.Message}");
            }
        }

        public void UpdateCache()
        {
            cachedList = manager.LoadList();
            logger.Log($"Cache updated. Enabled: {cachedList.Enabled}, Banned Programs Count: {cachedList.Programs.Count}");
        }

        private void ProcessStarted(object sender, EventArrivedEventArgs e)
        {
            string processName = "Unknown";
            string processId = "Unknown";

            try
            {
                processName = e.NewEvent.Properties["ProcessName"].Value.ToString();
                processId = e.NewEvent.Properties["ProcessID"].Value.ToString();
                logger.Log($"Process started: {processName} (ID: {processId})");

                if (!cachedList.Enabled)
                {
                    logger.Log($"System is disabled, skipping: {processName} (ID: {processId})");
                    return;
                }

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
                    logger.Log($"Process info (via MainModule): Path={filePath}, Hash={currentHash}");
                }
                catch (Exception ex)
                {
                    logger.Log($"Failed to get MainModule for {processName} (ID: {processId}): {ex.Message}");
                    try
                    {
                        using (var searcher = new ManagementObjectSearcher($"SELECT ExecutablePath FROM Win32_Process WHERE ProcessId = {processId}"))
                        {
                            foreach (ManagementObject obj in searcher.Get())
                            {
                                filePath = obj["ExecutablePath"]?.ToString();
                                if (!string.IsNullOrEmpty(filePath))
                                {
                                    currentHash = manager.CalculateHash(filePath);
                                    logger.Log($"Process info (via WMI): Path={filePath}, Hash={currentHash}");
                                }
                                break;
                            }
                        }
                    }
                    catch (Exception wmiEx)
                    {
                        logger.Log($"Failed to get path via WMI for {processName} (ID: {processId}): {wmiEx.Message}");
                    }
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

                                if (Application.OpenForms.Count > 0)
                                {
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

        private void PollRunningProcesses(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                if (!cachedList.Enabled)
                {
                    logger.Log("Polling: System is disabled, skipping check.");
                    return;
                }

                logger.Log("Polling running processes...");
                var processes = Process.GetProcesses();
                foreach (var process in processes)
                {
                    string processName = "Unknown";
                    string processId = "Unknown";
                    try
                    {
                        processName = process.ProcessName;
                        processId = process.Id.ToString();
                        logger.Log($"Polling: Checking process {processName} (ID: {processId})");

                        string filePath = string.Empty;
                        string currentHash = string.Empty;

                        try
                        {
                            filePath = process.MainModule.FileName;
                            currentHash = manager.CalculateHash(filePath);
                        }
                        catch (Exception ex)
                        {
                            logger.Log($"Polling: Failed to get MainModule for {processName} (ID: {processId}): {ex.Message}");
                            continue;
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
                                        logger.Log($"Polling: Blocked {processName} (ID: {processId}, Path: {filePath}, Hash: {currentHash}, Reason: {(isNameMatch ? "Name" : isPathMatch ? "Path" : "Hash")})");

                                        if (Application.OpenForms.Count > 0)
                                        {
                                            Task.Run(() =>
                                            {
                                                Application.OpenForms[0]?.Invoke((Action)(() =>
                                                {
                                                    MessageBox.Show($"The launch of {processName} is prohibited!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                                }));
                                            });
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    logger.Log($"Polling: Error terminating {processName} (ID: {processId}): {ex.Message}");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Log($"Polling: Error processing process {processName} (ID: {processId}): {ex.Message}");
                    }
                    finally
                    {
                        process.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log($"Polling: Error in PollRunningProcesses: {ex.Message}");
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
                string processName = "Unknown";
                string processId = "Unknown";

                try
                {
                    processName = process.ProcessName;
                    processId = process.Id.ToString();

                    if (process.Id == Process.GetCurrentProcess().Id || string.IsNullOrEmpty(processName))
                    {
                        continue;
                    }

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
                        bool isNameMatch = (processName + ".exe").Equals(banned.Name, StringComparison.OrdinalIgnoreCase);
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

                                    if (Application.OpenForms.Count > 0)
                                    {
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
                    logger.Log($"Error checking running process {processName} (ID: {processId}): {ex.Message}");
                }
                finally
                {
                    process.Dispose();
                }
            }

            logger.Log("Finished checking running processes.");
        }
    }
}