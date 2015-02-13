
// H - source-level proposition
// Y - measurements on recovered fragments
// X - measurements on control fragments
// tetha - background data

let H = Variable.Bernoulli(0.2)

let tetha = Variable.GaussianFromMeanAndVariance(2.0, 3.0)

let X = Variable.GaussianFromMeanAndVariance(tetha, 2.0)

let Y = Variable.New<double>()

let ifH = Variable.If(H)
Y.SetTo(Variable.GaussianFromMeanAndVariance(X, 3.2))
ifH.CloseBlock()

let ifNotH = Variable.IfNot(H)
Y.SetTo(Variable.GaussianFromMeanAndVariance(tetha, 5.0))
ifNotH.CloseBlock() 