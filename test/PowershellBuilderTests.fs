namespace HowFindRecursiveTests

open System
open HowFindRecursive.BuilderInput
open Xunit
open FsUnit

open HowFindRecursive

module PowershellBuilderTests =
    
    [<Fact>]
    let ``Set date parameters`` () =
        
        {qualifier=EarlierThan; number = 3; unit = Days}
        |> PowershellBuilder.modifiedParameter 
        |> should equal "| ?{ $_.LastWriteTime -lt (Get-Date).AddDays(-3) }"

        {qualifier=EarlierThan; number = 4; unit = Months}
                |> PowershellBuilder.modifiedParameter 
                |> should equal "| ?{ $_.LastWriteTime -lt (Get-Date).AddMonths(-4) }"
        
        {qualifier=EarlierThan; number = 4; unit = Weeks}
                |> PowershellBuilder.modifiedParameter 
                |> should equal "| ?{ $_.LastWriteTime -lt (Get-Date).AddDays(-28) }"
                
        {qualifier=EarlierThan; number = 3; unit = Hours}
        |> PowershellBuilder.modifiedParameter 
        |> should equal "| ?{ $_.LastWriteTime -lt (Get-Date).AddHours(-3) }"

        {qualifier=LaterThan; number = 3; unit = Hours}
        |> PowershellBuilder.modifiedParameter 
        |> should equal "| ?{ $_.LastWriteTime -gt (Get-Date).AddHours(-3) }"

        {qualifier=LaterThan; number = 3; unit = Hours}
        |> PowershellBuilder.accessedParameter 
        |> should equal "| ?{ $_.LastAccessTime -gt (Get-Date).AddHours(-3) }"

    [<Fact>]
    let ``Build correct find commands`` () =
        let mutable data = {
            folder = @"c:\users\my user\Downloads\test"
            style = Glob
            targetType = All
            pattern = "*.jpg"
            lastModified = emptyDateField
            lastAccessed = { qualifier=EarlierThan; number = 3; unit = Days }
            action = Delete
        }
        
        PowershellBuilder.build data
        |> should equal (
            [@"Get-ChildItem -Path 'c:\users\my user\Downloads\test' -Recurse"
             @"-Include '*.jpg'"
             @"| ?{ $_.LastAccessTime -lt (Get-Date).AddDays(-3) }"
             @"| foreach { $_.Delete() }"
            ] |> Utils.shellWrapPowershell 80)
        
        data <- { data with style = Regexp; pattern = @".+\.log"}
        
        PowershellBuilder.build data
        |> should equal (
            [@"Get-ChildItem -Path 'c:\users\my user\Downloads\test' -Recurse"
             @"| ?{ $baseDir=Convert-Path -LiteralPath 'c:\users\my user\Downloads\test'; $_.FullName.Replace($baseDir, '') -match '.+\.log' }"
             @"| ?{ $_.LastAccessTime -lt (Get-Date).AddDays(-3) }"
             @"| foreach { $_.Delete() }"
            ] |> Utils.shellWrapPowershell 80)
        
        data <- { data with targetType = Directory }
        
        PowershellBuilder.build data
        |> should equal (
            [@"Get-ChildItem -Path 'c:\users\my user\Downloads\test' -Recurse"
             @"-Directory"
             @"| ?{ $baseDir=Convert-Path -LiteralPath 'c:\users\my user\Downloads\test'; $_.FullName.Replace($baseDir, '') -match '.+\.log' }"
             @"| ?{ $_.LastAccessTime -lt (Get-Date).AddDays(-3) }"
             @"| foreach { $_.Delete() }" 
            ] |> Utils.shellWrapPowershell 80)
