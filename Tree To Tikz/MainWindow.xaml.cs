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
        List<int> Degrees { get { return new List<int>() { (int)degreeSlider.Value, (int)secondaryDegreeSlider.Value }; } }
        bool UsePreventiveSplits { get { return useSplits.IsChecked == true; } }
        bool IgnoreChange { get; set; } = false;
        Timer Timer { get; set; }
        TreeGenerator TreeGenerator { get; set; }

        public MainWindow()
        {
            SetTimer();
            InitializeComponent();
            TreeSelectionChanged(null, null);
        }

        void SetTimer()
        {
            Timer = new Timer(400);
            Timer.AutoReset = false;
            Timer.Elapsed += (o, e) => Dispatcher.InvokeAsync(GenerateOutput);
        }

        private void CopyOutput(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(output.Text);
        }

        private void CopyHead(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(Head());
        }

        private void useSplits_Checked(object sender, RoutedEventArgs e)
        {
            Change();
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Change();
        }

        private void DegreeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
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
            string source = input.Text;
            var s = Regex.Replace(source, @"\s+", "").Split(";");
            Logger l = new Logger();
            TreeGenerator.CreateTree(Degrees, l);

            foreach (string expr in s)
                TreeGenerator.Exec(expr, UsePreventiveSplits);
            output.Text = l.Content;
        }

        private void TreeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (degreeSlider == null)
                return;
            IgnoreChange = true;
            switch (selection.SelectedIndex)
            {
                case 0:
                    SetBTree();
                    break;
                case 1:
                    SetBPlusTree();
                    break;
                case 2:
                    SetBStarTree();
                    break;
                case 3:
                    SetRTree();
                    break;
            }
            if (useSplitsPanel != null)
                useSplitsPanel.Visibility = selection.SelectedIndex != 0 ? Visibility.Collapsed : Visibility.Visible;
            IgnoreChange = false;
            Change();
        }

        void SetBTree()
        {
            degreeSlider.Minimum = 2;
            degreeSlider.Maximum = 5;
            secondarySliderPanel.Visibility = Visibility.Collapsed;
            degreeSlider.Value = degreeSlider.Minimum;
            degreeType.Text = "Minimal degree";
            TreeGenerator = new BTreeGenerator();
        }

        void SetBPlusTree()
        {
            degreeSlider.Minimum = 3;
            degreeSlider.Maximum = 8;
            secondarySliderPanel.Visibility = Visibility.Collapsed;
            degreeSlider.Value = degreeSlider.Minimum;
            degreeType.Text = "Maximal degree";
            TreeGenerator = new BPlusTreeGenerator();
        }

        void SetBStarTree()
        {
            degreeSlider.Minimum = 3;
            degreeSlider.Maximum = 8;
            secondarySliderPanel.Visibility = Visibility.Collapsed;
            degreeSlider.Value = degreeSlider.Minimum;
            degreeType.Text = "Maximal degree";
            TreeGenerator = new BStarTreeGenerator();
        }

        void SetRTree()
        {
            degreeSlider.Minimum = 1;
            degreeSlider.Maximum = 3;
            secondarySliderPanel.Visibility = Visibility.Visible;
            degreeSlider.Value = degreeSlider.Minimum;
            secondaryDegreeSlider.Value = secondaryDegreeSlider.Minimum;
            degreeType.Text = "Minimal degree";
            TreeGenerator = new RTreeGenerator();
        }

        string Head()
        {
            return "\n"
            + @"\usepackage{environ}"                                
            + @"\usepackage{tikz}"
            + @"\usepackage{float}"
            + @"\usetikzlibrary{arrows.meta,shapes,snakes,matrix,shapes}"
            + @"\makeatletter"
            + @"\newsavebox{\measure@tikzpicture}"
            + @"\NewEnviron{scaletikzpicturemaxtowidth}[1]{%"
            + @"  \def\tikz@width{#1}%"
            + @"  \def\tikzscale{1}\begin{lrbox}{\measure@tikzpicture}%"
            + @"  \BODY"
            + @"  \end{lrbox}%"
            + @"  \pgfmathparse{min(4, #1/\wd\measure@tikzpicture)}%"
            + @"  \edef\tikzscale{\pgfmathresult}%"
            + @"  \BODY"
            + @"}"
            + @"\makeatother"
            + @"\makeatletter"
            + @"\newsavebox{\measure@tikzpicture}"
            + @"\NewEnviron{scaletikzpicturetowidth}[1]{%"
            + @"  \def\tikz@width{#1}%"
            + @"  \def\tikzscale{1}\begin{lrbox}{\measure@tikzpicture}%"
            + @"  \BODY"
            + @"  \end{lrbox}%"
            + @"  \pgfmathparse{#1/\wd\measure@tikzpicture}%"
            + @"  \edef\tikzscale{\pgfmathresult}%"
            + @"  \BODY"
            + @"}"
            + @"\makeatother";
        }
    }
}
