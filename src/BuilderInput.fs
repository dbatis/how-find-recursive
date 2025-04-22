namespace HowFindRecursive

open Fable.Core

module BuilderInput =
    
    // Enums
    [<StringEnum>] type PatternStyle = Glob | Regexp
    [<StringEnum>] type TimeUnit = Minutes | Hours | Days | Weeks | Months | Years
    [<StringEnum>] type TargetType = File | Directory | All
    [<StringEnum>] type Output = Find | Fd | Powershell
    [<StringEnum>] type TimeQualifier = EarlierThan | Exactly | LaterThan
    [<StringEnum>] type TrashEnvironment = Gnome | TrashCli | MacOS | PS
    
    type Destination = {
        dest: string
        preserveStructure: bool
    }
    
    /// Defines the action
    type Action =
        | List
        | Delete
        | MoveToTrash of TrashEnvironment
        | Copy of Destination
        | Move of Destination
    
    /// Defines the rules for date seek
    type DateSeek = {
        qualifier: TimeQualifier
        number: int
        unit: TimeUnit
    }
    
    /// Defines the rules to find files
    type BuildParams = {
        folder: string
        style: PatternStyle
        targetType: TargetType
        pattern: string
        lastModified: DateSeek
        lastAccessed: DateSeek
        action: Action
    }
    
    let emptyDateField = {qualifier = EarlierThan; number = 0; unit = Days}
    
    // helper functions for conditions
    let isMoveToTrash input =
        match input.action with
        | MoveToTrash _ -> true
        | _ -> false
    
    let isMoveOrCopy input =
        match input.action with
        | Copy _ -> true
        | Move _ -> true
        | _ -> false
    
    let isPreserve input =
        match input.action with
        | Copy x when x.preserveStructure -> true
        | Move x when x.preserveStructure -> true
        | _ -> false
    
    let isCopy input = 
        match input.action with
        | Copy _ -> true
        | _ -> false
