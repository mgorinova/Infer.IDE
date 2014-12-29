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
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System.Xml;
using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.AvalonEdit.Document;
using Microsoft.Win32;
using Backend;


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

        private readonly TextMarkerService textMarkerService;

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

            #region Initialise TextEditor details
            var Options = new TextEditorOptions();
            Options.ConvertTabsToSpaces = true;
            WriteBox.Options = Options;
            WriteBox.SyntaxHighlighting = ResourceLoader.LoadHighlightingDefinition("FSharp.xshd");

            textMarkerService = new TextMarkerService(WriteBox);
            TextView textView = WriteBox.TextArea.TextView;
            textView.BackgroundRenderers.Add(textMarkerService);
            textView.LineTransformers.Add(textMarkerService);
            textView.Services.AddService(typeof(TextMarkerService), textMarkerService);
            #endregion

            Cover.Visibility = Visibility.Hidden;
            LoadCodeBox.SelectedIndex = 0;
            WriteBox.Text = Strings.sprinkler;

            //int offset = WriteBox.Document.GetOffset(new TextLocation(3, 20));
            //textMarkerService.Create(offset, 20, "much wow");

            refreshThreadObject = new RefreshThread(WriteBox, ReadBox, Cover, Charts, ProgressBar, viewModel, fsiSession);

            var textchanges = Observable.FromEventPattern<EventHandler, EventArgs>(
                h => WriteBox.TextChanged += h,
                h => WriteBox.TextChanged -= h
                ).Select(x => ((TextEditor)x.Sender).Text)
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

        private void OnTextChanged(object sender, System.EventArgs e)
        {
            Console.WriteLine("Text Changed");
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            WriteBox.Text = Strings.namescapses;
        }
        private void Open_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.FileName = "Document";
            dialog.DefaultExt = ".fsx";
            dialog.Filter = "F# Script Files (.fsx)|*.fsx";

            Nullable<bool> result = dialog.ShowDialog();

            if (result == true) WriteBox.Text = File.ReadAllText(dialog.FileName);
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog();
            dialog.FileName = "Document";
            dialog.DefaultExt = ".fsx";
            dialog.Filter = "F# Script Files (.fsx)|*.fsx";

            Nullable<bool> result = dialog.ShowDialog();

            if (result == true) File.WriteAllText(dialog.FileName, WriteBox.Text);
        }
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            Expander expander = (Expander)sender;
            ModelVertex node = (ModelVertex)expander.Content;

            Console.WriteLine("label {0}, distribution {1}", node.Label, node.Distribution);

            WindowsFormsHost wfh = new WindowsFormsHost();
            wfh.Height = 100.0;

            node.WinHost = wfh;

            //Charts.Children.Add(wfh);
            Distributions.draw(wfh, node.Distribution, node.Label);

            expander.Header = null;

            viewModel.ReLayoutGraph();
            
        }


    }
}
