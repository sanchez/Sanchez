module Sanchez.OOS.Core.UDPHandler

open System.Net
open System.Net.Sockets
open System.Text

let encodeServerAction (a: ServerAction) =
    Microsoft.FSharpLu.Json.Compact.serialize a
    |> Encoding.ASCII.GetBytes
let decodeServerAction (b: byte array) =
    Encoding.ASCII.GetString b
    |> Microsoft.FSharpLu.Json.Compact.deserialize<ServerAction>
    
let encodeClientAction (a: ClientAction) =
    Microsoft.FSharpLu.Json.Compact.serialize a
    |> Encoding.ASCII.GetBytes
let decodeClientAction (a: byte array) =
    Encoding.ASCII.GetString a
    |> Microsoft.FSharpLu.Json.Compact.deserialize<ClientAction>

let private handleAllIncoming<'T> (decoder: byte array -> 'T) (handleAction: IPAddress -> 'T -> unit) (port: int) =
    async {
        let listener = new UdpClient(port)
        listener.EnableBroadcast <- true
        let mutable groupEP = new IPEndPoint(IPAddress.Any, port)
        
        try
            while (true) do
                let bytes = listener.Receive(&groupEP)
                let action = decoder bytes
                handleAction groupEP.Address action
        with
        | _ -> ()
    }
    
let initializeListener<'T> (decoder: byte array -> 'T) (handleAction: IPAddress -> 'T -> unit) (port: int) =
    async {
        do! Async.SwitchToNewThread()
        
        return! handleAllIncoming decoder handleAction port
    }
    |> Async.Start
    
let sendRequest<'T> (encoder: 'T -> byte array) (conn: ServerConnection) (msg: 'T) =
    let bytes = encoder msg
    let s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
    s.EnableBroadcast <- true
    let ep = new IPEndPoint(conn.IP, conn.Port)
    s.SendTo(bytes, ep) |> ignore
    
let broadcast<'T> (encoder: 'T -> byte array) (port: int) (msg: 'T) =
    let connDeets = { ServerConnection.IP = IPAddress.Parse("255.255.255.255"); Port = port }
    sendRequest encoder connDeets msg