module Sanchez.Serializer.Core.Converter

open System
open Microsoft.FSharp.Reflection
open System.Collections
open Sanchez.Serializer.Core.Types

let (|BooleanValue|_|) (arg: obj) =
    match arg with
    | :? bool as b -> b |> BooleanSymbol |> Some
    | _ -> None
    
let (|StringValue|_|) (arg: obj) =
    match arg with
    | :? string as s -> s |> StringSymbol |> Some
    | _ -> None
    
let (|NumberValue|_|) (arg: obj) =
    match arg with
    | :? double as i -> i |> NumberSymbol |> Some
    
    | :? int as i -> i |> double |> NumberSymbol |> Some
    | :? System.Int16 as i -> i |> double |> NumberSymbol |> Some
    | :? System.Int32 as i -> i |> double |> NumberSymbol |> Some
    | :? System.Int64 as i -> i |> double |> NumberSymbol |> Some
    
    | _ -> None
    
let (|ArrayValue|_|) (arg: obj) =
    match arg with
    | :? IEnumerable as i -> None
    | _ -> None
    
let (|UnionValue|_|) (converter: obj -> ConverterResult) (arg: obj) =
    if (FSharpType.IsUnion(arg.GetType(), true)) then
        let (name, fields) = FSharpValue.GetUnionFields(arg, arg.GetType())
        
        let body =
            if Array.length fields = 0 then
                () |> EmptySymbol |> Ok
            elif Array.length fields > 1 then
                fields
                |> Array.fold (fun acc x ->
                    match acc with
                    | Ok p ->
                        match converter x with
                        | Ok s -> s::p |> Ok
                        | Error err -> Error err
                    | err -> err) (Ok [])
                    |> Result.map ArraySymbol
            else fields |> Array.head |> converter
            
        body
        |> Result.map (fun x -> (name.Name, x) |> UnionSymbol)
        |> Some
    else None
    
let (|ObjectValue|_|) (converter: obj -> ConverterResult) (arg: obj) =
    let t = arg.GetType()
    
    let props =
        t.GetProperties()
        |> Seq.map (fun x ->
            match converter (x.GetValue(arg)) with
            | Ok s -> Ok (x.Name, s)
            | Error err -> Error err)
        |> Seq.fold (fun acc x ->
            match (acc, x) with
            | (Ok p, Ok c) -> c::p |> Ok
            | (Error err, _) -> Error err
            | (_, Error err) -> Error err) (Ok [])
        |> Result.map (Map.ofSeq >> ObjectSymbol)
        
    Some props
    
let rec convertFromType (a: obj) =
    match a with
    | BooleanValue b -> Ok b
    | NumberValue n -> Ok n
    | StringValue s -> Ok s
    | ArrayValue a -> Ok a
    | UnionValue convertFromType u -> u
    | ObjectValue convertFromType o -> o
    | other -> other.ToString() |> FailedToParseType |> Error
    
let loadTypeProperties (converter: Type -> Symbol -> ObjectConverterResult) (t: Type) (sym: Map<string, Symbol>) =
    FSharpType.GetRecordFields (t)
    |> Seq.map (fun x ->
        sym
        |> Map.tryFind x.Name
        |> function
            | Some s -> Ok s
            | None -> (x.Name, t.Name) |> MissingObjectKey |> Error
        |> Result.bind (fun sym -> converter x.PropertyType sym))
    |> Seq.fold (fun acc x ->
        match (acc, x) with
        | (Ok p, Ok c) -> Ok (c::p)
        | (Error err, _) -> Error err
        | (_, Error err) -> Error err) (Ok [])
    |> Result.map (fun x -> FSharpValue.MakeRecord(t, x |> List.rev |> List.toArray))
    
let (|RecordType|_|) (converter: Type -> Symbol -> ObjectConverterResult) (sym: Symbol) (t: Type) =
    if FSharpType.IsRecord (t) then
        match sym with
        | ObjectSymbol m -> loadTypeProperties converter t m |> Some
        | _ -> (t.Name, Symbol.toLabel sym) |> MismatchedTypes |> Error |> Some
    else None
    
let (|NumberType|_|) (sym: Symbol) (t: Type) =
    match sym with
    | NumberSymbol d ->
        if t = typeof<double> then
            d :> obj |> Ok
        elif t = typeof<int> then
            d |> int :> obj |> Ok
        else t.Name |> UnsupportedType |> Error
        |> Some
    | _ -> None
    
let (|StringType|_|) (sym: Symbol) (t: Type) =
    match sym with
    | StringSymbol s -> s :> obj |> Ok |> Some
    | _ -> None
    
let (|UnionType|_|) (converter: Type -> Symbol -> ObjectConverterResult) (sym: Symbol) (t: Type) =
    match sym with
    | UnionSymbol (caseName, unionSym) ->
        if FSharpType.IsUnion t then
            let case =
                FSharpType.GetUnionCases t
                |> Array.tryFind (fun x -> x.Name = caseName)
                |> function
                    | Some s -> Ok s
                    | None -> (caseName, t.Name) |> MissingUnionCase |> Error
            let fields =
                case
                |> Result.bind (fun x ->
                    let fields = x.GetFields()
                    if Array.length fields = 0 then
                        [] |> Ok
                    elif Array.length fields = 1 then
                        converter (Array.head fields).DeclaringType unionSym
                        |> Result.map (fun x -> [x])
                    else
                        match unionSym with
                        | ArraySymbol syms ->
                            Seq.zip fields syms
                            |> Seq.map (fun (fType, fSym) -> converter fType.DeclaringType fSym)
                            |> Seq.fold (fun acc x ->
                                match (acc, x) with
                                | (Ok p, Ok c) -> c::p |> Ok
                                | (Error err, _) -> Error err
                                | (_, Error err) -> Error err) (Ok [])
                        | sType -> (x.DeclaringType.Name, sType.GetType().Name) |> MismatchedTypes |> Error
                    |> Result.map (fun y -> (x, y))
                    )
                
            fields |> Result.map (fun (uCase, values) -> FSharpValue.MakeUnion(uCase, values |> List.toArray))
        else t.Name |> UnsupportedType |> Error
        |> Some
    | _ -> None
    
let (|EmptyType|_|) (sym: Symbol) (t: Type) =
    match sym with
    | EmptySymbol _ -> None
    | _ -> None
    
let rec convertToType (t: Type) (sym: Symbol) =
    match t with
    | RecordType convertToType sym r -> r
    | UnionType convertToType sym u -> u
    | NumberType sym n -> n
    | StringType sym s -> s
    | EmptyType sym e -> e
    | _ -> t.Name |> UnsupportedType |> Error