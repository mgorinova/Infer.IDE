
let CourseDifficult = Variable.Bernoulli(0.6)
let StudentIntelligent = Variable.Bernoulli(0.7)

// Grade = 0 if First Class
// Grade = 1 if Upper Second Class
// Grade = 2 if Lower Second CLass
// Grade = 3 if Third Class
// Grade = 4 if Unsuccessful
let Grade = Variable.New<int>()

let ifD = Variable.If(CourseDifficult)
let ifI1 = Variable.If(StudentIntelligent)
Grade.SetTo(Variable.Discrete([|0.2; 0.4; 0.2; 0.15; 0.05|]))
ifI1.CloseBlock()

let ifNotI1 = Variable.IfNot(StudentIntelligent)
Grade.SetTo(Variable.Discrete([|0.05; 0.2; 0.25; 0.35; 0.15|]))
ifNotI1.CloseBlock()
ifD.CloseBlock()

let ifNotD = Variable.IfNot(CourseDifficult) 
let ifI2 = Variable.If(StudentIntelligent)
Grade.SetTo(Variable.Discrete([|0.3; 0.35; 0.25; 0.09; 0.01|]))
ifI2.CloseBlock()

let ifNotI2 = Variable.IfNot(StudentIntelligent)
Grade.SetTo(Variable.Discrete([|0.1; 0.25; 0.45; 0.1; 0.1|]))
ifNotI2.CloseBlock()
ifNotD.CloseBlock()

let ReferenceLetter = Variable.New<bool>()

// ReferenceLetter is declared for all possible 
// values that Grade could take, using the 
// Variable.Case construct
let cs0 = Variable.Case(Grade, 0)
ReferenceLetter.SetTo(Variable.Bernoulli(1.0))
cs0.CloseBlock()

let cs1 = Variable.Case(Grade, 1)
ReferenceLetter.SetTo(Variable.Bernoulli(0.8))
cs1.CloseBlock()

let cs2 = Variable.Case(Grade, 2)
ReferenceLetter.SetTo(Variable.Bernoulli(0.7))
cs2.CloseBlock()

let cs3 = Variable.Case(Grade, 3)
ReferenceLetter.SetTo(Variable.Bernoulli(0.2))
cs3.CloseBlock()
   
let cs4 = Variable.Case(Grade, 4)
ReferenceLetter.SetTo(Variable.Bernoulli(0.0))
cs4.CloseBlock()   
     
     
     
     
     