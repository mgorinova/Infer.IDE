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

        public string CodeString { get { return code; } set { code = value; } }
        public int Id { get { return id; } set { id = value; } }
        //private string pathToSave = ("\"" + System.IO.Directory.GetCurrentDirectory() + "\"").Replace("\\", "\\\\");

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
        public void click()
        {
            //Shell.FsiEvaluationSessionHostConfig fsiConfig = Shell.FsiEvaluationSession.GetDefaultConfiguration();
            //fsiSession = Shell.FsiEvaluationSession.Create(fsiConfig, txt, inStream, outStream, errStream, FSharpOption<bool>.Some(true));

            

            Console.WriteLine("1");
            var pth = @"D:/tmp.fsx";
            //File.WriteAllText(pth, WriteBox.Text);
            Console.WriteLine("2");
            try
            {
                Console.WriteLine("3");
                fsiSession.EvalScript(pth);
                Console.WriteLine("4");

                fsiSession.EvalInteraction("open Tmp");
                fsiSession.EvalInteraction("let ie = new InferenceEngine()");
                fsiSession.EvalInteraction("printfn \"%A\" (ie.Infer(means))");

            }
            catch (Exception err) { Console.WriteLine("EXCEPTION! " + err.Message + "\n" + err.InnerException.Message); }
            Console.WriteLine("5");
        }

        public void run()
        {
            total.Restart();
            Monitor.Enter(lockCode);
            Console.WriteLine("Thread {0} started.", id);
            watch.Restart();
            // TODO: implement text highlighting, etc

            DispatcherOperation dRBox = rBox.Dispatcher.BeginInvoke(
                new Action(delegate()
                {
                    rBox.Text = "";
                }));
            dRBox.Completed += dRBox_Completed;


            string current = Strings.allAssemblies + code;

            File.WriteAllText(path, current);

            FSharpList<string> activeVars = null;

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
                       rBox.Text += err.Message;
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

            Monitor.Exit(lockCode);
            if (activeVars == null) return;
            else execute(activeVars);
            
        }

        private void execute(FSharpList<string> activeVars)
        {
            try
            {
                Monitor.Enter(this);
                Console.WriteLine("Thread {0} now executing.", id);

                setCover();

                updateStatusMessage(2);
                updateProgressBar(20);
 
                try
                {
                    /*  
                     *  F# code evaluation:
                     */

                    watch.Restart();
                    // FIXME: Maybe extract the text of the last namespace
                    // defined, to show in the "Read Box" of the IDE.  
                    Console.Write("script evaluation...");

                    //foreach (string v in activeVars) Console.WriteLine("{0} is an active var", v);
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

                    updateProgressBar(20);

                    /* 
                     * Injection: 
                     * Infer each variable that is in the active variables list
                     * Move that to ModelGraph constructor. Be careful when 
                     * there are more than one dgml files in that case.
                     */

                    // open module (that might be unsafe...?) -- should be fine,
                    // as we are checking the code before compilation - i.e.
                    // it makes sense on its own.

                    Console.Write("preparetion...");
                    watch.Restart();

                    fsiSession.EvalInteraction("open Tmp");

                    string pathToSave = ("\"" + System.IO.Directory.GetCurrentDirectory() + "\"").Replace("\\", "\\\\");
                    string pathToDGML = System.IO.Directory.GetCurrentDirectory(); //+ "\\Model.dgml";

                    Random r = new Random();

                    ///*
                    string eName = "tempName" + r.Next();
                    fsiSession.EvalInteraction("let " + eName + " = new InferenceEngine()");    // create infering engine
                    fsiSession.EvalInteraction(eName + ".SaveFactorGraphToFolder <-" + pathToSave);
                    //*/

                    HashSet<string> added = new HashSet<string>();

                    vModel.Reset();                                // zero the content of current model

                    DispatcherOperation dCharts = charts.Dispatcher.BeginInvoke(
                        new Action(delegate()
                        {
                            charts.Children.RemoveRange(0, charts.Children.Count);      // remove previous charts
                        }));

                    dCharts.Completed += dCharts_Completed;

                    updateStatusMessage(3);
                    updateProgressBar(25);

                    Console.WriteLine("OK {0} \n", watch.ElapsedMilliseconds);

                    foreach (string varName in activeVars)
                    {
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

                            string distribution = resultAsValue.ToString();
                            
                            /*DispatcherOperation dRBox = rBox.Dispatcher.BeginInvoke(
                                new Action(delegate()
                                {
                                    rBox.Text += varName + " = " + distribution + Environment.NewLine;
                                }));
                            dRBox.Completed += dRBox_Completed;*/

                            var varNode = vModel.findNodeByName(varName);

                            if (varNode == null)
                            {
                                // Update the graph accordingly and get set of connected vertices as a result

                                // List<string> connectedComponent = new List<string>();
                                try
                                {
                                    var connectedComponent = vModel.Update(pathToDGML + "\\Model.dgml");

                                    // Change the path where the next dgml file is saved, to maximise performance,
                                    // by allowing a new dgml file to be created while another one is being read
                                    // (the creation of the dgml file happens in a separate process - fsi.exe).
                                    // Currently, "path not found" exception might occur if a new dgml file is not 
                                    // produced on the next iteration, but we are infering a variable name, which 
                                    // name is not in the added to the model graph ones - i.e. if the variable is observed..............

                                    pathToDGML += "\\t";
                                    pathToSave = pathToSave.Remove(pathToSave.Length - 1) + "\\\\t\"";
                                    fsiSession.EvalInteraction(eName + ".SaveFactorGraphToFolder <-" + pathToSave);

                                    added.UnionWith(connectedComponent);

                                    varNode = vModel.findNodeByName(varName);
                                }
                                catch (DirectoryNotFoundException) { updateReadBox(varName + " is an observed variable"); }

                            }

                            vModel.UpdateDistribution(varNode, distribution);
                            drawDistribution(varNode, distribution);                            

                            Console.WriteLine("OK {0}", watch.ElapsedMilliseconds);
                            // TODO: change that to "createXAMLChild" or something. Assosiate the
                            // element with the coressponding node in the ModelGraph, so a 
                            // "node expansion" visualisation can be implemented on a later stage.

                        }
                        else Console.WriteLine("Error evaluating expression");
                    }
                    
                    updateProgressBar(25);
                    
                }
                catch (ThreadAbortException)
                {
                    Monitor.Exit(this);
                    Console.WriteLine("\n Thread {0} ABORTED.", id);
                    return;
                }
                catch (OperationCanceledException)
                {
                    Monitor.Exit(this);
                    Console.WriteLine("\n Thread {0} ABORTED.", id);
                    return;
                }
                catch (Exception err)
                {
                    DispatcherOperation dRBox = rBox.Dispatcher.BeginInvoke(
                        new Action(delegate()
                        {
                            rBox.Text = err.Message;
                            rBox.Text += "\n";
                            if(err.InnerException != null) 
                                rBox.Text += err.InnerException.Message;
                        }));

                    Console.WriteLine(err.Message + "\n");
                    if (err.InnerException != null)
                        Console.WriteLine("   " + err.InnerException.Message);

                    Console.WriteLine("Type of the exception: {0}", err.GetType());
                }

                //@"c:\temp\MyTest.txt"
                //string pathhh = @"d:\here.dgml\Model.dgml";
                vModel.ReLayoutGraph();
                
                //foreach (Backend.ModelVertex v in viewModel.Graph.Vertices)
                //Console.WriteLine("Vertex {0} with distribution {1}", v.Label, v.Distribution);

                removeCover();

                Console.WriteLine("\n Re-layout finished, total time: {0}", total.Elapsed);

                updateStatusMessage(0);
                updateProgressBar(0);

                try
                {
                    Directory.Delete(System.IO.Directory.GetCurrentDirectory() + "\\t", true);
                }
                catch (DirectoryNotFoundException) { }

                Console.WriteLine("\n Thread {0} finished execution.", id);
                Console.WriteLine("\n****************************************************************\n");
                Monitor.Exit(this);
            }
            catch (ThreadAbortException)
            {
                Monitor.Exit(this);
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
                        if(status.Text == "") status.Text = "Checking...";
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
            if(varNode == null) return;

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
                         DoubleAnimation da = new DoubleAnimation(progress.Value, progress.Value+value, duration);
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
