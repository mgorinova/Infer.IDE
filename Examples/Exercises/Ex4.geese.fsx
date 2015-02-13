

let Climber = Variable.Bernoulli(0.1)
let Geese = Variable.Bernoulli(0.5)

let Alarm = Variable.New<bool>()

let ifG = Variable.If(Geese)
let ifC1 = Variable.If(Climber)
Alarm.SetTo(Variable.Bernoulli(0.99))
ifC1.CloseBlock()

let ifNotC1 = Variable.IfNot(Climber)
Alarm.SetTo(Variable.Bernoulli(0.2))
ifNotC1.CloseBlock()
ifG.CloseBlock()

let ifNotG = Variable.IfNot(Geese)
let ifC2 = Variable.If(Climber)
Alarm.SetTo(Variable.Bernoulli(0.9))
ifC2.CloseBlock()

let ifNotC2 = Variable.IfNot(Climber)
Alarm.SetTo(Variable.Bernoulli(0.01))
ifNotC2.CloseBlock()
ifNotG.CloseBlock()

let HatredLodge = Variable.New<bool>()
let DeathMetalLodge = Variable.New<bool>()

let ifA = Variable.If(Alarm)
HatredLodge.SetTo(Variable.Bernoulli(1.0))
DeathMetalLodge.SetTo(Variable.Bernoulli(0.6))
ifA.CloseBlock()

let ifNotA = Variable.IfNot(Alarm)
HatredLodge.SetTo(Variable.Bernoulli(0.2))
DeathMetalLodge.SetTo(Variable.Bernoulli(0.01))
ifNotA.CloseBlock()

