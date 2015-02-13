//let i = new Range(5) // number of patients in the control group
//let j = new Range(5) // number of patients in the treated group

let controlGroup =   Variable.Observed<bool>([| false; false; true; false; false |]).Named("controlGroup")
//let controlGroup = Variable.Array<bool>(i)

let treatedGroup = Variable.Observed<bool>([| true; false; true; true; true |]).Named("treatedGroup")
//let treatedGroup = Variable.Array<bool>(j)

let i = controlGroup.Range
let j = treatedGroup.Range

// To determine whether the treament is effective, we will build two 
// models of this data: 
//      one which assumes the treatment has an effect and 
//      one which doesn't.  
// To perform Bayesian model selection, we need to introduce a boolean 
// random variable which switches between the two models
let isEffective = Variable.Bernoulli(0.5)

let mutable probIfTreated = Variable.New<double>()
let mutable probIfControl = Variable.New<double>()

begin
    use ifeff = Variable.If(isEffective)
    // Model if treatment is effective
    probIfControl <- (Variable.Beta(1.0, 1.0))
    controlGroup.[i] <- Variable.Bernoulli(probIfControl).ForEach(i)
    probIfTreated <- (Variable.Beta(1.0, 1.0))
    treatedGroup.[j] <- Variable.Bernoulli(probIfTreated).ForEach(j)
end

begin
    use ifnoteff = Variable.IfNot(isEffective)
    // Model if treatment is not effective
    let probAll = Variable.Beta(1.0, 1.0)
    controlGroup.[i] <- Variable.Bernoulli(probAll).ForEach(i)
    treatedGroup.[j] <- Variable.Bernoulli(probAll).ForEach(j)
end