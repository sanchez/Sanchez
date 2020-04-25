namespace Sanchez.Data

open System.Threading

type Actioner<'T>(cToken: CancellationToken) =
    let mutable actioners = []
    
    let actioner = MailboxProcessor<'T>.Start((fun inbox ->
        async {
            while (true) do
                let! a = inbox.Receive()
                
                actioners
                |> Seq.tryPick (snd >> (|>) a)
                |> function
                    | Some () -> ()
                    | None -> ()

            return ()
        }), cToken)
    
    member this.ExecuteAction a = actioner.Post(a)
    
    member this.AddActioner (key: string) (a: 'T -> unit option) =
        actioners <- (key, a)::actioners
        
    member this.RemoveActioner (key: string) =
        actioners <- actioners |> List.filter (fst >> ((=) key) >> not)

