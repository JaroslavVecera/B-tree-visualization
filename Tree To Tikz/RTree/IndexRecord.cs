using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tree_To_Tikz
{
    public class IndexRecord : Record
    {
        public int Id { get; set; }

        public override string ToString()
        {
            return $"[{MBR.Left}, {MBR.Right}], [{MBR.Bottom}, {MBR.Top}]";
        }
    }
}
