// Learn more about F# at http://fsharp.org

open System
open System.Text
open System.Threading
open Sanchez.Data
open Sanchez.Socketier

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

[<EntryPoint>]
let main argv =
    let cToken = new CancellationToken()
    
    let (poster, actioner) =
        Client.connectToServer serverDecoder clientEncoder "127.0.0.1" 25599 cToken
        |> AsyncResult.synchronously
    
    printfn "Hello World from F#!"
    0 // return an integer exit code
