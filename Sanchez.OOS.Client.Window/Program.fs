// Learn more about F# at http://fsharp.org

open System
open System.Diagnostics
open System.Threading
open Sanchez.Data
open Sanchez.Game.Core
open Sanchez.Game.Platformer
open Sanchez.OOS.Client.Window
open Sanchez.OOS.Client.Window.Assets
open Sanchez.OOS.Client.Window.Characters
open Sanchez.OOS.Core
open Sanchez.OOS.Server
open Sanchez.Socketier
open FSharp.Data.UnitSystems.SI.UnitNames

[<EntryPoint>]
let main argv =
    let serverPort = 25599
    let host = "127.0.0.1"
    printf "Enter Player Name: "
    let name = Console.ReadLine()
    
    let fixedSquareNumber = 2.f / 15.f
    let squareUnitToPx (sq: float<sq>) = sq |> float32 |> ((*) fixedSquareNumber)
    let manager = new GameManager<Textures, Keys>("Orbiting Outer Space", 800, 600, squareUnitToPx)
    Keys.loadKeys manager
    Textures.loadAllTextures manager
    
    let cToken = new CancellationToken()
    
    let mutable playerPosition = Position.create 0.<sq> 0.<sq>
    let mutable playerDir = PlayerRight
    let mutable isMoving = false
    let onPlayerPosition (pos, dir, mov) =
        playerPosition <- pos
        playerDir <- dir
        isMoving <- mov
    MainPlayer.loadMainPlayer manager name onPlayerPosition
    
    let connectToServer() = Client.connectToServer ServerAction.decode ClientAction.encode host serverPort cToken
    let createServerAndConnect () =
        ServerManager.createServer serverPort cToken
        |> AsyncResult.bind connectToServer
    let (poster, actioner) =
        connectToServer()
        |> AsyncResult.bindError (fun _ -> createServerAndConnect())
        |> AsyncResult.synchronously
        
    NetworkManager.setupNetworkManager name poster actioner manager
        
    manager.AddSchedule 0.05<second> (fun () ->
        let pl =
            {
                Player.Name = name
                Location = playerPosition
                Direction = playerDir
                IsMoving = isMoving
            }
        pl |> ClientAction.PlayerUpdate |> poster
        true)
        
    let mutable pingGuids: Map<Guid, Stopwatch> = Map.empty
    manager.AddSchedule 0.5<second> (fun () ->
        let id = Guid.NewGuid()
        let stW = new Stopwatch()
        
        pingGuids <- pingGuids |> Map.add id stW
        stW.Start()
        id |> Ping |> poster
        
        true)
    actioner.AddActioner "pingpong" (fun (ip, a) ->
        match a with
        | Pong id ->
            let stW = Map.find id pingGuids
            stW.Stop()
            
            let elapsed =
                stW.ElapsedTicks
                |> float
                |> (fun x -> x / (Stopwatch.Frequency |> float))
                |> ((*) 1.<second>)
            printfn "Received a ping of %fms" elapsed
            Some ()
        | _ -> None)
    
    manager.Run()
    
    0 // return an integer exit code
