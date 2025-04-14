namespace HowFindRecursive

open System
open HowFindRecursive.State

/// <summary>
/// Functions for <code>fd</code> tool. Reuses <code>FindBuilder</code> when result is the same between
/// the two commands.
/// </summary>
[<RequireQualifiedAccess>]
module FdBuilder =

    let namePattern (style: PatternStyle) (pattern: string) : string =
        match style with
        | Glob when pattern.Length > 0 -> $"-g '{pattern.Trim()}'"
        | Regexp when pattern.Length > 0  -> $"--regex '{pattern.Trim()}'"
        | _ -> "'.*'" // since we need a pattern before folder in fd

    let modifiedParameter (date: DateSeek option) : string =
        match date with
        | Some d ->
            let param = match d.qualifier with
                        | EarlierThan -> "--changed-before"
                        | LaterThan -> "--changed-within"
                        | Exactly -> "" // not supported
                       
            match d.unit with
            | Minutes -> $" {param} {d.number}mins"
            | Hours -> $" {param} {d.number}hours"
            | Days -> $" {param} {d.number}days"
            | Weeks -> $" {param} {d.number}weeks"
            | Months -> $" {param} {d.number}months"
            | Years -> $" {param} {d.number}years"
                    
        | _ -> ""

    let appendAction action sourceFolder findStr =
        // same as find, but use --exec instead of -exec
        FindBuilder.appendAction action sourceFolder "--exec" findStr
        
    /// <summary>
    /// Actual constructor that calculates the shell command
    /// </summary>
    /// <param name="rules">Find command parameters</param>
    let build rules =
        let folder = FindBuilder.escapePath (if rules.folder.Length > 0 then rules.folder else ".")
        let name = namePattern rules.style rules.pattern
        let ttype = (FindBuilder.typeParameter rules.targetType).Replace("-t", "--t")
        let modified = modifiedParameter rules.lastModified
        $"fd {name} {folder}{ttype}{modified}" |> appendAction rules.action folder
    