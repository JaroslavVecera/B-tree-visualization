using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tree_To_Tikz
{
    public class RTreeNode
    {
        public List<IndexRecord> IndexRecords { get; set; } = new List<IndexRecord>();
        public List<InnerRecord> InnerRecords { get; set; } = new List<InnerRecord>();
        public int Degree { get { return IndexRecords.Count + InnerRecords.Count; } }
        public bool IsLeaf { get { return IndexRecords.Any(); } }
        public RTreeNode Parent { get; private set; } = null;
        public List<string> ChildNames { get; set; } = new List<string>();
        public int Depth { get; set; }
        public int MaxPartWidth { get { return Math.Max(ChildNames.Max(c => c.ToString().Length), IsLeaf ? 0 : InnerRecords.Select(r => r.Node).Max(c => c.MaxPartWidth)); } }
        public Rectangle MinimalBoundedRectangle
        {
            get
            {
                if (IsLeaf)
                    return new Rectangle(IndexRecords.Cast<Record>().ToList());
                else
                    return new Rectangle(InnerRecords.Cast<Record>().ToList());
            }
        }

        public RTreeNode() { }

        public RTreeNode(IndexRecord r) 
        {
            Add(r);
        }

        public RTreeNode(List<IndexRecord> indexRecords)
        {
            IndexRecords = indexRecords;
        }

        public RTreeNode(List<InnerRecord> innerRecords)
        {
            Clear();
            foreach (InnerRecord r in innerRecords)
                Add(r);
        }

        public void Clear()
        {
            IndexRecords.Clear();
            InnerRecords.Clear();
        }

        public bool Contains(InnerRecord r)
        {
            return InnerRecords.Contains(r);
        }

        public bool Contains(IndexRecord r)
        {
            return IndexRecords.Contains(r);
        }

        public void Add(InnerRecord r)
        {
            if (IndexRecords.Any())
                throw new InvalidOperationException();
            InnerRecords.Add(r);
            r.Node.Parent = this;
        }

        public void Add(IndexRecord r)
        {
            if (InnerRecords.Any())
                throw new InvalidOperationException();
            IndexRecords.Add(r);
        }

        public RTreeNode ChooseChildWithMinimalExtend(IndexRecord r)
        {
            if (IsLeaf)
                throw new InvalidOperationException();
            InnerRecord minimal = InnerRecords.Aggregate((min, x) =>
            {
                double minExtention = new Rectangle(min.MBR, r.MBR).Area;
                double xExtention = new Rectangle(x.MBR, r.MBR).Area;
                if (minExtention < xExtention || (minExtention == xExtention && min.MBR.Area <= x.MBR.Area))
                    return min;
                else
                    return x;
            });
            return minimal.Node;
        }
    }
}
