namespace Sanchez.Game.Platformer.Entity

open Sanchez.Game.Platformer.Assets
open FSharp.Data.UnitSystems.SI.UnitNames
open Sanchez.Game.Core

type IGameObject<'TTextureKey when 'TTextureKey : comparison> =
    abstract member CurrentPosition : Position with get
    abstract member IsAlive : bool with get
    abstract member Name : string with get
    
    abstract member Update : (string -> IGameObject<'TTextureKey> option) * ('TTextureKey -> LoadedTexture option) * (float<second>) -> unit
    abstract member Render : float32 -> unit