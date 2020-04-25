module Sanchez.Socketier.Common

open System
open System.Net.Sockets
open System.Threading
open Sanchez.Data

let createPoster<'TInput> (encoder: 'TInput -> byte array) (cToken: CancellationToken) (client: Socket) =
    MailboxProcessor<'TInput>.Start((fun inbox ->
        async {
            while (true) do
                let! incoming = inbox.Receive()
                let bytes = Array.append (incoming |> encoder) ([| 0uy |])
                let! sendBytes = client.SendAsync(bytes |> ReadOnlyMemory, SocketFlags.None).AsTask() |> Async.AwaitTask
                if sendBytes <> (bytes |> Array.length) then
                    ()
                    
                ()
                
            return ()
        }), cToken)
    
let handleSocketConnection<'T> (decoder: byte array -> 'T option) (sock: Socket) (actioner: Actioner<string * 'T>) (cToken: CancellationToken) =
        let clientEP = sock.RemoteEndPoint.ToString()
        
        let mutable totalCollectedBytes = [||]
        
        let thread =
            async {
                do! Async.SwitchToNewThread()
                
                while (true) do
                    let bytes = Array.create sock.Available 0uy |> Memory
                    
                    let! readCount = sock.ReceiveAsync(bytes, SocketFlags.None).AsTask() |> Async.AwaitTask
                    let toppedBytes = bytes.ToArray() |> Array.take readCount
                    totalCollectedBytes <- Array.append totalCollectedBytes toppedBytes
                    
                    let test =
                        totalCollectedBytes
                        |> Array.tryFindIndex ((=) 0uy)
                        |> Option.bind (fun x ->
                            let cmdBytes = totalCollectedBytes.[0..(x - 1)]
                            totalCollectedBytes <- totalCollectedBytes.[(x + 1)..]
                            
                            cmdBytes |> decoder)
                        |> Option.map ((fun x -> actioner.ExecuteAction(clientEP, x)))
                    
                    ()
                return ()
            }
            
        Async.Start(thread, cToken)