namespace Sanchez.Game.Core

open FSharp.Data.UnitSystems.SI.UnitNames

[<Measure>] type frame

[<Measure>] type FPS = frame/second

[<Measure>] type px
[<Measure>] type sq

type SquareUnit = float<sq>

module Units =
    let (|SquareUnitValue|) (x: SquareUnit) = x |> float