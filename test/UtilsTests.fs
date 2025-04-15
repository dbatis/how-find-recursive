namespace HowFindRecursiveTests

open System
open HowFindRecursive.BuilderInput
open Xunit
open FsUnit

open HowFindRecursive

module UtilsTests =
    
    [<Fact>]
    let ``Strip margins before pipe character`` () =
        let mlString = """ | Line 1
                       |     Line 2 | not deleted
                       """

        let expected = " Line 1" + Environment.NewLine + "     Line 2 | not deleted" + Environment.NewLine
        
        mlString |> Utils.stripMargin |> should equal expected
    
    [<Fact>]
    let ``Must escape special characters for Linux shells`` () =
        Utils.escapeLinuxPath "Replace [brackets]" |> should equal @"Replace\ \[brackets\]"
        Utils.escapeLinuxPath @"file\backslashes\.txt" |> should equal @"file\\backslashes\\.txt"
        Utils.escapeLinuxPath "file @*&$()!#[]:.txt" |> should equal @"file\ \@\*\&\$\(\)\!\#\[\]:.txt"        

    let ``Must escape special characters for Windows shells`` () =
        Utils.escapePowershellPath "Replace [brackets]" |> should equal @"'Replace \[brackets\]'"
        Utils.escapePowershellPath "file @&$()!#[]:.txt" |> should equal @"'file @&$()!#\[\]:.txt'"
