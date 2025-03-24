using System;
using System.Management;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
            if (!cachedList.Enabled) return;

            WqlEventQuery query = new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace");
            watcher = new ManagementEventWatcher(query);
            watcher.EventArrived += ProcessStarted;
            watcher.Start();
        }

        public void Stop()
        {
            watcher?.Stop();
        }

        public void UpdateCache()
        {
            cachedList = manager.LoadList();
        }

        private void ProcessStarted(object sender, EventArrivedEventArgs e)
        {
            if (!cachedList.Enabled) return;

            string processName = e.NewEvent.Properties["ProcessName"].Value.ToString();
            string processId = e.NewEvent.Properties["ProcessID"].Value.ToString();

            try
            {
                // Проверяем, есть ли процесс в списке запрещённых
                bool isBanned = false;
                string filePath = null;
                string hash = null;

                // Проверяем по имени процесса
                foreach (var program in cachedList.Programs)
                {
                    if (program.Name.Equals(processName, StringComparison.OrdinalIgnoreCase))
                    {
                        isBanned = true;
                        break;
                    }
                }

                // Если процесс уже помечен как запрещённый, пытаемся завершить его
                if (isBanned)
                {
                    try
                    {
                        Process process = Process.GetProcessById(int.Parse(processId));
                        filePath = process.MainModule.FileName;
                        hash = manager.CalculateHash(filePath);

                        // Дополнительная проверка по пути и хэшу
                        foreach (var program in cachedList.Programs)
                        {
                            if (program.Path == filePath || program.Hash == hash)
                            {
                                process.Kill();
                                logger.Log($"Blocked: {processName} (Path: {filePath}, Hash: {hash})");
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
                        // Процесс завершился до того, как мы смогли его проверить
                        logger.Log($"Process {processName} (ID: {processId}) terminated before inspection.");
                        MessageBox.Show(
                            $"Попытка запуска программы {processName} была заблокирована, но процесс завершился слишком быстро.",
                            "Предупреждение",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                        );
                        return;
                    }
                }

                // Если процесс не был помечен как запрещённый по имени, проверяем путь и хэш
                try
                {
                    Process process = Process.GetProcessById(int.Parse(processId));
                    filePath = process.MainModule.FileName;
                    hash = manager.CalculateHash(filePath);

                    foreach (var program in cachedList.Programs)
                    {
                        if (program.Path == filePath || program.Hash == hash)
                        {
                            process.Kill();
                            logger.Log($"Blocked: {processName} (Path: {filePath}, Hash: {hash})");
                            MessageBox.Show(
                                $"Запуск программы {processName} запрещён!\nПуть: {filePath}",
                                "Запрещено",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error
                            );
                            return;
                        }
                    }
                    logger.Log($"Allowed: {processName} (Path: {filePath})");
                }
                catch (ArgumentException)
                {
                    logger.Log($"Process {processName} (ID: {processId}) terminated before inspection.");
                }
            }
            catch (Exception ex)
            {
                logger.Log($"Error processing {processName}: {ex.Message}");
            }
        }
    }
}
