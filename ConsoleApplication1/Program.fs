// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

open System
open System.Threading
open Suave
open Suave.Filters
open Suave.Operators
open Suave.Successful
open System.IO
open Suave.Json
open System.Runtime.Serialization
open Newtonsoft.Json
open Newtonsoft.Json.Serialization


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


      // 'a -> WebPart
  let JSON v =
      let jsonSerializerSettings = new JsonSerializerSettings()
      jsonSerializerSettings.ContractResolver <- new CamelCasePropertyNamesContractResolver()

      JsonConvert.SerializeObject(v, jsonSerializerSettings)
      |> OK
      >=> Writers.setMimeType "application/json; charset=utf-8"


  let startAnew =
        fun (str : String) ->
        File.WriteAllText("filez/" + str + ".txt", bluePrint)
        //let myNewBoard = File.ReadAllText(str + ".txt")
        //myNewBoard
        OK (str + " created")

  let pPlay =
        fun (game : String, player : String) -> 
        let myState = File.ReadAllText("filez/" + game + ".txt" )
        myState |> JSON

  let pStop =
        fun (game : String, player : String) -> 
        let myState = File.ReadAllText("filez/" + game + ".txt" )
        OK ("Player" + player + "stoped")

  let pWatch =
        fun (game : String, player : String) -> 
        let myState = File.ReadAllText("filez/" + game + ".txt" )
        OK myState
   
  let pJoin = 
        fun (str : String )->
        let lines = 
            File.ReadAllLines("filez/" + str + ".txt" )
            |> Array.map(fun line ->
                let newLine = line
                newLine )
        let listLines = lines |> Array.toList
        let item = listLines.Item(1)
        listLines |> JSON

  let app =
      choose [
        GET >=> choose
            [ path "/" >=> Files.browseFileHome "index.html"
             // path "/game.js" >=> Files.browseFileHome "game.js"
              path "/hello" >=> OK "why hey hello"
              path "/goodbye" >=> OK "Good bye GET"
              pathScan "/newgame/%s" startAnew 
              pathScan "/pick/%s/%s" pPlay 
              pathScan "/stop/%s/%s" pStop
              pathScan "/gameState/%s/%s" pWatch 
              pathScan "/join/%s" pJoin
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