## IDE for Probabilistic Programming

This IDE helps you to write, edit and understand probabilistic programs written using the <a href = "http://research.microsoft.com/en-us/um/cambridge/projects/infernet/">Infer.NET</a> framework through <a href="http://fsharp.org/">F\#</a>. It shows visualisations of the graphical models and random variable distributions, which update when a change to the source code has been made.

The paper describing this work and the experiment with human participants that was conducted [1], could be found <a href='https://www.cl.cam.ac.uk/~as2006/files/gorinova_2016_probabilistic.pdf'>here</a>.

To build the software, place follow the instructions below.

### Getting started

1. Firstly, you will need F/# in order to use this, so if you don't already have it on your machine, install it from <a href = "https://www.microsoft.com/en-us/download/details.aspx?id=48179">here</a>.
2. Download the repository.
3. Download Infer.NET from the <a href="http://research.microsoft.com/en-us/um/cambridge/projects/infernet/">Infer.NET website</a>. (NB. The original version of the software uses a slightly modified version of Infer.NET, which is not available online. This will introduce a bug in the visualisations. To be fixed in the future.)
4. Create a new folder, named `infer` in `...\Infer.IDE\Infer.IDE\bin\Debug\`. Copy the Infer.NET dll files in this new folder. 
5. Open the solution. Add references to the `Backend` and `FSharpBackend` projects in the Infer.IDE project. 
6. Build and run.

### Current support

* The software is currently limited to one F\# script file only.
* It could visualise Bayesian network models. 
* The following Infer.NET distributions have visualisation support: Bernoulli, Discrete, Poisson, Gaussian, Gamma, Gamma.PointMass, Beta. Support for other distributions could be added through `Distributions.fs` in `FSharpBackend`. If a model which has an unsupported random variable is described in the IDE, the model, together with all its supported random variables will be visualised, and the unsupported one will be ignored.

[1] Maria I. Gorinova, Advait Sarkar, Alan F. Blackwell, and Don Syme. A Live, Multiple-Representation Probabilistic Programming Environment for Novices. In Proceedings of CHI 2016.
