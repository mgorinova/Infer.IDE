open MicrosoftResearch.Infer
open MicrosoftResearch.Infer.Models
open MicrosoftResearch.Infer.Distributions
open MicrosoftResearch.Infer.Factors
open MicrosoftResearch.Infer.FSharp

let Cloudy = Variable.Bernoulli(0.5)

let Sprinkler = Variable.New<bool>()
let Rain = Variable.New<bool>()

let ifc = Variable.If(Cloudy)
Rain.SetTo(Variable.Bernoulli(0.8))

let ifr = Variable.If(Rain)
Sprinkler.SetTo(Variable.Bernoulli(0.01))
ifr.CloseBlock()
let ifnotr = Variable.IfNot(Rain)
Sprinkler.SetTo(Variable.Bernoulli(0.4))
ifnotr.CloseBlock()

ifc.CloseBlock()   
    
let ifnotc = Variable.IfNot(Cloudy)
Rain.SetTo(Variable.Bernoulli(0.2))

let ifr2 = Variable.If(Rain)
Sprinkler.SetTo(Variable.Bernoulli(0.005))
ifr2.CloseBlock()
let ifnotr2 = Variable.IfNot(Rain)
Sprinkler.SetTo(Variable.Bernoulli(0.1))
ifnotr2.CloseBlock()

ifnotc.CloseBlock()

let Wet = Variable.New<bool>()

let ifs = Variable.If(Sprinkler)
let ifr3 = Variable.If(Rain)
Wet.SetTo(Variable.Bernoulli(0.99))
ifr3.CloseBlock()
let ifnotr3 = Variable.IfNot(Rain)
Wet.SetTo(Variable.Bernoulli(0.9))
ifnotr3.CloseBlock()
ifs.CloseBlock()

let ifs2 = Variable.IfNot(Sprinkler)
let ifr4 = Variable.If(Rain)
Wet.SetTo(Variable.Bernoulli(0.9))
ifr4.CloseBlock()
let ifnotr4 = Variable.IfNot(Rain)
Wet.SetTo(Variable.Bernoulli(0.0))
ifnotr4.CloseBlock()
ifs2.CloseBlock()    

//Sprinkler.ObservedValue <- true
             