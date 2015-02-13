open MicrosoftResearch.Infer
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

let z = Variable.Array<int>(n)
let data : VariableArray<Vector> = Variable.Array(n)

begin
    use tmp = Variable.ForEach(n) 
    z.[n] <- Variable.Discrete(weights)
    begin
        use tmp2 = Variable.Switch(z.[n])
        data.[n] <- Variable.VectorGaussianFromMeanAndPrecision(means.[z.[n]], precs.[z.[n]])
    end
end

data.ObservedValue <- GenerateData(n.SizeAsInt)

//let zinit = Array.init n.SizeAsInt (fun _ -> Discrete.PointMass(Rand.Int(k.SizeAsInt), k.SizeAsInt))
//let _ = z.InitialiseTo(Distribution.Array(zinit))


//let data = Variable.ArrayInit n (fun i -> Variable.SwitchExpr (z.[i]) (fun zi -> Variable.VectorGaussianFromMeanAndPrecision(means.[zi], precs.[zi])))

// Binding the data
//data.ObservedValue <- GenerateData(n.SizeAsInt)

// The inference
//let ie = InferenceEngine(VariationalMessagePassing())
//ie.ShowFactorGraph <- true
            