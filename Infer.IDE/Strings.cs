using System;

namespace Infer.IDE
{
    /// <summary>
    /// A static class which holds strings and lists of strings, specifying
    /// particular working files/directories, as well as F# Interctive commands 
    /// for locating needed binaries.
    /// </summary>
    static class Strings
    {
        public static string path = System.IO.Directory.GetCurrentDirectory() + "\\tmp.fsx";
        public static string curDir = System.IO.Directory.GetCurrentDirectory();

        public static string[] assemblies = { 
                                          "#r \"infer\\Infer.Compiler.dll\"", 
                                          "#r \"infer\\Infer.Runtime.dll\"", 
                                          "#r \"infer\\Infer.FSharp.dll\"",
                                          "open MicrosoftResearch.Infer",
                                          "open MicrosoftResearch.Infer.Models",
                                          "open MicrosoftResearch.Infer.Distributions",
                                          "open MicrosoftResearch.Infer.Factors",
                                          "open MicrosoftResearch.Infer.FSharp",
                                          "open MicrosoftResearch.Infer.Maths"
                                            };

        public static string allAssemblies = "#r \"infer\\Infer.Compiler.dll\"" + Environment.NewLine +
                                "#r \"infer\\Infer.Runtime.dll\"" + Environment.NewLine +
                                "#r \"infer\\Infer.FSharp.dll\"" + Environment.NewLine +
                                "open MicrosoftResearch.Infer" + Environment.NewLine +
                                "open MicrosoftResearch.Infer.Models" + Environment.NewLine +
                                "open MicrosoftResearch.Infer.Distributions" + Environment.NewLine +
                                "open MicrosoftResearch.Infer.Factors" + Environment.NewLine +
                                "open MicrosoftResearch.Infer.FSharp" + Environment.NewLine +
                                "open MicrosoftResearch.Infer.Maths" + Environment.NewLine;

        public static string namescapses = "open MicrosoftResearch.Infer" + Environment.NewLine +
                                "open MicrosoftResearch.Infer.Models" + Environment.NewLine +
                                "open MicrosoftResearch.Infer.Distributions" + Environment.NewLine +
                                "open MicrosoftResearch.Infer.Factors" + Environment.NewLine +
                                "open MicrosoftResearch.Infer.FSharp" + Environment.NewLine +
                                "open MicrosoftResearch.Infer.Maths" + Environment.NewLine;
    }
}
