module Checker

open Microsoft.FSharp.Compiler.SimpleSourceCodeServices
open Microsoft.FSharp.Compiler.Interactive.Shell
open Microsoft.FSharp.Compiler.SourceCodeServices
open Microsoft.FSharp.Compiler
open System.IO
open System

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

    if checkProjectResults.Errors.Length = 0 then printfn "No Errors" // should be empty
    else printfn "%s" (checkProjectResults.Errors.[0].ToString())


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
    let rec filter decls sourceLocation acc =
        match decls with 
        | [] -> acc
        | d::ds ->
            match d with
            | FSharpImplementationFileDeclaration.MemberOrFunctionOrValue(symbol, args, expression) ->

                let t = expression.Type.ToString()     
                if(t.StartsWith("type MicrosoftResearch.Infer.Models.Variable")) then 
                    let innerType = t.Substring(44)
                    filter ds sourceLocation ((symbol.CompiledName, "Variable", innerType)::acc)
                elif(t.StartsWith("type MicrosoftResearch.Infer.InferenceEngine")) then
                    //filter ds sourceLocation ((symbol.CompiledName, "Engine", "")::acc)
                    filter ds sourceLocation acc
                else filter ds sourceLocation acc                

            | FSharpImplementationFileDeclaration.InitAction(e) ->
                filter ds sourceLocation acc
            | _ -> filter ds sourceLocation acc


    let vars = filter declarations "" []

    let activeVars = 
        List.map (fun (name, _, _) -> 
                        //printfn "active: %s" name
                        name)  vars

    activeVars