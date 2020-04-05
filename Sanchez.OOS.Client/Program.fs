open Sanchez.OOS.Core
open Sanchez.Socker
open System.Threading

[<EntryPoint>]
let main argv =
    let serverPort = 25599
//    let port = 25598
//    let userName = "daniel"
    
    Thread.Sleep 1000
    
    let cToken = new CancellationToken()
    let (poster, actioner) =
        Client.connectToServer ServerAction.decode ClientAction.encode "127.0.0.1" serverPort cToken
        |> Async.RunSynchronously
        
    "daniel" |> Register |> poster
        
    actioner |> ignore
    
//    use game = new Game(800, 600)
//    game.Run()

    while true do
        ()
    
    0
