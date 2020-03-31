open System.Net
open Sanchez.OOS.Core
open Sanchez.OOS.Server.Parts

[<EntryPoint>]
let main argv =
    let port = 25599
    
    let handleIncomingAction (addr: IPAddress) (action: ClientAction) =
        let sendReponse (port: int) =
            UDPHandler.sendRequest UDPHandler.encodeServerAction ({ ServerConnection.IP = addr; Port = port })
        
        match action with
        | ServerRequest returnPort ->
            Register.handleServerRequest port
            |> sendReponse returnPort
    
    let clientListener =
        UDPHandler.initializeListener UDPHandler.decodeClientAction handleIncomingAction port
        
        
    while true do
        ()
    
    printfn "Hello World from F#!"
    0 // return an integer exit code
