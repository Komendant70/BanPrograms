using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Newtonsoft.Json;
using System.Security.Principal;
using System.Reflection;

namespace BanPrograms
{
    public class ProgramListManager
    {
        
        private readonly Logger logger = new Logger();
        private const string FilePath = "banned_programs.json";

        public ProgramList LoadList()
        {
            try
            {
                string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "banned_programs.json");
                logger.Log($"Attempting to load banned_programs.json from: {path}");

                if (!File.Exists(path))
                {
                    logger.Log("banned_programs.json does not exist, creating default.");
                    var defaultList = new ProgramList { Enabled = false, Programs = new List<ProgramInfo>() };
                    SaveList(defaultList);
                    return defaultList;
                }

                string json = File.ReadAllText(path);
                var list = Newtonsoft.Json.JsonConvert.DeserializeObject<ProgramList>(json);
                if (list == null)
                {
                    logger.Log("Deserialization returned null, creating default list.");
                    list = new ProgramList { Enabled = false, Programs = new List<ProgramInfo>() };
                    SaveList(list);
                }

                logger.Log($"Loaded banned_programs.json: Enabled={list.Enabled}, Programs count={list.Programs.Count}");
                return list;
            }
            catch (Exception ex)
            {
                logger.Log($"Error loading banned_programs.json: {ex.Message}");
                return new ProgramList { Enabled = false, Programs = new List<ProgramInfo>() };
            }
        }

        public void SaveList(ProgramList list)
        {
            try
            {
                string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "banned_programs.json");
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(list, Formatting.Indented);
                File.WriteAllText(path, json);
                logger.Log($"Saved banned_programs.json: Enabled={list.Enabled}, Programs count={list.Programs.Count}");
            }
            catch (Exception ex)
            {
                logger.Log($"Error saving banned_programs.json: {ex.Message}");
            }
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