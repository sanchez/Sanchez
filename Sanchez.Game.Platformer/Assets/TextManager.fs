namespace Sanchez.Game.Platformer.Assets

open SkiaSharp
open OpenToolkit.Graphics.OpenGL

type TextTexture = TextTexture of string*int*float32

module Text =
    let createTexture (str: string) =
        let textHeight = 32.f
        
        use paint = new SKPaint()
        paint.TextSize <- textHeight
        paint.IsAntialias <- true
        paint.Color <- SKColors.White
        paint.IsStroke <- false
        
        let textWidth = paint.MeasureText str
        
        let info = new SKImageInfo(textWidth |> int, textHeight |> int)
        use surface = SKSurface.Create(info)
        let canvas = surface.Canvas
        
        canvas.Clear(SKColors.Black.WithAlpha(0uy))
        canvas.Scale(1.f, -1.f, 0.f, textHeight / 2.f)
        canvas.DrawText(str, 0.f, textHeight, paint)
        
        canvas.Flush()
        let result = surface.Snapshot()
        let skBMP = SKBitmap.FromImage result
        
        let texId = GL.GenTexture()
        
        GL.BindTexture(TextureTarget.Texture2D, texId)
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Nearest)
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Nearest)
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) TextureWrapMode.ClampToEdge)
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) TextureWrapMode.ClampToEdge)
        
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, skBMP.Width, skBMP.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, skBMP.Bytes)
        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D)
        
        (str, texId, textWidth / textHeight) |> TextTexture
        
type TextManager() =
    let mutable texts: Map<string, TextTexture list> = Map.empty
    
    member this.LoadText (str: string) =
        let text = Text.createTexture str
        let newList =
            texts
            |> Map.tryFind str
            |> Option.defaultValue []
            |> (fun x -> text::x)
        texts <- texts |> Map.add str newList
        text
        
    member this.FindText (str: string) =
        texts
        |> Map.tryFind str
        |> Option.bind List.tryHead
        |> Option.defaultWith (fun () -> this.LoadText str)