using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BanPrograms
{
    public class ProgramInfo
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string Hash { get; set; }
    }

    public class ProgramList
    {
        public bool Enabled { get; set; }
        public List<ProgramInfo> Programs { get; set; }
    }
}
