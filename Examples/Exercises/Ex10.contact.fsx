

// H - Bob doesn't like Alice
// FibresA - quantity of fibres found 
//           on Alice's clothing
// FibresB - quantity of fibres found 
//           on Bob's clothing
//      FibresA,B = 0 if none
//      FibresA,B = 1 if few
//      FibresA,B = 2 if many

// Contact - contact
//      Contact = 0 if light contact
//      Contact = 1 if medium contact
//      Contact = 2 if heavy contact
//      Contact = 3 if no contact

let H = Variable.Bernoulli(0.3)

let Contact = Variable.New<int>()
let FibresA = Variable.New<int>()
let FibresB = Variable.New<int>()

let ifH = Variable.If(H)
Contact.SetTo(Variable.Discrete([|10.0; 30.0; 60.0; 0.0|]))

let c1 = Variable.Case(Contact, 0)
FibresA.SetTo(Variable.Discrete([|0.2; 0.6; 0.2|]))
FibresB.SetTo(Variable.Discrete([|0.2; 0.6; 0.2|]))
c1.CloseBlock()
let c2 = Variable.Case(Contact, 1)
FibresA.SetTo(Variable.Discrete([|0.1; 0.4; 0.5|]))
FibresB.SetTo(Variable.Discrete([|0.1; 0.4; 0.5|]))
c2.CloseBlock()
let c3 = Variable.Case(Contact, 2)
FibresA.SetTo(Variable.Discrete([|0.02; 0.28; 0.7|]))
FibresB.SetTo(Variable.Discrete([|0.02; 0.28; 0.7|]))
c3.CloseBlock()
let c4 = Variable.Case(Contact, 3)
FibresA.SetTo(Variable.Discrete([|1.0; 0.0; 0.0|]))
FibresB.SetTo(Variable.Discrete([|1.0; 0.0; 0.0|]))
c4.CloseBlock()

ifH.CloseBlock()

let ifNotH = Variable.IfNot(H)
Contact.SetTo(Variable.Discrete([|0.0; 0.0; 0.0; 100.0|]))
FibresA.SetTo(Variable.Discrete([|0.97; 0.02; 0.01|]))
FibresB.SetTo(Variable.Discrete([|0.97; 0.02; 0.01|]))
ifNotH.CloseBlock()


