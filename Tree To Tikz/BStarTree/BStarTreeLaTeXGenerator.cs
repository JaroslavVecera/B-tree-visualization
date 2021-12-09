using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tree_To_Tikz
{
    class BStarTreeLaTeXGenerator
    {
        Logger Logger { get; set; }
        double DigitWidth { get; } = 0.194;
        int MaxDegree { get; set; }
        bool CurvedArrows { get; set; } = false;
        string[] IntText { get; } = new string[] { "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fiveteen", "sixteen", "seventeen", "eighteen", "nineteen", "twenty" };
        BStarTreeNode Marked { get; set; } = null;
        BStarTree Tree { get; set; }

        public BStarTreeLaTeXGenerator(Logger l, BStarTree tree)
        {
            Logger = l;
            Tree = tree;
            MaxDegree = tree.MaxDegree;
        }

        public void Draw(BStarTreeNode marked)
        {
            Logger.Log(ToLaTeX(Tree.Root, marked));
        }

        public void Add(int i)
        {
            Logger.Log($"Přidání klíče {i}\n\n");
        }

        public void Split()
        {
            Logger.Log("Označený uzel je přeplněný.");
        }

        public void CreateRoot(int i)
        {
            Logger.Log($"Vytvoří se nový kořen s klíčem {i}\n\n");
        }

        public void AddToList(int i, bool alreadyContains, bool mustSplit)
        {
            Logger.Log($"Nalezne se odpovídající list{(alreadyContains ? $", ten již {i} obsahuje" : $" a vloží se klíč {i}")}");
            Logger.Log($"{ (mustSplit ? ".Uzel je přeplněný, je třeba Split" : "") }");
            Logger.Log("\n\n");
        }

        public void SplitCreatedNewRoot()
        {
            Logger.Log("Splitem byl vytvořen nový kořen\n\n");
        }

        public void TwoThreeSplitRight()
        {
            Logger.Log("Označený uzel je přeplněný a všichni sourozenci mají maximální stupeň. Provede se rozdělení na tři uzly s pravým sourozencem\n\n");
        }

        public void TwoThreeSplitLeft()
        {
            Logger.Log("Označený uzel je přeplněný a všichni sourozenci mají maximální stupeň. Provede se rozdělení na tři uzly s levým sourozencem\n\n");
        }

        public void RotRight()
        {
            Logger.Log("Označený uzel je přeplněný a pravý sourozenec nemá maximální stupeň. Provedeme rotaci vpravo\n\n");
        }

        public void RotLeft()
        {
            Logger.Log("Označený uzel je přeplněný a levý sourozenec nemá maximální stupeň. Provedeme rotaci vlevo\n\n");
        }

        string ToLaTeX(BStarTreeNode root, BStarTreeNode marked)
        {
            if (root == null)
                return "";
            Marked = marked;
            int maxPartWidth = root.MaxPartWidth;
            string levels = Levels(root);
            string positionStyles = PositionStyles();
            string nodes = NodeStructure(root);
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
levels + @"% styles for edge positions" + Environment.NewLine + positionStyles + @"% list style
    marked/.style= { blue }
    ]" + Environment.NewLine + nodes + @"
\end{tikzpicture}
\end{scaletikzpicturemaxtowidth}
\end{figure}

";
            Marked = null;
            return res;
        }

        string Levels(BStarTreeNode root)
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

        List<double> CountDistances(BStarTreeNode root)
        {
            if (root.IsList)
                return new List<double>();
            Stack<int> maxLevelDegrees = new Stack<int>(MaxLevelDegrees(root));
            List<double> revRes = new List<double>();
            revRes.Add(maxLevelDegrees.Pop() * (root.MaxPartWidth + 2) * DigitWidth);
            while (maxLevelDegrees.Any())
                revRes.Add(revRes.Last() * (maxLevelDegrees.Pop() + 1));
            revRes.Reverse();
            return revRes;
        }


        List<int> MaxLevelDegrees(BStarTreeNode root)
        {
            List<int> res = new List<int>();
            Queue<BStarTreeNode> q = new Queue<BStarTreeNode>();
            q.Enqueue(root);
            root.Depth = 0;
            while (q.Any())
            {
                BStarTreeNode n = q.Dequeue();
                if (n.IsList)
                    continue;
                foreach (BStarTreeNode child in n.Children)
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

        string NodeContentToString(BStarTreeNode n)
        {
            string res = "{";
            res += n.Content[0];
            for (int i = 1; i < n.Content.Count; i++)
            {
                res += " \\nodepart{" + IntText[i] + "} " + n.Content[i];
            }
            return res + "}";
        }

        string NodeStructure(BStarTreeNode root)
        {
            string indent = "        ";
            string res = indent + "\\node" + (root == Marked ? "[marked]" : "") + NodeContentToString(root) + Environment.NewLine;
            if (!root.IsList)
                for (int i = 0; i < root.Children.Count; i++)
                    res += NodeStructure(root.Children[i], i, indent + "        ");
            return res + "        ;";
        }

        string NodeStructure(BStarTreeNode n, int i, string indent)
        {
            //string res = indent + "child[" + NumberToSerial(i + 1) + "] { node" + (n.IsList ? "[list]" : "") + " " + NodeContentToString(n) + Environment.NewLine;string res = indent + "child[" + NumberToSerial(i + 1) + "] { node" + (n.IsList ? "[list]" : "") + " " + NodeContentToString(n) + Environment.NewLine;
            string res = indent + "child[" + NumberToSerial(i + 1) + "] { node" + (n == Marked ? "[marked]" : "") + " " + NodeContentToString(n) + Environment.NewLine;
            if (!n.IsList)
                for (int j = 0; j < n.Children.Count; j++)
                    res += NodeStructure(n.Children[j], j, indent + "        ");
            res += indent + "        " + "edge from parent [-Triangle[scale=2]]}\n";
            return res;
        }
    }
}
