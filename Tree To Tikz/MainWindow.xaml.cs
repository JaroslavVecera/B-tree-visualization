using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;

namespace Tree_To_Tikz
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Type TreeType { get; set; } = typeof(BTree);
        int Degree { get { return (int)degreeSlider.Value; } }
        bool IgnoreChange { get; set; } = false;
        Timer Timer { get; }
        DB Database { get; set; } = new DB();

        public MainWindow()
        {
            Timer = new Timer(400);
            Timer.AutoReset = false;
            Timer.Elapsed += (o, e) => Dispatcher.InvokeAsync(GenerateOutput);
            InitializeComponent();
            TreeSelectionChanged(null, null);
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Change();
        }

        private void CopyOutput(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(output.Text);
        }

        private void CopySetting(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(Setting());
        }

        private void useSplits_Checked(object sender, RoutedEventArgs e)
        {
            Change();
        }

        void Change()
        {
            if (IgnoreChange)
                return;
            Timer.Stop();
            Timer.Start();
        }

        void TestR()
        {
            Database.Clear();
            if (input == null)
                return;
            Logger l = new Logger();
            string source = input.Text;
            var s = Regex.Replace(source, @"\s+", "").Split(";");
            RTree t;
            try
            {
                t = new RTree((int)degreeSlider.Value, (int)secondaryDegreeSlider.Value, l);
            }
            catch (Exception e)
            {
                output.Text = "";
                return;
            }
            foreach (string expr in s)
            {
                if (expr == "log")
                {
                    l.IsActive = true;
                }
                else if (expr.StartsWith("add"))
                {
                    string[] parts = expr.Remove(0, 3).Split(",");
                    if (parts.Length == 4)
                    {
                        if (parts[0].StartsWith("[") && parts[2].StartsWith("[") && parts[1].EndsWith("]") && parts[3].EndsWith("]") && parts.ToList().All(p => p.Length >= 2))
                        {
                            double left;
                            double top;
                            double right;
                            double bottom;
                            if (double.TryParse(parts[0].Substring(1, 1), out left)
                                && double.TryParse(parts[1].Substring(0, 1), out right)
                                && double.TryParse(parts[2].Substring(1, 1), out bottom)
                                && double.TryParse(parts[3].Substring(0, 1), out top))
                            {
                                Rectangle r = null;
                                try
                                {
                                    r = new Rectangle(left, top, right, bottom);
                                }
                                catch (Exception e) { }
                                if (r != null)
                                    t.Add(new IndexRecord() { MBR = r, Id = Database.GenerateNew() });
                            }
                        }
                    }
                }
            }
            output.Text = l.Content;
        }

        void GenerateOutput()
        {
            if (TreeType == null)
            {
                TestR();
                return;
            }
            if (input == null)
                return;
            Logger l = new Logger();
            string source = input.Text;
            var s = Regex.Replace(source, @"\s+", "").Split(";");
            Tree t = (Tree)Activator.CreateInstance(TreeType, Degree, l);
            foreach (string expr in s)
            {
                if (expr == "log")
                {
                    l.IsActive = true;
                }
                else if (expr.StartsWith("add"))
                {
                    int n;
                    if (expr.Length > 3 && int.TryParse(expr.Remove(0, 3), out n))
                    {
                        if (TreeType == typeof(BTree) && useSplits.IsChecked == true)
                            ((BTree)t).AddWithPreventiveSplit(n);
                        else
                            t.Add(n);
                    }
                }
                else if (expr.StartsWith("remove"))
                {
                    continue;
                    int n;
                    if (expr.Length > 6 && int.TryParse(expr.Remove(0, 6), out n))
                    {
                        t.Remove(n);
                    }
                }
            }
            output.Text = l.Content;
        }

        private void TreeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (degreeSlider == null)
                return;
            IgnoreChange = true;
            if (selection.SelectedIndex == 0)
            {
                degreeSlider.Minimum = 2;
                degreeSlider.Maximum = 5;
                secondarySliderPanel.Visibility = Visibility.Collapsed;
                degreeSlider.Value = degreeSlider.Minimum;
                degreeType.Text = "Minimal degree";
                TreeType = typeof(BTree);
            }
            else if (selection.SelectedIndex == 1)
            {
                degreeSlider.Minimum = 3;
                degreeSlider.Maximum = 8;
                secondarySliderPanel.Visibility = Visibility.Collapsed;
                degreeSlider.Value = degreeSlider.Minimum;
                degreeType.Text = "Maximal degree";
                TreeType = typeof(BPlusTree);
            }
            else if (selection.SelectedIndex == 2)
            {
                degreeSlider.Minimum = 3;
                degreeSlider.Maximum = 8;
                secondarySliderPanel.Visibility = Visibility.Collapsed;
                degreeSlider.Value = degreeSlider.Minimum;
                degreeType.Text = "Maximal degree";
                TreeType = typeof(BStarTree);
            }
            else
            {
                degreeSlider.Minimum = 1;
                degreeSlider.Maximum = 3;
                secondarySliderPanel.Visibility = Visibility.Visible;
                degreeSlider.Value = degreeSlider.Minimum;
                secondaryDegreeSlider.Value = secondaryDegreeSlider.Minimum;
                degreeType.Text = "Minimal degree";
                TreeType = null;
            }
            if (useSplitsPanel != null)
                useSplitsPanel.Visibility = selection.SelectedIndex != 0 ? Visibility.Collapsed : Visibility.Visible;
            IgnoreChange = false;
            Change();
        }

        private void DegreeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Change();
        }

        string Setting()
        {
            return @"
\usepackage{environ}
\usepackage{tikz}
\usepackage{float}
\usetikzlibrary{arrows.meta,shapes,snakes,matrix,shapes}
\makeatletter
\newsavebox{\measure@tikzpicture}
\NewEnviron{scaletikzpicturemaxtowidth}[1]{%
  \def\tikz@width{#1}%
  \def\tikzscale{1}\begin{lrbox}{\measure@tikzpicture}%
  \BODY
  \end{lrbox}%
  \pgfmathparse{min(4, #1/\wd\measure@tikzpicture)}%
  \edef\tikzscale{\pgfmathresult}%
  \BODY
}
\makeatother
\makeatletter
\newsavebox{\measure@tikzpicture}
\NewEnviron{scaletikzpicturetowidth}[1]{%
  \def\tikz@width{#1}%
  \def\tikzscale{1}\begin{lrbox}{\measure@tikzpicture}%
  \BODY
  \end{lrbox}%
  \pgfmathparse{#1/\wd\measure@tikzpicture}%
  \edef\tikzscale{\pgfmathresult}%
  \BODY
}
\makeatother";
        }
    }
}
