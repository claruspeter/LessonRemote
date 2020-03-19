module Game.Hubs

open System.Threading.Tasks
open Microsoft.AspNetCore.SignalR
open Microsoft.AspNetCore.Authorization


type IGameHubCommands =
    abstract member NavigateTo: string -> Task
    abstract member Refresh: unit -> Task

type IGameHub = 
    inherit IGameHubCommands
    abstract member Navigated: string -> string -> Task

[<Authorize>]
type GameHub() =
    inherit Hub<IGameHub>()
    
    interface IGameHubCommands with 
        member this.NavigateTo s = this.NavigateTo s
        member this.Refresh() = this.Refresh()

    member this.NavigateTo (location: string) : Task =
        async{
            printfn "nav: (%s) %A" this.UserName location
            return! this.Clients.Others.Navigated this.UserName location |> Async.AwaitTask
        }
        |> Async.StartAsTask :> _

    member this.Refresh () : Task =
        async{
            printfn "refreshing players"
            //return! this.Clients.All.AllPlayers (Actions.resolvedPlayersFor TheGame resolver None) |> Async.AwaitTask
        }
        |> Async.StartAsTask :> _

    member private this.UserName : string  =
        this.Context.User.Identity.Name

