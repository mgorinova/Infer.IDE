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
open MicrosoftResearch.Infer.Maths


let var = Variable.GaussianFromMeanAndVariance(0.0, 1.0)

let gaussian mean variance x = Gaussian.FromMeanAndVariance(mean, variance).GetLogProb x |> Operators.exp
let gamma shape scale x = Gamma.FromShapeAndScale(shape, scale).GetLogProb x |> Operators.exp
let beta trueCount falseCount x = Beta(trueCount, falseCount).GetLogProb x |> Operators.exp    
let poisson (mean : float) x = Poisson(mean).GetLogProb x |> Operators.exp

let dirichlet arr x = 
    let v:Vector = Vector.FromArray(arr)
    Dirichlet.FromMeanLog(v).GetLogProb x |> Operators.exp

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

    | "Gaussian.PointMass" -> 
        winForm.Child <- null
        printfn "%s is a point mass var!!!" varName       

    | "Gaussian" ->        
            
        let (mean, variance) = 
            let getFromArray (arr:string[]) = 
                (System.Convert.ToDouble(arr.[0]), System.Convert.ToDouble(arr.[1]))

            getFromArray (distType.[1].Split([|','|]))
            
        //printfn "%f, %f" mean variance
        
        let rightBound = Math.Round(mean + 2.5*sqrt(variance), 1) //mean + 2.0*variance         
        let leftBound  = Math.Round(2.0*mean - rightBound, 1)
        let top        = 1.0
        let step       = if Math.Round((rightBound - leftBound)/30.0, 1) > 0.0 then Math.Round((rightBound - leftBound)/30.0, 1) else 0.01
        //let xAxisStyle = new LabelStyle(TruncatedLabels = false, IsStaggered = true, FontSize = float 8.0f)

        printfn "lb:%A, rb:%A, step:%A" leftBound rightBound step

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

        let rightBound = Math.Round(10.0*shape, 1)
        let step = if rightBound/50.0 > 0.0 then Math.Round(rightBound/50.0, 1) else 0.01

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

        let rightBound = Math.Round(2.0*trueCount, 1)
        let step = if rightBound/30.0 > 0.0 then Math.Round(rightBound/30.0, 1) else 0.05

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
                  |> Chart.WithYAxis(Min = 0.0, Max = 1.0, LabelStyle = yAxisStyle, MinorGrid = grid, MajorGrid = grid)    

        winForm.Child <- new ChartControl(chart)



    | "Poisson" ->
        let mean = System.Convert.ToDouble distType.[1]

        let rightBound = (int) (3.0*mean)
        let chart = Chart.Point ([for i in 0..rightBound -> (i, (poisson mean i))], Name = varName)
                  |> Chart.WithTitle(Text = varName, InsideArea = true, Alignment = ContentAlignment.TopCenter, Docking = Docking.Top)
                  |> Chart.WithXAxis(LabelStyle = xAxisStyle, MinorGrid = grid, MajorGrid = grid)
                  |> Chart.WithYAxis(LabelStyle = yAxisStyle, MinorGrid = grid, MajorGrid = grid)

        winForm.Child <- new ChartControl(chart)
        
    | "Dirichlet" ->
        let floatArr = distType.[1].Split(' ')
                       |> Array.map (System.Convert.ToDouble)

        // as dirichlet is a multivariate distribution, i is not a float, but a vector... need to think of a clever way to visualise
        // from different angles or something. Maybe ask Advait?
        
        (*let chart = Chart.Line ([for i in 0.0 .. 0.1 .. 10.0 -> (i, (dirichlet floatArr i))], Name = varName)
                    |> Chart.WithTitle(Text = varName, InsideArea = true, Alignment = ContentAlignment.TopCenter, Docking = Docking.Top)
                    |> Chart.WithXAxis(Min = 0.0, Max = 10.0, LabelStyle = xAxisStyle, MinorGrid = grid, MajorGrid = grid)
                    |> Chart.WithYAxis(Min = 0.0, Max = 10.0, LabelStyle = yAxisStyle, MinorGrid = grid, MajorGrid = grid)*)
        
        winForm.Child <- null
        printfn "%A" distribution

    | "Wishart" ->
        winForm.Child <- null
        printfn "%A" distribution

    | "VectorGaussian" ->
        winForm.Child <- null
        printfn "%A" distribution
    (*
    | "ConjugateDirichlet" ->
    | "GammaPower" ->
    | "NonconjugateGaussian" ->
    | "TruncatedGaussian" ->
    | "WrappedGaussian" ->*)
    | _ -> 

        // TODO: implement visualisation for arrays?
        if (distribution.StartsWith "[") then 
            winForm.Child <- null
            printfn "data array"
        else 
            winForm.Child <- null
            printfn "\n%s is an unexpected distribution..." distType.[0]
            printfn "\n***\n%s\n***\n" distribution
            
            printfn "unexpected distributions"

            //failwith "unexpected"