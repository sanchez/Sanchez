module Sanchez.Socketier.Client

open System.Net
open System.Net.Sockets
open System.Threading
open Sanchez.Data
open Sanchez.Socketier.Errors

let private openConnection (serverAddr: string) (port: int) (cToken: CancellationToken) =
    async {
        let ipHostInfo = Dns.GetHostEntry(serverAddr)
        let ipAddress =
            ipHostInfo.AddressList
            |> Seq.pick (fun x ->
                if x.AddressFamily = AddressFamily.InterNetwork then
                    Some x
                else None)
        let remoteEP = IPEndPoint(ipAddress, port)
        
        let client = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
        
        do! client.ConnectAsync(remoteEP) |> Async.AwaitTask
        
        return client
    }
    
let connectToServer<'TResult, 'TInput> (decoder: byte array -> 'TResult option) (encoder: 'TInput -> byte array) (serverAddr: string) (port: int) (cToken: CancellationToken) =
    let actioner = Actioner<string*'TResult>(cToken)

    asyncResult {
        let! client =
            openConnection serverAddr port cToken
            |> AsyncResult.fromAsync HostNotFound
        
        let poster = Common.createPoster encoder cToken client
        do Common.handleSocketConnection decoder client actioner cToken
     
        return (poster.Post, actioner)
    }