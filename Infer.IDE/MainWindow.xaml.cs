using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Threading;

using System.Windows.Forms.Integration;

using System.IO;

using Microsoft.FSharp.Compiler.Interactive;
using Microsoft.FSharp.Compiler;
using Microsoft.FSharp.Core;
using Microsoft.FSharp.Collections;
using System.Reactive.Linq;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Rendering;
using Microsoft.Win32;
using Backend;


namespace Infer.IDE
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static Thread currentExecutingThread;

        private readonly RefreshThread refreshThreadObject;
        private readonly Object lockRefreshThread = new Object();
        private Thread previousThread;

        private ViewModel viewModel;
        private string path = System.IO.Directory.GetCurrentDirectory() + "\\tmp.fsx";
        private Shell.FsiEvaluationSession fsiSession;

        private StringReader inStream;
        private StringWriter outStream;
        private StringWriter errStream;

        private LineColorizer highlighted;

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

            fsiSession.EvalInteraction("let ie" + " = new InferenceEngine()");

            InitializeComponent();

            #region Initialise TextEditor details
            var Options = new TextEditorOptions();
            Options.ConvertTabsToSpaces = true;
            WriteBox.Options = Options;
            WriteBox.SyntaxHighlighting = ResourceLoader.LoadHighlightingDefinition("FSharp.xshd");

            TextView textView = WriteBox.TextArea.TextView;
            #endregion

            Cover.Visibility = Visibility.Hidden;
            WriteBox.Text = "";

            refreshThreadObject = new RefreshThread(WriteBox, ReadBox, Cover, Charts, ProgressBar, StatusString, viewModel, fsiSession);

            var textchanges = Observable.FromEventPattern<EventHandler, EventArgs>(
                h => WriteBox.TextChanged += h,
                h => WriteBox.TextChanged -= h
                ).Select(x => ((TextEditor)x.Sender).Text)
                 .Throttle(TimeSpan.FromMilliseconds(500))
                 .ObserveOnDispatcher()
                 .Subscribe(Recompile);
        }
        
        private void Recompile(string s)
        {
            ProgressBar.Visibility = Visibility.Visible;
            lock(lockRefreshThread)
            {

                if (refreshThreadObject == null) return;

                refreshThreadObject.CodeString = WriteBox.Text;
                refreshThreadObject.Id++;

                previousThread = new Thread(new ThreadStart(refreshThreadObject.run));
                // The calling thread must be STA(Single-Threaded Apartment), 
                // because many UI components require this.
                previousThread.SetApartmentState(ApartmentState.STA);

                previousThread.Start();
            }
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

        private void TextBlock_MouseEnter(object sender, MouseEventArgs e)
        {
            TextBlock block = (TextBlock)sender;
            ModelVertex vertex = (ModelVertex) block.DataContext;

            Console.WriteLine("Defined on line {0}", vertex.Location);

            try
            {
                highlighted = new LineColorizer(vertex.Location);
                WriteBox.TextArea.TextView.LineTransformers.Add(highlighted);
            }
            catch (ArgumentOutOfRangeException) { Console.WriteLine("temp var not declared in the code"); }

        }

        private void scrollSmoothly(int from, int to)
        {
            if (to >= from)
            {
                for (int i = from; i < to; i += 10)
                {
                    if(i>Scroll.ScrollableHeight) break;
                    Console.Write("scrolling");                                                                                                                                                                                                                                                                                             
                    Scroll.ScrollToVerticalOffset((double)(i));
                }
            }
            else
            {
                for (int i = from; i > to; i -= 10)
                {
                    Scroll.ScrollToVerticalOffset((double)(i));
                }
            }
        }

        private void TextBlock_MouseLeave(object sender, MouseEventArgs e)
        {
            WriteBox.TextArea.TextView.LineTransformers.Remove(highlighted);
        }
    }
}
