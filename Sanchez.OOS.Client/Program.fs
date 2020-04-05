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
        Client.connectToServer (UDPHandler.decodeServerAction >> Some) (UDPHandler.encodeClientAction) "127.0.0.1" serverPort cToken
        |> Async.RunSynchronously
        
    "daniel" |> Register |> poster
        
    actioner |> ignore
    
//    let (actioner, sender, broadcaster) =
//        async {
//            let! (actioner, serverList) = Server.startClientCommunication port serverPort
//            
//            let firstServer = // TODO: Fix all this and the server searcher up
//                serverList
//                |> List.tryHead
//                |> function
//                    | Some s -> s
//                    | None -> failwith "Failed to find any servers, this needs to be all reworked at some point!"
//            
//            let broadcaster = Server.broadcastData firstServer.Port
//            let sender = Server.sendData firstServer
//            
//            return (actioner, sender, broadcaster)
//        }
//        |> Async.RunSynchronously
//        
//    Register.registerUser sender userName
    
//    use game = new Game(800, 600)
//    game.Run()

    while true do
        ()
    
    0
