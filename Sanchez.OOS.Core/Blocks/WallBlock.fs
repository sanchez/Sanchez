namespace Sanchez.OOS.Core.Blocks

type WallBlock = WallBlock of unit

module WallBlock =
    let generateWallBlock () =
        () |> WallBlock