namespace Sanchez.OOS.Client

open Sanchez.Game.Platformer

type Keys =
    | LeftKey
    | RightKey
    
module Keys =
    let loadKeys (manager: GameManager<_, Keys>) =
        manager.AddKeyBinding LeftKey "A"
        manager.AddKeyBinding RightKey "D"