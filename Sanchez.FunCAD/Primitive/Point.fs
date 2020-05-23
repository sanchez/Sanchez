namespace Sanchez.FunCAD.Primitive

open Sanchez.Data.Positional
open Sanchez.ThreeWin
open Sanchez.FunCAD

type Point =
    {
        Position: Vector<decimal<mm>>
    }

module Point =
    let create x y z =
        {
            Point.Position = Vector.create x y z
        }
        
    let rasterize shaders color size (point: Point) =
        let pointVerts =
            [
                point.Position
            ]
            |> List.map (Vector.map (decimal >> float32))
        let pointColorize _ = color
        let pointSize _ = size
            
        Vertexor.createColoredVertices shaders pointColorize pointSize pointVerts