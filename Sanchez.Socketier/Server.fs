module Sanchez.Socketier.Server

open System.Net
open System.Net.Sockets
open Sanchez.Data
open System.Threading
open Sanchez.Socketier.Errors

type SenderAddress =
    | Broadcast
    | SingleSend of string

let createServer<'TResult, 'TInput> (decoder: byte array -> 'TResult option) (encoder: 'TInput -> byte array) (port: int) (cToken: CancellationToken) =
    let actioner = Actioner<string * 'TResult>(cToken)
    
    let mutable connections = Map.empty
    
    let handleNewConnection (sock: Socket) =
        let ip = sock.RemoteEndPoint.ToString()
        let poster = Common.createPoster encoder cToken sock
        Common.handleSocketConnection decoder sock actioner cToken
        
        connections <- connections |> Map.add ip poster.Post
        
    let poster = MailboxProcessor<SenderAddress * 'TInput>.Start((fun inbox ->
        async {
            while (true) do
                match! inbox.Receive() with
                | (Broadcast, a) ->
                    connections
                    |> Map.toSeq
                    |> Seq.iter (snd >> ((|>) a))
                | (SingleSend addr, a) ->
                    connections
                    |> Map.tryFind addr
                    |> Option.map ((|>) a)
                    |> ignore
                    
                ()
                
            return ()
        }), cToken)
    
    try
//        let ipHostInfo = Dns.GetHostEntry(Dns.GetHostName())
        let ipAddress = IPAddress.Any
        let localEndPoint = IPEndPoint(ipAddress, port)
        let listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        
        listener.Bind(localEndPoint)
        listener.Listen(100)
        
        let server =
            async {
                do! Async.SwitchToNewThread()
                
                while (true) do
                    let sock = listener.Accept()
                    do handleNewConnection sock
                    
                    ()
                
                return ()
            }
        Async.Start(server, cToken)
        
        Ok (poster.Post, actioner)
        
    with
    | err -> Error FailedToBind
    |> AsyncResult.fromResult
    
    