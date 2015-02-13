open MicrosoftResearch.Infer
open MicrosoftResearch.Infer.Models
open MicrosoftResearch.Infer.Distributions
open MicrosoftResearch.Infer.Factors
open MicrosoftResearch.Infer.FSharp

let coin1 = Variable.Bernoulli(0.5)
let coin2 = Variable.Bernoulli(0.3)
let oneAndTwo = coin1 &&& coin2

let coin3 = Variable.Bernoulli(0.1)
let coin4 = Variable.Bernoulli(0.8)
let threeAndFour = coin3 &&& coin4

let oneAndThree = coin1 &&& coin3

            