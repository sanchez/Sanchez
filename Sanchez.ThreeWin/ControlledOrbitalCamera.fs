namespace Sanchez.ThreeWin

open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames
open Sanchez.Data.Positional

module ControlledOrbitalCamera =
    let create rotateSpeed scrollSpeed (win: ThreeWin<_>) =
        let camera =
            Vector.create 4.f 4.f 4.f
            |> OrbitalCamera.create
           
        let mutable lastMousePos = None
        let onUpdate (timeElapsed: float<second>) =
            if win.IsMouseButtonDown MouseButtonRight then
                let (width, height) = win.GetWindowDimensions()
                let mousePos = win.GetMousePosition()
                match lastMousePos with
                | Some x ->
                    let currentPoint = OrbitalCamera.mapMouseToXZPlane camera width height mousePos
                    let lastPoint = OrbitalCamera.mapMouseToXZPlane camera width height x
                    match (currentPoint, lastPoint) with
                    | (Some a, Some b) ->
                        let diff = b - a
                        if diff.X = 0.f && diff.Y = 0.f then ()
                        else
                            camera |> OrbitalCamera.setPosition (camera.Position + diff) |> ignore
                    | _ -> ()
                | None -> ()
                lastMousePos <- Some mousePos
            elif win.IsMouseButtonDown MouseButtonMiddle then
                let (width, height) = win.GetWindowDimensions()
                let mousePose = win.GetMousePosition()
                match lastMousePos with
                | Some x ->
                    let diff = mousePose - x
                    if diff.X = 0.f && diff.Y = 0.f then ()
                    else
                        let eye = Vector.map float camera.EyeOffset
                        let phi = (Vector.phi eye) + (System.Math.PI * rotateSpeed * (diff.X |> float))
                        let theta =
                            match (Vector.theta eye) + (System.Math.PI * rotateSpeed * (diff.Y |> float)) with
                            | t when t > System.Math.PI -> System.Math.PI
                            | t when t < 0.001 -> 0.001
                            | t -> t
                        let mag = Vector.mag eye
                        
                        (Vector.fromPolar mag phi theta |> Vector.map float32 |> OrbitalCamera.setEyeOffset) camera |> ignore
                | None -> ()
                lastMousePos <- Some mousePose
            else
                lastMousePos <- None
                
            let mouseWheelDelta = win.GetMouseScroll()
            if mouseWheelDelta = 0.f then ()
            else
                let newCameraDiff = (camera.EyeOffset) * (mouseWheelDelta * scrollSpeed * 0.005f)
                camera |> OrbitalCamera.setEyeOffset (newCameraDiff + camera.EyeOffset) |> ignore
                
        let onRender widthScale =
            OrbitalCamera.renderCamera camera widthScale
            
        (onUpdate, onRender, camera)