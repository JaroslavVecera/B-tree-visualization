using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tree_To_Tikz
{
    abstract public class BXTreeGenerator : TreeGenerator
    {
        protected BXTree Tree { get; set; }

        override protected void Add(string expr, bool preventiveSplits)
        {
            int i;
            if (Tree == null || !TryParseExpr(expr, out i))
                return;
            if (preventiveSplits && Tree is BTree)
                ((BTree)Tree).AddWithPreventiveSplit(i);
            else
                Tree.Add(i);
        }

        override protected void Remove(string expr)
        {
            int i;
            if (Tree == null || !TryParseExpr(expr, out i))
                return;
            Tree.Remove(i);
        }

        protected bool TryParseExpr(string expr, out int res)
        {
            return int.TryParse(expr, out res);
        }
    }
}
