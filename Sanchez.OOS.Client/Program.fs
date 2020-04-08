open Sanchez.OOS.Client
open Sanchez.OOS.Client.Connection
open Sanchez.OOS.Core
open Sanchez.Socker
open System.Threading

[<EntryPoint>]
let main argv =
    let serverPort = 25599
//    let port = 25598
//    let userName = "daniel"
    
    Thread.Sleep 5000
    
    let cToken = new CancellationToken()
    let (poster, actioner) =
        Client.connectToServer ServerAction.decode ClientAction.encode "127.0.0.1" serverPort cToken
        |> Async.RunSynchronously
        
    Users.registerUser poster "daniel"
    
    actioner.AddActioner "missing" (fun ip -> printfn "Missing action binding: %A" >> Some)
        
    use game = new Game(800, 600, poster)
    
    game.Run()
    
    0
