module Game.App

open Giraffe

// ---------------------------------
// Models
// ---------------------------------

type Message =
    {
        Text : string
    }

// ---------------------------------
// Views
// ---------------------------------

module Views =
    open GiraffeViewEngine
    let _role = attr "role"
    let _data field = attr (sprintf "data_%s" field)
    let _aria_controls = attr "aria-controls"

    // Vue attributes
    let _vfor = attr "v-for"
    let _vif = attr "v-if"
    let _velse = flag "v-else"
    let _vbind x = sprintf "v-bind:%s" x |> attr
    let _vue x =  sprintf "{{%s}}" x |> encodedText



    let pageHead username =
        header [] [
            nav [ _class "navbar navbar-expand-sm navbar-toggleable-sm navbar-light bg-white border-bottom box-shadow mb-3"] [
                div [ _class "container"][
                    a [ _class "navbar-brand"; _href "/"][ encodedText "Lesson Remote" ]
                    button [ _class "navbar-toggler"; _type "button"; _data "toggle" "collapse"; _data "target" ".navbar-collapse"; _aria_controls "navbarSupportContent" ][
                        span [ _class "navbar-toggler-icon"][]
                    ]
                ]
                div [ _class "navbar-collapse collapse d-sm-inline-flex flex-sm-row" ][
                    ul [ _class "navbar-nav flex-grow-1" ][

                    ]
                    ul [ _class "navbar-nav" ][
                        li [ _class "nav-item flex-right" ][
                            a [ _class "nav-link text-dark btn btn-outline-info"; _href "/signout" ][ encodedText username]
                        ]
                    ]
                ]
            ]
        ]

    let layout username  (content: XmlNode list) =
        html [] [
            head [] [
                title []  [ encodedText "Lesson Remote" ]
                link [ _rel  "stylesheet"; _type "text/css"; _href "https://stackpath.bootstrapcdn.com/bootstrap/4.4.1/css/bootstrap.min.css" ]
                link [ _rel  "stylesheet"; _type "text/css"; _href "/main.css" ]
            ]
            body [] (
                [
                    pageHead username
                    div [ _class "container" ] [
                        main[ _role "main"; _class "pb-3"] content
                        form [ _class "hijackJoin"][
                            p [] [
                                label [][ encodedText "Join Class"]
                                input [ _name "name"; ]
                                input [ _type "submit"; _value "Join"]
                            ]
                        ]                        
                        div [  ][ ul [ _id "msglist"; _class "naked"][] ]
                    ]
                ]
                @ [
                    script [ _src "https://code.jquery.com/jquery-3.4.1.min.js"][]
                    script [ _src "https://stackpath.bootstrapcdn.com/bootstrap/4.4.1/js/bootstrap.min.js"][]
                    script [ _src "https://cdn.jsdelivr.net/npm/vue/dist/vue.js" ][]
                    script [ _src "/js/signalr/dist/browser/signalr.js" ][]
                    script [ _src "/js/gamehub.js" ][]
                    script [ _src "/js/site.js" ][]
                ]
            )
        ]

    type LinkPage = { p : string; h : string list }
    let controller username =
        let pages = [
            { p = "ice-breaker"; h = [] }
            { p = "Sync"; h = [] }
            { p = "Threads"; h = ["whats-happening"; "why"] }
            { p = "Callbacks"; h = [] }
            { p = "Events"; h = [] }
            { p = "Promises"; h = [] }
            { p = "Await"; h = [] }
        ]

        [
            div [ _id "app"; _class "pb-3" ] [
                div [ _id "game-controls"](
                    pages 
                    |> List.map (fun x ->
                        p[]  (
                            [
                                a [ _class "hijackLink"; _href ("https://pete-the-programmer.com/asynchronous-programming/" + x.p) ][ encodedText x.p]
                            ]
                            @ (
                                x.h |> List.map (fun h -> 
                                    a [ _class "hijackLink"; _href (sprintf "https://pete-the-programmer.com/asynchronous-programming/%s#%s" x.p h) ][ encodedText ("#" + h)]
                                )
                            )
                        )
                    )
                )
            ]
        ] |> layout username

    let index username =
        [
            div [ _class "col-sm-9"; _id "app" ] [
                p [] [ encodedText "This is a remote controller for a lesson.  It will open lesson pages in another tab - please allow 'Pop-ups' from this window."]

            ]
        ] |> layout username


// ---------------------------------
// Web app
// ---------------------------------

let indexHandler  =
    fun (next : HttpFunc) (ctx : Microsoft.AspNetCore.Http.HttpContext) ->
        let view = Views.index ctx.User.Identity.Name
        htmlView view next ctx

let controllerHandler  =
    fun (next : HttpFunc) (ctx : Microsoft.AspNetCore.Http.HttpContext) ->
        let view = Views.controller ctx.User.Identity.Name
        htmlView view next ctx

let app : HttpHandler =
    choose [
        GET >=>
            choose [
                route "/" >=> indexHandler
                route "/controller" >=> controllerHandler
            ]
        setStatusCode 404 >=> text "Not Found" ]