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

let Cloudy = Variable.Bernoulli(0.5)

let Sprinkler = Variable.New<bool>()
let Rain = Variable.New<bool>()

let ifc = Variable.If(Cloudy)
Sprinkler.SetTo(Variable.Bernoulli(0.1))
Rain.SetTo(Variable.Bernoulli(0.8))
ifc.CloseBlock()   
    
let els = Variable.IfNot(Cloudy)
Sprinkler.SetTo(Variable.Bernoulli(0.5))
Rain.SetTo(Variable.Bernoulli(0.2))
els.CloseBlock()

let Wet = Variable.New<bool>()

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

let bernoulli = Variable.Bernoulli(0.6)                             
                         
let gaussian = Variable.GaussianFromMeanAndPrecision(0.0, 0.5)                                 
         
let gamma = Variable.GammaFromShapeAndScale(1.0, 5.0)                            
                                                           
let poisson = Variable.Poisson(Variable.Observed(0.3))                                
let binomial = Variable.Binomial(10, Variable.Observed(0.5))                          
let beta = Variable.Beta(2.0, 2.0)                    

            ";


        public static string learningGaussian = @"open MicrosoftResearch.Infer
open MicrosoftResearch.Infer.Models
open MicrosoftResearch.Infer.Distributions
open MicrosoftResearch.Infer.Factors
open MicrosoftResearch.Infer.FSharp
        
let data = [|for i in [0..1] do yield Rand.Normal(0.0, 1.0)|]

let mean = Variable.GaussianFromMeanAndVariance(0.0, 100.0)
let precision = Variable.GammaFromShapeAndScale(1.0, 1.0)

let dataRange = Range(2)
let x = Variable.Array<double>(dataRange)

x.[dataRange] <- Variable.GaussianFromMeanAndPrecision(mean, precision).ForEach(dataRange)

//x.ObservedValue <- data

            ";


        public static string twoCoins = @"open MicrosoftResearch.Infer
open MicrosoftResearch.Infer.Models
open MicrosoftResearch.Infer.Distributions
open MicrosoftResearch.Infer.Factors
open MicrosoftResearch.Infer.FSharp

let coin1 = Variable.Bernoulli(0.5)
let coin2 = Variable.Bernoulli(0.5)

let bothHeads = coin1 &&& coin2

//bothHeads.ObservedValue <- false

            ";


        public static string truncGaussian = @"open MicrosoftResearch.Infer
open MicrosoftResearch.Infer.Models
open MicrosoftResearch.Infer.Distributions
open MicrosoftResearch.Infer.Factors
open MicrosoftResearch.Infer.FSharp

let x:Variable<double> = Variable.GaussianFromMeanAndVariance(0.0, 1.0)
let tresh = Variable.New<double>()
Variable.ConstrainTrue(x >> tresh)

//for tr in [for i in [0.0 .. 10.0] -> i/10.0] do          
//      tresh.ObservedValue <- tr    

            ";

        public static string mixGaussians = @"open MicrosoftResearch.Infer
open MicrosoftResearch.Infer.Models
open MicrosoftResearch.Infer.Distributions
open MicrosoftResearch.Infer.Factors
open MicrosoftResearch.Infer.FSharp
open MicrosoftResearch.Infer.Maths

let GenerateData nData =
    let trueM1,trueP1 = Vector.FromArray[|2.0;3.0|],PositiveDefiniteMatrix(Array2D.create2D [ [3.0;0.2];[0.2;2.0] ])
    let trueM2,trueP2 = Vector.FromArray[|7.0;5.0|],PositiveDefiniteMatrix(Array2D.create2D [ [2.0;0.4];[0.4;4.0] ])

    let trueVG1 = VectorGaussian.FromMeanAndPrecision(trueM1,trueP1)
    let trueVG2 = VectorGaussian.FromMeanAndPrecision(trueM2,trueP2)
    let truePi = 0.6
    let trueB = new Bernoulli(truePi) 
    Rand.Restart(12347) // Restart the infer.NET random number generator 
    Array.init nData (fun j -> if trueB.Sample()then trueVG1.Sample() else trueVG2.Sample())
 

let k = Range(2)

let means = Variable.ArrayInit k (fun k -> Variable.VectorGaussianFromMeanAndPrecision( Vector.Zero(2), PositiveDefiniteMatrix.IdentityScaledBy(2,0.01)))
   
let precs = Variable.ArrayInit k (fun k -> Variable.WishartFromShapeAndScale( 100.0, PositiveDefiniteMatrix.IdentityScaledBy(2,0.01)))

let weights = Variable.Dirichlet(k,[|1.0; 1.0|])
let n = new Range(300)

let z = Variable.ArrayInit n (fun i -> Variable.Discrete(weights))

let zinit = Array.init n.SizeAsInt (fun _ -> Discrete.PointMass(Rand.Int(k.SizeAsInt), k.SizeAsInt))
let _ = z.InitialiseTo(Distribution.Array(zinit))


let data = Variable.ArrayInit n (fun i -> Variable.SwitchExpr (z.[i]) (fun zi -> Variable.VectorGaussianFromMeanAndPrecision(means.[zi], precs.[zi])))

// Binding the data
//data.ObservedValue <- GenerateData(n.SizeAsInt)

// The inference
//let ie = InferenceEngine(VariationalMessagePassing())
//ie.ShowFactorGraph <- true
            ";





    }
}
