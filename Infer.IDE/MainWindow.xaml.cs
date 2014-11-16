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

        public MainWindow()
        {
            InitializeComponent();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Compile_Click(object sender, RoutedEventArgs e)
        {
            System.Text.StringBuilder sbOut = new System.Text.StringBuilder();
            System.Text.StringBuilder sbErr = new System.Text.StringBuilder();

            StringReader inStream = new StringReader("");
            StringWriter outStream = new StringWriter(sbOut);
            StringWriter errStream = new StringWriter(sbErr);

            string[] txt = { "fsi.exe " + "--noninteractive" };

            Shell.FsiEvaluationSessionHostConfig fsiConfig = Shell.FsiEvaluationSession.GetDefaultConfiguration();
            Shell.FsiEvaluationSession fsiSession = Shell.FsiEvaluationSession.Create(fsiConfig, txt, inStream, outStream, errStream, FSharpOption<bool>.None);


            string[] lines = (WriteBox.Text).Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            foreach(string line in lines)
            {
                if (line.StartsWith("="))
                {
                    FSharpOption<Shell.FsiValue> val = fsiSession.EvalExpression(line.Substring(1));

                    if (FSharpOption<Shell.FsiValue>.get_IsSome(val))
                        ReadBox.Text = ((val.Value).ReflectionValue).ToString();
                }
                else 
                {
                    fsiSession.EvalInteraction(line);
                }

            }
            /*

            string text = "let a = 5";
            fsiSession.EvalInteraction(text);

            text = "printfn \"HELLOOOOOO %d\" a";
            fsiSession.EvalInteraction(text);


            Random r = new Random();

            text = "= a*" + r.Next(100);
            FSharpOption<Shell.FsiValue> val = fsiSession.EvalExpression(text.Substring(1));

            if (FSharpOption<Shell.FsiValue>.get_IsSome(val))
                ReadBox.Text = ((val.Value).ReflectionValue).ToString();*/
        }

        private void TextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {

        }
    }
}
