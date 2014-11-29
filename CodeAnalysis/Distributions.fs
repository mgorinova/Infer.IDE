module Distributions

open Core.Operators
open FSharp.Charting
open FSharp.Charting.ChartTypes
open System.Windows.Forms
open System.Windows.Forms.Integration

let pi = System.Math.PI

let gaussian mean variance x = Operators.exp(-(x-mean)*(x-mean)/((2.0)*variance))/Operators.sqrt((2.0)*pi*variance)

let draw (winForm : WindowsFormsHost) (distribution:string) = 

    //let chart = Chart.Line [ for i in 0 .. 10 -> (i,i*i) ]
    //let chart = Chart.Spline([for i in -2.0 .. 2.0 -> gaussian 0.0 1.0 i]).WithXAxis(Min = -2.0, Max = 2.0)
    //winForm.Child <- new ChartControl(chart)

    let arr = [| '('; ')' |]

    let distType = distribution.Split(arr)
    printfn "1[%s] 2[%s] 3[%s]" distType.[0] distType.[1] distType.[2]

    match distType.[0] with
    | "Bernoulli" -> 

        let prob = System.Convert.ToDouble(distType.[1])
        let data = [("True", (float) prob); ("False", (1.0 - ((float) prob)))]
        let chart = Chart.Column(data)
        winForm.Child <- new ChartControl(chart)


    (*| "Gaussian" ->
    | "Gamma" ->
    | "Beta" ->
    | "Binomial" -> 
    | "ConjugateDirichlet" ->
    | "GammaPower" ->
    | "NonconjugateGaussian" ->
    | "Poisson" ->
    | "TruncatedGaussian" ->
    | "WrappedGaussian" ->*)
    | _ -> failwith "unexpected"