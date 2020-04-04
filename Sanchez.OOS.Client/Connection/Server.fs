module Sanchez.OOS.Client.Connection.Server

open System.Net
open System.Net.Sockets
open System.Text
open System.Threading
open Sanchez.OOS.Core
open Sanchez.OOS.Core

let initializeServerListener (port: int) handleServerAction =
    let serverListener = UDPHandler.initializeListener UDPHandler.decodeServerAction handleServerAction port
    
    serverListener
    
let loadServers (port: int) (serverPort: int) (onServer: FullConnection list -> unit) =
    let mutable serverList = []
    let actioner = Actioner()
    
    actioner.AddActioner ("serverListener", (fun serverAddr -> function
        | Announcement serverDeets ->
            let conn = { FullConnection.Port = serverDeets.Port; IP = serverAddr; Version = serverDeets.Version }
            serverList <- conn::serverList
            onServer serverList
            Some ()
        | _ -> None))
    
    let handleServerAction (serverAddr: IPAddress) (a: ServerAction) =
        actioner.ExecuteAction(serverAddr, a)
    
    let listener = initializeServerListener port handleServerAction
    
    do port |> ClientAction.ServerRequest |> UDPHandler.broadcast UDPHandler.encodeClientAction serverPort
    
    actioner
    
let startClientCommunication port serverPort =
    let mutable serverList = []
    let onServer a = serverList <- a
    
    async {
        let actioner = loadServers port serverPort onServer
        
        do! Async.Sleep 1000
        
        return (actioner, serverList)
    }
    
let sendData (conn: FullConnection) =
    UDPHandler.sendRequest UDPHandler.encodeClientAction { IP = conn.IP; Port = conn.Port }
    
let broadcastData (serverPort: int) =
    UDPHandler.broadcast UDPHandler.encodeClientAction serverPort