
let data2014 = [|35.0; 79.0; 45.0; 67.0; 54.0; 58.0; 51.0; 71.0; 61.0; 63.0|]
let data2013 = [|25.0; 69.0; 35.0; 57.0; 44.0; 48.0; 41.0; 61.0; 51.0; 53.0|]

let n = Range(10)

let Fair = Variable.Bernoulli(0.5)

let mean2014 = Variable.GaussianFromMeanAndVariance(50.0, 20.0)
let mean2013 = Variable.GaussianFromMeanAndVariance(50.0, 20.0)


let Marks2014 = Variable.Array<double>(n)
let Marks2013 = Variable.Array<double>(n)

let ifF = Variable.If(Fair)
let prec = Variable.GammaFromShapeAndScale(1.0, 1.0)

Marks2014.[n] <- Variable.GaussianFromMeanAndPrecision(mean2014, prec).ForEach(n)
Marks2013.[n] <- Variable.GaussianFromMeanAndPrecision(mean2013, prec).ForEach(n)
ifF.CloseBlock()


let ifNotF = Variable.IfNot(Fair)
let prec2014 = Variable.GammaFromShapeAndScale(1.0, 1.0)
let prec2013 = Variable.GammaFromShapeAndScale(1.0, 1.0)

Marks2014.[n] <- Variable.GaussianFromMeanAndPrecision(mean2014, prec2014).ForEach(n)
Marks2013.[n] <- Variable.GaussianFromMeanAndPrecision(mean2013, prec2013).ForEach(n)
ifNotF.CloseBlock()

Marks2014.ObservedValue <- data2014
Marks2013.ObservedValue <- data2013