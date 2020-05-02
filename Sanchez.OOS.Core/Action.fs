namespace Sanchez.OOS.Core

open System
open System.Text
open Sanchez.OOS.Core.World

module Action =
    let staticVersion = "0.0.1"
    
type ServerAction =
    | Pong of Guid
    | PlayerUpdate of Player list
    | SinglePlayerUpdate of string*Player
    | WorldUpdate of World
    
module ServerAction =
    let encode (a: ServerAction) =
        Microsoft.FSharpLu.Json.Compact.serialize a
        |> Encoding.ASCII.GetBytes
    let decode (a: byte array) =
        Encoding.ASCII.GetString a
        |> Microsoft.FSharpLu.Json.Compact.tryDeserialize<ServerAction>
        |> function
            | Choice1Of2 res -> Some res
            | Choice2Of2 err -> None
    
type ClientAction =
    | Ping of Guid
    | PlayerUpdate of Player
    
module ClientAction =
    let encode (a: ClientAction) =
        Microsoft.FSharpLu.Json.Compact.serialize a
        |> Encoding.ASCII.GetBytes
    let decode (a: byte array) =
        Encoding.ASCII.GetString a
        |> Microsoft.FSharpLu.Json.Compact.tryDeserialize<ClientAction>
        |> function
            | Choice1Of2 res -> Some res
            | Choice2Of2 err -> None