namespace HowFindRecursiveTests

open System
open HowFindRecursive.State
open Xunit
open FsUnit

open HowFindRecursive

module FindBuilderTests =
    
    [<Fact>]
    let ``Must escape special characters`` () =
        FindBuilder.escapePath "Replace [brackets]" |> should equal @"Replace\ \[brackets\]"
        FindBuilder.escapePath @"file\backslashes\.txt" |> should equal @"file\\backslashes\\.txt"
        FindBuilder.escapePath "file @*&$()!#[]:.txt" |> should equal @"file\ \@\*\&\$\(\)\!\#\[\]:.txt"        

    [<Fact>]
    let ``Copy and move action must escape destination`` () =
        let dest = {dest = "Replace [brackets]"; preserveStructure = false}
        FindBuilder.appendAction (Action.Copy dest) "." "find" 
        |> should equal @"find -exec cp -rf {} Replace\ \[brackets\] \;"
    
        FindBuilder.appendAction (Action.Move dest) ".""find"
        |> should equal @"find -exec mv {} Replace\ \[brackets\] \;"

    [<Fact>]
    let ``Do not do anything if dest is empty`` () =
        let dest = {dest = ""; preserveStructure = false}
        FindBuilder.appendAction (Action.Copy dest) "." "find"
        |> should equal ""

        FindBuilder.appendAction (Action.Move dest) ".""find"
        |> should equal ""

    [<Fact>]
    let ``Set date parameters`` () =
        let steadyDate = new DateTime(2025, 4, 20)
        
        Some {qualifier=EarlierThan; number = 3; unit = Days}
        |> FindBuilder.modifiedParameter steadyDate
        |> should equal " -mtime +3"

        Some {qualifier=EarlierThan; number = 4; unit = Months}
                |> FindBuilder.modifiedParameter steadyDate
                |> should equal " -mtime +121"
        
        Some {qualifier=EarlierThan; number = 3; unit = Hours}
        |> FindBuilder.modifiedParameter steadyDate
        |> should equal " -mmin +180"

        Some {qualifier=Exactly; number = 3; unit = Hours}
        |> FindBuilder.modifiedParameter steadyDate
        |> should equal " -mmin 180"

        Some {qualifier=LaterThan; number = 3; unit = Hours}
        |> FindBuilder.modifiedParameter steadyDate
        |> should equal " -mmin -180"

        Some {qualifier=LaterThan; number = 3; unit = Hours}
        |> FindBuilder.accessedParameter steadyDate
        |> should equal " -amin -180"

    [<Fact>]
    let ``Build correct find commands`` () =
        let steadyDate = new DateTime(2025, 4, 20)
        let mutable data = {
            folder = "/home/user/downloads"
            style = Glob
            targetType = All
            pattern = "*.jpg"
            lastModified = None
            lastAccessed = Some { qualifier=EarlierThan; number = 3; unit = Days }
            action = Delete
        }
        
        FindBuilder.build steadyDate data
        |> should equal @"find /home/user/downloads -name '*.jpg' -atime +3 -exec rm -rf {} \;"
        
        data <- { data with style = Regexp; pattern = @".+\.log"}
        
        FindBuilder.build steadyDate data
        |> should equal @"find /home/user/downloads -regex '.+\.log' -atime +3 -exec rm -rf {} \;"

        data <- { data with
                    lastAccessed = None
                    action = Copy {dest="/home/user/documents";preserveStructure=false}
                }
        FindBuilder.build steadyDate data
        |> should equal @"find /home/user/downloads -regex '.+\.log' -exec cp -rf {} /home/user/documents \;"

        data <- { data with
                    action = Copy {dest="/home/user/documents";preserveStructure=true}
                }
        FindBuilder.build steadyDate data
        |> should equal @"rsync -av --progress --file-from <(find /home/user/downloads -regex '.+\.log') /home/user/downloads /home/user/documents"
