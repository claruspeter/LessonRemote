module Game.Hubs

open System.Threading.Tasks
open Microsoft.AspNetCore.SignalR


type IGameHubCommands =
    abstract member JoinClass: string -> Task
    abstract member NavigateTo: string -> string -> Task
    abstract member Refresh: unit -> Task

type IGameHub = 
    inherit IGameHubCommands
    abstract member Navigated: string -> string -> Task
    abstract member ReceivedMessage: string -> string -> Task

type GameHub() =
    inherit Hub<IGameHub>()
    
    interface IGameHubCommands with 
        member this.JoinClass name = this.JoinClass name
        member this.NavigateTo g s = this.NavigateTo g s
        member this.Refresh() = this.Refresh()

    member this.JoinClass (name: string) : Task =
        async{
            printfn "nav: (%s) joined %A" this.UserName name
            return! this.Groups.AddToGroupAsync(this.Context.ConnectionId, name) |> Async.AwaitTask
            return! this.Clients.Group(name).ReceivedMessage name (sprintf "Welcome %s" this.UserName) |> Async.AwaitTask
        }
        |> Async.StartAsTask :> _

    member this.NavigateTo (groupName: string) (location: string) : Task =
        async{
            printfn "nav: (%s) %A" this.UserName location
            return! this.Clients.OthersInGroup(groupName).Navigated this.UserName location |> Async.AwaitTask
        }
        |> Async.StartAsTask :> _

    member this.Refresh () : Task =
        async{
            printfn "refreshing players"
            //return! this.Clients.All.AllPlayers (Actions.resolvedPlayersFor TheGame resolver None) |> Async.AwaitTask
        }
        |> Async.StartAsTask :> _

    member private this.UserName : string  =
        this.Context.ConnectionId

