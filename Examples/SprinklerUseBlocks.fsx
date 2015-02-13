open MicrosoftResearch.Infer
open MicrosoftResearch.Infer.Models
open MicrosoftResearch.Infer.Distributions
open MicrosoftResearch.Infer.Factors
open MicrosoftResearch.Infer.FSharp

let Cloudy = Variable.Bernoulli(0.5)

let Sprinkler = Variable.New<bool>()
let Rain = Variable.New<bool>()

begin
    use ifc = Variable.If(Cloudy) 
    Sprinkler.SetTo(Variable.Bernoulli(0.1))
    Rain.SetTo(Variable.Bernoulli(0.8))
end
    
begin    
    use els = Variable.IfNot(Cloudy)
    Sprinkler.SetTo(Variable.Bernoulli(0.5))
    Rain.SetTo(Variable.Bernoulli(0.2))
end

let Wet = Variable.New<bool>()

begin
    use ifs = Variable.If(Sprinkler)
    begin
        use ifr = Variable.If(Rain)
        Wet.SetTo(Variable.Bernoulli(0.99))
    end
    begin
        use ifr2 = Variable.IfNot(Rain)
        Wet.SetTo(Variable.Bernoulli(0.9))
    end
end

begin
    use ifs2 = Variable.IfNot(Sprinkler)
    begin
        use ifr3 = Variable.If(Rain)
        Wet.SetTo(Variable.Bernoulli(0.9))
    end
    begin
        use ifr4 = Variable.IfNot(Rain)
        Wet.SetTo(Variable.Bernoulli(0.0))
    end
end 

//Sprinkler.ObservedValue <- true
            