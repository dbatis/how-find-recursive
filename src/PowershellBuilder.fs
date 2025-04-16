namespace HowFindRecursive

open System.Text.RegularExpressions
open HowFindRecursive.BuilderInput

/// <summary>
/// Functions to build command for Windows PowerShell.
/// </summary>
[<RequireQualifiedAccess>]
module PowershellBuilder =

    /// <summary>
    /// Builds file property filter for access/modified time.
    /// </summary>
    /// <param name="fileProperty">file property to compare (LastWriteTime, LastAccessTime)</param>
    /// <param name="date">date filter info</param>
    let timeParam (fileProperty: string) (date: DateSeek) : string =
        let comparator = match date.qualifier with
                         | EarlierThan -> "-lt"
                         | LaterThan -> "-gt"
                         | Exactly -> "" // not supported 
                   
        match date.unit with
        | Minutes -> $" | ?{{ $_.{fileProperty} {comparator} (Get-Date).AddMinutes(-{date.number}) }}"
        | Hours -> $" | ?{{ $_.{fileProperty} {comparator} (Get-Date).AddHours(-{date.number}) }}"
        | Days -> $" | ?{{ $_.{fileProperty} {comparator} (Get-Date).AddDays(-{date.number}) }}"
        | Weeks -> $" | ?{{ $_.{fileProperty} {comparator} (Get-Date).AddDays(-{date.number*7}) }}"
        | Months -> $" | ?{{ $_.{fileProperty} {comparator} (Get-Date).AddMonths(-{date.number}) }}"
        | Years -> $" | ?{{ $_.{fileProperty} {comparator} (Get-Date).AddYears(-{date.number}) }}"
    
    let modifiedParameter (date: DateSeek) : string =
        match date.number with
        | 0 -> ""
        | _ when date.qualifier <> Exactly -> timeParam "LastWriteTime" date
        | _ -> ""
    
    let accessedParameter (date: DateSeek) : string =
        match date.number with
        | 0 -> ""
        | _ when date.qualifier <> Exactly -> timeParam "LastAccessTime" date
        | _ -> ""

    /// Build glob pattern, which is a Get-ChildItem argument
    let globPattern action =
        match action.style with
        | Glob when action.pattern <> "" -> $" -Include '{action.pattern}'"
        | _ -> ""
    
    /// Build regex pattern, which is a piped filter
    let regexPattern action =
        match action.style with
        | Regexp -> $" | ?{{ $baseDir=Convert-Path -LiteralPath {Utils.escapePowershellPath action.folder}; $_.FullName.Replace($baseDir, '') -match '{action.pattern}' }}"
        | _ -> ""
    
    let typeParameter targetType =
        match targetType with
        | Directory -> " -Directory"
        | File -> " -File"
        | _ -> ""
    
    /// Literal with the PS code to move each item to trash
    let private moveToTrashForeach =
        """foreach {
          |    if (Test-Path -Type Container -Path $_.FullName) {
          |        [Microsoft.VisualBasic.FileIO.FileSystem]::DeleteDirectory($_.FullName,'OnlyErrorDialogs','SendToRecycleBin')
          |    } else {
          |        [Microsoft.VisualBasic.FileIO.FileSystem]::DeleteFile($_.FullName,'OnlyErrorDialogs','SendToRecycleBin')
          |    } 
          |}""" |> Utils.stripMargin
    
    /// Literal with the PS code to copy each item to destination directory. Expects to be used by copyCmd.
    let private copyForeach =
        """foreach {
          |    $targetFile = $_.FullName.replace($srcDir,$destDir)
          |    $targetParent = Split-Path -Path $targetFile -Parent
          |    if (!(Test-Path -Type Container -Path $targetParent)) {
          |        New-Item -Type Directory -Path "$targetParent" -Force
          |    }
          |    Copy-Item -Path $_.FullName -Destination $targetFile -Force -Recurse -Container:$false
          |}""" |> Utils.stripMargin
    
    let copyCmd sourceFolder targetFolder findStr=
        $"""$srcDir=Convert-Path -LiteralPath {sourceFolder}
           |$destDir=Convert-Path -LiteralPath {Utils.escapePowershellPath targetFolder}
           |{findStr} | {copyForeach}
           """ |> Utils.stripMargin
         
    /// Literal with the PS code to move each item to destination directory. Expects to be used by copyCmd.
    let private moveForeach =
        """foreach {
          |    $targetFile = $_.FullName.replace($srcDir,$destDir)
          |    $targetParent = Split-Path -Path $targetFile -Parent
          |    if (!(Test-Path -Type Container -Path $targetParent)) {
          |        New-Item -Type Directory -Path "$targetParent" -Force
          |    }
          |    Move-Item -Path $_.FullName -Destination $targetFile -Force 
          |}""" |> Utils.stripMargin
          
    let moveCmd sourceFolder targetFolder findStr=
        $"""$srcDir=Convert-Path -LiteralPath {sourceFolder}
           |$destDir=Convert-Path -LiteralPath {Utils.escapePowershellPath targetFolder}
           |{findStr} | {moveForeach}
           """ |> Utils.stripMargin
           
    let appendAction action sourceFolder findStr =
        match action with
        | List -> $"{findStr} | more"
        | Delete -> $"{findStr} | foreach {{ $_.Delete() }}"
        | MoveToTrash -> $"Add-Type -AssemblyName Microsoft.VisualBasic; ${findStr} | {moveToTrashForeach}"
        | Copy attr when attr.dest.Length > 0 && attr.preserveStructure -> copyCmd sourceFolder attr.dest findStr
        | Copy attr when attr.dest.Length > 0 ->
            $"{findStr} | foreach {{ Copy-Item -Path $_.FullName -Destination {Utils.escapePowershellPath attr.dest} -Container -Force }}"
        | Move attr when attr.dest.Length > 0 && attr.preserveStructure -> moveCmd sourceFolder attr.dest findStr
        | Move attr when attr.dest.Length > 0 ->
            $"{findStr} | foreach {{ Move-Item -Path $_.FullName -Destination {Utils.escapePowershellPath attr.dest} -Force }}"
        | _ -> ""
    
    /// <summary>
    /// Actual constructor that calculates the shell command
    /// </summary>
    /// <param name="rules">Find command parameters</param>
    let build rules =
        let folder = if rules.folder.Length > 0 then $"{Utils.escapePowershellPath rules.folder}" else "'.'"
        let glob = globPattern rules
        let regex = regexPattern rules
        let ttype = typeParameter rules.targetType
        let modified = modifiedParameter rules.lastModified
        let accessed = accessedParameter rules.lastAccessed
        $"Get-ChildItem -Path {folder} -Recurse{glob}{ttype}{regex}{modified}{accessed}" |> appendAction rules.action folder
