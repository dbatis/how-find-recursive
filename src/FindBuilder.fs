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
        specialCharsRx.Replace(path.Trim(), "\\$1")
    
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
        | Weeks -> $" --{prefix}time {modifier}{date.number * 7}"
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
        | Glob when pattern.Length > 0 -> $" -name '{pattern.Trim()}'"
        | Regexp when pattern.Length > 0  -> $" -regex '{pattern.Trim()}'"
        | _ -> ""
        
    let typeParameter targetType =
        match targetType with
        | Directory -> " -type d"
        | File -> " -type f"
        | _ -> ""

    let (|IsPreserve|_|) (attr: Destination) =
        attr.preserveStructure

    /// <summary>
    /// Append the action part. Some actions are just additions in the end of the string, others
    /// encapsulate the <code>find</code> operation.
    /// </summary>
    /// <param name="action">What to do with found files</param>
    /// <param name="sourceFolder">Give source folder again for rsync</param>
    /// <param name="execParam">Style of exec param, since find and fd differ.</param>
    /// <param name="findStr">The rest of the find command, as its location will differ.</param>
    let appendAction action sourceFolder execParam findStr =
        match action with
        | List -> $"{findStr} | less"
        | Delete -> $"{findStr} {execParam} rm -rf {{}} \;"
        | MoveToTrash -> $"{findStr} {execParam} gio trash {{}} \;"
        | Copy attr when attr.dest.Length > 0 ->
            match attr with
            | IsPreserve -> $"rsync -av --progress --file-from <({findStr}) {sourceFolder} {escapePath attr.dest}"
            | _ -> $"{findStr} {execParam} cp -rf {{}} {escapePath attr.dest} \;"
        | Move attr when attr.dest.Length > 0 ->
            match attr with
            | IsPreserve -> $"rsync -av --remove-source-files --prune-empty-dirs --progress --file-from <({findStr}) {sourceFolder} {escapePath attr.dest}"
            | _ -> $"{findStr} {execParam} mv {{}} {escapePath attr.dest} \;"
        | _ -> ""
    
    /// <summary>
    /// Actual constructor that calculates the shell command
    /// </summary>
    /// <param name="sinceDate">Date to convert to days since, when required</param>
    /// <param name="rules">Find command parameters</param>
    let build sinceDate rules =
        let folder = escapePath (if rules.folder.Length > 0 then rules.folder else ".")
        let name = namePattern rules.style rules.pattern
        let ttype = typeParameter rules.targetType
        let modified = modifiedParameter sinceDate rules.lastModified
        let accessed = accessedParameter sinceDate rules.lastAccessed
        $"find {folder}{name}{ttype}{modified}{accessed}" |> appendAction rules.action folder "-exec"
        
    
    /// <summary>
    /// Impure variant of <code>build()</code> which uses current date as input.
    /// </summary>
    /// <param name="rules">Find command parameters</param>
    let buildImpure rules = build DateTime.Now rules