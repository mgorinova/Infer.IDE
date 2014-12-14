using Microsoft.FSharp.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Threading;

namespace Infer.IDE
{
    public class Code
    {
        private string path = Strings.path;

        private string codeString;        
        private FSharpList<string> activeVars;

        private TextBox rBox;

        public Code(string code, TextBox readBox)
        {
            codeString = code;
            rBox = readBox;
            activeVars = null;
        }

        public void check()
        {
            Monitor.Enter(this);

            // TODO: implement text highlighting, etc
            DispatcherOperation dRBox = rBox.Dispatcher.BeginInvoke(
                new Action(delegate()
                {
                    rBox.Text = "";
                }));

            string current = Strings.allAssemblies + codeString;

            File.WriteAllText(path, current);

            Console.Write("checking code...");
            try
            {
                /*
                 * F# code checking:
                 * Get a list of Infer.NET variables.
                 * If the code fails to check - an exception is thrown
                 */

                activeVars = Checker.check(path, current);
                Console.WriteLine(" OK \n");
            }
            catch (Exception err)
            {
                dRBox = rBox.Dispatcher.BeginInvoke(
                   new Action(delegate()
                   {
                       rBox.Text += err.Message;
                   }));

                Console.WriteLine("{0}", err.Message);
            }

           // if (activeVars == null) return;
           // else execute(activeVars);

            Monitor.Exit(this);
        }


        public string CodeString { get { return codeString; } set { codeString = value; } }
        public FSharpList<string> ActiveVars { get { return activeVars; } }
    }
}
