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
using System.Windows.Media.Animation;
using System.Windows.Forms;


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

        private User user;
        private bool visible = true;

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
            WriteBox.Text = Strings.sprinkler;

            //int offset = WriteBox.Document.GetOffset(new TextLocation(3, 20));
            //textMarkerService.Create(offset, 20, "much wow");

            refreshThreadObject = new RefreshThread(WriteBox, ReadBox, Cover, Charts, ProgressBar, StatusString, viewModel, fsiSession);

            var textchanges = Observable.FromEventPattern<EventHandler, EventArgs>(
                h => WriteBox.TextChanged += h, 
                h => WriteBox.TextChanged -= h
                ).Select(x => ((TextEditor)x.Sender).Text)
                 .Throttle(TimeSpan.FromMilliseconds(500))
                 .ObserveOnDispatcher()
                 .Subscribe(OnUserChange);

            InitialPanel.Visibility = Visibility.Visible;
            FurtherInfo.Content = Strings.furtherInfo;
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

        private void ProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void OnTextChanged(object sender, System.EventArgs e)
        {
            if (user != null) user.Keystroke();
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            WriteBox.Text = Strings.namescapses;
        }
        private void Open_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.FileName = "Document";
            dialog.DefaultExt = ".fsx";
            dialog.Filter = "F# Script Files (.fsx)|*.fsx";

            Nullable<bool> result = dialog.ShowDialog();

            if (result == true) WriteBox.Text = File.ReadAllText(dialog.FileName);
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog();
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

        private void TextBlock_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            /*
            TextBlock block = (TextBlock)sender;
            ModelVertex vertex = (ModelVertex) block.DataContext;

            if (vertex.WinHost != null)
            {
                scrollSmoothly((int)Scroll.VerticalOffset ,vertex.HostID*150);  
              
/*              DoubleAnimation verticalAnimation = new DoubleAnimation();

                verticalAnimation.From = Scroll.VerticalOffset;
                verticalAnimation.To = vertex.HostID*150;
                verticalAnimation.Duration = new Duration(new TimeSpan(0,0,1));

                Storyboard storyboard = new Storyboard();

                storyboard.Children.Add(verticalAnimation);
                Storyboard.SetTarget(verticalAnimation, Scroll);
                //Storyboard.SetTargetProperty(verticalAnimation, new PropertyPath(ScrollAnimationBehavior.VerticalOffsetProperty)); // Attached dependency property
                storyboard.Begin();*/

            //}

            //vertex.WinHost.Opacity = 0.0;
                        
            //vertex.WinHost.Margin = new Thickness(5);


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
                    //Thread.Sleep(200);
                }
            }
            else
            {
                for (int i = from; i > to; i -= 10)
                {
                    Scroll.ScrollToVerticalOffset((double)(i));
                    //Thread.Sleep(200);
                }
            }
        }

        private void TextBlock_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
           /* TextBlock block = (TextBlock)sender;
            ModelVertex vertex = (ModelVertex)block.DataContext;

            if (vertex.WinHost != null)
            {
                vertex.WinHost.Visibility = Visibility.Visible;
            }*/
            //vertex.WinHost.Opacity = 100.0;
            //vertex.WinHost.Margin = new Thickness(0);
        }

        private void Initial_Click(object sender, RoutedEventArgs e)
        {
            user = new User(uID.Text, gID.Text);

            var context = user.InitialContext();

            ExererciseLabel.Content = context.Label;
            WriteBox.Text = context.Code;
            if (context.Visualisations)
            {
                visible = false;
                EnableVisualisations();
            }
            else DisableVisualisations();
            
            user.StartExercise();

            InitialPanel.Visibility = Visibility.Collapsed;
        }

        private void Finish_Click(object sender, RoutedEventArgs e)
        {
            var context = user.FinishCurrent(WriteBox.Text);

            ExererciseLabel.Content = context.Label;
            InfoLabel.Content = Strings.InfoLabels[context.getType()];
            WriteBox.Text = context.Code;
            if (context.Visualisations) EnableVisualisations();
            else DisableVisualisations();
            if (context.FurtherInfo) FurtherInfo.Visibility = Visibility.Visible;
            else FurtherInfo.Visibility = Visibility.Collapsed;

            Charts.Visibility = Visibility.Hidden;
            IntermediatePanel.Visibility = Visibility.Visible;
        }

        private void DisableVisualisations()
        {
            if (visible)
            {
                ChartsColumn.Width = new GridLength(0);
                TextColumn.Width = new GridLength(this.ActualWidth - 23);
                GraphColumn.Width = new GridLength(0);
                Status.Visibility = Visibility.Hidden;
                visible = false;
            }
        }

        private void EnableVisualisations()
        {
            if (!visible)
            {
                ChartsColumn.Width = new GridLength(190);
                TextColumn.Width = new GridLength(500);
                GraphColumn.Width = new GridLength(1, GridUnitType.Star);
                Status.Visibility = Visibility.Visible;
                visible = true;
            }
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            user.StartExercise();
            IntermediatePanel.Visibility = Visibility.Hidden;
            Charts.Visibility = Visibility.Visible;
        }

        private void WriteBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {

            if (e.Key.ToString() == Keys.Back.ToString() || e.Key.ToString() == Keys.Delete.ToString())
            {
                user.Backspace();
            }
        }
    }
}
