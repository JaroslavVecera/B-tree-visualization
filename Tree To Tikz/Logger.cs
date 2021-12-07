using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tree_To_Tikz
{
    public class Logger
    {
        public string Content { get; set; }
        public bool IsActive { get; set; } = false;

        public Logger()
        {
            Clear();
        }

        public void Log(string s)
        {
            if (IsActive)
                Content += s;
        }

        public void Clear()
        {
            Content = "";
        }
    }
}
