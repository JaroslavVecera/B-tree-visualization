using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tree_To_Tikz
{
    public class BTree : BXTree
    {
        public int MinDegree { get; private set; }
        public int MaxDegree { get { return 2 * MinDegree + 1; } }
        public BTreeNode Root { get; private set; }
        BTreeLaTeXGenerator Latex { get; set; }

        public BTree(int degree, Logger l)
        {
            MinDegree = degree - 1;
            Latex = new BTreeLaTeXGenerator(l, this);
            Root = null;
        }

        public Stack<BTreeNode> FindLeaf(int i)
        {
            var node = Root;
            Stack<BTreeNode> path = new Stack<BTreeNode>();
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

        BTreeNode FindLeafWithPreventiveSplit(int i)
        {
            BTreeNode parent = null;
            var node = Root;
            while (true)
            {
                if (node.Degree == MaxDegree)
                {
                    Latex.PreventiveSplitNeeded();
                    Draw(node);
                    Stack<BTreeNode> s = new();
                    if (node == Root)
                    {
                        s.Push(node);
                        Split(s);
                        parent = Root;
                    }
                    else
                    {
                        s.Push(parent);
                        s.Push(node);
                        Split(s);
                    }
                    node = parent;
                    continue;
                }
                var child = 0;
                for (; child < node.Degree && i > node.Content[child];) { child++; }
                if (node.Content[Math.Max(child - 1, 0)] == i || node.Children[child] == null)
                    return node;
                else
                {
                    parent = node;
                    node = node.Children[child];
                }
            }
        }

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

        public void AddWithPreventiveSplit(int i)
        {
            Latex.Add(i);
            Draw();
            if (Root == null)
                CreateRoot(i);
            else
            {
                var l = FindLeafWithPreventiveSplit(i);
                var contains = l.Contains(i);
                l.Add(i);
                Latex.AddToLeaf(i, contains, false);
                Draw(l);
            }
        }

        void CreateRoot(int i)
        {
            Latex.CreateRoot(i);
            Root = new BTreeNode();
            Root.Add(i);
            Draw();
        }

        void Split(Stack<BTreeNode> l)
        {
            BTreeNode curr = l.Pop();
            var split = curr.GetSplit();
            if (!l.Any())
            {
                Root = new BTreeNode();
                Root.Add(split.Item2);
                Root.Children[0] = split.Item1;
                Root.Children[1] = split.Item3;
                Latex.SplitCreatedNewRoot();
                Draw(Root);
            }
            else
            {
                BTreeNode parent = l.Peek();
                parent.Add(split.Item2);
                int parentIndex = parent.Children.IndexOf(curr);
                parent.Children[parentIndex] = split.Item1;
                parent.Children[parentIndex + 1] = split.Item3;

                if (parent.Degree > MaxDegree)
                {
                    Latex.Split(true);
                    Draw(parent);
                    Split(l);
                }
                else
                {
                    Latex.Split(false);
                    Draw(parent);
                }
            }
        }

        public override void Remove(int i)
        {
            Latex.Remove(i);
            Draw();
            if (Root == null)
            {
                Latex.EmptyCantRemove(i);
                return;
            }
            RemoveFrom(i, Root);
        }

        void RemoveFrom(int i, BTreeNode n)
        {
            if (n.IsLeaf)
                RemoveFromLeaf(i, n);
            else
                RemoveFromInnerNode(i, n);
        }

        void RemoveFromLeaf(int i, BTreeNode n)
        {
            if (!n.Contains(i))
                Latex.RemoveFromLeaf(i, false);
            else
            {
                Latex.RemoveFromLeaf(i, true);
                Draw(n);
                n.Remove(i);
                Draw();
            }
        }

        void RemoveFromInnerNode(int i, BTreeNode n)
        {
            if (!n.Contains(i))
            {
                int child = 0;
                for (; child < n.Degree && i > n.Content[child];) { child++; }
                if (n.Children[child].Degree == MinDegree)
                {
                    Latex.MustEnforceMoreThanMin();
                    EnforceMoreThanMin(n, child, i);
                    RemoveFromInnerNode(i, n);
                }
                else
                {
                    Latex.MoreThanMinDegree();
                    Draw(n);
                    RemoveFrom(i, n.Children[child]);
                }
            }
            else
            {
                int index = n.Content.IndexOf(i);
                if (n.Children[index].Degree > MinDegree)
                {
                    var max = n.Children[index].FindMax();
                    n.Content[index] = max;
                    Latex.ReplaceLeftmost(i);
                    Draw(n);
                    RemoveFrom(max, n.Children[index]);
                }
                else if (n.Children[index + 1].Degree > MinDegree)
                {
                    var min = n.Children[index + 1].FindMin();
                    n.Content[index] = min;
                    Latex.ReplaceRightmost(i);
                    Draw(n);
                    RemoveFrom(min, n.Children[index + 1]);
                }
                else
                {
                    Latex.ReplaceMerge(i);
                    Draw(n);
                    var key = n.Content[index];
                    var leftSubTree = n.Children[index];
                    leftSubTree.Add(key);
                    leftSubTree.Children.RemoveAt(leftSubTree.Degree);
                    var rightSubTree = n.Children[index + 1];
                    for (int j = 0; j < rightSubTree.Degree; j++)
                    {
                        leftSubTree.Content.Add(rightSubTree.Content[j]);
                        leftSubTree.Children.Add(rightSubTree.Children[j]);
                    }
                    leftSubTree.Children.Add(rightSubTree.Children[rightSubTree.Degree]);
                    n.Content.RemoveAt(index);
                    n.Children.RemoveAt(index);
                    n.Children[index] = leftSubTree;
                    if (Root.Degree == 0)
                        Root = Root.Children[0];
                    RemoveFrom(i, leftSubTree);
                }
            }
        }

        void EnforceMoreThanMin(BTreeNode n, int child, int i)
        {
            if (child > 0 && n.Children[child - 1].Degree > MinDegree)
            {
                var leftChild = n.Children[child - 1];
                var max = leftChild.Content.Max();
                var rightmostSubtree = leftChild.RemoveMaxPopSubTree();
                var key = n.Content[child - 1];
                n.Content[child - 1] = max;
                n.Children[child].PushLeft(key, rightmostSubtree);
                Latex.EnforceLeftHasMore(i);
            }
            else if (child < n.Degree && n.Children[child + 1].Degree > MinDegree)
            {
                var rightChild = n.Children[child + 1];
                var min = rightChild.Content.Min();
                var leftmostSubtree = rightChild.RemoveMinPopSubTree();
                var key = n.Content[child];
                n.Content[child] = min;
                n.Children[child].PushRight(key, leftmostSubtree);
                Latex.EnforceRightHasMore(i);
            }
            else
            {
                Merge(n, child, i);
            }
            Draw(n);
        }

        void Merge(BTreeNode n, int child, int i)
        {
            var right = false;
            if (child == 0)
            {
                var key = n.Content.Min();
                var leftmostSubTree = n.RemoveMinPopSubTree();
                leftmostSubTree.Add(key);
                leftmostSubTree.Children.RemoveAt(leftmostSubTree.Degree);
                var rightSubTree = n.Children[0];
                for (int j = 0; j < rightSubTree.Degree; j++)
                {
                    leftmostSubTree.Content.Add(rightSubTree.Content[j]);
                    leftmostSubTree.Children.Add(rightSubTree.Children[j]);
                }
                leftmostSubTree.Children.Add(rightSubTree.Children[rightSubTree.Degree]);
                n.Children[0] = leftmostSubTree;
                if (Root.Degree == 0)
                    Root = Root.Children[0];
                right = true;
            }
            else
            {
                var key = n.Content[child - 1];
                var leftSubTree = n.Children[child - 1];
                leftSubTree.Add(key);
                leftSubTree.Children.RemoveAt(leftSubTree.Degree);
                var rightSubTree = n.Children[child];
                for (int j = 0; j < rightSubTree.Degree; j++)
                {
                    leftSubTree.Content.Add(rightSubTree.Content[j]);
                    leftSubTree.Children.Add(rightSubTree.Children[j]);
                }
                leftSubTree.Children.Add(rightSubTree.Children[rightSubTree.Degree]);
                n.Content.RemoveAt(child - 1);
                n.Children.RemoveAt(child - 1);
                n.Children[child - 1] = leftSubTree;
                if (Root.Degree == 0)
                    Root = Root.Children[0];
            }
            Latex.Merge(right, i);
        }

        public void Draw()
        {
            Draw(null);
        }

        void Draw(BTreeNode marked)
        {
            Latex.Draw(marked);
        }
    }
}
