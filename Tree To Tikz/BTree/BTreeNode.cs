﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tree_To_Tikz
{
    public class BTreeNode
    {
        public List<int> Content { get; private set; }
        public List<BTreeNode> Children { get; private set; }
        public int Degree { get { return Content.Count; } }
        public bool IsLeaf { get { return Children[0] == null; } }
        public int MaxPartWidth { get { return Math.Max(Content.Max(c => c.ToString().Length), IsLeaf? 0 : Children.Max(c => c.MaxPartWidth)); } }
        public int Depth { get; set; }

        public BTreeNode()
        {
            Content = new List<int>();
            Children = new List<BTreeNode>() { null };
        }

        public bool Contains(int i)
        {
            return Content.Contains(i);
        }

        public void Add(int i)
        {
            if (Contains(i))
                return;
            int index = 0;
            while (index < Degree && i > Content[index]) { index++; }
            Content.Add(0);
            Children.Add(Children.Last());
            for (int j = Degree - 1; j > index; j--)
            {
                Content[j] = Content[j - 1];
                Children[j] = Children[j - 1];
            }
            Content[index] = i;
        }

        public void Remove(int i)
        {
            if (!Content.Contains(i))
                return;
            Content.Remove(i);
            Children.Remove(null);
        }

        public BTreeNode RemoveMinPopSubTree()
        {
            var res = Children[0];
            Children.Remove(res);
            Content.RemoveAt(0);
            return res;
        }

        public BTreeNode RemoveMaxPopSubTree()
        {
            var res = Children[Degree];
            Children.Remove(res);
            Content.RemoveAt(Degree - 1);
            return res;
        }

        public Tuple<BTreeNode, int, BTreeNode> GetSplit()
        {
            int splitDegree = (Degree - 1) / 2;
            var l = new BTreeNode();
            for (int i = 0; i < splitDegree; i++)
            {
                l.Add(Content[i]);
                l.Children[i] = Children[i];
            }
            l.Children[splitDegree] = Children[splitDegree];
            var r = new BTreeNode();
            for (int i = 0; i < Degree - splitDegree - 1; i++)
            {
                r.Add(Content[splitDegree + 1 + i]);
                r.Children[i] = Children[splitDegree + 1 + i];
            }
            r.Children[Degree - splitDegree - 1] = Children[Degree];
            return new Tuple<BTreeNode, int, BTreeNode>(l, Content[splitDegree],r);
        }

        public  void PushLeft(int key, BTreeNode subTree)
        {
            Content.Insert(0, key);
            Children.Insert(0, subTree);
        }

        public void PushRight(int key, BTreeNode subTree)
        {
            Content.Add(key);
            Children.Add(subTree);
        }

        public int FindMax()
        {
            if (IsLeaf)
                return Content.Max();
            else
                return Children[Degree].FindMax();
        }

        public int FindMin()
        {
            if (IsLeaf)
                return Content.Min();
            else
                return Children[0].FindMin();
        }
    }
}
