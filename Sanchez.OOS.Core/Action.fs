namespace Sanchez.OOS.Core

open Sanchez.Game.Core
open System
open System.Net
open System.Text

module Action =
    let staticVersion = "0.0.1"
    
type ServerAction =
    | Players of string[]
    | Pong of Guid
    | LocationUpdate of string*Position
    
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
    | Register of string
    | Ping of Guid
    | Location of Position
    
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