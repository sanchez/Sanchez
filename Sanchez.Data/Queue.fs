namespace Sanchez.Data

type Queue<'T>() =
    let mutable items: 'T list = []
    
    let queuer = MailboxProcessor<'T>.Start(fun inbox ->
        async {
            while true do
                let! item = inbox.Receive()
                items <- items @ [item]
            
            return ()
        })
    
    member this.Queue (a: 'T) =
        queuer.Post a
        
    member this.Peek () =
        items |> List.tryHead
        
    member this.Pop () =
        opt {
            let! head = items |> List.tryHead
            
            do items <- items |> List.tail
            
            return head
        }

