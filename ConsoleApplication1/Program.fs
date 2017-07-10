// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

open System
open System.Threading
open Suave
open Suave.Filters
open Suave.Operators
open Suave.Successful
open System.IO

[<EntryPoint>]
let main argv = 
  let cts = new CancellationTokenSource()
  let conf = { defaultConfig with cancellationToken = cts.Token ; homeFolder = Some (Path.GetFullPath "filez")}

  let sleep milliseconds message: WebPart =
      fun (x : HttpContext) ->
        async {
          do! Async.Sleep milliseconds
          return! OK message x
        }

  //File.WriteAllText("temp.txt","toto D:")
  let bluePrint = File.ReadAllText("filez/temp.txt")

  let startAnew =
        fun (str : String) ->
        File.WriteAllText("filez/" + str + ".txt", bluePrint)
        //let myNewBoard = File.ReadAllText(str + ".txt")
        //myNewBoard
        OK (str + " created")

  let pPlay =
        fun (str : String) -> 
        let myState = File.ReadAllText("filez/" + str + ".txt" )
        OK myState



  let app =
      choose [
        GET >=> choose
            [ path "/" >=> Files.browseFileHome "index.html"
             // path "/game.js" >=> Files.browseFileHome "game.js"
              path "/hello" >=> OK "why hey hello"
              path "/goodbye" >=> OK "Good bye GET"
              pathScan "/newgame/%s" startAnew 
              pathScan "/pick/%s" pPlay 
              pathScan "/stop/%s" pPlay
              pathScan "/gameState/%s" pPlay 
            ]
        POST >=> choose
            [ path "/hello" >=> OK "Hello POST"
              path "/" >=> Files.browseFileHome "index.html"
              path "/goodbye" >=> OK "Good bye POST" ]
        GET >=> Files.browseHome
        RequestErrors.NOT_FOUND "Page not found."  
        ]


  let listening, server = startWebServerAsync conf (app)
    
  Async.Start(server, cts.Token)
  printfn "Make requests now"
  Console.ReadKey true |> ignore
    
  cts.Cancel()

  0 // return an integer exit code