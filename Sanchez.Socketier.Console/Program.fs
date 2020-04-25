// Learn more about F# at http://fsharp.org

open System
open System.Diagnostics
open System.Diagnostics
open System.Text
open System.Threading
open Sanchez.Data
open Sanchez.Socketier
open Sanchez.Socketier.Server
open FSharp.Data.UnitSystems.SI.UnitNames
open Sanchez.Data

type ClientToServer =
    | Ping of Guid
    
let clientEncoder (a: ClientToServer) =
    Microsoft.FSharpLu.Json.Compact.serialize a
    |> Encoding.ASCII.GetBytes
let clientDecoder (a: byte array) =
    Encoding.ASCII.GetString a
    |> Microsoft.FSharpLu.Json.Compact.tryDeserialize<ClientToServer>
    |> function
        | Choice1Of2 res -> Some res
        | Choice2Of2 err -> None
    
type ServerToClient =
    | Pong of Guid
    
let serverEncoder (a: ServerToClient) =
    Microsoft.FSharpLu.Json.Compact.serialize a
    |> Encoding.ASCII.GetBytes
let serverDecoder (a: byte array) =
    Encoding.ASCII.GetString a
    |> Microsoft.FSharpLu.Json.Compact.tryDeserialize<ServerToClient>
    |> function
        | Choice1Of2 res -> Some res
        | Choice2Of2 err -> None
        
let createServer port cToken =
    asyncResult {
        do printfn "Creating Server..."
        let! (poster, actioner) = Server.createServer clientDecoder serverEncoder port cToken
        do printfn "Server Created Successfully"
        
        do! async {
            do! Async.Sleep 500
            return Ok ()
        }
        
        do actioner.AddActioner "pingpong" (fun (ip, a) ->
            match a with
            | Ping id ->
                poster (SenderAddress.SingleSend ip, Pong id)
                Some ())
        
        return ()
    }

[<EntryPoint>]
let main argv =
    let cToken = new CancellationToken()
    let port = 25599
    let host = "127.0.0.1"
    
    let createServerAndConnect () =
        createServer port cToken
        |> AsyncResult.bind (fun () -> Client.connectToServer serverDecoder clientEncoder host port cToken)
    
    let (poster, actioner) =
        Client.connectToServer serverDecoder clientEncoder host port cToken
        |> AsyncResult.bindError (fun _ -> createServerAndConnect())
        |> AsyncResult.inject (fun _ -> printfn "Finished network setup")
        |> AsyncResult.synchronously
        
    let mutable pingGuids: Map<Guid, Stopwatch> = Map.empty
    
    actioner.AddActioner "pingPong" (fun (ip, a) ->
        match a with
        | Pong id ->
            let stW = Map.find id pingGuids
            stW.Stop()
            
            let elapsed =
                stW.ElapsedTicks
                |> float
                |> (fun x -> x / (Stopwatch.Frequency |> float))
                |> ((*) 1.<second>)
            printfn "Received a ping of %fms" elapsed
            
            Some ())
        
    let scheduler = new Scheduler()
    scheduler.AddSchedule 0.2<second> (fun () ->
        let id = Guid.NewGuid()
        let stW = new Stopwatch()
        
        pingGuids <- pingGuids |> Map.add id stW
        stW.Start()
        id |> Ping |> poster
        true)
    scheduler.Run(0.01<second>)
    
    printfn "Hello World from F#!"
    0 // return an integer exit code
