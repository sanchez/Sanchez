module Sanchez.OOS.Client.Connection.Server

open System.Net
open System.Net.Sockets
open System.Text
open Sanchez.OOS.Core

let handleServerAction (serverAddr: IPAddress) (a: ServerAction) =
    ()

let initializeServerListener (port: int) =
    let serverListener = UDPHandler.initializeListener UDPHandler.decodeServerAction handleServerAction port
    
    serverListener
    
let loadServers (port: int) (serverPort: int) =
    let listener = initializeServerListener port
    
    port |> ClientAction.ServerRequest |> UDPHandler.broadcast UDPHandler.encodeClientAction serverPort
    
    ()