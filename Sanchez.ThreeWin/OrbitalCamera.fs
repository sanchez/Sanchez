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
        
    let mapMouseToView (distFromEye: float32) (cam: OrbitalCamera) (screenWidth: float) (screenHeight: float) (pos: PointVector<float32>) =
        let renderedCam = renderCamera cam ((screenWidth / screenHeight) |> float32)
        
        let fov = System.Math.PI / 4.
        let halfHeight = System.Math.Tan(fov / 2.) |> float32
        let halfWidth = (halfHeight / (screenWidth |> float32)) * (screenHeight |> float32)
        let vecCursor =
            Vector4(pos.X * halfWidth, pos.Y * halfHeight, 0.f, 1.f) * distFromEye
            |> Vector4.Normalize
        let quat = renderedCam.View.ExtractRotation()
        let rotCursor = quat * vecCursor
        let rotCursorVec = Vector.create rotCursor.X rotCursor.Y rotCursor.Z
        let posCursor = (cam.Position + cam.EyeOffset) + rotCursorVec
            
        let cameraMat = (renderedCam.Projection * renderedCam.View) |> Matrix4.Invert
        let screenSpaceMouse = Vector4(pos.X, pos.Y, 0.f, 1.f) * cameraMat
        
//        Vector.create (screenSpaceMouse.X) (screenSpaceMouse.Y) (screenSpaceMouse.Z)
        posCursor
        
//        Vector.create (screenSpaceMouse.X / screenSpaceMouse.W) (screenSpaceMouse.Y / screenSpaceMouse.W) (screenSpaceMouse.Z / screenSpaceMouse.W)
        
    let mapMouseToPosition (cam: OrbitalCamera) =
        let dist = cam.EyeOffset |> Vector.map float |> Vector.mag |> float32
        mapMouseToView dist cam