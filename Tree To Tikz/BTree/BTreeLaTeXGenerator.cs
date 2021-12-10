using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tree_To_Tikz
{
    class BTreeLaTeXGenerator
    {
        Logger Logger { get; set; }
        double DigitWidth { get; } = 0.194;
        int MaxDegree { get; set; }
        bool CurvedArrows { get; set; } = false;
        string[] IntText { get; } = new string[] { "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fiveteen", "sixteen", "seventeen", "eighteen", "nineteen", "twenty" };
        BTreeNode Marked { get; set; } = null;
        BTree Tree { get; set; }

        public BTreeLaTeXGenerator(Logger l, BTree tree)
        {
            Logger = l;
            Tree = tree;
            MaxDegree = tree.MaxDegree;
        }

        public void Draw(BTreeNode marked)
        {
            Logger.Log(ToLaTeX(Tree.Root, marked));
        }

        public void Add(int i)
        {
            Logger.Log($"Přidání klíče {i}\n\n");
        }

        public void Remove(int i)
        {
            Logger.Log($"Odebrání klíče {i}\n\n");
        }

        public void EmptyCantRemove(int i)
        {
            Logger.Log($"Strom je prázdný, nelze odebrat {i}\n\n");
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

        public void RemoveFromLeaf(int i, bool contains)
        {
            Logger.Log($"Označený uzel je list{(contains ? $", odstraníme klíč {i}" : $" a klíč {i} neobsahuje")}\n\n");
        }

        public void SplitCreatedNewRoot()
        {
            Logger.Log("Splitem byl vytvořen nový kořen\n\n");
        }

        public void Split(bool mustSplit)
        {
            Logger.Log("Uzel se rozdělil" + (mustSplit ? ", označený uzel je přeplněný a musí se opět rozdělit" : "") + "\n\n");
        }

        public void Merge(bool right, int i)
        {
            Logger.Log($"Všichni sourozenci potomka, který by měl obsahovat klíč {i}, mají minimální stupeň.\n\n" + "Slijeme potomka s " + (right ? "pravým" : "levým") + "sourozencem\n\n");
        }

        public void PreventiveSplitNeeded()
        {
            Logger.Log("Uzel má maximální stupeň, preventivně se rozdělí.");
        }

        public void MoreThanMinDegree()
        {
            Logger.Log("Označený uzel má více než minimum klíčů\n\n");
        }

        public void MustEnforceMoreThanMin()
        {
            Logger.Log("Označený uzel má minimum klíčů\n\n");
        }

        public void EnforceLeftHasMore(int i)
        {
            Logger.Log($"Levý sourozenec potomka, který by měl obsahovat klíč {i} má více než minimální stupeň, provedeme rotaci\n\n");
        }

        public void EnforceRightHasMore(int i)
        {
            Logger.Log($"Pravý sourozenec potomka, který by měl obsahovat klíč {i} má více než minimální stupeň, provedeme rotaci\n\n");
        }

        public void ReplaceLeftmost(int i)
        {
            Logger.Log($"Potomek následující odstraňovanému klíči má více než minimální stupeň. Nahradíme {i} nejmenším prvkem z tohoto podstromu. Z podstromu jej potom rekurzivně odstraníme\n\n");
        }

        public void ReplaceRightmost(int i)
        {
            Logger.Log($"Potomek předcházející odstraňovanému klíči má více než minimální stupeň. Nahradíme {i} největším prvkem z tohoto podstromu. Z podstromu jej potom rekurzivně odstraníme\n\n");
        }

        public void ReplaceMerge(int i)
        {
            Logger.Log($"slejeme potomky předcházející a následující klíči {i}. z nově vzniklého potomka {i} rekurzivně odstraníme\n\n");
        }

        string ToLaTeX(BTreeNode root, BTreeNode marked)
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
levels + @"% styles for edge positions" + Environment.NewLine + positionStyles + @"% marked style
    marked/.style= { blue }
    ]" + Environment.NewLine + nodes + @"
\end{tikzpicture}
\end{scaletikzpicturemaxtowidth}
\end{figure}

";
            Marked = null;
            return res;
        }

        string Levels(BTreeNode root)
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

        List<double> CountDistances(BTreeNode root)
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


        List<int> MaxLevelDegrees(BTreeNode root)
        {
            List<int> res = new List<int>();
            Queue<BTreeNode> q = new Queue<BTreeNode>();
            q.Enqueue(root);
            root.Depth = 0;
            while (q.Any())
            {
                BTreeNode n = q.Dequeue();
                if (n.IsLeaf)
                    continue;
                foreach (BTreeNode child in n.Children)
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

        string NodeContentToString(BTreeNode n)
        {
            string res = "{";
            res += n.Content[0];
            for (int i = 1; i < n.Content.Count; i++)
            {
                res += " \\nodepart{" + IntText[i] + "} " + n.Content[i];
            }
            return res + "}";
        }

        string NodeStructure(BTreeNode root)
        {
            string indent = "        ";
            string res = indent + "\\node" + (root == Marked ? "[marked]" : "") + NodeContentToString(root) + Environment.NewLine;
            if (!root.IsLeaf)
                for (int i = 0; i < root.Children.Count; i++)
                    res += NodeStructure(root.Children[i], i, indent + "        ");
            return res + "        ;";
        }

        string NodeStructure(BTreeNode n, int i, string indent)
        {
            string res = indent + "child[" + NumberToSerial(i + 1) + "] { node" + (n == Marked ? "[marked]" : "") + " " + NodeContentToString(n) + Environment.NewLine;
            if (!n.IsLeaf)
                for (int j = 0; j < n.Children.Count; j++)
                    res += NodeStructure(n.Children[j], j, indent + "        ");
            res += indent + "        " + "edge from parent [-Triangle[scale=2]]}\n";
            return res;
        }
    }
}
