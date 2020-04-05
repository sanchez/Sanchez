namespace Sanchez.OOS.Core.GameCore

open Sanchez.OOS.Core.GameCore.Units

type Position =
    {
        X: SquareUnit
        Y: SquareUnit
    }
    static member (+) (p1: Position, p2: Position) =
        {
            Position.X = p1.X + p2.X
            Y = p1.Y + p2.Y
        }
    static member (+) (p1: Position, p2: SquareUnit) =
        {
            Position.X = p1.X + p2
            Y = p1.Y + p2
        }
    static member (*) (p1: Position, SquareUnitValue p2) =
        {
            Position.X = p1.X * p2
            Y = p1.Y * p2
        }
    
module Position =
    let create x y =
        { Position.X = x; Y = y }