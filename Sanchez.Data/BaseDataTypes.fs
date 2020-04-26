namespace Sanchez.Data

[<AutoOpen>]
module Option =
    type OptionBuilder() =
        member this.Bind(v, f) = Option.bind f v
        member this.Return v = Some v
        member this.ReturnFrom o = o
        member this.Zero () = None
        
    let opt = OptionBuilder()

[<AutoOpen>]
module Result =
    type ResultBuilder() =
        member this.Bind(v, f) = Result.bind f v
        member this.Return v = Ok v
        member this.ReturnFrom o = o
        
    let result = ResultBuilder()
    
type AsyncResult<'T, 'E> = Async<Result<'T, 'E>>
    
module AsyncResult =
    let bind f v =
        async.Bind(v, function
            | Ok x -> f x
            | Error err -> err |> Error |> async.Return)
    let bindError f v =
        async.Bind(v, function
            | Ok x -> x |> Ok |> async.Return
            | Error err -> f err)
    
    let map f v =
        bind (f >> Ok >> async.Return) v
    let mapError f v =
        bindError (f >> Error >> async.Return) v
        
    let inject f v =
        async.Bind(v, function
            | Ok x -> f x; Ok x
            | Error err -> Error err
            >> async.Return)
        
    let fromValue a =
        a |> Ok |> async.Return
    let fromAsync (err: 'e) (a: Async<'a>) =
        async.Bind(a |> Async.Catch, function
            | Choice1Of2 x -> Ok x
            | Choice2Of2 _ -> Error err
            >> async.Return)
    let fromOption err a =
        match a with
        | Some s -> Ok s
        | None -> Error err
        |> async.Return
    let fromOptionAsync err a =
        fromAsync err a
        |> bind (fromOption err)
    let fromResult a =
        a |> async.Return
        
    let synchronously a =
        Async.RunSynchronously a
        |> function
            | Ok x -> x
            | Error err -> err |> sprintf "AsyncResult Failed with: %A" |> failwith
        
    let inline (>>=) v f = bind f v
    let inline (<!>) v f = map f v
    
[<AutoOpen>]
module AsyncResultBuilder =
    type AsyncResultBuilder() =
        member this.Bind(v, f) = AsyncResult.bind f v
        member this.Return v = AsyncResult.fromValue v
        member this.ReturnFrom o = o |> async.ReturnFrom
        member this.Zero() =
            async {
                return Ok ()
            }
        
    let asyncResult = AsyncResultBuilder()