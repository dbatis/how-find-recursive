namespace HowFindRecursive

open System
open System.Text.RegularExpressions
open HowFindRecursive.BuilderInput

/// <summary>
/// Functions to build command for <c>find</c> tool.
/// </summary>
/// <remarks>
/// Some functions are re-used in <c>fd</c> module,
/// when there is no difference between these tools
/// </remarks>
[<RequireQualifiedAccess>]
module FindBuilder =

    /// Helper function to always have a correct path
    let pathOrDefault path =
        if String.length path > 0 then Utils.escapeLinuxPath path else "."
    
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
        | Minutes -> $"-{prefix}min {modifier}{date.number}"
        | Hours -> $"-{prefix}min {modifier}{date.number * 60}"
        | Days -> $"-{prefix}time {modifier}{date.number}"
        | Weeks -> $" --{prefix}time {modifier}{date.number * 7}"
        | Months -> $"-{prefix}time {modifier}{date.number |> monthsToDays sinceDate}"
        | Years -> $"-{prefix}time {modifier}{date.number |> yearsToDays sinceDate}"
    
    let modifiedParameter (sinceDate: DateTime) (date: DateSeek) : string =
        match date.number with
        | 0 -> ""
        | _ -> timeParam sinceDate "m" date
        
    let accessedParameter (sinceDate: DateTime) (date: DateSeek) : string =
        match date.number with
        | 0 -> ""
        | _ -> timeParam sinceDate "a" date
    
    let namePattern (style: PatternStyle) (pattern: string) : string =
        match style with
        | Glob when pattern.Length > 0 -> $"-name '{pattern.Trim()}'"
        | Regexp when pattern.Length > 0  -> $"-regex '{pattern.Trim()}'"
        | _ -> ""
        
    let typeParameter targetType =
        match targetType with
        | Directory -> "-type d"
        | File -> "-type f"
        | _ -> ""

    let private copyForEach =
        """-print0 | while read -d $'\0' file
          |do
          |    SRC_FILE=$(readlink -f "$file")
          |    TARGET_FILE=${"SRC_FILE/$SRC_DIR/$DEST_DIR"}
          |    TARGET_DIR=$(dirname $TARGET_FILE)
          |    mkdir -p "$TARGET_DIR"
          |    cp -rf "$SRC_FILE" "$TARGET_FILE"
          |done""" |> Utils.stripMargin
          
    let copyCmd sourceFolder targetFolder findParts =
        [$"""SRC_DIR=$(readlink -f {sourceFolder}); \
            |DEST_DIR=$(readlink -f {targetFolder}); \
            """ |> Utils.stripMargin
        ] @ findParts @ [copyForEach]
    
    let private moveForEach =
        """-print0 | while read -d $'\0' file
          |do
          |    SRC_FILE=$(readlink -f "$file")
          |    TARGET_FILE=${"SRC_FILE/$SRC_DIR/$DEST_DIR"}
          |    TARGET_DIR=$(dirname $TARGET_FILE)
          |    mkdir -p "$TARGET_DIR"
          |    if [ -e "$SRC_FILE" ]; then
          |        mv -f "$SRC_FILE" "$TARGET_FILE"
          |    fi
          |done""" |> Utils.stripMargin
    
    let moveCmd sourceFolder targetFolder findParts =
        [$"""SRC_DIR=$(readlink -f {sourceFolder}); \
            |DEST_DIR=$(readlink -f {targetFolder}); \
            """ |> Utils.stripMargin
        ] @ findParts @ [moveForEach]
    
    /// <summary>
    /// Append the action part. Some actions are just additions in the end of the string, others
    /// encapsulate the <c>find</c> operation.
    /// </summary>
    /// <param name="action">What to do with found files</param>
    /// <param name="sourceFolder">Give source folder again for rsync</param>
    /// <param name="execParam">Style of exec param, since find and fd differ.</param>
    /// <param name="findParts">The rest of the find command, as its location will differ.</param>
    let appendAction action sourceFolder execParam findParts =
        match action with
        | List -> findParts @ ["| less"]
        | Delete -> findParts @ [$"{execParam} rm -rf {{}} \;"]
        | MoveToTrash Gnome -> findParts @ [$"{execParam} gio trash {{}} \;"]
        | MoveToTrash TrashCli -> ["# sudo apt install trash-cli\n"] @ findParts @ [$"{execParam} trash-put {{}} \;"]
        | MoveToTrash MacOS -> ["# brew install trash\n"] @ findParts @ [$"{execParam} trash {{}} \;"]
        | Copy attr when attr.preserveStructure ->
            let dest = pathOrDefault attr.dest
            copyCmd sourceFolder dest findParts
        | Copy attr -> // do not preserve structure
            let dest = pathOrDefault attr.dest
            findParts @ [$"{execParam} cp -rf {{}} {dest} \;"]
        | Move attr when attr.preserveStructure ->
            let dest = pathOrDefault attr.dest
            moveCmd sourceFolder dest findParts
        | Move attr -> // do not preserve structure
            let dest = pathOrDefault attr.dest
            findParts @ [$"{execParam} mv -f {{}} {dest} \;"]
        | _ -> [""] // should not occur unless by bug, like MoveToTrash Powershell
    
    /// <summary>
    /// Actual constructor that calculates the shell command
    /// </summary>
    /// <param name="sinceDate">Date to convert to days since, when required</param>
    /// <param name="rules">Find command parameters</param>
    let build sinceDate rules =
        let folder = pathOrDefault rules.folder
        [
            $"find {folder}"
            namePattern rules.style rules.pattern
            typeParameter rules.targetType
            modifiedParameter sinceDate rules.lastModified
            accessedParameter sinceDate rules.lastAccessed
        ] |> appendAction rules.action folder "-exec" |> Utils.shellWrapBash 80
            
    /// <summary>
    /// Impure variant of <c>build()</c> which uses current date as input.
    /// </summary>
    /// <param name="rules">Find command parameters</param>
    let buildImpure rules = build DateTime.Now rules