using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BanPrograms
{
    public class Logger
    {
        private readonly string _logFile;
        public string LogFile => _logFile;

        public Logger()
        {
            string exePath = AppDomain.CurrentDomain.BaseDirectory;
            _logFile = Path.Combine(exePath, "attempts.log");
        }

        public void Log(string message)
        {
            try
            {
                string entry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
                File.AppendAllText(LogFile, entry + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to write to log file: {ex.Message}");
            }
        }
    }

}
