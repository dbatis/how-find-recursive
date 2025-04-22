namespace HowFindRecursive

open Elmish
open Fable.Core
open HowFindRecursive.BuilderInput


module UiModel =
    
    [<StringEnum>]
    type OutputType = Find | Fd | Powershell
    
    type Model = {
        buildParams: BuildParams
        outputType: OutputType
        output: string
        modifiedNumber: string // separate from build params, to avoid issues with string -> int conversion
        accessedNumber: string
        copyMoveParams: Destination // to cache them when action is switched
        trashEnvironment: TrashEnvironment // keep it separate to re-insert it as needed
    }
    
    type Msg =
        | BuildOutput
        | ChangeOutputType of OutputType
        | ChangeSourceFolder of string
        | ChangeTargetType of TargetType
        | ChangeStyle of PatternStyle
        | ChangePattern of string
        | ChangeModifiedQualifier of TimeQualifier
        | ChangeModifiedUnit of TimeUnit
        | ChangeModifiedNumber of string
        | ChangeAccessedQualifier of TimeQualifier
        | ChangeAccessedUnit of TimeUnit
        | ChangeAccessedNumber of string
        | ChangeAction of Action
        | ChangeCopyMoveDest of string
        | ChangeCopyMovePreserve of bool
        | CopyChanged 
        | MoveChanged
        | TrashEnvironmentChanged of TrashEnvironment
    
    let init() =
        {
            buildParams = {
                folder = ""
                style = Glob
                targetType = File
                pattern = ""
                lastModified = emptyDateField
                lastAccessed = emptyDateField
                action = List
            }
            outputType = Find
            output = ""
            modifiedNumber = ""
            accessedNumber = ""
            copyMoveParams = { dest = ""; preserveStructure = false }
            trashEnvironment = Gnome
        }, Cmd.ofMsg BuildOutput

    let update msg model =
        match msg with
        | BuildOutput ->
            let output = match model.outputType with
                         | Find -> FindBuilder.buildImpure model.buildParams
                         | Fd -> FdBuilder.build model.buildParams
                         | Powershell -> PowershellBuilder.build model.buildParams
            {model with output = output}, Cmd.none
            
        | ChangeOutputType outputType ->
            {model with outputType = outputType}, Cmd.ofMsg BuildOutput
        
        | ChangeSourceFolder str ->
            let buildParams = {model.buildParams with folder = str}
            {model with buildParams = buildParams}, Cmd.ofMsg BuildOutput
        
        | ChangeTargetType target ->
            let buildParams = {model.buildParams with targetType = target}
            {model with buildParams = buildParams}, Cmd.ofMsg BuildOutput
        
        | ChangeStyle patternStyle -> 
            let buildParams = {model.buildParams with style = patternStyle}
            {model with buildParams = buildParams}, Cmd.ofMsg BuildOutput
        
        | ChangePattern s -> 
            let buildParams = {model.buildParams with pattern = s}
            {model with buildParams = buildParams}, Cmd.ofMsg BuildOutput
        
        | ChangeModifiedQualifier timeQualifier ->
            let modified = {model.buildParams.lastModified with qualifier = timeQualifier}
            let buildParams = {model.buildParams with lastModified = modified}
            {model with buildParams = buildParams}, Cmd.ofMsg BuildOutput
        
        | ChangeModifiedUnit timeUnit -> 
            let modified = {model.buildParams.lastModified with unit = timeUnit}
            let buildParams = {model.buildParams with lastModified = modified}
            {model with buildParams = buildParams}, Cmd.ofMsg BuildOutput
        
        | ChangeModifiedNumber s ->
            let modified = {model.buildParams.lastModified with number = Utils.tryIntegerOrZero s}
            let buildParams = {model.buildParams with lastModified = modified}
            {model with buildParams = buildParams; modifiedNumber = s}, Cmd.ofMsg BuildOutput
        
        | ChangeAccessedQualifier timeQualifier -> 
            let accessed = {model.buildParams.lastAccessed with qualifier = timeQualifier}
            let buildParams = {model.buildParams with lastAccessed = accessed}
            {model with buildParams = buildParams}, Cmd.ofMsg BuildOutput
        
        | ChangeAccessedUnit timeUnit -> 
            let accessed = {model.buildParams.lastAccessed with unit = timeUnit}
            let buildParams = {model.buildParams with lastAccessed = accessed}
            {model with buildParams = buildParams}, Cmd.ofMsg BuildOutput
        
        | ChangeAccessedNumber s -> 
            let accessed = {model.buildParams.lastAccessed with number = Utils.tryIntegerOrZero s}
            let buildParams = {model.buildParams with lastAccessed = accessed}
            {model with buildParams = buildParams; accessedNumber = s}, Cmd.ofMsg BuildOutput
        
        | ChangeAction action -> 
            let buildParams = {model.buildParams with action = action}
            {model with buildParams = buildParams}, Cmd.ofMsg BuildOutput
        
        | ChangeCopyMoveDest s ->
            let newDest = {model.copyMoveParams with dest = s}
            let newModel = {model with copyMoveParams = newDest}
            
            match model.buildParams.action with
            | Copy _ -> newModel, Cmd.ofMsg CopyChanged       
            | Move _ -> newModel, Cmd.ofMsg MoveChanged
            | _ -> newModel, Cmd.none
        
        | ChangeCopyMovePreserve b -> 
            let newDest = {model.copyMoveParams with preserveStructure = b}
            let newModel = {model with copyMoveParams = newDest}
            
            match model.buildParams.action with
            | Copy _ -> newModel, Cmd.ofMsg CopyChanged       
            | Move _ -> newModel, Cmd.ofMsg MoveChanged
            | _ -> newModel, Cmd.none
        
        | CopyChanged ->
            match model.buildParams.action with
            | Copy _ ->
                let buildParams = {model.buildParams with action = Copy model.copyMoveParams}
                {model with buildParams = buildParams}, Cmd.ofMsg BuildOutput
            | _ -> model, Cmd.none
        
        | MoveChanged ->
            match model.buildParams.action with
            | Move _ ->
                let buildParams = {model.buildParams with action = Move model.copyMoveParams}
                {model with buildParams = buildParams}, Cmd.ofMsg BuildOutput
            | _ -> model, Cmd.none
        
        | TrashEnvironmentChanged environment ->
            match model.buildParams.action with
            | MoveToTrash _ ->
                let buildParams = {model.buildParams with action = MoveToTrash environment}
                {model with buildParams = buildParams; trashEnvironment = environment}, Cmd.ofMsg BuildOutput
            | _ -> model, Cmd.none
