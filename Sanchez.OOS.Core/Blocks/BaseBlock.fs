namespace Sanchez.OOS.Core.Blocks

open Sanchez.Game.Core

type Health = Health of int

type BlockTypes =
    | Wall of WallBlock

type BaseBlock =
    {
        Position: Position
        Health: Health
        Size: (float<sq>*float<sq>)
        
        Type: BlockTypes
    }
    
module BaseBlock =
    let createBasicBaseBlock t pos health =
        {
            BaseBlock.Position = pos
            Health = health |> Health
            Size = (1.<sq>, 1.<sq>)
            Type = t
        }