namespace HowFindRecursive

open System
open System.Text.RegularExpressions
open HowFindRecursive.State

/// <summary>
/// Functions to build command for <code>find</code> tool.
/// </summary>
/// <remarks>
/// Some functions are re-used in <code>fd</code> module,
/// when there is no difference between these tools
/// </remarks>
[<RequireQualifiedAccess>]
module FindBuilder =

    /// Regular expression to capture characters in shell that need escaping
    let specialCharsRx = Regex(@"([ \\@$\*&\(\)!#\[\]])", RegexOptions.ECMAScript)
        
    let escapePath (path: string) : string =
        specialCharsRx.Replace(path, "\\$1")
    
    let (|IsPreserve|_|) (attr: Destination) =
        attr.preserveStructure
    
    /// Calculate actual number of days since X months ago.
    let monthsToDays (sinceDate: DateTime) (months: int) : int =
        sinceDate - sinceDate.AddMonths(0 - months) |> _.Days

    /// Calculate actual number of days since X years ago.
    let yearsToDays (sinceDate: DateTime) (years: int) : int =
        sinceDate - sinceDate.AddYears(0 - years) |> _.Days
        
    let timeParam (sinceDate: DateTime) (prefix: string) (date: DateSeek) : string =
        let modifier = match date.qualifier with
                       | EarlierThan -> "+"
                       | LaterThan -> "-"
                       | Exactly -> ""
        
        match date.unit with
        | Minutes -> $" -{prefix}min {modifier}{date.number}"
        | Hours -> $" -{prefix}min {modifier}{date.number * 60}"
        | Days -> $" -{prefix}time {modifier}{date.number}"
        | Months -> $" -{prefix}time {modifier}{date.number |> monthsToDays sinceDate}"
        | Years -> $" -{prefix}time {modifier}{date.number |> yearsToDays sinceDate}"
    
    let modifiedParameter (sinceDate: DateTime) (date: DateSeek option) : string =
        match date with
        | Some d -> timeParam sinceDate "m" d
        | _ -> ""
        
    let accessedParameter (sinceDate: DateTime) (date: DateSeek option) : string =
        match date with
        | Some d -> timeParam sinceDate "a" d
        | _ -> ""
    
    let namePattern (style: PatternStyle) (pattern: string) : string =
        match style with
        | Glob when pattern.Length > 0 -> $" -name '{pattern}'"
        | Regexp when pattern.Length > 0  -> $" -regex '{pattern}'"
        | _ -> ""
        
    let typeParameter targetType =
        match targetType with
        | Directory -> " -type d"
        | File -> " -type f"
        | _ -> ""

    /// <summary>
    /// Append the action part. Some actions are just additions in the end of the string, others
    /// encapsulate the <code>find</code> operation.
    /// </summary>
    let appendAction action sourceFolder findStr =
        match action with
        | List -> $"{findStr} | less"
        | Delete -> $"{findStr} -exec rm -rf {{}} \;"
        | MoveToTrash -> $"{findStr} -exec gio trash {{}} \;"
        | Copy attr when attr.dest.Length > 0 ->
            match attr with
            | IsPreserve -> $"rsync -av --progress --file-from <({findStr}) {sourceFolder} {escapePath attr.dest}"
            | _ -> $"{findStr} -exec cp -rf {{}} {escapePath attr.dest} \;"
        | Move attr when attr.dest.Length > 0 ->
            match attr with
            | IsPreserve -> $"rsync -av --remove-source-files --prune-empty-dirs --progress --file-from <({findStr}) {sourceFolder} {escapePath attr.dest}"
            | _ -> $"{findStr} -exec mv {{}} {escapePath attr.dest} \;"
        | _ -> ""
    
    /// <summary>
    /// Actual constructor that calculates the shell command
    /// </summary>
    /// <param name="sinceDate">Date to convert to days since, when required</param>
    /// <param name="rules">Find command parameters</param>
    let build sinceDate rules =
        let folder = escapePath (if rules.folder.Length > 0 then rules.folder else ".")
        let name = namePattern rules.style rules.pattern
        let modified = modifiedParameter sinceDate rules.lastModified
        let accessed = accessedParameter sinceDate rules.lastAccessed
        $"find {folder}{name}{modified}{accessed}" |> appendAction rules.action folder
        
    
    /// <summary>
    /// Impure variant of <code>build()</code> which uses current date as input.
    /// </summary>
    /// <param name="rules">Find command parameters</param>
    let buildImpure rules = build DateTime.Now rules