namespace HowFindRecursive

open System.Text.RegularExpressions

[<RequireQualifiedAccess>]
module Utils =

    let private stripMarginRegex = Regex(@"\s*\|")

    /// <summary>
    /// In a multi-line string, this will replace any whitespace before the first pipe (|) character,
    /// similar to how Scala's stripMargin works.
    ///
    /// The pipe characters need not be in the same indentation
    /// </summary>
    /// <param name="str">multi-line string</param>
    /// <returns>a string with the indentation before the pipe character, and the pipe itself, removed.</returns>
    let stripMargin (str: string) =
        str.ReplaceLineEndings().Split(System.Environment.NewLine)
        |> Array.map (fun line -> stripMarginRegex.Replace(line, ""))
        |> String.concat System.Environment.NewLine

    let private specialLinuxCharsRx = Regex(@"([ \\@$\*&\(\)!#\[\]])", RegexOptions.ECMAScript)
    
    /// Escape a path that will be used in bash/zsh shells, without requiring quotes around it.
    let escapeLinuxPath (path: string) : string =
        specialLinuxCharsRx.Replace(path.Trim(), "\\$1")

    let private specialPSCharsRx = Regex(@"(\[\])", RegexOptions.ECMAScript)
    
    /// Escape path characters as required for Powershell. Will put path inside quotes.
    let escapePowershellPath (path: string) : string =
        "'" + specialPSCharsRx.Replace(path.Trim(), "`$1") + "'"
