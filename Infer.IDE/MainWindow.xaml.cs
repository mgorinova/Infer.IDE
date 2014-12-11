using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;

using System.Windows.Forms.Integration;

using System.IO;

using Microsoft.FSharp.Compiler.Interactive;
using Microsoft.FSharp.Compiler;
using Microsoft.FSharp.Core;
using Microsoft.FSharp.Collections;


namespace Infer.IDE
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ViewModel viewModel;
        private int cnt = 0;
        private string [] paths = {"TwoCoins.dgml", "Sprinkler-Mine.dgml", "Sprinkler.dgml"};
        private string path = System.IO.Directory.GetCurrentDirectory() + "\\tmp.fsx";
        private Shell.FsiEvaluationSession fsiSession;

        private StringReader inStream;
        private StringWriter outStream;
        private StringWriter errStream;
        public MainWindow()
        {
            viewModel = new ViewModel();
            this.DataContext = viewModel;

            System.Text.StringBuilder sbOut = new System.Text.StringBuilder();
            System.Text.StringBuilder sbErr = new System.Text.StringBuilder();

            inStream = new StringReader("");
            outStream = new StringWriter(sbOut);
            errStream = new StringWriter(sbErr);

            string[] txt = { "fsi.exe " + "--noninteractive" };

            Shell.FsiEvaluationSessionHostConfig fsiConfig = Shell.FsiEvaluationSession.GetDefaultConfiguration();
            fsiSession = Shell.FsiEvaluationSession.Create(fsiConfig, txt, inStream, outStream, errStream, FSharpOption<bool>.Some(true));

            foreach(string a in CompilerStrings.assemblies)                
                fsiSession.EvalInteraction(a);
                

            InitializeComponent();

            Cover.Visibility = Visibility.Hidden;
            LoadCodeBox.SelectedIndex = 0;
            WriteBox.Text = CompilerStrings.sprinkler;

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {            
            Console.WriteLine(paths[0]);
            //viewModel.ReLayoutGraph(paths[cnt++]);
            if (cnt == 3) cnt = 0;
        }

        private void Compile_Click(object sender, RoutedEventArgs e)
        {
            var rt = new RefreshThread(path, WriteBox.Text, ReadBox, Cover, Charts, ProgressBar, viewModel, fsiSession);

            var refreshThread = new Thread(new ThreadStart(rt.run));
            // The calling thread must be STA(Single-Threaded Apartment), 
            // because many UI components require this.
            refreshThread.SetApartmentState(ApartmentState.STA);            
            
            refreshThread.Start();
            
            
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selection = LoadCodeBox.SelectedIndex;

            switch (selection)
            {
                case 0:
                    WriteBox.Text = CompilerStrings.sprinkler;
                    break;
                case 1:
                    WriteBox.Text = CompilerStrings.allDistributions;
                    break;
                case 2:
                    WriteBox.Text = CompilerStrings.learningGaussian;
                    break;
                case 3:
                    WriteBox.Text = CompilerStrings.truncGaussian;
                    break;
                case 4:
                    WriteBox.Text = CompilerStrings.twoCoins;
                    break;
                case 5:
                    WriteBox.Text = CompilerStrings.mixGaussians;
                    break;
            }

        }

        private void ProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

    }
}
