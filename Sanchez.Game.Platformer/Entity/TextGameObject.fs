namespace Sanchez.Game.Platformer.Entity

open OpenToolkit.Mathematics
open Sanchez.Game.Platformer.Assets
open Sanchez.Game.Core
open FSharp.Data.UnitSystems.SI.UnitNames
open OpenToolkit.Graphics.OpenGL

type TextGameObject(name: string, id: int, shader, onUpdate, sqToFloat: float<sq> -> float32) =
    let conversion = Matrix4.CreateScale (1.<sq> |> sqToFloat)
    
    let mutable currentPosition = Position.create 0.<sq> 0.<sq>
    let mutable isAlive = true