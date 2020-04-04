open System.Threading
open Sanchez.OOS.Client
open Sanchez.OOS.Client.Connection

[<EntryPoint>]
let main argv =
    let serverPort = 25599
    let port = 25598
    
    Thread.Sleep 1000
    
    let (actioner, sender, broadcaster) =
        async {
            let! (actioner, serverList) = Server.startClientCommunication port serverPort
            
            let firstServer = // TODO: Fix all this and the server searcher up
                serverList
                |> List.tryHead
                |> function
                    | Some s -> s
                    | None -> failwith "Failed to find any servers, this needs to be all reworked at some point!"
            
            let broadcaster = Server.broadcastData firstServer.Port
            let sender = Server.sendData firstServer
            
            return (actioner, sender, broadcaster)
        }
        |> Async.RunSynchronously
    
//    use game = new Game(800, 600)
//    game.Run()

    while true do
        ()
    
    0
