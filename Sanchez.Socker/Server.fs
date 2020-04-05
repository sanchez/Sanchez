module Sanchez.Socker.Server

open System.Net
open System.Net.Sockets
open System.Threading
    
type SenderAddress =
    | Broadcast
    | SingleSend of string
    
let createServer<'TResult, 'TInput> (decoder: byte array -> 'TResult option) (encoder: 'TInput -> byte array) (port: int) (cToken: CancellationToken) =
    let actioner = Actioner<'TResult>()
    
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
                    |> Seq.map snd
                    |> Seq.iter ((|>) a)
                | (SingleSend addr, a) ->
                    connections
                    |> Map.tryFind addr
                    |> Option.map ((|>) a)
                    |> ignore
                
                ()
            
            return ()
        }))
    
    let server =
        async {
            do! Async.SwitchToNewThread()
            
            let ipHostInfo = Dns.GetHostEntry(Dns.GetHostName())
            let ipAddress = IPAddress.Any
            let localEndPoint = IPEndPoint(ipAddress, port)
            let listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            
            try
                listener.Bind(localEndPoint)
                listener.Listen(100)
                
                while (true) do
                    let sock = listener.Accept()
                    handleNewConnection sock
                    
                    ()
            with
            | err -> failwithf "Failed to server: %A" err
                    
            return ()
        }
        
    Async.Start(server, cToken)
        
    (actioner, poster.Post)