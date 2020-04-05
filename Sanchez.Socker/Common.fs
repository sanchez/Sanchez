module Sanchez.Socker.Common

open System.Net
open System.Net.Sockets
open System.Threading

let createPoster<'TInput> (encoder: 'TInput -> byte array) (cToken: CancellationToken) (client: Socket) =
    MailboxProcessor<'TInput>.Start((fun inbox ->
        async {
            while (true) do
                let! incoming = inbox.Receive()
                let bytes = Array.append (incoming |> encoder) ([| 0uy |])
                let sentBytes = client.Send(bytes, SocketFlags.None)
                if sentBytes <> (bytes |> Array.length) then
                    ()
                    
                ()
            
            return()
        }))
    
let handleSocketConnection<'T> (decoder: byte array -> 'T option) (sock: Socket) (actioner: Actioner<'T>) (cToken: CancellationToken) =
    let clientEP = sock.RemoteEndPoint.ToString()
    
    let mutable totalCollectedBytes = [||]
    
    let thread =
        async {
            do! Async.SwitchToNewThread()
            
            while (true) do
                let bytes = Array.create sock.Available 0uy
                
                let readCount = sock.Receive bytes
                let toppedBytes = bytes |> Array.take readCount
                totalCollectedBytes <- Array.append totalCollectedBytes toppedBytes
                
                let test =
                    totalCollectedBytes
                    |> Array.tryFindIndex ((=) 0uy)
                    |> Option.bind (fun x ->
                        let cmdBytes = totalCollectedBytes.[0..(x - 1)]
                        totalCollectedBytes <- totalCollectedBytes.[(x + 1)..]
                        
                        cmdBytes |> decoder)
                    |> Option.map (actioner.ExecuteAction clientEP)
                    
                ()
                
            return ()
        }
    Async.Start (thread, cToken)