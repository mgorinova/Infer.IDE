
let BadWeather = Variable.Bernoulli(0.4)

let BadRoadConditions = Variable.New<bool>()
let HighNumberOfJourneys = Variable.New<bool>()

let HighSpeed = Variable.New<bool>()
let HighDangerLevel = Variable.New<bool>()

let HighNumberOfAccidents = Variable.New<bool>()

let ifBW = Variable.If(BadWeather)
BadRoadConditions.SetTo(Variable.Bernoulli(0.7))
HighNumberOfJourneys.SetTo(Variable.Bernoulli(0.1))
ifBW.CloseBlock()

let ifNotBW = Variable.IfNot(BadWeather)
BadRoadConditions.SetTo(Variable.Bernoulli(0.1))
HighNumberOfJourneys.SetTo(Variable.Bernoulli(0.8))
ifNotBW.CloseBlock()

let ifGRC = Variable.If(BadRoadConditions)
HighSpeed.SetTo(Variable.Bernoulli(0.1))

let ifHS = Variable.If(HighSpeed)
HighDangerLevel.SetTo(Variable.Bernoulli(0.9))
ifHS.CloseBlock()

let ifNotHS = Variable.IfNot(HighSpeed)
HighDangerLevel.SetTo(Variable.Bernoulli(0.7))
ifNotHS.CloseBlock()
ifGRC.CloseBlock()

let ifNotGRC = Variable.IfNot(BadRoadConditions)
HighSpeed.SetTo(Variable.Bernoulli(0.8))

let ifHS2 = Variable.If(HighSpeed)
HighDangerLevel.SetTo(Variable.Bernoulli(0.6))
ifHS2.CloseBlock()

let ifNotHS2 = Variable.IfNot(HighSpeed)
HighDangerLevel.SetTo(Variable.Bernoulli(0.5))
ifNotHS2.CloseBlock()
ifNotGRC.CloseBlock()

let ifHNJ = Variable.If(HighNumberOfJourneys)
let ifHDL1 = Variable.If(HighDangerLevel)
HighNumberOfAccidents.SetTo(Variable.Bernoulli(0.9))
ifHDL1.CloseBlock()

let ifNotHDL1 = Variable.IfNot(HighDangerLevel)
HighNumberOfAccidents.SetTo(Variable.Bernoulli(0.5))
ifNotHDL1.CloseBlock()
ifHNJ.CloseBlock()

let ifNotHNJ = Variable.IfNot(HighNumberOfJourneys)
let ifHDL2 = Variable.If(HighDangerLevel)
HighNumberOfAccidents.SetTo(Variable.Bernoulli(0.4))
ifHDL2.CloseBlock()

let ifNotHDL2 = Variable.IfNot(HighDangerLevel)
HighNumberOfAccidents.SetTo(Variable.Bernoulli(0.1))
ifNotHDL2.CloseBlock()
ifNotHNJ.CloseBlock()