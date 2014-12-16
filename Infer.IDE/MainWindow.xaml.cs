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
using System.Diagnostics;
using System.Reactive.Linq;
using System.Linq;


namespace Infer.IDE
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly RefreshThread refreshThreadObject;
        private Thread previousThread;

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

            foreach(string a in Strings.assemblies)                
                fsiSession.EvalInteraction(a);

            InitializeComponent();

            Cover.Visibility = Visibility.Hidden;
            LoadCodeBox.SelectedIndex = 0;
            WriteBox.Text = Strings.sprinkler;

            refreshThreadObject = new RefreshThread(ReadBox, Cover, Charts, ProgressBar, viewModel, fsiSession);

            var textchanges = Observable.FromEventPattern<TextChangedEventHandler, TextChangedEventArgs>(
                h => WriteBox.TextChanged += h,
                h => WriteBox.TextChanged -= h
                ).Select(x => ((TextBox)x.Sender).Text)
                 .Throttle(TimeSpan.FromMilliseconds(500))
                 .ObserveOnDispatcher()
                 .Subscribe(OnUserChange);
        }
        
        private void OnUserChange(string s)
        {
            if (refreshThreadObject == null) return;

            if (!Monitor.TryEnter(refreshThreadObject))
            {
                previousThread.Abort();

                Monitor.Enter(refreshThreadObject);

                refreshThreadObject.CodeString = WriteBox.Text;
                refreshThreadObject.Id++;

                previousThread = new Thread(new ThreadStart(refreshThreadObject.run));
                // The calling thread must be STA(Single-Threaded Apartment), 
                // because many UI components require this.
                previousThread.SetApartmentState(ApartmentState.STA);

                previousThread.Start();

                Monitor.Exit(refreshThreadObject);
            }
            else
            {
                refreshThreadObject.CodeString = WriteBox.Text;
                refreshThreadObject.Id++;

                previousThread = new Thread(new ThreadStart(refreshThreadObject.run));
                // The calling thread must be STA(Single-Threaded Apartment), 
                // because many UI components require this.
                previousThread.SetApartmentState(ApartmentState.STA);

                previousThread.Start();
                Monitor.Exit(refreshThreadObject);
            }                    

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Console.WriteLine("Text changed");
        }

        private void Compile_Click(object sender, RoutedEventArgs e)
        {
            OnUserChange(""); 
            //refreshThreadObject.click();           
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selection = LoadCodeBox.SelectedIndex;

            switch (selection)
            {
                case 0:
                    WriteBox.Text = Strings.sprinkler;
                    break;
                case 1:
                    WriteBox.Text = Strings.allDistributions;
                    break;
                case 2:
                    WriteBox.Text = Strings.learningGaussian;
                    break;
                case 3:
                    WriteBox.Text = Strings.truncGaussian;
                    break;
                case 4:
                    WriteBox.Text = Strings.twoCoins;
                    break;
                case 5:
                    WriteBox.Text = Strings.mixGaussians;
                    break;
            }

        }

        private void ProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

    }
}
