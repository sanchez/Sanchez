open System.Threading
open Sanchez.OOS.Core
open Sanchez.Socker

[<EntryPoint>]
let main argv =
    let port = 25599
    
    let cToken = new CancellationToken()
    let actioner = Server.createServer (UDPHandler.decodeClientAction >> Some) port cToken
    actioner |> ignore
    
    actioner.AddActioner "test" (fun ip a -> None)
    
//    let handleIncomingAction (addr: IPAddress) (action: ClientAction) =
//        let sendReponse (port: int) =
//            UDPHandler.sendRequest UDPHandler.encodeServerAction ({ ServerConnection.IP = addr; Port = port })
//        
//        match action with
//        | ServerRequest returnPort ->
//            Register.handleServerRequest port
//            |> sendReponse returnPort
//    
//    let clientListener =
//        UDPHandler.initializeListener UDPHandler.decodeClientAction handleIncomingAction port
        
        
    while true do
        ()
    
    printfn "Hello World from F#!"
    0 // return an integer exit code
