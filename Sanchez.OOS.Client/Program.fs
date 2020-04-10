open System
open Sanchez.OOS.Client
open Sanchez.OOS.Client.Connection
open Sanchez.OOS.Core
open Sanchez.Socker
open System.Threading
open FSharp.Data.UnitSystems.SI.UnitNames

[<EntryPoint>]
let main argv =
    let serverPort = 25599
    
    Thread.Sleep 1000
    
    let cToken = new CancellationToken()
    let (poster, actioner) =
        Client.connectToServer ServerAction.decode ClientAction.encode "127.0.0.1" serverPort cToken
        |> Async.RunSynchronously
        
    Users.registerUser poster "daniel"
    
    use game = new Game(800, 600, poster)
    
    let mutable pingCounters = Map.empty
    actioner.AddActioner "pingpong" (fun ip ->
        function
            | Pong id ->
                let current = DateTime.UtcNow
                let (dt: DateTime) = pingCounters |> Map.find id
                let t = current.Subtract(dt)
                printfn "Current Server Ping: %.3fms" t.TotalMilliseconds
                Some ()
            | _ -> None)
    game.AddSchedule (1.<second>) (fun () ->
        let id = Guid.NewGuid()
        pingCounters <- pingCounters |> Map.add id DateTime.UtcNow
        id |> Ping |> poster
        true)
    
    
//    actioner.AddActioner "missing" (fun ip ->
//        printfn "Missing action binding: %A" >> Some)
    
    game.Run()
    
    0
