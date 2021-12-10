using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tree_To_Tikz
{
    class BPlusTreeLaTeXGenerator
    {
        Logger Logger { get; set; }
        double DigitWidth { get; } = 0.194;
        int MaxDegree { get; set; }
        bool CurvedArrows { get; set; } = false;
        string[] IntText { get; } = new string[] { "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fiveteen", "sixteen", "seventeen", "eighteen", "nineteen", "twenty" };
        BPlusTreeNode Marked { get; set; } = null;
        BPlusTree Tree { get; set; }
        List<BPlusTreeNode> LeafList { get; set; }

        public BPlusTreeLaTeXGenerator(Logger l, BPlusTree tree)
        {
            LeafList = new List<BPlusTreeNode>();
            Logger = l;
            Tree = tree;
            MaxDegree = tree.MaxDegree;
        }

        public void Draw(BPlusTreeNode marked)
        {
            Logger.Log(ToLaTeX(Tree.Root, marked));
        }

        public void Add(int i)
        {
            Logger.Log($"Přidání klíče {i}\n\n");
        }
        public void CreateRoot(int i)
        {
            Logger.Log($"Vytvoří se nový kořen s klíčem {i}\n\n");
        }

        public void AddToLeaf(int i, bool alreadyContains, bool mustSplit)
        {
            Logger.Log($"Nalezne se odpovídající list{(alreadyContains ? $", ten již {i} obsahuje" : $" a vloží se klíč {i}")}");
            Logger.Log($"{ (mustSplit ? ".Uzel je přeplněný, je třeba Split" : "") }");
            Logger.Log("\n\n");
        }

        public void SplitCreatedNewRoot(bool isLeaf)
        {
            Logger.Log("Splitem byl vytvořen nový kořen" + (isLeaf ? ", nejmenší hodnota z pravé poloviny je do něj nakopírována" : "") + "\n\n");
        }

        public void Split(bool mustSplit, bool isLeaf)
        {
            Logger.Log("Uzel se rozdělil" + (isLeaf ? ", nejmenší hodnota z pravé poloviny je nakopírována do rodiče" : "") + (mustSplit ? ", označený uzel je přeplněný a musí se opět rozdělit" : "") + "\n\n");
        }

        string ToLaTeX(BPlusTreeNode root, BPlusTreeNode marked)
        {
            if (root == null)
                return "";
            Marked = marked;
            int maxPartWidth = root.MaxPartWidth;
            string levels = Levels(root);
            string positionStyles = PositionStyles();
            LeafList.Clear();
            string nodes = NodeStructure(root);
            string links = LeafLinks();
            string data = Data();
            string pointers = Pointers();
            string res = @"\begin{figure}[H]
\centering
\begin{scaletikzpicturemaxtowidth}{\textwidth}
\begin{tikzpicture} [
    % scaling
    scale=\tikzscale,
    every node/.style={scale=\tikzscale, text width=" + DigitWidth.ToString("0.00000", System.Globalization.CultureInfo.InvariantCulture) + "cm * " + maxPartWidth + @", align=center, rectangle split,  rectangle split parts = 20, rectangle split horizontal,rectangle split ignore empty parts,draw},
    every edge/.style={->,scale=\tikzscale},
    % level styling
    % for each level, the approximate maximum width of the node was calculated and the distance between siblings was set accordingly" + Environment.NewLine +
levels + @"% styles for edge positions" + Environment.NewLine + positionStyles + @"% marked style
    marked/.style= { blue }
    ]" + Environment.NewLine + nodes + Environment.NewLine + links + @"
\node(data) " + data + @";
        " + Environment.NewLine + pointers + @"
\end{tikzpicture}
\end{scaletikzpicturemaxtowidth}
\end{figure}

";
            Marked = null;
            return res;
        }

        int LevelCount()
        {
            int count = 0;
            BPlusTreeNode curr = Tree.Root;
            while (curr != null)
            {
                count++;
                curr = curr.Children[0];
            }
            return count;
        }

        string LeafLinks()
        {
            string res = "";
            for (int i = 0; i < LeafList.Count() - 1; i++)
                res += $"\\draw[->, dashed](l{i})--(l{i + 1});\n";
            return res;
        }

        string Data()
        {
            BPlusTreeNode DataNode = new BPlusTreeNode();
            foreach (var leaf in LeafList)
            {
                foreach (int i in leaf.Content)
                {
                    DataNode.Add(i);
                }
            }
            return NodeContentToString(DataNode);
        }

        string Pointers()
        {
            string res = "";
            int dataIndex = 0;
            for (int leafIndex = 0; leafIndex < LeafList.Count(); leafIndex++)
            {
                var leaf = LeafList[leafIndex];
                for (int partIndex = 0; partIndex < leaf.Content.Count(); partIndex++)
                {
                    res += $"\\draw[->, dotted](l{leafIndex}.{IntText[partIndex]} south)--(data.{IntText[dataIndex]} north);\n";
                    dataIndex++;
                }
            }
            return res;
        }

        string Levels(BPlusTreeNode root)
        {
            List<double> distances = CountDistances(root);
            string res = "";
            int i = 1;
            foreach (float d in distances)
            {
                res += "    level " + i + "/.style = { sibling distance = " + distances[i - 1].ToString("0.00000", System.Globalization.CultureInfo.InvariantCulture) + "cm },\n";
                i++;
            }
            return res;
        }

        List<double> CountDistances(BPlusTreeNode root)
        {
            if (root.IsLeaf)
                return new List<double>();
            Stack<int> maxLevelDegrees = new Stack<int>(MaxLevelDegrees(root));
            List<double> revRes = new List<double>();
            revRes.Add(maxLevelDegrees.Pop() * (root.MaxPartWidth + 2) * DigitWidth);
            while (maxLevelDegrees.Any())
                revRes.Add(revRes.Last() * (maxLevelDegrees.Pop() + 1));
            revRes.Reverse();
            return revRes;
        }


        List<int> MaxLevelDegrees(BPlusTreeNode root)
        {
            List<int> res = new List<int>();
            Queue<BPlusTreeNode> q = new Queue<BPlusTreeNode>();
            q.Enqueue(root);
            root.Depth = 0;
            while (q.Any())
            {
                BPlusTreeNode n = q.Dequeue();
                if (n.IsLeaf)
                    continue;
                foreach (BPlusTreeNode child in n.Children)
                {
                    child.Depth = n.Depth + 1;
                    q.Enqueue(child);
                    if (res.Count < child.Depth)
                        res.Add(0);
                    if (res[child.Depth - 1] < child.Degree)
                        res[child.Depth - 1] = child.Degree;
                }
            }
            return res;
        }

        string PositionStyles()
        {
            string res = "";
            for (int i = 1; i <= MaxDegree + 2; i++)
            {
                res += "    " + NumberToSerial(i) +
                "/.style = { edge from parent path={(\\tikzparentnode." +
                (i == 1 ? "south west" : IntText[i - 2] + " split south") +
                ")" + (CurvedArrows ? " .. controls +(0,-1) and +(0,1) .. " : "->") + "(\\tikzchildnode.north)}},\n";
            }
            return res;
        }

        string NumberToSerial(int i)
        {
            return "" + i + (i < 3 ? (i == 1 ? "st" : "nd") : (i == 3 ? "rd" : "th"));
        }

        string NodeContentToString(BPlusTreeNode n)
        {
            string res = "{";
            res += n.Content[0];
            for (int i = 1; i < n.Content.Count; i++)
            {
                res += " \\nodepart{" + IntText[i] + "} " + n.Content[i];
            }
            return res + "}";
        }

        string NodeStructure(BPlusTreeNode root)
        {
            string indent = "        ";
            string res = indent + "\\node" + (root.IsLeaf ? " (l0)" : "") + $" at(0, {(LevelCount() * 1.3 + 1).ToString(CultureInfo.InvariantCulture)}) " + (root == Marked ? "[marked]" : "") + NodeContentToString(root) + Environment.NewLine;
            if (!root.IsLeaf)
                for (int i = 0; i < root.Children.Count; i++)
                    res += NodeStructure(root.Children[i], i, indent + "        ");
            if (root.IsLeaf)
                LeafList.Add(root);
            return res + "        ;";
        }

        string NodeStructure(BPlusTreeNode n, int i, string indent)
        {
            string res = indent + "child[" + NumberToSerial(i + 1) + "] { node" + (n.IsLeaf ? $"(l{LeafList.Count()})" : "") + (n == Marked ? "[marked]" : "") + " " + NodeContentToString(n) + Environment.NewLine;
            if (n.IsLeaf)
                LeafList.Add(n);
            if (!n.IsLeaf)
                for (int j = 0; j < n.Children.Count; j++)
                    res += NodeStructure(n.Children[j], j, indent + "        ");
            res += indent + "        " + "edge from parent [-Triangle[scale=2]]}\n";
            return res;
        }
    }
}
