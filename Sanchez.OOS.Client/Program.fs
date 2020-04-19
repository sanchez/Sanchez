open System
open Sanchez.OOS.Client.Connection
open Sanchez.OOS.Core
open Sanchez.Socker
open System.Threading
open FSharp.Data.UnitSystems.SI.UnitNames
open Sanchez.Game.Core
open Sanchez.Game.Platformer
open Sanchez.OOS.Client
open Sanchez.OOS.Client.Assets
open Sanchez.OOS.Client.Characters

[<EntryPoint>]
let main argv =
    let serverPort = 25599
    printf "Enter Player Name: "
    let name = Console.ReadLine()
    
    let fixedSquareNumber = 2.f / 15.f
    let squareUnitToPx (sq: float<sq>) = sq |> float32 |> ((*) fixedSquareNumber)
    let manager = new GameManager<Textures, Keys>("Orbiting Outer Space", 800, 600, squareUnitToPx)
    Keys.loadKeys manager
    Textures.loadAllTextures manager
    
    Thread.Sleep 1000
    
    let cToken = new CancellationToken()
    let (poster, actioner) =
        Client.connectToServer ServerAction.decode ClientAction.encode "127.0.0.1" serverPort cToken
        |> Async.RunSynchronously
        
    Users.registerUser poster name
    
    let mutable playerPos = Position.create 0.<sq> 0.<sq>
    MainPlayer.loadMainPlayer manager name (fun x -> playerPos <- x)
    manager.AddSchedule (1.<second>) (fun () ->
        playerPos |> Location |> poster
        true)
    
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
    manager.AddSchedule (1.<second>) (fun () ->
        let id = Guid.NewGuid()
        pingCounters <- pingCounters |> Map.add id DateTime.UtcNow
        id |> Ping |> poster
        true)
    
//    actioner.AddActioner "missing" (fun ip ->
//        printfn "Missing action binding: %A" >> Some)
    
    manager.Run()
    0
