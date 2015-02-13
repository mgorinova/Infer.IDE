let Chair = Variable.Bernoulli(0.5)
let Sport = Variable.Bernoulli(0.02)

let Worker = Variable.New<bool>()
let Back = Variable.New<bool>()
let Ache = Variable.New<bool>()

let ifC = Variable.If(Chair)
Worker.SetTo(Variable.Bernoulli(0.9))

let ifS1 = Variable.If(Sport)
Back.SetTo(Variable.Bernoulli(0.9))
ifS1.CloseBlock()

let ifNotS1 = Variable.IfNot(Sport)
Back.SetTo(Variable.Bernoulli(0.2))
ifNotS1.CloseBlock()
ifC.CloseBlock()

let ifNotC = Variable.IfNot(Chair)
Worker.SetTo(Variable.Bernoulli(0.01))

let ifS2 = Variable.If(Sport)
Back.SetTo(Variable.Bernoulli(0.9))
ifS2.CloseBlock()

let ifNotS2 = Variable.IfNot(Sport)
Back.SetTo(Variable.Bernoulli(0.01))
ifNotS2.CloseBlock()
ifNotC.CloseBlock()

let ifB = Variable.If(Back)
Ache.SetTo(Variable.Bernoulli(0.7))
ifB.CloseBlock()

let ifNotB = Variable.IfNot(Back)
Ache.SetTo(Variable.Bernoulli(0.1))
ifNotB.CloseBlock()