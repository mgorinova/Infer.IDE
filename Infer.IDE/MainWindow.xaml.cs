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

            WriteBox.Text = CompilerStrings.allDistributions;

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
            ReadBox.Text = "";

            // FIXME: feedback + exception handling

            // FIXME: for now we assume Infer.NET var names = compiled var names 

            // TODO: implement text highlighting, etc

            var current = CompilerStrings.allAssemblies+ WriteBox.Text;

            File.WriteAllText("tmp.fsx", current);   

            Console.Write("checking code...");
            FSharpList<string> activeVars = null;
            try
            {
                /*
                 * F# code checking:
                 * Get a list of Infer.NET variables.
                 * If the code fails to check - an exception is thrown
                 */

                activeVars = Checker.check(path, current);
                Console.WriteLine(" OK \n");

                foreach (string v in activeVars) Console.WriteLine("{0} is an active var", v);

                /*  
                 *  F# code evaluation:
                 */
                Console.Write("script evaluation...");
                try
                {
                    fsiSession.EvalScript("tmp.fsx");
                    Console.WriteLine(" OK \n");
                }
                catch (Exception err)
                {
                    ReadBox.Text = err.Message;
                    ReadBox.Text += err.InnerException.Message;

                    Console.WriteLine(err.Message);
                    Console.WriteLine(err.InnerException.Message);
                }

                // FIXME: Maybe extract the text of the last namespace
                // defined, to show in the "Read Box" of the IDE.

                //string output = outStream.ToString();
                //Console.WriteLine(output);     

                /* 
                 * Injection: 
                 * Infer each variable that is in the active variables list
                 * TODO: add a DGML request injection when Infer.NET 2.6 is available.
                 * Move that to ModelGraph constructor. Be careful when 
                 * there are more than one dgml files in that case.
                 */

                fsiSession.EvalInteraction("open Tmp");         // open module (that might be unsafe...?) -- should be fine,
                                                                // as we are checking the code before compilation - i.e.
                                                                // it makes sense on its own.

                string pathToDGML = @"d:\here.dgml\Model.dgml";

                string pathh = "\"D:\\here.dgml\"";

                Random r = new Random();

                /*
                string eName = "tempName" + r.Next();                
                fsiSession.EvalInteraction("let " + eName + " = new InferenceEngine()");    // create infering engine
                fsiSession.EvalInteraction(eName + ".SaveFactorGraphToFolder <-" + pathh);
                */

                HashSet<string> added = new HashSet<string>();

                viewModel.Reset();                                // zero the content of current model
                Charts.Children.RemoveRange(0, Charts.Children.Count);      // remove previous charts

                foreach (string varName in activeVars)
                {
                    Console.WriteLine("processing var {0}", varName);

                    //*
                    string eName = "tempName" + r.Next();
                    fsiSession.EvalInteraction("let " + eName + " = new InferenceEngine()");    // create infering engine
                    fsiSession.EvalInteraction(eName + ".SaveFactorGraphToFolder <-" + pathh);
                     //*/

                    Console.Write("infering... ");
                    FSharpOption<Shell.FsiValue> val = fsiSession.EvalExpression(eName + ".Infer(" + varName + ")");
                    Console.WriteLine(" OK\n"); 

                    if (FSharpOption<Shell.FsiValue>.get_IsSome(val))
                    {
                        string distribution = ((val.Value).ReflectionValue).ToString();
                        ReadBox.Text += varName + " = " + distribution + Environment.NewLine;

                        if (!added.Contains(varName))
                        {
                            // Update the graph accordingly and get set of connected vertices as a result                            
                            var connectedComponent = viewModel.Update(pathToDGML);
                            added.UnionWith(connectedComponent);                        
                        }

                        viewModel.UpdateDistribution(varName, distribution);

                         drawDistribution(distribution, varName);
                        // TODO: change that to "createXAMLChild" or something. Assosiate the
                        // element with the coressponding node in the ModelGraph, so a 
                        // "node expansion" visualisation can be implemented on a later stage.
                        
                    }
                    else Console.WriteLine("Error evaluating expression");
                }

            }
            catch (Exception err) 
            {
                ReadBox.Text = err.Message;
                Console.WriteLine("{0}", err.Message );

                Console.Write(outStream.ToString());

            }
            //@"c:\temp\MyTest.txt"
            //string pathhh = @"d:\here.dgml\Model.dgml";
            viewModel.ReLayoutGraph();

            //foreach (Backend.ModelVertex v in viewModel.Graph.Vertices)
                //Console.WriteLine("Vertex {0} with distribution {1}", v.Label, v.Distribution);
            


            #region F# Eval each line
            /*string[] lines = (WriteBox.Text).Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            foreach(string line in lines)
            {
                //string current = line;

                if (line.StartsWith("printfn"))
                {
                    string current = line.Replace("printfn", "sprintf");

                    FSharpOption<Shell.FsiValue> val = fsiSession.EvalExpression(current);

                    if (FSharpOption<Shell.FsiValue>.get_IsSome(val))
                        ReadBox.Text += ((val.Value).ReflectionValue).ToString() + Environment.NewLine;
                    else Console.WriteLine("Error evaluating expression \"{0}\"", line);

                }
                else 
                { 
                    if (line.StartsWith("="))
                    {
                        FSharpOption<Shell.FsiValue> val = fsiSession.EvalExpression(line.Substring(1));

                        if (FSharpOption<Shell.FsiValue>.get_IsSome(val))
                            ReadBox.Text += ((val.Value).ReflectionValue).ToString() + Environment.NewLine;
                        else Console.WriteLine("Error evaluating expression \"{0}\"", line.Substring(1));

                    }
                    else 
                    {
                        try { fsiSession.EvalInteraction(line); }
                        catch (Exception) 
                        { 
                            // FIXME: this try/catch shouldn't be here, as
                            // we will do checking before calling eval.
                            Console.WriteLine("Error evaluating interaction \"{0}\"", line);
                        }                                                                                        
                    }

                }

            } */
            #endregion
            

        }

        private void drawDistribution(string distribution, string name)
        {
            var wfh = new WindowsFormsHost();
            wfh.Height = 150.0;
            Charts.Children.Add(wfh);

            Distributions.draw(wfh, distribution, name);
        }

        private void TextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {

        }
    }
}
