namespace HowFindRecursiveTests

open System
open HowFindRecursive.BuilderInput
open Xunit
open FsUnit

open HowFindRecursive

module FindBuilderTests =
    
    [<Fact>]
    let ``Copy and move action must escape destination`` () =
        let dest = {dest = "Replace [brackets]"; preserveStructure = false}
        FindBuilder.appendAction (Action.Copy dest) "." "-exec" ["find"]
        |> Utils.shellWrapBash 80 
        |> should equal @"find -exec cp -rf {} Replace\ \[brackets\] \;"
    
        FindBuilder.appendAction (Action.Move dest) "." "-exec" ["find"]
        |> Utils.shellWrapBash 80 
        |> should equal @"find -exec mv -f {} Replace\ \[brackets\] \;"

    [<Fact>]
    let ``Do not do anything if dest is empty`` () =
        let dest = {dest = ""; preserveStructure = false}
        FindBuilder.appendAction (Action.Copy dest) "." "-exec" ["find"]
        |> should be Empty

        FindBuilder.appendAction (Action.Move dest) "." "-exec" ["find"]
        |> should be Empty

    [<Fact>]
    let ``Set date parameters`` () =
        let steadyDate = new DateTime(2025, 4, 20)
        
        {qualifier=EarlierThan; number = 3; unit = Days}
        |> FindBuilder.modifiedParameter steadyDate
        |> should equal "-mtime +3"

        {qualifier=EarlierThan; number = 4; unit = Months}
                |> FindBuilder.modifiedParameter steadyDate
                |> should equal "-mtime +121"
        
        {qualifier=EarlierThan; number = 3; unit = Hours}
        |> FindBuilder.modifiedParameter steadyDate
        |> should equal "-mmin +180"

        {qualifier=Exactly; number = 3; unit = Hours}
        |> FindBuilder.modifiedParameter steadyDate
        |> should equal "-mmin 180"

        {qualifier=LaterThan; number = 3; unit = Hours}
        |> FindBuilder.modifiedParameter steadyDate
        |> should equal "-mmin -180"

        {qualifier=LaterThan; number = 3; unit = Hours}
        |> FindBuilder.accessedParameter steadyDate
        |> should equal "-amin -180"

    [<Fact>]
    let ``Build correct find commands`` () =
        let steadyDate = new DateTime(2025, 4, 20)
        let mutable data = {
            folder = "/home/user/downloads"
            style = Glob
            targetType = All
            pattern = "*.jpg"
            lastModified = emptyDateField
            lastAccessed = { qualifier=EarlierThan; number = 3; unit = Days }
            action = Delete
        }
        
        FindBuilder.build steadyDate data
        |> should equal (
            ["find /home/user/downloads"; "-name '*.jpg'"; "-atime +3"; @"-exec rm -rf {} \;"]
            |> Utils.shellWrapBash 80
        )
        
        data <- { data with style = Regexp; pattern = @".+\.log"}
        
        FindBuilder.build steadyDate data
        |> should equal (
            ["find /home/user/downloads"; @"-regex '.+\.log'"; "-atime +3"; @"-exec rm -rf {} \;"]
            |> Utils.shellWrapBash 80
        )

        data <- { data with
                    lastAccessed = emptyDateField
                    action = Copy {dest="/home/user/documents";preserveStructure=false}
                }
        FindBuilder.build steadyDate data
        |> should equal (
            ["find /home/user/downloads"; @"-regex '.+\.log'"; @"-exec cp -rf {} /home/user/documents \;"]
            |> Utils.shellWrapBash 80
        )

        // data <- { data with
        //             action = Copy {dest="/home/user/documents";preserveStructure=true}
        //         }
        // FindBuilder.build steadyDate data
        // |> should equal @"rsync -av --progress --file-from <(find /home/user/downloads -regex '.+\.log') /home/user/downloads /home/user/documents"
        //
        // data <- { data with targetType = File }
        // FindBuilder.build steadyDate data
        // |> should equal @"rsync -av --progress --file-from <(find /home/user/downloads -regex '.+\.log' -type f) /home/user/downloads /home/user/documents"
