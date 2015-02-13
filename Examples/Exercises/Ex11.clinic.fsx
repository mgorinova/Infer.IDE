let controlGroup =   Variable.Observed<bool>([| false; false; true; false; false |])
let treatedGroup = Variable.Observed<bool>([| true; false; true; true; true |])

let i = controlGroup.Range
let j = treatedGroup.Range

let isEffective = Variable.Bernoulli(0.5)

let ifeff = Variable.If(isEffective)
let probIfControl = (Variable.Beta(1.0, 1.0))
controlGroup.[i] <- Variable.Bernoulli(probIfControl).ForEach(i)
let probIfTreated = (Variable.Beta(1.0, 1.0))
treatedGroup.[j] <- Variable.Bernoulli(probIfTreated).ForEach(j)
ifeff.CloseBlock()

let ifnoteff = Variable.IfNot(isEffective)
let probAll = Variable.Beta(1.0, 1.0)
controlGroup.[i] <- Variable.Bernoulli(probAll).ForEach(i)
treatedGroup.[j] <- Variable.Bernoulli(probAll).ForEach(j)
ifnoteff.CloseBlock()