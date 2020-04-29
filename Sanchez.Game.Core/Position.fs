namespace Sanchez.Game.Core

open System
open Sanchez.Game.Core.Units

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
    static member (-) (p1: Position, p2: Position) =
        {
            Position.X = p1.X - p2.X
            Y = p1.Y - p2.Y
        }
    static member (-) (p1: Position, p2: SquareUnit) =
        {
            Position.X = p1.X - p2
            Y = p1.Y - p2   
        }
    static member (*) (p1: Position, SquareUnitValue p2) =
        {
            Position.X = p1.X * p2
            Y = p1.Y * p2
        }
    static member (/) (p1: Position, SquareUnitValue p2) =
        {
            Position.X = p1.X / p2
            Y = p1.Y / p2
        }
        
module Position =
    let create x y = { Position.X = x; Y = y }
    let distance (a: Position) (b: Position) =
        let xDiff = a.X - b.X |> float
        let yDiff = a.Y - b.Y |> float
        Math.Sqrt((xDiff * xDiff) + (yDiff * yDiff)) * 1.<sq>
        
    let mag (a: Position) =
        let x = a.X |> float
        let y = a.Y |> float
        Math.Sqrt((x * x) + (y * y)) * 1.<sq>
        
    let unitify (a: Position) =
        let dist = mag a |> float
        {
            Position.X = a.X / dist
            Y = a.Y / dist
        }