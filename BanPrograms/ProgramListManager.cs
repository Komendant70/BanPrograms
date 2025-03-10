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
        private const string FilePath = "banned_programs.json";

        public ProgramList LoadList()
        {
            if (!File.Exists(FilePath))
            {
                return new ProgramList { Enabled = false, Programs = new List<ProgramInfo>() };
            }
            string json = File.ReadAllText(FilePath);
            return JsonConvert.DeserializeObject<ProgramList>(json); // Прямое десериализация строки
        }

        public void SaveList(ProgramList list)
        {
            string json = JsonConvert.SerializeObject(list, Formatting.Indented); // Сериализация с отступами
            File.WriteAllText(FilePath, json);
        }

        public string CalculateHash(string filePath)
        {
            using (var sha256 = SHA256.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    byte[] hash = sha256.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLower();
                }
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