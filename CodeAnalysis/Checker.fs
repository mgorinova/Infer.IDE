module Checker

open Microsoft.FSharp.Compiler.SimpleSourceCodeServices
open Microsoft.FSharp.Compiler.Interactive.Shell
open Microsoft.FSharp.Compiler.SourceCodeServices
open Microsoft.FSharp.Compiler
open System.IO
open System

type RandomVariable(n:string, l:int) =
    member this.name = n
    member this.line = l

let check location input =
    
    let checker = FSharpChecker.Create(keepAssemblyContents=true)

    let parseAndCheckSingleFile (inp) = 
    
        // Get context representing a stand-alone (script) file
        let projOptions = 
            checker.GetProjectOptionsFromScript(location, inp)
            |> Async.RunSynchronously

        checker.ParseAndCheckProject(projOptions)
        |> Async.RunSynchronously


    let checkProjectResults = 
        parseAndCheckSingleFile(input)

    if checkProjectResults.Errors.Length = 0 then 
        //printfn "No Errors"     
        ()
    else 
        failwith (checkProjectResults.Errors.[0].ToString())


    let checkedFile = checkProjectResults.AssemblyContents.ImplementationFiles.[0]

    let declarations =  
        // NB: We assume there is only one file in the project and only one 
        // entity in this file. The IDE won't support multiple files at this
        // stage. Multiple entities in a file will be considered.
        // FIXME: maybe add multiple entities support. 
        match checkedFile.Declarations.[0] with 
        | FSharpImplementationFileDeclaration.Entity (e,subDecls) -> subDecls
        | _ -> failwith "unexpected"


    // TODO: assign names to Infer.NET vars automatically (.Named("-//-")) 
    let filterAndName decls pathToSource =

        let source = (input.Split([|'\n'|]))
                   |> Array.map (fun (x:string) -> String.Concat(x, "\n"))
                   |> ref

        let addName (location:Range.range) name =

            //FIXME: declarations of variables in loops will break horribly with that approach
            //FIXME: add support for declarations on multiple lines
            let newLine = (!source).[location.StartLine - 1].Insert((location.StartColumn - 1), " (").Insert((location.EndColumn + 2), (" ).Named(\"" + name + "\")")  )
            (!source).[location.StartLine - 1]  <- newLine
            
        let rec filter decls acc =           
            match decls with 
            | [] -> acc
            | d::ds ->
                match d with
                | FSharpImplementationFileDeclaration.MemberOrFunctionOrValue(symbol, args, expression) ->

                    let t = expression.Type.ToString()     
                    if(t.StartsWith("type MicrosoftResearch.Infer.Models.Variable")) then 
                        let innerType = t.Substring(44)
                        addName (expression.Range) (symbol.CompiledName) 
                        filter ds ((symbol.CompiledName, expression.Range.StartLine - 9)::acc)
                    elif(t.StartsWith("type MicrosoftResearch.Infer.Models.Range")) then 
                        addName (expression.Range) (symbol.CompiledName)
                        filter ds acc
                    elif(t.StartsWith("type MicrosoftResearch.Infer.InferenceEngine")) then
                        filter ds acc
                    else filter ds acc                

                | FSharpImplementationFileDeclaration.InitAction(e) ->
                    filter ds acc
                | _ -> filter ds acc

        let result = filter decls []        
        let newSource = String.Concat(!source)
        File.WriteAllText(pathToSource, newSource) 
        result

    let vars = filterAndName declarations location

    let activeVars = List.map (fun (name, location) -> RandomVariable(name, location))  vars
                  |> List.rev  
    activeVars