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
        private const string LogFile = "attempts.log";

        public void Log(string message)
        {
            string entry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
            File.AppendAllText(LogFile, entry + Environment.NewLine);
        }
    }
    
}
