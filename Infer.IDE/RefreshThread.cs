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


namespace Infer.IDE
{
    class RefreshThread
    {
        private string pth;
        private string code;
        private Shell.FsiEvaluationSession fsiSession;
        private TextBox rBox;
        private Rectangle cover;
        private StackPanel charts;
        private ProgressBar progress;
        private ViewModel vModel;        

        //private string pathToSave = ("\"" + System.IO.Directory.GetCurrentDirectory() + "\"").Replace("\\", "\\\\");

        public RefreshThread(string path, string codeToEvaluate, TextBox readBox, Rectangle workingCover,
                             StackPanel chartsPanel, ProgressBar progressBar, ViewModel viewModel, Shell.FsiEvaluationSession fsiEvaluationSession)
        {
            pth = path;
            code = codeToEvaluate;
            rBox = readBox;
            cover = workingCover;
            charts = chartsPanel;
            progress = progressBar;
            vModel = viewModel;
            fsiSession = fsiEvaluationSession;
        }

        public void run()
        { 
            // TODO: implement text highlighting, etc

            DispatcherOperation dRBox = rBox.Dispatcher.BeginInvoke(
                new Action(delegate()
                {
                    rBox.Text = "";
                }));
            dRBox.Completed += dRBox_Completed;


            string current = CompilerStrings.allAssemblies + code;

            File.WriteAllText(pth, current);

            FSharpList<string> activeVars = null;

            Console.Write("checking code...");
            try
            {
                /*
                 * F# code checking:
                 * Get a list of Infer.NET variables.
                 * If the code fails to check - an exception is thrown
                 */

                activeVars = Checker.check(pth, current);
                Console.WriteLine(" OK \n");
            }
            catch (Exception err)
            {
                dRBox = rBox.Dispatcher.BeginInvoke(
                   new Action(delegate()
                   {
                       rBox.Text += err.Message;
                   }));
                dRBox.Completed += dRBox_Completed;

                Console.WriteLine("{0}", err.Message);
            }

            if (activeVars == null) return;
            else execute(activeVars);
        }

        private void execute(FSharpList<string> activeVars)
        {         
            DispatcherOperation dCov = cover.Dispatcher.BeginInvoke(
                new Action(delegate()
                {
                    cover.Visibility = Visibility.Visible;
                }));

            updateProgressBar(20);
 
            foreach (string v in activeVars) Console.WriteLine("{0} is an active var", v);

            /*  
             *  F# code evaluation:
             */
            Console.Write("script evaluation...");
            try
            {
                fsiSession.EvalScript(pth);
                Console.WriteLine(" OK \n");
            }
            catch (Exception err)
            {
                DispatcherOperation dRBox = rBox.Dispatcher.BeginInvoke(
                    new Action(delegate()
                    {
                        rBox.Text = err.Message;
                        rBox.Text += "/n";
                        rBox.Text += err.InnerException.Message;                            
                    }));

                dRBox.Completed += dRBox_Completed;

                Console.WriteLine(err.Message + "/n");
                Console.WriteLine("   " + err.InnerException.Message);
            }
            updateProgressBar(20);
            // FIXME: Maybe extract the text of the last namespace
            // defined, to show in the "Read Box" of the IDE.  

            /* 
                * Injection: 
                * Infer each variable that is in the active variables list
                * Move that to ModelGraph constructor. Be careful when 
                * there are more than one dgml files in that case.
                */
                         
            // open module (that might be unsafe...?) -- should be fine,
            // as we are checking the code before compilation - i.e.
            // it makes sense on its own.

            try
            {            
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

                updateProgressBar(25);

                foreach (string varName in activeVars)
                {
                    Console.WriteLine("processing var {0}", varName);

                    /*
                    string eName = "tempName" + r.Next();
                    fsiSession.EvalInteraction("let " + eName + " = new InferenceEngine()");    // create infering engine
                    fsiSession.EvalInteraction(eName + ".SaveFactorGraphToFolder <-" + pathToSave);
                    */

                    Console.Write("infering... ");

                    FSharpOption<Shell.FsiValue> val = fsiSession.EvalExpression("try Choice1Of2(" + eName + ".Infer(" + varName + ")) with exn -> Choice2Of2(exn)");
                    Console.WriteLine(" OK\n");     

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
                        DispatcherOperation dRBox = rBox.Dispatcher.BeginInvoke(
                            new Action(delegate()
                            {
                                rBox.Text += varName + " = " + distribution + Environment.NewLine;
                            }));
                        dRBox.Completed += dRBox_Completed;

                        if (!added.Contains(varName))
                        {
                            // Update the graph accordingly and get set of connected vertices as a result                            
                            var connectedComponent = vModel.Update(pathToDGML + "\\Model.dgml");

                            // Change the path where the next dgml file i saved, to maximise performance,
                            // by allowing a new dgml file to be created while another one is being read
                            // (the creation of the dgml file happens in a separate process - fsi.exe).
                            // Currently, "path not found" exception might occur if a new dgml file is not 
                            // produced on the next iteration, but we are infering a variable name, which 
                            // name is not in the added to the model graph ones - i.e. if the variable is observed..............

                            pathToDGML += "\\t";
                            pathToSave = pathToSave.Remove(pathToSave.Length - 1) + "\\\\t\"";
                            fsiSession.EvalInteraction(eName + ".SaveFactorGraphToFolder <-" + pathToSave);

                            added.UnionWith(connectedComponent);
                        }

                        vModel.UpdateDistribution(varName, distribution);

                        drawDistribution(distribution, varName);
                        // TODO: change that to "createXAMLChild" or something. Assosiate the
                        // element with the coressponding node in the ModelGraph, so a 
                        // "node expansion" visualisation can be implemented on a later stage.

                    }
                    else Console.WriteLine("Error evaluating expression");
                }

                updateProgressBar(25);
            }
            catch (Exception err)
            {
                DispatcherOperation dRBox = rBox.Dispatcher.BeginInvoke(
                    new Action(delegate()
                    {
                        rBox.Text += err.Message;
                    }));
                dRBox.Completed += dRBox_Completed;

                Console.WriteLine("{0}", err.Message);

                //Console.Write(outStream.ToString());

            }

            //@"c:\temp\MyTest.txt"
            //string pathhh = @"d:\here.dgml\Model.dgml";
            vModel.ReLayoutGraph();

            //foreach (Backend.ModelVertex v in viewModel.Graph.Vertices)
            //Console.WriteLine("Vertex {0} with distribution {1}", v.Label, v.Distribution);

            dCov = cover.Dispatcher.BeginInvoke(
                new Action(delegate()
                {
                    cover.Visibility = Visibility.Hidden;
                }));

            Console.WriteLine("Re-layout finished");
            updateProgressBar(0);

            try
            {
                Directory.Delete(System.IO.Directory.GetCurrentDirectory() + "\\t", true);
            }
            catch (Exception e) { }

        }

        void dCharts_Completed(object sender, EventArgs e)
        {
            Console.WriteLine("Charts updated");
        }

        void dRBox_Completed(object sender, EventArgs e)
        {
            Console.WriteLine("ReadBox updated");
        }

        private void drawDistribution(string distribution, string name)
        {
            DispatcherOperation dCharts = charts.Dispatcher.BeginInvoke(
                new Action(delegate()
                {
                    var wfh = new WindowsFormsHost();
                    wfh.Height = 150.0;
                    charts.Children.Add(wfh);
                    Distributions.draw(wfh, distribution, name);
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
 

    }
}
