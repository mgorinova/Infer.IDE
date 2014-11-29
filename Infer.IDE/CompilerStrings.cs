using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Infer.IDE
{
    static class CompilerStrings
    {
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


        public static string sprinkler = @"open MicrosoftResearch.Infer
open MicrosoftResearch.Infer.Models
open MicrosoftResearch.Infer.Distributions
open MicrosoftResearch.Infer.Factors
open MicrosoftResearch.Infer.FSharp

let C = Variable.Bernoulli(0.5).Named(""cloudy"")

let S = Variable.New<bool>().Named(""sprinkler"")
let R = Variable.New<bool>().Named(""rain"")

let ifc = Variable.If(C)
S.SetTo(Variable.Bernoulli(0.1))
R.SetTo(Variable.Bernoulli(0.8))
ifc.CloseBlock()   
    
let els = Variable.IfNot(C)
S.SetTo(Variable.Bernoulli(0.5))
R.SetTo(Variable.Bernoulli(0.2))
els.CloseBlock()

let W = Variable.New<bool>().Named(""wet"")

let ifs = Variable.If(S)
let ifr = Variable.If(R)
W.SetTo(Variable.Bernoulli(0.99))
ifr.CloseBlock()
let ifr2 = Variable.IfNot(R)
W.SetTo(Variable.Bernoulli(0.9))
ifr2.CloseBlock()
ifs.CloseBlock()

let ifs2 = Variable.IfNot(S)
let ifr3 = Variable.If(R)
W.SetTo(Variable.Bernoulli(0.9))
ifr3.CloseBlock()
let ifr4 = Variable.IfNot(R)
W.SetTo(Variable.Bernoulli(0.0))
ifr4.CloseBlock()
ifs2.CloseBlock()    

S.ObservedValue <- true
            ";




    }
}
