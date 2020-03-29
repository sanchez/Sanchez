open Sanchez.OOS.Client

[<EntryPoint>]
let main argv =
    
    use game = new Game(800, 600)
    game.Run()
    
    0
