namespace Sanchez.OOS.Core

open System.Net

type ServerConnection =
    {
        IP: IPAddress
        Port: int
    }