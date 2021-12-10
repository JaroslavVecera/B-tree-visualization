using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tree_To_Tikz
{
    public class RTreeGenerator : TreeGenerator
    {
        protected RTree Tree { get; set; }
        DB Database { get; set; } = new DB();

        public override void CreateTree(List<int> degrees, Logger l)
        {
            Logger = l;
            Database.Clear();
            try
            {
                Tree = new RTree(degrees[0], degrees[1], Logger);
            }
            catch (Exception e)
            {
                Logger.Clear();
                Tree = null;
            }
        }

        override protected void Add(string expr, bool preventiveSplits)
        {
            IndexRecord rec;
            if (Tree == null || !TryParseExpr(expr, out rec))
                return;
            Tree.Add(rec);
        }

        override protected void Remove(string expr) { }

        protected bool TryParseExpr(string expr, out IndexRecord rec)
        {
            string[] parts = expr.Split(",");
            if (parts.Length == 4
                && parts[0].StartsWith("[")
                && parts[2].StartsWith("[")
                && parts[1].EndsWith("]")
                && parts[3].EndsWith("]")
                && parts.ToList().All(p => p.Length >= 2))
            {
                double left;
                double top;
                double right;
                double bottom;
                if (double.TryParse(parts[0].Substring(1, 1), out left)
                    && double.TryParse(parts[1].Substring(0, 1), out right)
                    && double.TryParse(parts[2].Substring(1, 1), out bottom)
                    && double.TryParse(parts[3].Substring(0, 1), out top))
                {
                    Rectangle r = null;
                    try
                    {
                        r = new Rectangle(left, top, right, bottom);
                    }
                    catch (Exception e) { }
                    if (r != null)
                    {
                        rec = new IndexRecord() { MBR = r, Id = Database.GenerateNew() };
                        return true;
                    }
                }
            }
            rec = null;
            return false;
        }
    }
}
