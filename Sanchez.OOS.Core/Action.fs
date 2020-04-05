namespace Sanchez.OOS.Core

open System.Net

module Action =
    let staticVersion = "0.0.1"
    
type ServerStats =
    {
        Port: int
        Version: string
    }
type FullConnection =
    {
        IP: IPAddress
        Port: int
        Version: string
    }

type ServerAction =
    | Announcement of ServerStats
    | NewPlayer of string
    
type ClientAction =
    | ServerRequest of int
    | Register of string