namespace Sanchez.FunCAD.Shapes

open Sanchez.Data.Positional
open Sanchez.FunCAD
open Sanchez.ThreeWin

type Line3D =
    {
        Points: Vector<decimal<mm>> list
    }
    
module Line3D =
    let create points =
        {
            Line3D.Points = points
        }
        
    let rasterize shaders colorizer (s: Line3D) =
        s.Points
        |> List.map (Vector.map (decimal >> float32))
        |> Vertexor.createColoredLine shaders colorizer