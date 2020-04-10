module Sanchez.OOS.Client.Assets.Assets

open OpenToolkit.Graphics.OpenGL
open System.Drawing
open System.Drawing.Imaging
open System.IO
open Sanchez.Data

type ImageAssets =
    | StandardWallBlock
    | SealedDoor
    | Body
    
type LoadedTexture =
    | StaticTexture of int
    | AnimatedTexture of int array
    
type TextureInformation =
    | StaticTextureInformation of ImageAssets*string
    | AnimationTextureInformation of ImageAssets*string*int
    
let textures =
    [
        (StandardWallBlock, "block1.png") |> StaticTextureInformation
        (SealedDoor, "sealedDoor.png", 12) |> AnimationTextureInformation
        (Body, "body.png", 12) |> AnimationTextureInformation
    ]
    
let private loadBitmap (info: TextureInformation) =
    let fileName =
        match info with
        | StaticTextureInformation (_, fName) -> fName
        | AnimationTextureInformation (_, fName, _) -> fName
        
    try
        new Bitmap("Assets/" + fileName) |> Some
    with
    | :? FileNotFoundException as ex -> None
    
let private createTexture (image: BitmapData) (top: int) (left: int) (width: int) (height: int) =
    let texId = GL.GenTexture()
    
    GL.BindTexture(TextureTarget.Texture2D, texId)
    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Nearest)
    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Nearest)
    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) TextureWrapMode.ClampToEdge)
    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) TextureWrapMode.ClampToEdge)
    
    GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1)
    GL.TexSubImage2D(TextureTarget.Texture2D, 0, left, top, width, height, PixelFormat.Bgra, PixelType.UnsignedByte, image.Scan0)
    GL.GenerateMipmap(GenerateMipmapTarget.Texture2D)
    
    texId
    
let private loadAnimatedImage (image: BitmapData) (frameWidth: int) =
    let frames = image.Width / frameWidth
    
    Seq.init frames (fun i ->
        let left = i * frameWidth
        createTexture image 0 left frameWidth image.Height)
    |> Seq.toArray
    
let private loadStaticImage (image: BitmapData) =
    createTexture image 0 0 image.Width image.Height
    
let private loadImage (info: TextureInformation) =
    opt {
        let! file = loadBitmap info
        let data = file.LockBits(System.Drawing.Rectangle(0, 0, file.Width, file.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb)
        
        let tex =
            match info with
            | StaticTextureInformation (name, _) ->
                loadStaticImage data
                |> StaticTexture
                |> (fun x -> (name, x))
            | AnimationTextureInformation (name, _, frameWidth) ->
                loadAnimatedImage data frameWidth
                |> AnimatedTexture
                |> (fun x -> (name, x))
        
        do file.UnlockBits data
        
        return tex
    }
    
let loadTextures () =
    textures
    |> Seq.choose loadImage
    |> Map.ofSeq
    
let drawTexture (textures: Map<_, int> option) name =
    opt {
        let! tex =
            textures
            |> Option.bind (Map.tryFind name)
            
        do GL.Enable(EnableCap.Texture2D)
        do GL.Color4(1., 0., 0., 1.)
        do GL.BindTexture(TextureTarget.Texture2D, tex)
//        do GL.Begin(BeginMode.Quads)
        
        return ()
    }
    |> ignore