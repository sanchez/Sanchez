namespace Sanchez.Game.Platformer

open FSharp.Data.UnitSystems.SI.UnitNames

type GameManager(title, width, height) =
    let onLoad () =
        ()
        
    let onUpdate (timeSince: float<second>) =
        ()
        
    let onRender () =
        ()
    
    let game = new Game(title, width, height, onLoad, onUpdate, onRender)
    
    member this.LoadTexture () =
        ()
        
    member this.LoadGameObject () =
        ()
        
    member this.Launch () =
        ()
        
    member this.Run () =
        game.Run()