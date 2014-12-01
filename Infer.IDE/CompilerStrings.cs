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

let Cloudy = Variable.Bernoulli(0.5).Named(""Cloudy"")

let Sprinkler = Variable.New<bool>().Named(""Sprinkler"")
let Rain = Variable.New<bool>().Named(""Rain"")

let ifc = Variable.If(Cloudy)
Sprinkler.SetTo(Variable.Bernoulli(0.1))
Rain.SetTo(Variable.Bernoulli(0.8))
ifc.CloseBlock()   
    
let els = Variable.IfNot(Cloudy)
Sprinkler.SetTo(Variable.Bernoulli(0.5))
Rain.SetTo(Variable.Bernoulli(0.2))
els.CloseBlock()

let Wet = Variable.New<bool>().Named(""Wet"")

let ifs = Variable.If(Sprinkler)
let ifr = Variable.If(Rain)
Wet.SetTo(Variable.Bernoulli(0.99))
ifr.CloseBlock()
let ifr2 = Variable.IfNot(Rain)
Wet.SetTo(Variable.Bernoulli(0.9))
ifr2.CloseBlock()
ifs.CloseBlock()

let ifs2 = Variable.IfNot(Sprinkler)
let ifr3 = Variable.If(Rain)
Wet.SetTo(Variable.Bernoulli(0.9))
ifr3.CloseBlock()
let ifr4 = Variable.IfNot(Rain)
Wet.SetTo(Variable.Bernoulli(0.0))
ifr4.CloseBlock()
ifs2.CloseBlock()    

//Sprinkler.ObservedValue <- true
            ";


        public static string allDistributions = @"open MicrosoftResearch.Infer
open MicrosoftResearch.Infer.Models
open MicrosoftResearch.Infer.Distributions
open MicrosoftResearch.Infer.Factors
open MicrosoftResearch.Infer.FSharp

let bernoulli = Variable.Bernoulli(0.6).Named(""bernoulli"")                              
                         
let gaussian = Variable.GaussianFromMeanAndPrecision(0.0, 0.5).Named(""gaussian"")                                    
         
let gamma = Variable.GammaFromShapeAndScale(1.0, 5.0).Named(""gamma"")                             
                                                           
let poisson = Variable.Poisson(Variable.Observed(0.3)).Named(""poisson"")                                  
let binomial = Variable.Binomial(10, Variable.Observed(0.5)).Named(""binomial"")                           
let beta = Variable.Beta(2.0, 2.0).Named(""beta"")                          


            ";




    }
}
