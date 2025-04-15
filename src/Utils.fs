namespace HowFindRecursive

open System
open System.Text.RegularExpressions

[<RequireQualifiedAccess>]
module Utils =

    let private stripMarginRegex = Regex(@"^\s*\|")
    let private onlyWhitepaceRegex = Regex(@"^\s*$")

    /// Call to a regex replace that can be piped.
    let inline regexReplace (regex: Regex) (replacement: string) (source: string) =
        regex.Replace(source, replacement)
    
    let inline addToList (list: 'a list) (item: 'a) =
        list @ [item]
    
    /// <summary>
    /// In a multi-line string, this will replace any whitespace before the first pipe (|) character,
    /// similar to how Scala's stripMargin works.
    ///
    /// The pipe characters need not be in the same indentation on each line. Furthermore, the last line,
    /// if it contains only whitespace characters, will be trimmed.
    /// </summary>
    /// <param name="str">multi-line string</param>
    /// <returns>a string with the indentation before the pipe character, and the pipe itself, removed.</returns>
    let stripMargin (str: string) =
        let rec strip acc list =
            match list with
            | [] -> acc
            | [x] -> x |> regexReplace stripMarginRegex "" |> regexReplace onlyWhitepaceRegex "" |> addToList acc
            | x :: tail -> strip (x |> regexReplace stripMarginRegex "" |> addToList acc) tail
        
        str.ReplaceLineEndings().Split(System.Environment.NewLine)
        |> Array.toList
        |> strip []
        |> String.concat System.Environment.NewLine

    let private specialLinuxCharsRx = Regex(@"([ \\@$\*&\(\)!#\[\]])", RegexOptions.ECMAScript)
    
    /// Escape a path that will be used in bash/zsh shells, without requiring quotes around it.
    let escapeLinuxPath (path: string) : string =
        specialLinuxCharsRx.Replace(path.Trim(), "\\$1")

    let private specialPSCharsRx = Regex(@"(\[\])", RegexOptions.ECMAScript)
    
    /// Escape path characters as required for Powershell. Will put path inside quotes.
    let escapePowershellPath (path: string) : string =
        "'" + specialPSCharsRx.Replace(path.Trim(), "`$1") + "'"
