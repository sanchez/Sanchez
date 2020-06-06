namespace Sanchez.ThreeWin

open System.Drawing
open OpenToolkit.Graphics.OpenGL
open OpenToolkit.Mathematics
open Sanchez.Data.Positional

type LightCurve = LightCurve of float32*float32*float32

module LightCurve =
    let create constant linear quadratic =
        (constant, linear, quadratic) |> LightCurve
    let map f (LightCurve (c, l, q)) = f c l q
    let value (LightCurve (c, l, q)) = (c, l, q)
    
type LightColor = LightColor of Color * Color * Color

module LightColor =
    let create ambient diffuse specular = (ambient, diffuse, specular) |> LightColor
    let createBasic c = (c, c, c) |> LightColor

type Light =
    | PointLight of (Vector<float32> * LightCurve * LightColor)

type LightManager =
    {
        mutable Lights: Light list
    }
    
module LightManager =
    let create () =
        { LightManager.Lights = [] }
        
    let addLight l (m: LightManager) =
        m.Lights <- l::m.Lights
        m
        
    let addPointLight pos curve color (m: LightManager) =
        addLight ((pos, curve, color) |> PointLight) m
        
    let renderPointLights maxCount formatName (m: LightManager) =
        let colorToFloats (c: Color) = Vector3((c.R |> float32) / 255.f, (c.G |> float32) / 255.f, (c.B |> float32) / 255.f)
        m.Lights
        |> Seq.choose (function
            | PointLight (pos, curve, color) -> Some (pos, curve, color)
            | _ -> None)
        |> Seq.mapi (fun i x -> (i, x))
        |> Seq.filter (fun (i, _) -> i < maxCount)
        |> Seq.map snd
        |> Seq.iteri (fun i (pos, (LightCurve (constant, linear, quadratic)), (LightColor (ambient, diffuse, specular))) ->
            let formater = formatName i
            
            GL.Uniform3(formater "position", Vector3(pos.X, pos.Y, pos.Z))
            GL.Uniform1(formater "constant", constant)
            GL.Uniform1(formater "linear", linear)
            GL.Uniform1(formater "quadratic", quadratic)
            GL.Uniform3(formater "ambient", colorToFloats ambient)
            GL.Uniform3(formater "diffuse", colorToFloats diffuse)
            GL.Uniform3(formater "specular", colorToFloats specular)
            ())
        
        ()