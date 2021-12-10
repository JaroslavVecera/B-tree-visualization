using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tree_To_Tikz
{
    abstract public class TreeGenerator
    {
        protected Logger Logger { get; set; }

        abstract public void CreateTree(List<int> degrees, Logger l);
        abstract protected void Add(string expr, bool preventiveSplits);
        abstract protected void Remove(string expr);

        public void Exec(string expr, bool preventiveSplits)
        {
            if (expr == "log")
                Logger.IsActive = true;
            else if (expr.Length > 3 && expr.StartsWith("add"))
                Add(expr.Remove(0, 3), preventiveSplits);
            else if (expr.Length > 6 && expr.StartsWith("remove"))
                Remove(expr.Remove(0, 6));
        }
    }
}
