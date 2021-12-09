using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tree_To_Tikz
{
    public class RTree
    {
        public int MaxDegree { get; set; }
        public int MinDegree { get; set; }
        public RTreeNode Root { get; set; }
        public bool Any { get { return Root != null; } }
        RTreeLaTeXGenerator Latex { get; set; }

        public RTree(int minDegree, int maxDegree, Logger logger)
        {
            if (minDegree < 1 || maxDegree < 1 || minDegree * 2 > maxDegree)
                throw new ArgumentException();
            MaxDegree = maxDegree;
            MinDegree = minDegree;
            Latex = new RTreeLaTeXGenerator(logger, this);
        }

        public void Add(IndexRecord r)
        {
            Latex.Add(r);
            Draw();
            if (!Any)
                CreateRoot(r);
            else
            {
                RTreeNode leaf = ChooseLeaf(r);
                leaf.Add(r);
                if (leaf.Degree > MaxDegree)
                {
                    Latex.AddToLeaf(r, true);
                    Draw(leaf);
                    Tuple<RTreeNode, RTreeNode> split = SplitNode(leaf);
                    RTreeNode node2 = AdjustTree(split.Item1, split.Item2);
                    if (node2 != null)
                    {
                        InnerRecord record1 = new InnerRecord() { Node = Root, MBR = Root.MinimalBoundedRectangle };
                        InnerRecord record2 = new InnerRecord() { Node = node2, MBR = node2.MinimalBoundedRectangle };
                        Root = new RTreeNode();
                        Root.Add(record1);
                        Root.Add(record2);
                        Latex.SplitCreatedNewRoot();
                        Draw(Root);
                    }
                }
                else
                {
                    Latex.AddToLeaf(r, false);
                    Draw(leaf);
                    AdjustTree(leaf);
                }
            }
        }

        RTreeNode AdjustTree(RTreeNode node, RTreeNode node2 = null)
        {
            while (node != Root)
            {
                RTreeNode parent = node.Parent;
                parent.InnerRecords.Find(r => r.Node == node).MBR = node.MinimalBoundedRectangle;
                if (node2 != null)
                {
                    InnerRecord record = new InnerRecord() { Node = node2, MBR = node2.MinimalBoundedRectangle };
                    parent.InnerRecords.Add(record);
                    node2 = null;
                    if (parent.Degree > MaxDegree)
                    {
                        var split = SplitNode(parent);
                        node2 = split.Item2;
                        Latex.AdjustTree(true, true);
                    }
                    else
                        Latex.AdjustTree(true, false);
                }
                else
                    Latex.AdjustTree(false, false);
                Draw(parent);
                node = parent;
            }
            return node2;
        }

        void CreateRoot(IndexRecord r)
        {
            Latex.CreateRoot(r);
            Root = new RTreeNode(r);
            Draw();
        }

        RTreeNode ChooseLeaf(IndexRecord r)
        {
            RTreeNode curr = Root;
            while (!curr.IsLeaf)
            {
                curr = curr.ChooseChildWithMinimalExtend(r);
            }
            return curr;
        }

        Tuple<RTreeNode, RTreeNode> SplitNode(RTreeNode node)
        {
            if (node.IsLeaf)
                return SplitLeaf(node);
            else
                return SplitInnerNode(node);
        }

        Tuple<RTreeNode, RTreeNode> SplitLeaf(RTreeNode node)
        {
            List<IndexRecord> records = node.IndexRecords;
            var seeds = PickSeeds(records.Cast<Record>().ToList());

            List<IndexRecord> g1 = new List<IndexRecord>() { (IndexRecord)seeds.Item1 };
            List<IndexRecord> g2 = new List<IndexRecord>() { (IndexRecord)seeds.Item2 };
            Rectangle rect1 = null;
            Rectangle rect2 = null;
            records.Remove((IndexRecord)seeds.Item1);
            records.Remove((IndexRecord)seeds.Item2);
            while (true)
            {
                if (!records.Any())
                    break;
                if (MinDegree - g1.Count == records.Count)
                {
                    g1.AddRange(records);
                    break;
                }
                if (MinDegree - g2.Count == records.Count)
                {
                    g2.AddRange(records);
                    break;
                }
                rect1 = new Rectangle(g1.Cast<Record>().ToList());
                rect2 = new Rectangle(g2.Cast<Record>().ToList());
                IndexRecord r = (IndexRecord)PickNext(records.Cast<Record>().ToList(), rect1, rect2);
                double diff1 = new Rectangle(rect1, r.MBR).Area - rect1.Area;
                double diff2 = new Rectangle(rect2, r.MBR).Area - rect2.Area;
                if (diff1 < diff2 || (diff1 == diff2 && rect1.Area < rect2.Area) || (diff1 == diff2 && rect1.Area == rect2.Area && g1.Count < g2.Count))
                    g1.Add(r);
                else
                    g2.Add(r);
                records.Remove(r);
            }
            node.Clear();
            node.IndexRecords = g1;
            return new Tuple<RTreeNode, RTreeNode>(node, new RTreeNode(g2));
        }

        Tuple<RTreeNode, RTreeNode> SplitInnerNode(RTreeNode node)
        {
            List<InnerRecord> records = node.InnerRecords;
            var seeds = PickSeeds(records.Cast<Record>().ToList());

            List<InnerRecord> g1 = new List<InnerRecord>() { (InnerRecord)seeds.Item1 };
            List<InnerRecord> g2 = new List<InnerRecord>() { (InnerRecord)seeds.Item2 };
            Rectangle rect1 = null;
            Rectangle rect2 = null;
            records.Remove((InnerRecord)seeds.Item1);
            records.Remove((InnerRecord)seeds.Item2);
            while (true)
            {
                if (!records.Any())
                    break;
                if (MinDegree - g1.Count == records.Count)
                {
                    g1.AddRange(records);
                    break;
                }
                if (MinDegree - g2.Count == records.Count)
                {
                    g2.AddRange(records);
                    break;
                }
                rect1 = new Rectangle(g1.Cast<Record>().ToList());
                rect2 = new Rectangle(g2.Cast<Record>().ToList());
                InnerRecord r = (InnerRecord)PickNext(records.Cast<Record>().ToList(), rect1, rect2);
                double diff1 = new Rectangle(rect1, r.MBR).Area - rect1.Area;
                double diff2 = new Rectangle(rect2, r.MBR).Area - rect2.Area;
                if (diff1 < diff2 || (diff1 == diff2 && rect1.Area < rect2.Area) || (diff1 == diff2 && rect1.Area == rect2.Area && g1.Count < g2.Count))
                    g1.Add(r);
                else
                    g2.Add(r);
                records.Remove(r);
            }
            node.Clear();
            node.InnerRecords = g1;
            return new Tuple<RTreeNode, RTreeNode>(node, new RTreeNode(g2));
        }

        Tuple<Record, Record> PickSeeds(List<Record> records)
        {
            List<Tuple<Record, Record>> tuples = records.SelectMany(x => records, (x, y) => Tuple.Create(x, y))
                       .Where(tuple => tuple.Item1 != tuple.Item2).ToList();
            return tuples.Aggregate((max, x) =>
            {
                double maxDeadZome = new Rectangle(max.Item1.MBR, max.Item2.MBR).Area - max.Item1.MBR.Area - max.Item2.MBR.Area;
                double xDeadZome = new Rectangle(x.Item1.MBR, x.Item2.MBR).Area - x.Item1.MBR.Area - x.Item2.MBR.Area;
                if (maxDeadZome > xDeadZome)
                    return max;
                else
                    return x;
            });
        }

        Record PickNext(List<Record> records, Rectangle group1MBR, Rectangle group2MBR)
        {
            double maxDifference = 0;
            Record maxRecord = records[0];
            foreach (Record record in records)
            {
                double difference = Math.Abs(GroupExtendCost(record, group1MBR) - GroupExtendCost(record, group2MBR));
                if (difference > maxDifference)
                {
                    maxDifference = difference;
                    maxRecord = record;
                }
            }
            return maxRecord;
        }

        double GroupExtendCost(Record r, Rectangle rect)
        {
            return new Rectangle(r.MBR, rect).Area - rect.Area;
        }

        void Draw(RTreeNode node = null)
        {
            Latex.Draw(node);
        }
    }
}
