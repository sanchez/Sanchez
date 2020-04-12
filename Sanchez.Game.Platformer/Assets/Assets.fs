module Sanchez.Game.Platformer.Assets

open OpenToolkit.Graphics.OpenGL
open System.Drawing
open System.Drawing.Imaging
open System.IO
open Sanchez.Data
open Sanchez.Game.Core

type LoadedTexture =
    | StaticTexture of int
    | AnimatedTexture of int array*float<FPS>
    
let private loadBitmap (fileLocation: string) =
    try
        let i = new Bitmap(fileLocation)
        i.RotateFlip(RotateFlipType.RotateNoneFlipY)
        Some i
    with
    | :? FileNotFoundException as ex -> None
    
let private createTexture (image: BitmapData) =
    let texId = GL.GenTexture()
    
    GL.BindTexture(TextureTarget.Texture2D, texId)
    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Nearest)
    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Nearest)
    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) TextureWrapMode.ClampToEdge)
    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) TextureWrapMode.ClampToEdge)
    
    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, image.Scan0)
    GL.GenerateMipmap(GenerateMipmapTarget.Texture2D)
    
    texId
    
let private loadAnimatedImage (image: Bitmap) (frameWidth: int) =
    let frames = image.Width / frameWidth
    
    Seq.init frames (fun i ->
        let left = i * frameWidth
        let croppedImage = image.Clone(System.Drawing.Rectangle(left, 0, frameWidth, image.Height), image.PixelFormat)
        let data = croppedImage.LockBits(System.Drawing.Rectangle(0, 0, croppedImage.Width, croppedImage.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb)
        
        let id = createTexture data
        croppedImage.UnlockBits data
        id)
    |> Seq.toArray
    
let private loadStaticImage (image: Bitmap) =
    let data = image.LockBits(System.Drawing.Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb)
    let id = createTexture data
    image.UnlockBits data
    id
    
type TextureManager<'T when 'T : comparison>() =
    let mutable textures: Map<'T, LoadedTexture> = Map.empty
    
    member this.LoadTexture (key: 'T, fileName: string, ?animationDeets: int*float<FPS>) =
        opt {
            let! file = loadBitmap fileName
            
            let tex =
                match animationDeets with
                | Some (w, fps) ->
                    let t = loadAnimatedImage file w
                    AnimatedTexture (t, fps)
                | None -> loadStaticImage file |> StaticTexture
                
            do textures <- textures |> Map.add key tex
                
            return tex 
        }