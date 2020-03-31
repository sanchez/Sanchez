namespace Sanchez.OOS.Core

module Action =
    let staticVersion = "0.0.1"
    
type ServerStats =
    {
        Port: int
        Version: string
    }

type ServerAction =
    | Announcement of ServerStats
    
type ClientAction =
    | ServerRequest of int