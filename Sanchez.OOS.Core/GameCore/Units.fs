namespace Sanchez.OOS.Core.GameCore

[<Measure>] type px
[<Measure>] type sq

type SquareUnit = float<sq>

module Units =
    let pixelsPerSquare: float<px sq^-1> = 12.<px/sq>
    let convertSquareToPixels (x: float<sq>) = x / pixelsPerSquare
    let (|SquareUnitValue|) (x: SquareUnit) = x |> float