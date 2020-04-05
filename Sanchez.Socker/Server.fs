module Sanchez.Socker.Server

open System.Net
open System.Net.Sockets
open System.Threading

let private handleSocketConnection<'T> (decoder: byte array -> 'T option) (sock: Socket) (actioner: Actioner<'T>) (cToken: CancellationToken) =
    let clientEP = sock.RemoteEndPoint :?> IPEndPoint
    
    let mutable totalCollectedBytes = [||]
    
    let thread =
        async {
            do! Async.SwitchToNewThread()
            
            while (true) do
                let available = sock.Available
                let bytes = Array.create available 0uy
                
                let readCount = sock.Receive(bytes)
                let toppedBytes = bytes |> Array.take readCount
                totalCollectedBytes <- Array.append totalCollectedBytes toppedBytes
                
                let test =
                    totalCollectedBytes
                    |> Array.tryFindIndex ((=) 0uy)
                    |> Option.bind (fun x ->
                        let cmdBytes = totalCollectedBytes.[0..(x - 1)]
                        totalCollectedBytes <- totalCollectedBytes.[(x + 1)..]
                        
                        cmdBytes |> decoder)
                    |> Option.map (actioner.ExecuteAction clientEP.Address)
                
                ()
            
            return ()
        }
    Async.Start(thread, cToken)
    
let createServer<'T> (decoder: byte array -> 'T option) (port: int) (cToken: CancellationToken) =
    let actioner = Actioner<'T>()
    
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
                    handleSocketConnection decoder sock actioner cToken
                    
                    ()
            with
            | err -> failwithf "Failed to server: %A" err
                    
            return ()
        }
        
    Async.Start(server, cToken)
        
    actioner