let visitedAsia = Variable.Bernoulli(0.01)
let smokes = Variable.Bernoulli(0.3)

let hasBronchitis = Variable.New<bool>()
let hasLungCancer = Variable.New<bool>()
let hasTuberculosis = Variable.New<bool>()

let ifAsia = Variable.If(visitedAsia)
hasTuberculosis.SetTo(Variable.Bernoulli(0.05))
ifAsia.CloseBlock()
let ifNotAsia = Variable.IfNot(visitedAsia)
hasTuberculosis.SetTo(Variable.Bernoulli(0.01))
ifNotAsia.CloseBlock()

let ifSmokes = Variable.If(smokes)
hasLungCancer.SetTo(Variable.Bernoulli(0.1))
hasBronchitis.SetTo(Variable.Bernoulli(0.3))
ifSmokes.CloseBlock()
let ifNotSmokes = Variable.IfNot(smokes)
hasLungCancer.SetTo(Variable.Bernoulli(0.01))
hasBronchitis.SetTo(Variable.Bernoulli(0.2))
ifNotSmokes.CloseBlock()

let hasTuberculosisOrCancer = hasTuberculosis ||| hasLungCancer

let hasDyspnea = Variable.New<bool>()
let abnormalXRay = Variable.New<bool>()

let ifTorC = Variable.If(hasTuberculosisOrCancer)
abnormalXRay.SetTo(Variable.Bernoulli(0.9))

let ifBronchitis = Variable.If(hasBronchitis)
hasDyspnea.SetTo(Variable.Bernoulli(0.95))
ifBronchitis.CloseBlock()
let ifNotBronchitis = Variable.IfNot(hasBronchitis)
hasDyspnea.SetTo(Variable.Bernoulli(0.8))
ifNotBronchitis.CloseBlock()
ifTorC.CloseBlock()

let ifNotTorC = Variable.IfNot(hasTuberculosisOrCancer)
abnormalXRay.SetTo(Variable.Bernoulli(0.1))

let ifBronchitis2 = Variable.If(hasBronchitis)
hasDyspnea.SetTo(Variable.Bernoulli(0.5))
ifBronchitis2.CloseBlock()
let ifNotBronchitis2 = Variable.IfNot(hasBronchitis)
hasDyspnea.SetTo(Variable.Bernoulli(0.01))
ifNotBronchitis2.CloseBlock()
ifNotTorC.CloseBlock()



