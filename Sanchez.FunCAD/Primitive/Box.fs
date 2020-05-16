namespace Sanchez.FunCAD.Primitive

open OpenToolkit.Mathematics
open Sanchez.Data.Positional
open Sanchez.ThreeWin

type Box =
    {
        Position: Vector<float>
        Size: Vector<float>
    }
    
module Box =
    let create centerPoint width depth height =
        {
            Box.Position = centerPoint
            Size = Vector.create width height depth
        }
        
    let rasterize shaders color (box: Box) =
        let halfWidth = Vector.create (box.Size.X / 2.) 0. 0.
        let halfHeight = Vector.create 0. (box.Size.Y / 2.) 0.
        let halfDepth = Vector.create 0. 0. (box.Size.Z / 2.)
        
        let boxVerts =
            [
                - halfWidth - halfHeight - halfDepth; // front bottom left
                halfWidth - halfHeight - halfDepth; // front bottom right
                halfWidth + halfHeight - halfDepth; // front top right
                - halfWidth + halfHeight - halfDepth; // front top left
                
                - halfWidth - halfHeight + halfDepth; // back bottom left
                halfWidth - halfHeight + halfDepth; // back bottom right
                halfWidth + halfHeight + halfDepth; // back top right
                - halfWidth + halfHeight + halfDepth; // back top left
            ]
            |> List.map (Vector.map float32)
        let boxIndices =
            [
                (0, 1, 2); (0, 3, 2)   // front face
                (0, 3, 4); (4, 7, 3)   // left face
                (1, 2, 6); (6, 5, 1)   // right face
                (4, 5, 6); (6, 7, 4)   // back face
                (0, 1, 4); (4, 5, 1)   // bottom face
                (2, 3, 7); (7, 6, 2)   // top face
            ]
        let boxColorizer _ = color
        
        let pos = box.Position |> Vector.map float32
        
        Vertexor.createColoredObject shaders boxColorizer boxVerts boxIndices
        |> Vertexor.applyStaticTransformation (Matrix4.CreateTranslation(pos.X, pos.Y, pos.Z))