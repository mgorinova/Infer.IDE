module Distributions

open Core.Operators
open FSharp.Charting
open FSharp.Charting.ChartTypes
open System.Windows.Forms
open System.Windows.Forms.Integration
open System.Drawing
open System

open MicrosoftResearch.Infer.Distributions
open MicrosoftResearch.Infer.Models


let var = Variable.GaussianFromMeanAndVariance(0.0, 1.0)

let gaussian mean variance x = Gaussian.FromMeanAndVariance(mean, variance).GetLogProb x |> Operators.exp
let gamma shape scale x = Gamma.FromShapeAndScale(shape, scale).GetLogProb x |> Operators.exp
let beta trueCount falseCount x = Beta(trueCount, falseCount).GetLogProb x |> Operators.exp
    
let poisson (mean : float) x = Poisson(mean).GetLogProb x |> Operators.exp

let draw (winForm : WindowsFormsHost) (distribution:string) (varName:string) = 

    //let chart = Chart.Line [ for i in 0 .. 10 -> (i,i*i) ]
    //let chart = Chart.Spline([for i in -2.0 .. 2.0 -> gaussian 0.0 1.0 i]).WithXAxis(Min = -2.0, Max = 2.0)
    //winForm.Child <- new ChartControl(chart)

    let arr = [| '('; ')' |]

    let distType = distribution.Split(arr)
    //printfn "1[%s] 2[%s] 3[%s]" distType.[0] distType.[1] distType.[2]
        
    let arr = [| '('; ')' |]

    let distType = distribution.Split(arr)
    //printfn "1[%s] 2[%s] 3[%s]" distType.[0] distType.[1] distType.[2]

    let xAxisStyle = new LabelStyle(FontSize = 8.0)
    let yAxisStyle = new LabelStyle(FontSize = 8.0)
    

    let grid = new Grid(LineColor = Color.LightGray) 

    match distType.[0] with
    | "Bernoulli" -> 

        let prob = System.Convert.ToDouble(distType.[1])
        let data = [("True", prob); ("False", (1.0 - (prob)))]

        let chart = Chart.Column(data, Name = varName)//, Labels = (["True"; "False"]))
                    |> Chart.WithTitle(Text = varName, InsideArea = true, Alignment = ContentAlignment.TopCenter, Docking = Docking.Top)
                    |> Chart.WithXAxis(Min = 0.0, Max = 3.0, LabelStyle = xAxisStyle, MinorGrid = grid, MajorGrid = grid, Enabled = true)
                    |> Chart.WithYAxis(Min = 0.0, Max = 1.0, LabelStyle = yAxisStyle, MinorGrid = grid, MajorGrid = grid)                 

        winForm.Child <- new ChartControl(chart)

    //| "Gaussian.PointMass" ->             

    | "Gaussian" ->        
            
        let (mean, variance) = 
            let getFromArray (arr:string[]) = 
                (System.Convert.ToDouble(arr.[0]), System.Convert.ToDouble(arr.[1]))

            getFromArray (distType.[1].Split([|','|]))
            
        //printfn "%f, %f" mean variance
        
        let rightBound = mean + Math.Round(2.5*sqrt(variance), 1) //mean + 2.0*variance 
        let leftBound  = 2.0*mean - rightBound
        let top        = 1.0
        let step       = Math.Round((rightBound - leftBound)/30.0, 1)
        //let xAxisStyle = new LabelStyle(TruncatedLabels = false, IsStaggered = true, FontSize = float 8.0f)

        let chart = Chart.Line ([for i in leftBound .. step .. rightBound -> (i, (gaussian mean variance i))], Name = varName)
                    |> Chart.WithTitle(Text = varName, InsideArea = true, Alignment = ContentAlignment.TopCenter, Docking = Docking.Top)
                    |> Chart.WithXAxis(Min = leftBound, Max = rightBound, LabelStyle = xAxisStyle, MinorGrid = grid, MajorGrid = grid, Enabled = true)
                    |> Chart.WithYAxis(Min = 0.0, Max = top, LabelStyle = yAxisStyle, MinorGrid = grid, MajorGrid = grid)  

        winForm.Child <- new ChartControl(chart)

    | "Gamma" ->

        let (shape, scale) = 
            let getFromArray (arr:string[]) = 
                (System.Convert.ToDouble(arr.[0]), System.Convert.ToDouble(arr.[1]))

            getFromArray (distType.[1].Split([|','|]))
            
        //printfn "%f, %f" shape scale

        let rightBound = 10.0*shape
        let step = rightBound/50.0

        let chart = Chart.Line ([for i in 0.0 .. step .. rightBound -> (i, (gamma shape scale i))], Name = varName)
                    |> Chart.WithTitle(Text = varName, InsideArea = true, Alignment = ContentAlignment.TopCenter, Docking = Docking.Top)
                    |> Chart.WithXAxis(Min = 0.0, Max = rightBound, LabelStyle = xAxisStyle, MinorGrid = grid, MajorGrid = grid)
                    |> Chart.WithYAxis(Min = 0.0, Max = 1.0, LabelStyle = yAxisStyle, MinorGrid = grid, MajorGrid = grid)  

        winForm.Child <- new ChartControl(chart)

    | "Beta" ->

        let (trueCount, falseCount) = 
            let getFromArray (arr:string[]) = 
                (System.Convert.ToDouble(arr.[0]), System.Convert.ToDouble(arr.[1]))

            getFromArray (distType.[1].Split([|','|]))
            
        //printfn "%f, %f" trueCount falseCount

        let rightBound = 2.0*trueCount
        let step = rightBound/30.0

        let chart = Chart.Line ([for i in 0.0 .. step .. rightBound -> (i, (beta trueCount falseCount i))], Name = varName)
                    |> Chart.WithTitle(Text = varName, InsideArea = true, Alignment = ContentAlignment.TopCenter, Docking = Docking.Top)
                    |> Chart.WithXAxis(Min = 0.0, Max = rightBound, LabelStyle = xAxisStyle, MinorGrid = grid, MajorGrid = grid)
                    |> Chart.WithYAxis(Min = 0.0, Max = trueCount, LabelStyle = yAxisStyle, MinorGrid = grid, MajorGrid = grid)  

        winForm.Child <- new ChartControl(chart)

    | "Discrete" ->
        let data = (distType.[1]).Split([|' '|])

        let chart = Chart.Column(data, Name = varName)
                  |> Chart.WithTitle(Text = varName, InsideArea = true, Alignment = ContentAlignment.TopCenter, Docking = Docking.Top)
                  |> Chart.WithXAxis(LabelStyle = xAxisStyle, MinorGrid = grid, MajorGrid = grid)
                  |> Chart.WithYAxis(LabelStyle = yAxisStyle, MinorGrid = grid, MajorGrid = grid)    

        winForm.Child <- new ChartControl(chart)

    | "Poisson" ->
        let mean = System.Convert.ToDouble distType.[1]

        let rightBound = 20.0*mean
        let step = rightBound/10.0

        let chart = Chart.Point ([for i in 0..20 -> (i, (poisson mean i))], Name = varName)
                  |> Chart.WithTitle(Text = varName, InsideArea = true, Alignment = ContentAlignment.TopCenter, Docking = Docking.Top)
                  |> Chart.WithXAxis(LabelStyle = xAxisStyle, MinorGrid = grid, MajorGrid = grid)
                  |> Chart.WithYAxis(LabelStyle = yAxisStyle, MinorGrid = grid, MajorGrid = grid)

        winForm.Child <- new ChartControl(chart)

    (*
    | "ConjugateDirichlet" ->
    | "GammaPower" ->
    | "NonconjugateGaussian" ->
    | "TruncatedGaussian" ->
    | "WrappedGaussian" ->*)
    | _ -> 
        printfn "\n%s is an unexpected distribution..." distType.[0]
        printfn "\n***\n%s\n***\n" distribution

        // TODO: implement visualisation for arrays?
        if (distribution.StartsWith "[") then printfn "data array"
        else failwith "unexpected"