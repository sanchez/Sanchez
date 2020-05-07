namespace Sanchez.ThreeWin

open OpenToolkit.Mathematics
open Sanchez.Data.Positional

type OrbitalCamera =
    {
        mutable Position: Vector<float32>
        mutable EyeOffset: Vector<float32>
        mutable Up: Vector<float32>
    }
    
module OrbitalCamera =
    let create eyeOffset =
        {
            OrbitalCamera.Position = Vector.create 0.f 0.f 0.f
            EyeOffset = eyeOffset
            Up = Vector.create 0.f 1.f 0.f
        }
        
    let setPosition pos (cam: OrbitalCamera) =
        cam.Position <- pos
        cam
    
    let setEyeOffset pos (cam: OrbitalCamera) =
        cam.EyeOffset <- pos
        cam
        
    let renderCamera (cam: OrbitalCamera) widthScale =
        let projection = Matrix4.CreatePerspectiveFieldOfView(System.Math.PI / 4. |> float32, widthScale, 0.1f, 100.f)
        let lookingAt = cam.Position
        let eyePosition = cam.Position + cam.EyeOffset
        let view = Matrix4.LookAt(
                                     Vector3(eyePosition.X, eyePosition.Y, eyePosition.Z),
                                     Vector3(lookingAt.X, lookingAt.Y, lookingAt.Z),
                                     Vector3(cam.Up.X, cam.Up.Y, cam.Up.Z)
                                 )
        
        {
            RenderedCamera.Projection = projection
            View = view
        }
        
    let mapMouseToView (distFromEye: float32) (cam: OrbitalCamera) (widthScale: float32) (pos: PointVector<float32>) =
        let renderCamera = renderCamera cam widthScale
        let finalMatrix = renderCamera.Projection * renderCamera.View
        finalMatrix.Invert()
        let pos3d = Vector4(pos.X, pos.Y, -1.f, 1.f)
        
        let mouseMapped = pos3d * finalMatrix
        let mouseMappedV = Vector.create mouseMapped.X mouseMapped.Y mouseMapped.Z
        let eyeLookingDir = (cam.EyeOffset * -1.f) |> Vector.map float |> Vector.normalize |> Vector.map float32
        let finalEyeOffset = (eyeLookingDir * distFromEye) - cam.EyeOffset + cam.Position
        
//        mouseMappedV + cam.Position
        
//        Vector.create (mouseMapped.X / mouseMapped.W) (mouseMapped.Y / mouseMapped.W) (mouseMapped.Z / mouseMapped.W)
        Vector.create (mouseMapped.X) (mouseMapped.Y) (mouseMapped.Z)
        
    let mapMouseToPosition (cam: OrbitalCamera) =
        let dist = cam.EyeOffset |> Vector.map float |> Vector.mag |> float32
        mapMouseToView dist cam