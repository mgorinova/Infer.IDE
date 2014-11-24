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
using System.IO;

using Microsoft.FSharp.Compiler.Interactive;
using Microsoft.FSharp.Core;

namespace Infer.IDE
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ViewModel vm;
        private int cnt = 0;
        private string [] paths = {"TwoCoins.dgml", "Sprinkler-Mine.dgml", "Sprinkler.dgml"};
        private string path = System.IO.Directory.GetCurrentDirectory() + "\\tmp.fsx";
        private Shell.FsiEvaluationSession fsiSession;
        private string[] assemblies = { 
                                          "#r \"infer\\Infer.Compiler.dll\"", 
                                          "#r \"infer\\Infer.Runtime.dll\"", 
                                          "#r \"infer\\Infer.FSharp.dll\"" };

        private string allAssemblies = "#r \"infer\\Infer.Compiler.dll\"" + Environment.NewLine +
                                        "#r \"infer\\Infer.Runtime.dll\"" + Environment.NewLine +
                                        "#r \"infer\\Infer.FSharp.dll\"" + Environment.NewLine +
                                        "open MicrosoftResearch.Infer" + Environment.NewLine +
                                        "open MicrosoftResearch.Infer.Models" + Environment.NewLine +
                                        "open MicrosoftResearch.Infer.Distributions" + Environment.NewLine +
                                        "open MicrosoftResearch.Infer.Factors" + Environment.NewLine +
                                        "open MicrosoftResearch.Infer.FSharp" + Environment.NewLine +
                                        "open MicrosoftResearch.Infer.Maths" + Environment.NewLine;

        private StringReader inStream;
        private StringWriter outStream;
        private StringWriter errStream;
        public MainWindow()
        {
            vm = new ViewModel();
            this.DataContext = vm;

            System.Text.StringBuilder sbOut = new System.Text.StringBuilder();
            System.Text.StringBuilder sbErr = new System.Text.StringBuilder();

            inStream = new StringReader("");
            outStream = new StringWriter(sbOut);
            errStream = new StringWriter(sbErr);

            string[] txt = { "fsi.exe " + "--noninteractive" };

            Shell.FsiEvaluationSessionHostConfig fsiConfig = Shell.FsiEvaluationSession.GetDefaultConfiguration();
            fsiSession = Shell.FsiEvaluationSession.Create(fsiConfig, txt, inStream, outStream, errStream, FSharpOption<bool>.None);

            foreach(string a in assemblies)
                fsiSession.EvalInteraction(a);

            fsiSession.EvalInteraction("open MicrosoftResearch.Infer");
            fsiSession.EvalInteraction("open MicrosoftResearch.Infer.Models");
            fsiSession.EvalInteraction("open MicrosoftResearch.Infer.Distributions");
            fsiSession.EvalInteraction("open MicrosoftResearch.Infer.Factors");
            fsiSession.EvalInteraction("open MicrosoftResearch.Infer.FSharp");
            fsiSession.EvalInteraction("open MicrosoftResearch.Infer.Maths");
            InitializeComponent();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(paths[0]);
            vm.ReLayoutGraph(paths[cnt++]);
            if (cnt == 3) cnt = 0;
        }

        private void Compile_Click(object sender, RoutedEventArgs e)
        {
            ReadBox.Text = "";

            // FIXME: feedback + exception handling

            // TODO: implement text highlighting, etc

            var current = allAssemblies + WriteBox.Text;

            File.WriteAllText("tmp.fsx", current);

            #region Check and Modify F# Code
            // Should happen in one go, but I might need different outputs:
            // - for when there are compile errors - lines with errors, so I don't try to compile (we don't want terrible crashes, do we?) 
            // - for when there are no compile errors - list of Infer.NET variables declared in the code            

            var activeVars = Checker.check(path, current);

            foreach (string v in activeVars)
            {
                Console.WriteLine("{0} is an active var", v);
            }

            #endregion

            #region Evaluate F# Script
            fsiSession.EvalScript("tmp.fsx");

            // FIXME: Maybe extract the text of the last namespace
            // defined, to show in the "Read Box" of the IDE.
            string output = outStream.ToString();
            Console.WriteLine(output);           

            #endregion

            #region Inject
            // Infer each variable that is in the active variables list

            // open module (that might be unsafe...?)
            fsiSession.EvalInteraction("open Tmp");

            // engine
            string eName = "badNameThatNoOneWillUse";
            fsiSession.EvalInteraction("let " + eName + " = new InferenceEngine()");

            foreach (string varName in activeVars)
            {
                //fsiSession.EvalInteraction(eName +".Infer(" + varName + ")");

                FSharpOption<Shell.FsiValue> val = fsiSession.EvalExpression(eName + ".Infer(" + varName + ")");

                if (FSharpOption<Shell.FsiValue>.get_IsSome(val))
                    ReadBox.Text += varName + " = " + ((val.Value).ReflectionValue).ToString() + Environment.NewLine;
                else Console.WriteLine("Error evaluating expression");
            }

            #endregion

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

        private void TextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {

        }
    }
}
