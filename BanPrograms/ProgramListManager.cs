using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Newtonsoft.Json;
using System.Security.Principal;

namespace BanPrograms
{
    public class ProgramListManager
    {
        
        private readonly Logger logger = new Logger();
        private const string FilePath = "banned_programs.json";

        public ProgramList LoadList()
        {
            if (!File.Exists(FilePath))
            {
                return new ProgramList { Enabled = false, Programs = new List<ProgramInfo>() };
            }
            string json = File.ReadAllText(FilePath);
            return JsonConvert.DeserializeObject<ProgramList>(json); 
        }

        public void SaveList(ProgramList list)
        {
            string json = JsonConvert.SerializeObject(list, Formatting.Indented); 
            File.WriteAllText(FilePath, json);
        }

        public string CalculateHash(string filePath)
        {
            try
            {
                using (var sha256 = SHA256.Create())
                {
                    using (var stream = File.OpenRead(filePath))
                    {
                        byte[] hash = sha256.ComputeHash(stream);
                        string hashString = BitConverter.ToString(hash).Replace("-", "").ToLower();
                        logger.Log($"Calculated hash for {filePath}: {hashString}");
                        return hashString;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log($"Error calculating hash for {filePath}: {ex.Message}");
                return string.Empty;
            }
        }

        public bool IsAdmin()
        {
            using (var identity = WindowsIdentity.GetCurrent())
            {
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }
    }
}