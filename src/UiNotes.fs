namespace HowFindRecursive

open Fable.React
open Fable.React.Props
open Elmish.React
open Elmish

open HowFindRecursive.BuilderInput
open HowFindRecursive.UiModel

module UiNotes =
    
    let notesLinux (model: UiModel.Model) = [
        (isPreserve model.buildParams,
         "<code>rsync</code> can also be used as an alternative with <code>--from-files</code>, but you need to be careful about path structure.")
        (isCopy model.buildParams && isPreserve model.buildParams,
         "<code>cp --parents</code> is another option, but this parameter does not exist in MacOS.")
        (isPreserve model.buildParams,
         "<code>readlink -f</code> is only available in MacOS version 12.3.1 and above.")
        (isMoveToTrash model.buildParams && model.trashEnvironment = Gnome,
         "In WSL, you need to install the <code>gvfs</code> package.")
        (model.outputType = Fd,
         "In some distributions, like Ubuntu, fd may have been renamed to <code>fdfind</code> due to naming conflicts.")
    ]

    let notesPowershell (model: UiModel.Model) = [
        (isPreserve model.buildParams,
         "The alternative is to use <a href=\"https://learn.microsoft.com/en-us/windows-server/administration/windows-commands/robocopy\">robocopy.exe</code>.")
    ]
    
    let notesView model dispatch =
        let notes = if model.outputType = Powershell then notesPowershell model
                    else notesLinux model
                    |> List.filter fst
                    |> List.map (fun (_, text) -> li [ DangerouslySetInnerHTML { __html =text } ] [])
        
        div [ClassName "row mb-2 ms-2 me-2"; Hidden (List.isEmpty notes)] [
            div [ClassName "col text-secondary"] [
                em [] [ str "Notes:" ]
                ul [] notes
            ]
        ]
        
