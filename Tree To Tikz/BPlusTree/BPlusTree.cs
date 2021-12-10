using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tree_To_Tikz
{
    public class BPlusTree : BXTree
    {
        public int MinDegree { get { return (MaxDegree - 1) / 2 + 1; ; } }
        public int MaxDegree { get; private set; }
        public BPlusTreeNode Root { get; private set; }
        BPlusTreeLaTeXGenerator Latex { get; set; }

        public BPlusTree(int maxDegree, Logger l)
        {
            MaxDegree = maxDegree - 1;
            Latex = new BPlusTreeLaTeXGenerator(l, this);
            Root = null;
        }

        public override void Remove(int i) { }

        public override void Add(int i)
        {
            Latex.Add(i);
            Draw();
            if (Root == null)
                CreateRoot(i);
            else
            {
                var l = FindLeaf(i);
                var contains = l.Peek().Contains(i);
                l.Peek().Add(i);
                if (l.Peek().Degree > MaxDegree)
                {
                    Latex.AddToLeaf(i, contains, true);
                    Draw(l.Peek());
                    Split(l);
                }
                else
                {
                    Latex.AddToLeaf(i, contains, false);
                    Draw(l.Peek());
                }
            }
        }

        public Stack<BPlusTreeNode> FindLeaf(int i)
        {
            var node = Root;
            Stack<BPlusTreeNode> path = new Stack<BPlusTreeNode>();
            while (true)
            {
                var child = 0;
                path.Push(node);
                for (; child < node.Degree && i > node.Content[child];) { child++; }
                if (node.Content[Math.Max(child - 1, 0)] == i || node.Children[child] == null)
                    return path;
                else
                    node = node.Children[child];
            }
        }

        void CreateRoot(int i)
        {
            Latex.CreateRoot(i);
            Root = new BPlusTreeNode();
            Root.Add(i);
            Draw();
        }

        void Split(Stack<BPlusTreeNode> l)
        {
            BPlusTreeNode curr = l.Pop();
            bool isLeaf = curr.IsLeaf;
            var split = curr.GetSplit();
            if (!l.Any())
            {
                Root = new BPlusTreeNode();
                Root.Add(split.Item2);
                Root.Children[0] = split.Item1;
                Root.Children[1] = split.Item3;
                Latex.SplitCreatedNewRoot(isLeaf);
                Draw(Root);
            }
            else
            {
                BPlusTreeNode parent = l.Peek();
                parent.Add(split.Item2);
                int parentIndex = parent.Children.IndexOf(curr);
                parent.Children[parentIndex] = split.Item1;
                parent.Children[parentIndex + 1] = split.Item3;

                if (parent.Degree > MaxDegree)
                {
                    Latex.Split(true, isLeaf);
                    Draw(parent);
                    Split(l);
                }
                else
                {
                    Latex.Split(false, isLeaf);
                    Draw(parent);
                }
            }
        }

        public void Draw()
        {
            Draw(null);
        }

        void Draw(BPlusTreeNode marked)
        {
            Latex.Draw(marked);
        }
    }
}
