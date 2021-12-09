using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tree_To_Tikz
{
    class RTreeLaTeXGenerator
    {
        Logger Logger { get; set; }
        double DigitWidth { get; } = 0.194;
        int MaxDegree { get; set; }
        bool CurvedArrows { get; set; } = false;
        string[] IntText { get; } = new string[] { "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fiveteen", "sixteen", "seventeen", "eighteen", "nineteen", "twenty" };
        RTreeNode Marked { get; set; } = null;
        RTree Tree { get; set; }

        public RTreeLaTeXGenerator(Logger l, RTree tree)
        {
            Logger = l;
            Tree = tree;
            MaxDegree = tree.MaxDegree;
        }

        public void Draw(RTreeNode marked)
        {
            Logger.Log(ToLaTeX(Tree.Root, marked));
        }

        public void Add(IndexRecord r)
        {
            Logger.Log($"Přidání klíče s MBR {r}\n\n");
        }

        public void CreateRoot(IndexRecord r)
        {
            Logger.Log($"Vytvoří se nový kořen s klíčem {r}\n\n");
        }

        public void AddToLeaf(IndexRecord r, bool mustSplit)
        {
            Logger.Log($"Nalezne se odpovídající list a vloží se klíč {r}");
            Logger.Log($"{ (mustSplit ? ". Uzel je přeplněný, je třeba Split" : "") }");
            Logger.Log("\n\n");
        }

        public void AdjustTree(bool splitBefore, bool mustSplit)
        {
            Logger.Log($"Označený uzel si upraví MBR");
            if (splitBefore)
            {
                if (mustSplit)
                    Logger.Log(", přidáním uzlu z předchozího splitu by došlo k přeplnění, je třeba split");
                else
                    Logger.Log(", přidá se uzel z předchozího splitu");
            }
            Logger.Log("\n\n");
        }

        public void SplitCreatedNewRoot()
        {
            Logger.Log("Splitem byl vytvořen nový kořen\n\n");
        }

        public void Split(bool mustSplit)
        {
            Logger.Log("Uzel se rozdělil" + (mustSplit ? ", označený uzel je přeplněný a musí se opět rozdělit" : "") + "\n\n");
        }

        string ToLaTeX(RTreeNode root, RTreeNode marked)
        {
            if (root == null)
                return "";
            Marked = marked;
            string levels = Levels(root);
            int maxPartWidth = root.MaxPartWidth;
            string positionStyles = PositionStyles();
            string nodes = NodeStructure(root);
            string plane = Plane(root);
            string areas = Areas(root);
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

\begin{figure}[H]
\centering
\begin{scaletikzpicturetowidth}{\textwidth}
\begin{tikzpicture} [
    % scaling
    scale=\tikzscale,
    every rectangle/.style={scale=\tikzscale},
    every edge/.style={->,scale=\tikzscale}
    ]
" + plane + Environment.NewLine + areas + Environment.NewLine + @"

\end{tikzpicture}
\end{scaletikzpicturetowidth}
\end{figure}
";
            Marked = null;
            return res;
        }

        string Plane(RTreeNode root)
        {
            Rectangle rootR = root.MinimalBoundedRectangle;
            double maxSide = Math.Max(rootR.Right - rootR.Left, rootR.Top - rootR.Bottom);
            string right = DoubleToString(rootR.Left + maxSide);
            string top = DoubleToString(rootR.Bottom + maxSide);
            return @"\draw[draw=black, dotted] (" + DoubleToString(rootR.Left) + ", " + DoubleToString(rootR.Bottom) + ") rectangle (" + right + ", " + top + ");\n";
        }

        string DoubleToString(double d)
        {
            return d.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        string Areas(RTreeNode root)
        {
            string res = "";
            Queue<RTreeNode> q = new Queue<RTreeNode>();
            q.Enqueue(root);
            while (q.Any())
            {
                RTreeNode n = q.Dequeue();
                if (n.IsLeaf)
                {
                    var records = n.IndexRecords;
                    var names = n.ChildNames;
                    records.Zip(names).ToList().ForEach(tuple => res += RecordToString(tuple.Item1, tuple.Item2, true));
                    continue;
                }
                else
                {
                    var records = n.InnerRecords;
                    var names = n.ChildNames;
                    records.Zip(names).ToList().ForEach(tuple => res += RecordToString(tuple.Item1, tuple.Item2, false));
                }
                foreach (RTreeNode child in n.InnerRecords.Select(r => r.Node))
                    q.Enqueue(child);
            }
            return res;
        }

        string RecordToString(Record r, string name, bool isLeaf)
        {
            return @"\draw[draw=black" + (isLeaf ? "" : ", dashed") + "] (" + DoubleToString(r.MBR.Left) + ", " + DoubleToString(r.MBR.Bottom) + ") rectangle (" + DoubleToString(r.MBR.Right) + ", " + DoubleToString(r.MBR.Top) + ") node[pos=0.5] {" + name + "};";
        }

        string Levels(RTreeNode root)
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

        List<double> CountDistances(RTreeNode root)
        {
            Stack<int> maxLevelDegrees = new Stack<int>(MaxLevelDegrees(root));
            List<double> revRes = new List<double>();
            if (root.IsLeaf)
                return new List<double>();
            revRes.Add(maxLevelDegrees.Pop() * (root.MaxPartWidth + 2) * DigitWidth);
            while (maxLevelDegrees.Any())
                revRes.Add(revRes.Last() * (maxLevelDegrees.Pop() + 1));
            revRes.Reverse();
            return revRes;
        }


        List<int> MaxLevelDegrees(RTreeNode root)
        {
            List<int> res = new List<int>();
            Queue<RTreeNode> q = new Queue<RTreeNode>();
            q.Enqueue(root);
            root.Depth = 0;
            int nameIndex = 1;
            while (q.Any())
            {
                RTreeNode n = q.Dequeue();
                n.ChildNames.Clear();
                for (int i = 0; i < n.Degree; i++)
                {
                    n.ChildNames.Add($"R{nameIndex++}");
                }
                if (n.IsLeaf)
                    continue;
                foreach (RTreeNode child in n.InnerRecords.Select(r => r.Node))
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
                IntText[i - 1] + " south" +
                ")" + (CurvedArrows ? " .. controls +(0,-1) and +(0,1) .. " : "->") + "(\\tikzchildnode.north)}},\n";
            }
            return res;
        }

        string NumberToSerial(int i)
        {
            return "" + i + (i < 3 ? (i == 1 ? "st" : "nd") : (i == 3 ? "rd" : "th"));
        }

        string NodeContentToString(RTreeNode n)
        {
            string res = "{";
            res += n.ChildNames[0];
            for (int i = 1; i < n.ChildNames.Count; i++)
            {
                res += " \\nodepart{" + IntText[i] + "} " + n.ChildNames[i];
            }
            return res + "}";
        }

        string NodeStructure(RTreeNode root)
        {
            string indent = "        ";
            string res = indent + "\\node" + (root == Marked ? "[marked]" : "") + NodeContentToString(root) + Environment.NewLine;
            if (!root.IsLeaf)
            {
                var children = root.InnerRecords.Select(r => r.Node).ToList();
                for (int i = 0; i < children.Count(); i++)
                    res += NodeStructure(children[i], i, indent + "        ");
            }
            return res + "        ;";
        }

        string NodeStructure(RTreeNode n, int i, string indent)
        {
            //string res = indent + "child[" + NumberToSerial(i + 1) + "] { node" + (n.IsList ? "[list]" : "") + " " + NodeContentToString(n) + Environment.NewLine;string res = indent + "child[" + NumberToSerial(i + 1) + "] { node" + (n.IsList ? "[list]" : "") + " " + NodeContentToString(n) + Environment.NewLine;
            string res = indent + "child[" + NumberToSerial(i + 1) + "] { node" + (n == Marked ? "[marked]" : "") + " " + NodeContentToString(n) + Environment.NewLine;
            if (!n.IsLeaf)
            {
                var children = n.InnerRecords.Select(r => r.Node).ToList();
                for (int j = 0; j < children.Count; j++)
                    res += NodeStructure(children[j], j, indent + "        ");
            }
            res += indent + "        " + "edge from parent [-Triangle[scale=2]]}\n";
            return res;
        }
    }
}
