using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tree_To_Tikz
{
    public class BStarTree : BXTree
    {
        public int MaxRootDegree { get { return ((2 * MaxDegree) / 3) * 2; } }
        public int MaxDegree { get; private set; }
        public BStarTreeNode Root { get; private set; }
        BStarTreeLaTeXGenerator Latex { get; set; }

        public BStarTree(int degree, Logger l)
        {
            MaxDegree = degree - 1;
            Latex = new BStarTreeLaTeXGenerator(l, this);
            Root = null;
        }

        public Stack<BStarTreeNode> FindLeaf(int i)
        {
            var node = Root;
            Stack<BStarTreeNode> path = new Stack<BStarTreeNode>();
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
                if (l.Peek().Degree > MaxRootDegree || (l.Peek() != Root && l.Peek().Degree > MaxDegree))
                {
                    Overfull(l);
                }
                else
                {
                    Latex.AddToLeaf(i, contains, false);
                    Draw(l.Peek());
                }
            }
        }

        void CreateRoot(int i)
        {
            Latex.CreateRoot(i);
            Root = new BStarTreeNode();
            Root.Add(i);
            Draw();
        }

        void Overfull(Stack<BStarTreeNode> path)
        {
            BStarTreeNode curr = path.Pop();
            BStarTreeNode parent = path.Any() ? path.Peek() : null;
            if (parent == null)
            {
                path.Push(curr);
                SplitRoot(path);
            }
            else
            {
                int parentIndex = parent.Children.IndexOf(curr);
                BStarTreeNode rightSibling = parentIndex + 1 < parent.Children.Count ? parent.Children[parentIndex + 1] : null;
                BStarTreeNode leftSibling = parentIndex - 1 >= 0 ? parent.Children[parentIndex - 1] : null;
                if (rightSibling != null && rightSibling.Degree < MaxDegree)
                    RotationRight(curr, parent, parentIndex, rightSibling);
                else if (leftSibling != null && leftSibling.Degree < MaxDegree)
                    RotationLeft(curr, parent, parentIndex, leftSibling);
                else if (rightSibling != null)
                    TwoThreeSplitRight(path, curr, rightSibling, parent, parentIndex);
                else
                    TwoThreeSplitLeft(path, curr, leftSibling, parent, parentIndex);
            }
        }

        void TwoThreeSplitRight(Stack<BStarTreeNode> path, BStarTreeNode curr, BStarTreeNode rightSibling, BStarTreeNode parent, int parentIndex)
        {
            Latex.TwoThreeSplitRight();
            Draw(curr);
            int leftDegree = (2 * curr.Degree - 2) / 3;
            List<int> keys = curr.Content.Select(k => k).ToList();
            keys.Add(parent.Content[parentIndex]);
            keys.AddRange(rightSibling.Content.Select(c => c));
            int newParentLeftKey = keys[leftDegree];
            int newParentRightKey = keys[2 * leftDegree + 1];
            keys.RemoveAt(2 * leftDegree + 1);
            keys.RemoveAt(leftDegree);
            List<BStarTreeNode> children = curr.Children.Select(k => k).ToList();
            children.AddRange(rightSibling.Children.Select(c => c));
            parent.Content[parentIndex] = newParentLeftKey;
            parent.Content.Insert(parentIndex + 1, newParentRightKey);
            curr.Content.Clear();
            curr.Content.AddRange(keys.Take(leftDegree));
            curr.Children.Clear();
            curr.Children.AddRange(children.Take(leftDegree + 1));
            BStarTreeNode center = new BStarTreeNode();
            parent.Children.Insert(parentIndex + 1, center);
            center.Content.AddRange(keys.Skip(leftDegree).Take(leftDegree));
            center.Children.Clear();
            center.Children.AddRange(children.Skip(leftDegree + 1).Take(leftDegree + 1));
            rightSibling.Content.Clear();
            rightSibling.Content.AddRange(keys.Skip(2 * leftDegree));
            rightSibling.Children.Clear();
            rightSibling.Children.AddRange(children.Skip(2 * leftDegree + 2));
            if (parent.Degree > MaxRootDegree || (parent != Root && parent.Degree > MaxDegree))
                Overfull(path);
            else
                Draw();
        }

        void TwoThreeSplitLeft(Stack<BStarTreeNode> path, BStarTreeNode curr, BStarTreeNode leftSibling, BStarTreeNode parent, int parentIndex)
        {
            Latex.TwoThreeSplitLeft();
            Draw(curr);
            int leftDegree = (2 * curr.Degree - 2) / 3;
            List<int> keys = leftSibling.Content.Select(k => k).ToList();
            keys.Add(parent.Content[parentIndex - 1]);
            keys.AddRange(curr.Content.Select(c => c));
            int newParentLeftKey = keys[leftDegree];
            int newParentRightKey = keys[2 * leftDegree + 1];
            keys.RemoveAt(2 * leftDegree + 1);
            keys.RemoveAt(leftDegree);
            List<BStarTreeNode> children = leftSibling.Children.Select(k => k).ToList();
            children.AddRange(curr.Children.Select(c => c));
            parent.Content[parentIndex - 1] = newParentRightKey;
            parent.Content.Insert(parentIndex - 1, newParentLeftKey);
            leftSibling.Content.Clear();
            leftSibling.Content.AddRange(keys.Take(leftDegree));
            leftSibling.Children.Clear();
            leftSibling.Children.AddRange(children.Take(leftDegree + 1));
            BStarTreeNode center = new BStarTreeNode();
            parent.Children.Insert(parentIndex, center);
            center.Content.AddRange(keys.Skip(leftDegree).Take(leftDegree));
            center.Children.Clear();
            center.Children.AddRange(children.Skip(leftDegree + 1).Take(leftDegree + 1));
            curr.Content.Clear();
            curr.Content.AddRange(keys.Skip(2 * leftDegree));
            curr.Children.Clear();
            curr.Children.AddRange(children.Skip(2 * leftDegree + 2));
            if (parent.Degree > MaxRootDegree || (parent != Root && parent.Degree > MaxDegree))
                Overfull(path);
            else
                Draw();
        }

        void RotationRight(BStarTreeNode curr, BStarTreeNode parent, int parentIndex, BStarTreeNode rightSibling)
        {
            Latex.RotRight();
            Draw(curr);
            int leftDegree = (curr.Degree + rightSibling.Degree) / 2;
            int newParentKey = curr.Content[leftDegree];
            curr.Content.RemoveAt(leftDegree);
            List<int> keys = curr.Content.Select(k => k).ToList();
            keys.Add(parent.Content[parentIndex]);
            keys.AddRange(rightSibling.Content.Select(c => c));
            List<BStarTreeNode> children = curr.Children.Select(k => k).ToList();
            children.AddRange(rightSibling.Children.Select(c => c));
            parent.Content[parentIndex] = newParentKey;
            curr.Content.Clear();
            curr.Content.AddRange(keys.Take(leftDegree));
            curr.Children.Clear();
            curr.Children.AddRange(children.Take(leftDegree + 1));
            rightSibling.Content.Clear();
            rightSibling.Content.AddRange(keys.Skip(leftDegree));
            rightSibling.Children.Clear();
            rightSibling.Children.AddRange(children.Skip(leftDegree + 1));
            Draw();
        }

        void RotationLeft(BStarTreeNode curr, BStarTreeNode parent, int parentIndex, BStarTreeNode leftSibling)
        {
            Latex.RotLeft();
            Draw(curr);
            int leftDegree = (curr.Degree + leftSibling.Degree) / 2;
            List<int> keys = leftSibling.Content.Select(k => k).ToList();
            keys.Add(parent.Content[parentIndex - 1]);
            keys.AddRange(curr.Content.Select(c => c));
            int newParentKey = keys[leftDegree];
            keys.RemoveAt(leftDegree);
            List<BStarTreeNode> children = leftSibling.Children.Select(k => k).ToList();
            children.AddRange(curr.Children.Select(c => c));
            parent.Content[parentIndex - 1] = newParentKey;
            leftSibling.Content.Clear();
            leftSibling.Content.AddRange(keys.Take(leftDegree));
            leftSibling.Children.Clear();
            leftSibling.Children.AddRange(children.Take(leftDegree + 1));
            curr.Content.Clear();
            curr.Content.AddRange(keys.Skip(leftDegree));
            curr.Children.Clear();
            curr.Children.AddRange(children.Skip(leftDegree + 1));
            Draw();
        }

        void SplitRoot(Stack<BStarTreeNode> l)
        {
            Latex.Split();
            Draw(Root);
            BStarTreeNode curr = l.Pop();
            var split = curr.GetSplit();
            Root = new BStarTreeNode();
            Root.Add(split.Item2);
            Root.Children[0] = split.Item1;
            Root.Children[1] = split.Item3;
            Latex.SplitCreatedNewRoot();
            Draw(Root);
        }

        public void Draw()
        {
            Draw(null);
        }

        void Draw(BStarTreeNode marked)
        {
            Latex.Draw(marked);
        }

        public override void Remove(int i) { }
    }
}
