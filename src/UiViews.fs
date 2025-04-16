namespace HowFindRecursive

open Fable.Core.JsInterop
open Fable.React
open Fable.React.Props
open Elmish.React
open Elmish
open HowFindRecursive.BuilderInput
open HowFindRecursive.UiModel

module UiViews =
    
    // helper methods for elements that repeat a lot
    let formDiv l = div [ ClassName "row mb-2" ] l
    
    let formDivConditional condition l =
        div [ ClassName "row mb-2"; Hidden (not condition) ] l
    
    let inputDiv additionalClasses l=
        div [ ClassName ("col-sm-9 " + additionalClasses)] l

    let outputDiv additionalClasses l=
        div [ ClassName ("col-sm-12 " + additionalClasses)] l

    let formLabel forStr labelText =
        if forStr = "" then
            label [ ClassName "col-sm-3 col-form-label" ] [ str labelText ]
        else 
            label [ ClassName "col-sm-3 col-form-label"; HtmlFor forStr] [ str labelText ]
        
    let dropdownButton additionalClasses text =
        button [
            ClassName ("btn btn-primary dropdown-toggle " + additionalClasses)
            HTMLAttr.Custom("data-bs-toggle", "dropdown")
            AriaExpanded false
            Type "button"
        ] [ str text ]
                
    let dropdownOption text action =
        li [] [
            a [
                ClassName "dropdown-item"
                Href "#"
                OnClick action
            ] [ str text ]
        ]
    
    let buttonInGroup text condition action =
        button [
            Type "button"
            classList [
                ("btn", true)
                ("btn-primary", condition)
                ("btn-secondary", not condition)
                ("w-100", true)
            ]
            OnClick action
        ] [ str text ]
    
    
    // Input form elements
    let sourceFolderView model dispatch =
        formDiv [
            formLabel "inputBaseDir" "Search folder:"
            inputDiv "" [
                input [
                    Type "text"
                    ClassName "form-control"
                    Id "inputBaseDir"
                    Placeholder "Examples: . (for current directory), /home/user/Downloads/"
                    Value model.buildParams.folder
                    OnChange (fun ev -> !!ev.target?value |> ChangeSourceFolder |> dispatch)
                ]
            ]
        ] 

    let patternView model dispatch =
        formDiv [
            formLabel "inputPattern" "Pattern:"
            inputDiv "input-group" [
                dropdownButton "patternDropdown" (if model.buildParams.style = Glob then "Glob" else "Regex")
                ul [ ClassName "dropdown-menu" ] [
                    dropdownOption "Glob" (fun _ -> Glob |> ChangeStyle |> dispatch)
                    dropdownOption "Regex" (fun _ -> Regexp |> ChangeStyle |> dispatch)
                ]
                input [
                    Type "text"
                    ClassName "form-control"
                    Id "inputPattern"
                    Placeholder "Examples: *.jpg, my-app-*.log"
                    Value model.buildParams.pattern
                    OnChange (fun ev -> !!ev.target?value |> ChangePattern |> dispatch)
                ]
            ]
        ] 
        
    let targetTypeView model dispatch =
        formDiv [
            formLabel "" "Target type:"
            inputDiv "" [
                div [ ClassName "btn-group d-flex"; Role "group"; AriaLabel "Target type" ] [
                    buttonInGroup "Only files" (model.buildParams.targetType = File) (fun _ -> File |> ChangeTargetType |> dispatch)
                    buttonInGroup "Only folders" (model.buildParams.targetType = Directory) (fun _ -> Directory |> ChangeTargetType |> dispatch)
                    buttonInGroup "Both" (model.buildParams.targetType = All) (fun _ -> All |> ChangeTargetType |> dispatch)
                ]
            ]
        ]
    
    let lastModifiedView model dispatch =
        formDiv [
            formLabel "inputModified" "Last modified:"
            inputDiv "input-group" [
                dropdownButton "qualifierDropdown" (if model.buildParams.lastModified.qualifier = EarlierThan then "Earlier than" else "Later than")
                ul [ ClassName "dropdown-menu" ] [
                    dropdownOption "Earlier than" (fun _ -> EarlierThan |> ChangeModifiedQualifier |> dispatch)
                    dropdownOption "Later than" (fun _ -> LaterThan |> ChangeModifiedQualifier |> dispatch)
                ]
                input [
                    Type "number"
                    ClassName "form-control"
                    Id "inputModified"
                    Placeholder "(if not applicable, leave empty)"
                    Value model.modifiedNumber
                    OnChange (fun ev -> !!ev.target?value |> ChangeModifiedNumber |> dispatch)
                ]
                dropdownButton "unitDropdown" (match model.buildParams.lastModified.unit with
                                               | Minutes -> "minutes"
                                               | Hours -> "hours"
                                               | Days -> "days"
                                               | Weeks -> "weeks"
                                               | Months -> "months"
                                               | Years -> "years")
                ul [ ClassName "dropdown-menu" ] [
                    dropdownOption "minutes" (fun _ -> Minutes |> ChangeModifiedUnit |> dispatch)
                    dropdownOption "hours" (fun _ -> Hours |> ChangeModifiedUnit |> dispatch)
                    dropdownOption "days" (fun _ -> Days |> ChangeModifiedUnit |> dispatch)
                    dropdownOption "weeks" (fun _ -> Weeks |> ChangeModifiedUnit |> dispatch)
                    dropdownOption "months" (fun _ -> Months |> ChangeModifiedUnit |> dispatch)
                    dropdownOption "years" (fun _ -> Years |> ChangeModifiedUnit |> dispatch)
                ]
            ]
        ]
    
    let lastAccessedView model dispatch =
        if model.outputType <> Fd then
            formDiv [
                formLabel "inputAccessed" "Last accessed:"
                inputDiv "input-group" [
                    dropdownButton "qualifierDropdown" (if model.buildParams.lastAccessed.qualifier = EarlierThan then "Earlier than" else "Later than")
                    ul [ ClassName "dropdown-menu" ] [
                        dropdownOption "Earlier than" (fun _ -> EarlierThan |> ChangeAccessedQualifier |> dispatch)
                        dropdownOption "Later than" (fun _ -> LaterThan |> ChangeAccessedQualifier |> dispatch)
                    ]
                    input [
                        Type "number"
                        ClassName "form-control"
                        Id "inputAccessed"
                        Placeholder "(if not applicable, leave empty)"
                        Value model.accessedNumber
                        OnChange (fun ev -> !!ev.target?value |> ChangeAccessedNumber |> dispatch)
                    ]
                    dropdownButton "unitDropdown" (match model.buildParams.lastAccessed.unit with
                                                   | Minutes -> "minutes"
                                                   | Hours -> "hours"
                                                   | Days -> "days"
                                                   | Weeks -> "weeks"
                                                   | Months -> "months"
                                                   | Years -> "years")
                    ul [ ClassName "dropdown-menu" ] [
                        dropdownOption "minutes" (fun _ -> Minutes |> ChangeAccessedUnit |> dispatch)
                        dropdownOption "hours" (fun _ -> Hours |> ChangeAccessedUnit |> dispatch)
                        dropdownOption "days" (fun _ -> Days |> ChangeAccessedUnit |> dispatch)
                        dropdownOption "weeks" (fun _ -> Weeks |> ChangeAccessedUnit |> dispatch)
                        dropdownOption "months" (fun _ -> Months |> ChangeAccessedUnit |> dispatch)
                        dropdownOption "years" (fun _ -> Years |> ChangeAccessedUnit |> dispatch)
                    ]
                ]
            ]
        else
            formDiv [
                formLabel "inputAccessed" "Last accessed:"
                inputDiv "input-group justify-content-center" [
                    div [ ClassName "text-center " ] [
                        em [ ClassName "form-control not-a-real-control" ] [ str "(fd does not support this option)" ]
                    ]
                ]
            ]
    
    let actionView model dispatch =
        formDiv [
            formLabel "" "Action:"
            inputDiv "" [
                div [ ClassName "btn-group d-flex"; Role "group"; AriaLabel "Action" ] [
                    buttonInGroup "List" (model.buildParams.action = List) (fun _ -> List |> ChangeAction |> dispatch)
                    buttonInGroup "Delete" (model.buildParams.action = Delete) (fun _ -> Delete |> ChangeAction |> dispatch)
                    buttonInGroup "Trash" (model.buildParams.action = MoveToTrash) (fun _ -> MoveToTrash |> ChangeAction |> dispatch)
                    buttonInGroup "Copy" (match model.buildParams.action with
                                          | Copy _ -> true
                                          | _ -> false) (fun _ -> Copy model.copyMoveParams |> ChangeAction |> dispatch)
                    buttonInGroup "Move" (match model.buildParams.action with
                                          | Move _ -> true
                                          | _ -> false) (fun _ -> Move model.copyMoveParams |> ChangeAction |> dispatch)
                ]
            ]
        ]
    
    let targetDirView model dispatch =
        formDivConditional (not (List.contains model.buildParams.action [List;Delete;MoveToTrash])) [
            formLabel "inputTargetDir" "Target folder:"
            inputDiv "" [
                input [
                    Type "text"
                    ClassName "form-control"
                    Id "inputTargetDir"
                    Placeholder "Examples: . (for current directory), /home/user/Downloads/"
                    Value model.copyMoveParams.dest
                    OnChange (fun ev -> !!ev.target?value |> ChangeCopyMoveDest |> dispatch)
                ]
            ]
        ]
    
    let targetPreserveView model dispatch =
        formDivConditional (not (List.contains model.buildParams.action [List;Delete;MoveToTrash])) [
            formLabel "" ""
            inputDiv "" [
                div [ ClassName "form-check" ] [
                    input [
                        Type "checkbox"
                        ClassName "form-check-input"
                        Id "preserveStructure"
                        Checked model.copyMoveParams.preserveStructure
                        OnChange (fun _ -> (not model.copyMoveParams.preserveStructure) |> ChangeCopyMovePreserve |> dispatch)
                    ]
                    label [ ClassName "form-check-label"; HtmlFor "preserveStructure" ] [ str "Preserve directory structure" ]
                ]
            ]
        ]
        
        
    
    // Output form elements
    let outputSelectView model dispatch =
        formDiv [
            outputDiv "" [
                div [ ClassName "btn-group d-flex"; Role "group"; AriaLabel "Output system" ] [
                    buttonInGroup "find (Linux / Mac)" (model.outputType = Find) (fun _ -> Find |> ChangeOutputType |> dispatch)
                    buttonInGroup "fd (Linux / Mac)" (model.outputType = Fd) (fun _ -> Fd |> ChangeOutputType |> dispatch)
                    buttonInGroup "Powershell (Windows)" (model.outputType = Powershell) (fun _ -> Powershell |> ChangeOutputType |> dispatch)
                ]
            ]
        ]
    
    let outputView model dispatch =
        formDiv [
            outputDiv "" [
                pre [ ClassName "output text-start"; Id "outputPre" ] [ str model.output ]
            ]
        ]
    
    let copyClipboardJs ev =
        emitJsStatement ev """
            var text = document.getElementById("outputPre").textContent;
            navigator.clipboard.writeText(text);
        """
        
    let copyToClipboardView model dispatch =
        formDiv [
            div [ ClassName "col-sm-12 text-center" ] [
                button [
                    ClassName "copy-btn btn btn-info"
                    HTMLAttr.Custom ("data-clipboard-action", "copy")
                    HTMLAttr.Custom ("data-clipboard-target", "#outputPre")
                    OnClick (fun ev ->
                        copyClipboardJs model.output |> ignore
                        ev.preventDefault()
                    )
                ] [ str "Copy to Clipboard" ]
            ]
        ]
     