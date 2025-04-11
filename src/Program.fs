module Program

open Fable.Core.JsInterop
open Fable.React
open Fable.React.Props
open Elmish

type Model = int

type Msg =
    | Increment
    | Decrement

let init() = 0

let update (msg: Msg) count =
    match msg with
    | Increment -> count + 1
    | Decrement -> count - 1

let view model dispatch =
    div []
        [
            button [ OnClick (fun _ -> dispatch Decrement); ClassName "btn btn-primary" ] [ str "-" ]
            div [] [ str (sprintf "%A" model) ]
            button [ OnClick (fun _ -> dispatch Increment); ClassName "btn btn-primary" ] [ str "+" ]
        ]

open Elmish.React
open Elmish.HMR

Program.mkSimple init update view
|> Program.withReactSynchronous "main-app"
|> Program.run