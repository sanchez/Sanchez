namespace Sanchez.OOS.Core.World

open Sanchez.OOS.Core.Blocks

type World =
    {
        Blocks: BaseBlock list
    }
    
module World =
    let createEmptyWorld () =
        {
            World.Blocks = []
        }
        
    let spawnBlock b w =
        { w with Blocks = b::w.Blocks }