namespace Sanchez.Socker

open System.Net

type ConnectionAction<'T> = string -> 'T -> unit option
type Actioner<'T> () =
    let mutable actioners = []
    
    let actioner = MailboxProcessor<string * 'T>.Start(fun inbox ->
        async {
            while (true) do
                let! (addr, a) = inbox.Receive()
                printfn "Executing action from %s: %A" addr a
                
                actioners
                |> Seq.map (snd >> (fun x -> x addr a))
                |> Seq.choose id
                |> Seq.tryHead
                |> function
                    | Some () -> ()
                    | None -> () // oh god, no actioners were able to execute
            
            return ()
        })
    
    member this.ExecuteAction (serverAddr: string) (a: 'T) = actioner.Post(serverAddr, a)
            
    member this.AddActioner (key: string) (a: ConnectionAction<'T>) =
        actioners <- (key, a)::actioners
        
    member this.RemoveActioner (key: string) =
        actioners <- actioners |> List.filter (fst >> ((=) key) >> not)