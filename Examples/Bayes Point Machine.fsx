let noise = 0.1
let len = Variable.New<int>()
let j = Range(len)
let x = Variable.Array<Vector>(j)
let w0 = VectorGaussian(Vector.Zero(3), PositiveDefiniteMatrix.Identity(3))
let w = Variable.Random<Vector>(w0)
let y = Variable.AssignVariableArray (Variable.Array<bool>(j)) j  (fun j -> Variable.IsPositive (Variable.GaussianFromMeanAndVariance(Variable.InnerProduct(w, x.[j]), noise))) 

// The data
let incomes = [|63.0; 16.0; 28.0; 55.0; 22.0; 20.0|]
let ages = [|38.0; 23.0; 40.0; 27.0; 18.0; 40.0|]
let willBuy = [|true; false; true; true; false; false|]
let dataLen = willBuy.Length
let xdata = Array.init dataLen (fun i -> Vector.FromArray([|incomes.[i]; ages.[i]; 1.0|]))
x.ObservedValue <- xdata
y.ObservedValue <- willBuy
len.ObservedValue <- dataLen 