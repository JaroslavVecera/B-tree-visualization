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

        public MainWindow()
        {
            InitializeComponent();
            Timer = new Timer(400);
            Timer.AutoReset = false;
            Timer.Elapsed += (o, e) => Dispatcher.InvokeAsync(GenerateOutput);
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

        void GenerateOutput()
        {
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
                degreeSlider.Value = degreeSlider.Minimum;
                degreeType.Text = "Minimal degree";
                TreeType = typeof(BTree);
            }
            else if (selection.SelectedIndex == 1)
            {
                degreeSlider.Minimum = 3;
                degreeSlider.Maximum = 8;
                degreeSlider.Value = degreeSlider.Minimum;
                degreeType.Text = "Maximal degree";
                TreeType = typeof(BPlusTree);
            }
            else
            {
                degreeSlider.Minimum = 3;
                degreeSlider.Maximum = 8;
                degreeSlider.Value = degreeSlider.Minimum;
                degreeType.Text = "Maximal degree";
                TreeType = typeof(BStarTree);
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
\NewEnviron{scaletikzpicturetowidth}[1]{%
  \def\tikz@width{#1}%
  \def\tikzscale{1}\begin{lrbox}{\measure@tikzpicture}%
  \BODY
  \end{lrbox}%
  \pgfmathparse{min(4, #1/\wd\measure@tikzpicture)}%
  \edef\tikzscale{\pgfmathresult}%
  \BODY
}
\makeatother";
        }
    }
}
