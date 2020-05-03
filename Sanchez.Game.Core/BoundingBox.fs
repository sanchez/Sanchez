namespace Sanchez.Game.Core

type BoundingBox =
    {
        Min: Position
        Max: Position
    }

module BoundingBox =
    let create min max =
        {
            BoundingBox.Min = min
            Max = max
        }
        
    let isIntersecting (a: BoundingBox) (b: BoundingBox) =
        if (a.Max.X < b.Min.X || a.Min.X > b.Max.X) then false
        elif (a.Max.Y < b.Min.Y || a.Min.Y > b.Max.Y) then false
        else true