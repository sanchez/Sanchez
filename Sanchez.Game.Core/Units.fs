namespace Sanchez.Game.Core

[<Measure>] type frame

[<Measure>] type px
[<Measure>] type sq

type SquareUnit = float<sq>

module Units =
    let (|SquareUnitValue|) (x: SquareUnit) = x |> float