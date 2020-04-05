module Sanchez.Socker.Client

open System.Net
open System.Net.Sockets
open System.Threading

let private createPoster<'TInput> (encoder: 'TInput -> byte array) (cToken: CancellationToken) (client: Socket) =
    MailboxProcessor<'TInput>.Start((fun inbox ->
        let rec messageLoop () =
            async {
                let! incoming = inbox.Receive()
                
                let bytes = Array.append (incoming |> encoder) ([| 0 |> byte |])
                
                let sentBytes = client.Send(bytes, SocketFlags.None)
                
                if sentBytes <> (bytes |> Array.length) then
                    ()
                
                return! messageLoop()
            }
            
        messageLoop()))
    
let private openConnection (serverAddr: string) (port: int) (cToken: CancellationToken) =
    async {
        let ipHostInfo = Dns.GetHostEntry(serverAddr)
        let ipAddress = ipHostInfo.AddressList.[0]
        let remoteEP = IPEndPoint(ipAddress, port)
        
        let client = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
        
        do! client.ConnectAsync(remoteEP) |> Async.AwaitTask
        
        return client
    }

let connectToServer<'TResult, 'TInput> (decoder: byte array -> 'TResult option) (encoder: 'TInput -> byte array) (serverAddr: string) (port: int) (cToken: CancellationToken) =
    let actioner = Actioner<'TResult>()
    
    async {
        let! client = openConnection serverAddr port cToken
        
        let poster = createPoster encoder cToken client
        
        return (poster.Post, actioner)
    }