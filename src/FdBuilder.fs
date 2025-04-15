namespace HowFindRecursive

open System
open HowFindRecursive.BuilderInput

/// <summary>
/// Functions for <c>fd</c> tool. Reuses <c>FindBuilder</c> when result is the same between
/// the two commands.
/// </summary>
[<RequireQualifiedAccess>]
module FdBuilder =

    let namePattern (style: PatternStyle) (pattern: string) : string =
        match style with
        | Glob when pattern.Length > 0 -> $"-g '{pattern.Trim()}'"
        | Regexp when pattern.Length > 0  -> $"--regex '{pattern.Trim()}'"
        | _ -> "'.*'" // since we need a pattern before folder in fd

    let modifiedParameter (date: DateSeek) : string =
        match date.number with
        | 0 -> ""
        | _ when date.qualifier <> Exactly ->
            let param = match date.qualifier with
                        | EarlierThan -> "--changed-before"
                        | LaterThan -> "--changed-within"
                        | Exactly -> "" // not supported
                       
            match date.unit with
            | Minutes -> $" {param} {date.number}mins"
            | Hours -> $" {param} {date.number}hours"
            | Days -> $" {param} {date.number}days"
            | Weeks -> $" {param} {date.number}weeks"
            | Months -> $" {param} {date.number}months"
            | Years -> $" {param} {date.number}years"
                    
        | _ -> ""

    let appendAction action sourceFolder findStr =
        // same as find, but use --exec instead of -exec
        FindBuilder.appendAction action sourceFolder "--exec" findStr
        
    /// <summary>
    /// Actual constructor that calculates the shell command
    /// </summary>
    /// <param name="rules">Find command parameters</param>
    let build rules =
        let folder = Utils.escapeLinuxPath (if rules.folder.Length > 0 then rules.folder else ".")
        let name = namePattern rules.style rules.pattern
        let ttype = (FindBuilder.typeParameter rules.targetType).Replace("-t", "--t")
        let modified = modifiedParameter rules.lastModified
        $"fd {name} {folder}{ttype}{modified}" |> appendAction rules.action folder
    