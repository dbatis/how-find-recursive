namespace HowFindRecursiveTests

open System
open HowFindRecursive.BuilderInput
open Xunit
open FsUnit

open HowFindRecursive


module FdBuilderTests =

    [<Fact>]
    let ``Set date parameters`` () =
        Some {qualifier=EarlierThan; number = 3; unit = Days}
        |> FdBuilder.modifiedParameter
        |> should equal " --changed-before 3days"

        Some {qualifier=EarlierThan; number = 4; unit = Months}
                |> FdBuilder.modifiedParameter
                |> should equal " --changed-before 4months"
        
        Some {qualifier=EarlierThan; number = 3; unit = Hours}
        |> FdBuilder.modifiedParameter
        |> should equal " --changed-before 3hours"

        Some {qualifier=LaterThan; number = 3; unit = Hours}
        |> FdBuilder.modifiedParameter
        |> should equal " --changed-within 3hours"
        
    [<Fact>]
    let ``Build correct find commands`` () =
        let mutable data = {
            folder = "/home/user/downloads"
            style = Glob
            targetType = All
            pattern = "*.jpg"
            lastModified = Some { qualifier=EarlierThan; number = 3; unit = Days }
            lastAccessed = None
            action = Delete
        }
        
        FdBuilder.build data
        |> should equal @"fd -g '*.jpg' /home/user/downloads --changed-before 3days --exec rm -rf {} \;"
        
        data <- { data with style = Regexp; pattern = @".+\.log"}
        
        FdBuilder.build data
        |> should equal @"fd --regex '.+\.log' /home/user/downloads --changed-before 3days --exec rm -rf {} \;"

        data <- { data with
                    lastModified = None
                    action = Copy {dest="/home/user/documents";preserveStructure=false}
                }
        FdBuilder.build data
        |> should equal @"fd --regex '.+\.log' /home/user/downloads --exec cp -rf {} /home/user/documents \;"

        data <- { data with
                    action = Copy {dest="/home/user/documents";preserveStructure=true}
                }
        FdBuilder.build data
        |> should equal @"rsync -av --progress --file-from <(fd --regex '.+\.log' /home/user/downloads) /home/user/downloads /home/user/documents"

        data <- { data with targetType = File }
        FdBuilder.build data
        |> should equal @"rsync -av --progress --file-from <(fd --regex '.+\.log' /home/user/downloads --type f) /home/user/downloads /home/user/documents"
