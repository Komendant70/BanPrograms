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
        private ProgramList cachedList; // Кэшированный список

        public void Start()
        {
            cachedList = manager.LoadList(); // Загружаем список один раз
            if (!cachedList.Enabled) return;

            WqlEventQuery query = new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace");
            watcher = new ManagementEventWatcher(query);
            watcher.EventArrived += ProcessStarted;
            watcher.Start();
        }

        public void UpdateCache()
        {
            cachedList = manager.LoadList(); // Обновляем кэш при изменении списка
        }

        private void ProcessStarted(object sender, EventArrivedEventArgs e)
        {
            if (!cachedList.Enabled) return;

            string processName = e.NewEvent.Properties["ProcessName"].Value.ToString();
            string processId = e.NewEvent.Properties["ProcessID"].Value.ToString();
            string filePath = Process.GetProcessById(int.Parse(processId)).MainModule.FileName;
            string hash = manager.CalculateHash(filePath);

            foreach (var program in cachedList.Programs) // Используем кэш
            {
                if (program.Name == processName || program.Path == filePath || program.Hash == hash)
                {
                    Process.GetProcessById(int.Parse(processId)).Kill();
                    logger.Log($"Blocked: {processName} (Path: {filePath}, Hash: {hash})");
                    MessageBox.Show($"Запуск программы {processName} заблокирован!", "Внимание");
                    return;
                }
            }
            logger.Log($"Allowed: {processName} (Path: {filePath})");
        }

        public void Stop()
        {
            watcher?.Stop();
        }
    }
}
