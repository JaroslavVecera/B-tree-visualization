using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tree_To_Tikz
{
    public class BTreeGenerator : BXTreeGenerator
    {
        override public void CreateTree(List<int> degrees, Logger logger)
        {
            Logger = logger;
            Tree = new BTree(degrees[0], Logger);
        }
    }
}
