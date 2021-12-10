using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tree_To_Tikz
{
    public class BPlusTreeGenerator : BXTreeGenerator
    {
        override public void CreateTree(List<int> degrees, Logger logger)
        {
            Logger = logger;
            Tree = new BPlusTree(degrees[0], Logger);
        }
    }
}
