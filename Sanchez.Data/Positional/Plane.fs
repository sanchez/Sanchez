namespace Sanchez.Data.Positional

open System

type Plane<'T> =
    {
        BasePoint: Vector<'T>
        Normal: Vector<'T>
    }
    
module Plane =
    let create (a: Vector<float32>) (b: Vector<float32>) (c: Vector<float32>) =
        let r = b - a
        let s = c - a
        {
            Plane.BasePoint = a
            Normal = r +* s
        }
        
    let projectToPlane (pt: PointVector<float>) (pl: Plane<float>) =
        let mapped = (Vector.create pt.X pt.Y 0.) +* pl.Normal
        mapped + pl.BasePoint
        
    let pointOnPlane (origin: Vector<float32>) (dir: Vector<float32>) (pl: Plane<float32>) =
        let normDir = dir |> Vector.map float |> Vector.normalize |> Vector.map float32
        let diff = normDir .* pl.Normal
        if diff = 0.f then None
        else
            let d = ((pl.BasePoint - origin) .* pl.Normal) / diff
            let test = normDir * d
            origin + (normDir * d) |> Some