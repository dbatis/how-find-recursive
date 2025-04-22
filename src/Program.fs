module Program

open Fable.Core.JsInterop
open Fable.React
open Fable.React.Props
open Elmish.React
open Elmish
open HowFindRecursive
open HowFindRecursive.UiViews

let mainView model dispatch =
    div [] [
        div [ ClassName "container text-center row justify-content-md-center" ] [
            div [ ClassName "col-lg-8 col-md-12" ] [
                h2 [ ClassName "mb-2" ] [ str "Parameters" ]
                form [ ClassName "text-start align-text-top" ] [
                    sourceFolderView model dispatch
                    patternView model dispatch
                    targetTypeView model dispatch
                    lastModifiedView model dispatch
                    lastAccessedView model dispatch
                    actionView model dispatch
                    targetDirView model dispatch
                    targetPreserveView model dispatch
                ]
            ]
        ]
        div [ ClassName "container row" ] [
            div [ ClassName "col-lg-12" ] [
                h2 [ ClassName "mb-2 text-center" ] [ str "Output" ]
                form [ ClassName "text-start align-text-top" ] [
                    outputSelectView model dispatch
                    outputSelectTrashEnv model dispatch
                    outputView model dispatch
                    copyToClipboardView model dispatch
                ]
            ]
        ]
    ]

open Elmish.HMR

Program.mkProgram UiModel.init UiModel.update mainView
|> Program.withReactSynchronous "main-app"
|> Program.run
