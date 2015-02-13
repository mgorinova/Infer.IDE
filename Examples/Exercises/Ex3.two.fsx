
let data1 = [|3.9; 4.1; 0.1; -3.9; -2.1; 0.9|]
let data2 = [|5.9; 8.1; 0.1; -10.9; -3.1; 0.9|]

let var1 = Variable.GammaFromShapeAndScale(2.0, 1.0)
let var2 = Variable.GammaFromShapeAndScale(2.0, 1.0)

let mean = Variable.GaussianFromMeanAndVariance(0.0, 1.0)

let n = Range(6)

let x = Variable.Array<double>(n)
x.[n] <- Variable.GaussianFromMeanAndVariance(mean, var1).ForEach(n)
x.ObservedValue <- data1
let y = Variable.Array<double>(n)
y.[n] <- Variable.GaussianFromMeanAndVariance(mean, var2).ForEach(n)



