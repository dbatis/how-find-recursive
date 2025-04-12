namespace HowFindRecursive

open Fable.Core

module State =
    
    // Enums
    [<StringEnum>] type PatternStyle = Glob | Regexp
    [<StringEnum>] type TimeUnit = Minutes | Hours | Days | Months | Years
    [<StringEnum>] type TargetType = File | Directory | All
    [<StringEnum>] type Output = Find | Fd | Powershell
    [<StringEnum>] type TimeQualifier = EarlierThan | Exactly | LaterThan

    type Destination = {
        dest: string
        preserveStructure: bool
    }
    
    /// Defines the action
    type Action =
        | List
        | Delete
        | MoveToTrash
        | Copy of Destination
        | Move of Destination
    
    /// Defines the rules for date seek
    type DateSeek = {
        qualifier: TimeQualifier
        number: int
        unit: TimeUnit
    }
    
    /// Defines the rules to find files
    type FindParameters = {
        folder: string
        style: PatternStyle
        targetType: TargetType
        pattern: string
        lastModified: DateSeek option
        lastAccessed: DateSeek option
        action: Action
    }
    
    