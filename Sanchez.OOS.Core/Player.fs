namespace Sanchez.OOS.Core

open Sanchez.Game.Core

type PlayerDirection =
    | PlayerLeft
    | PlayerRight

type Player =
    {
        Name: string
        Location: Position
        Direction: PlayerDirection
        IsMoving: bool
    }
