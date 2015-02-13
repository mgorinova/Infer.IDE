let a = Variable.GaussianFromMeanAndVariance(0.0, 0.09)
let b = Variable.GaussianFromMeanAndVariance(1.0, 3.0)
let c = Variable.GaussianFromMeanAndVariance(0.0, 5.0)
let constrain = a + b << c

let x = Variable.GaussianFromMeanAndVariance(1.0, 2.0)
let y = Variable.GaussianFromMeanAndVariance(-1.0, 0.3)
let constrain2 = y-x << c

Variable.ConstrainTrue(constrain)
Variable.ConstrainTrue(constrain2)