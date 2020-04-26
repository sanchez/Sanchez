// Learn more about F# at http://fsharp.org

open System
open System.Threading
open Sanchez.Game.Core
open Sanchez.Game.Platformer
open Sanchez.OOS.Client.Window
open Sanchez.OOS.Client.Window.Assets
open Sanchez.OOS.Client.Window.Characters

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
    
    let cToken = new CancellationToken()
    
    MainPlayer.loadMainPlayer manager name
    
    manager.Run()
    
    0 // return an integer exit code
