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

    [<Fact>]
    let ``Must escape special characters for Windows shells`` () =
        Utils.escapePowershellPath "Replace [brackets]" |> should equal @"'Replace `[brackets`]'"
        Utils.escapePowershellPath "file @&$()!#[]:.txt" |> should equal @"'file @&$()!#`[`]:.txt'"

    [<Fact>]
    let ``Must wrap lines if length exceeded`` () =
        let parts = [
            String.replicate 10 "a"
            String.replicate 5 "b"
            String.replicate 25 "c"
            String.replicate 41 "d"
        ]
        let expected = String.replicate 10 "a" + " " + String.replicate 5 "b" + " \\\n" + String.replicate 25 "c" + " \\\n" + String.replicate 41 "d"
        Utils.shellWrap "\\" 40 parts
        |> should equal expected
        
    let ``Must wrap lines based on length of last line`` () =
        let parts = [
            String.replicate 50 "a"
            String.replicate 5 "b"
            String.replicate 25 "c"
            String.replicate 41 "d"
        ]
        let expected = String.replicate 50 "a" + " \\\n " + String.replicate 5 "b" + " " + String.replicate 25 "c" + " \\\n" + String.replicate 41 "d"
        Utils.shellWrap "\\" 40 parts
        |> should equal expected
        