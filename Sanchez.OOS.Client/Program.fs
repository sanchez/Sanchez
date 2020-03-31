open System.Threading
open Sanchez.OOS.Client
open Sanchez.OOS.Client.Connection

[<EntryPoint>]
let main argv =
    let serverPort = 25599
    let port = 25598
    
    Thread.Sleep 1000
    
    let test = Server.loadServers port serverPort
    
//    use game = new Game(800, 600)
//    game.Run()

    while true do
        ()
    
    0
