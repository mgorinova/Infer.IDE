using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using System.Windows.Threading;

using Microsoft.FSharp.Compiler.Interactive;
using Microsoft.FSharp.Compiler;
using Microsoft.FSharp.Core;
using Microsoft.FSharp.Collections;
using System.Windows.Shapes;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;
using ICSharpCode.AvalonEdit;
using Backend;
using System.Diagnostics;
using System.Windows.Media;


namespace Infer.IDE
{
    class RefreshThread
    {
        private string path = Strings.path;
        private string code;
        private int id;
        private Shell.FsiEvaluationSession fsiSession;
        private TextEditor wBox;
        private TextBox rBox;
        private Rectangle cover;
        private StackPanel charts;
        private ProgressBar progress;
        private TextBlock status;
        private ViewModel vModel;

        private Stopwatch watch = new Stopwatch();
        private Stopwatch total = new Stopwatch();

        private readonly Object lockCode = new Object();
        private readonly Object lockExecution = new Object();

        public string CodeString { get { return code; } set { code = value; } }
        public int Id { get { return id; } set { id = value; } }
        
        public RefreshThread(TextEditor writeBox, TextBox readBox, Rectangle workingCover,
                             StackPanel chartsPanel, ProgressBar progressBar, TextBlock statusString,
                             ViewModel viewModel, Shell.FsiEvaluationSession fsiEvaluationSession)
        {
            id = 0;
            wBox = writeBox;
            rBox = readBox;
            cover = workingCover;
            charts = chartsPanel;
            progress = progressBar;
            status = statusString;
            vModel = viewModel;
            fsiSession = fsiEvaluationSession;
        }

        public void run()
        {
            Monitor.Enter(lockCode);
            total.Restart();
            Console.WriteLine("Thread {0} started.", id);
            watch.Restart();
            // TODO: implement text highlighting, etc

            DispatcherOperation dRBox = rBox.Dispatcher.BeginInvoke(
                new Action(delegate()
                {
                    rBox.Foreground = Brushes.Black;
                    rBox.Text = "";
                }));
            dRBox.Completed += dRBox_Completed;

            FSharpList<Checker.RandomVariable> activeVars = null;

            string current = Strings.allAssemblies + code;

            File.WriteAllText(path, current);

            Console.Write("\n checking code...");
            try
            {
                /*
                 * F# code checking:
                 * Get a list of Infer.NET variables.
                 * If the code fails to check - an exception is thrown
                 */
                updateStatusMessage(1);

                activeVars = Checker.check(path, current);

                Console.WriteLine(" OK, {0} \n", watch.ElapsedMilliseconds);
            }
            catch (Exception err)
            {
                dRBox = rBox.Dispatcher.BeginInvoke(
                   new Action(delegate()
                   {
                       rBox.Foreground = Brushes.Red;
                       var message = correctLineNumbers(err.Message.Substring(72));
                       rBox.Text += message;
                   }));
                dRBox.Completed += dRBox_Completed;

                updateStatusMessage(-1);
                removeCover();

                int offset = 2;
                int length = 10;

                Console.WriteLine("{0}", err.Message);
                Monitor.Exit(lockCode);
                return;
            }

            if (activeVars == null) { Monitor.Exit(lockCode); return; } // this happens only when checking does not complete. Otherwise, the list is empty or not, but it exists!
            else
            {
                if (!Monitor.TryEnter(lockExecution))
                {
                    MainWindow.currentExecutingThread.Abort();
                    Monitor.Enter(lockExecution);
                }

                MainWindow.currentExecutingThread = Thread.CurrentThread;  
              
                Monitor.Exit(lockCode);
                execute(activeVars);
            }

        }

        private string correctLineNumbers(string message)
        {
            var split = message.Split(new Char[] { '(', ',', ')', '-', '(', ',', ')', ' ' }, 5, StringSplitOptions.RemoveEmptyEntries);

            int line1 = Int32.Parse(split[0]) - 9;

            var ret = String.Format("Line {0}: {1}", line1, split[4]);

            return ret;
        }

        private void execute(FSharpList<Checker.RandomVariable> activeVars)
        {
            try
            {
                Console.WriteLine("Thread {0} now executing.", id);

                setCover();

                updateStatusMessage(2);
                updateProgressBar(35);

                try
                {
                    /*  
                     *  F# code evaluation:
                     */

                    watch.Restart();
                    // FIXME: Maybe extract the text of the last namespace
                    // defined, to show in the "Read Box" of the IDE.  
                    Console.Write("script evaluation...");

                    System.IO.StringWriter sbUserDiagnostics = new System.IO.StringWriter();
                    Console.SetError(sbUserDiagnostics);

                    fsiSession.EvalScript(path);

                    DispatcherOperation dRBox1 = rBox.Dispatcher.BeginInvoke(
                       new Action(delegate()
                       {
                           rBox.Text += sbUserDiagnostics.ToString();
                       }));
                    dRBox1.Completed += dRBox_Completed;

                    Console.WriteLine(" OK {0}\n", watch.ElapsedMilliseconds);

                    updateProgressBar(35);

                    /* 
                     * Injection: 
                     * Infer each variable that is in the active variables list
                     * Move that to ModelGraph constructor. Be careful when 
                     * there are more than one dgml files in that case.
                     */

                    // open module (that might be unsafe...?) -- should be fine,
                    // as we are checking the code before compilation - i.e.
                    // it makes sense on its own.

                    Console.Write("preparation...");
                    watch.Restart();

                    fsiSession.EvalInteraction("open Tmp");

                    string pathToSave = ("\"" + System.IO.Directory.GetCurrentDirectory() + "\"").Replace("\\", "\\\\");
                    string pathToDGML = System.IO.Directory.GetCurrentDirectory(); //+ "\\Model.dgml";

                    Random r = new Random();
                                   
                    string eName = "ie";
                    //string eName = "tempName" + r.Next();
                    //fsiSession.EvalInteraction("let " + eName + " = new InferenceEngine()");    // create inference engine
                    fsiSession.EvalInteraction(eName + ".SaveFactorGraphToFolder <-" + pathToSave);
                    
                    HashSet<string> added = new HashSet<string>();

                    vModel.Reset();                                // zero the content of current model

                    DispatcherOperation dCharts = charts.Dispatcher.BeginInvoke(
                        new Action(delegate()
                        {
                            charts.Children.RemoveRange(0, charts.Children.Count);      // remove previous charts
                        }));

                    dCharts.Completed += dCharts_Completed;

                    updateStatusMessage(3);
                    updateProgressBar(35);

                    Console.WriteLine("OK {0} \n", watch.ElapsedMilliseconds);

                    foreach (Checker.RandomVariable rv in activeVars)
                    {
                        string varName = rv.name;
                        var location = rv.line;

                        Console.Write("processing var {0}... ", varName);
                        watch.Restart();
                        /*
                        string eName = "tempName" + r.Next();
                        fsiSession.EvalInteraction("let " + eName + " = new InferenceEngine()");    // create infering engine
                        fsiSession.EvalInteraction(eName + ".SaveFactorGraphToFolder <-" + pathToSave);
                        */

                        //Console.Write("infering... ");

                        FSharpOption<Shell.FsiValue> val = fsiSession.EvalExpression("try Choice1Of2(" + eName + ".Infer(" + varName + ")) with exn -> Choice2Of2(exn)");
                        //Console.WriteLine(" OK\n");

                        if (FSharpOption<Shell.FsiValue>.get_IsSome(val))
                        {
                            object resultAsObj = (val.Value).ReflectionValue;
                            var resultAsChoice = resultAsObj as FSharpChoice<object, System.Exception>;
                            if (resultAsChoice.IsChoice2Of2)
                            {
                                System.Exception err = (resultAsChoice as FSharpChoice<object, System.Exception>.Choice2Of2).Item;
                                throw err;
                            }
                            var resultAsValue = (resultAsChoice as FSharpChoice<object, System.Exception>.Choice1Of2).Item;

                            //Distributions.checkIfMeIdiot(resultAsValue);

                            string distribution = resultAsValue.ToString();

                            var varNode = vModel.findNodeByName(varName);

                            if (varNode == null)
                            {
                                // Update the graph accordingly and get set of connected vertices as a result

                                // List<string> connectedComponent = new List<string>();
                                try
                                {
                                    var subGraph = vModel.Update(pathToDGML + "\\Model.dgml");

                                    // Change the path where the next dgml file is saved, to maximise performance,
                                    // by allowing a new dgml file to be created while another one is being read
                                    // (the creation of the dgml file happens in a separate process - fsi.exe).

                                    pathToDGML += "\\t";
                                    pathToSave = pathToSave.Remove(pathToSave.Length - 1) + "\\\\t\"";
                                    fsiSession.EvalInteraction(eName + ".SaveFactorGraphToFolder <-" + pathToSave);

                                    added.UnionWith(subGraph);

                                    varNode = vModel.findNodeByName(varName);
                                }
                                catch (DirectoryNotFoundException)
                                {
                                    updateReadBox("Something went wrong..."); 
                                }

                            }

                            varNode.Distribution = distribution;
                            varNode.Location = rv.line;
                            drawDistribution(varNode, distribution);

                            Console.WriteLine("OK {0}", watch.ElapsedMilliseconds);
                            // TODO: change that to "createXAMLChild" or something. Assosiate the
                            // element with the coressponding node in the ModelGraph, so a 
                            // "node expansion" visualisation can be implemented on a later stage.

                        }
                        else Console.WriteLine("Error evaluating expression");
                    }

                    updateProgressBar(35);

                }
                catch (ThreadAbortException)
                {
                    Monitor.Exit(lockExecution);
                    Console.WriteLine("\n Thread {0} ABORTED.", id);
                    return;
                }
                catch (OperationCanceledException)
                {
                    Monitor.Exit(lockExecution);
                    Console.WriteLine("\n Thread {0} ABORTED.", id);
                    return;
                }
                catch (Exception err)
                {
                    DispatcherOperation dRBox = rBox.Dispatcher.BeginInvoke(
                        new Action(delegate()
                        {
                            rBox.Foreground = Brushes.Red;
                            rBox.Text = err.Message;
                            rBox.Text += "\n";
                            if (err.InnerException != null)
                                rBox.Text += "   " + err.InnerException.Message;
                        }));

                    Console.WriteLine(err.Message + "\n");
                    if (err.InnerException != null)
                        Console.WriteLine("   " + err.InnerException.Message);

                    Console.WriteLine("Type of the exception: {0}", err.GetType());
                }

                vModel.ReLayoutGraph();

                removeCover();

                Console.WriteLine("\n Re-layout finished, total time: {0}", total.Elapsed);

                updateStatusMessage(0);
                updateProgressBar(0);

               /* try
                {
                    Directory.Delete(System.IO.Directory.GetCurrentDirectory() + "\\t", true);
                }
                catch (DirectoryNotFoundException) { }*/

                Console.WriteLine("\n Thread {0} finished execution.", id);
                Console.WriteLine("\n****************************************************************\n");
                Monitor.Exit(lockExecution);
            }
            catch (ThreadAbortException)
            {
                Monitor.Exit(lockExecution);
                Console.WriteLine("Thread {0} ABORTED.", id);
                Console.WriteLine("\n****************************************************************\n");
                return;
            }
        }

        private void updateReadBox(string p)
        {
            DispatcherOperation dRBox = rBox.Dispatcher.BeginInvoke(
                new Action(delegate()
                {
                    rBox.Text += p + "\n";
                }));
        }

        private void updateStatusMessage(int phase)
        {
            DispatcherOperation dStat = status.Dispatcher.BeginInvoke(
            new Action(delegate()
            {
                switch (phase)
                {
                    case 0: status.Text = "";
                        break;

                    case 1:
                        if (status.Text == "") status.Text = "Checking...";
                        break;

                    case 2: status.Text = "Compiling...";
                        break;

                    case 3: status.Text = "Inferring...";
                        break;

                    case -1: status.Text = "Could not compile.";
                        break;
                }

            }));
        }

        void dCharts_Completed(object sender, EventArgs e)
        {
            //Console.WriteLine("Charts updated");
        }

        void dRBox_Completed(object sender, EventArgs e)
        {
            //Console.WriteLine("ReadBox updated");
        }

        private void drawDistribution(ModelVertex varNode, string distribution)
        {
            if (varNode == null) return;

            DispatcherOperation dCharts = charts.Dispatcher.BeginInvoke(
                new Action(delegate()
                {
                    var border = new Border();
                    border.Background = Brushes.Red;
                    border.BorderThickness = new Thickness(5);

                    var wfh = new WindowsFormsHost();
                    wfh.Height = 150.0;

                    varNode.WinHost = wfh;

                    Distributions.draw(wfh, distribution, varNode.Label);

                    if (wfh.Child != null)
                    {
                        //border.Child = wfh;
                        //charts.Children.Add(border);
                        varNode.HostID = charts.Children.Count;

                        charts.Children.Add(wfh);
                    }

                }));
            dCharts.Completed += dCharts_Completed;

        }

        private void updateProgressBar(int value)
        {

            DispatcherOperation dProg = progress.Dispatcher.BeginInvoke(
                 new Action(delegate()
                 {
                     if (value == 0)
                     {
                         //progress.Value = 0;
                         Duration duration = new Duration(TimeSpan.FromSeconds(0.01));
                         DoubleAnimation da = new DoubleAnimation(progress.Value, 0, duration);
                         progress.BeginAnimation(ProgressBar.ValueProperty, da);
                     }
                     else
                     {
                         Duration duration = new Duration(TimeSpan.FromSeconds(0.2));
                         DoubleAnimation da = new DoubleAnimation(progress.Value, progress.Value + value, duration);
                         progress.BeginAnimation(ProgressBar.ValueProperty, da);

                     }
                 }));
        }

        private void setCover()
        {
            DispatcherOperation dCov = cover.Dispatcher.BeginInvoke(
            new Action(delegate()
            {
                cover.Visibility = Visibility.Visible;
            }));
        }

        private void removeCover()
        {
            DispatcherOperation dCov = cover.Dispatcher.BeginInvoke(
            new Action(delegate()
            {
                cover.Visibility = Visibility.Hidden;
            }));
        }


    }
}
