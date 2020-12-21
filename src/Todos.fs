module Todos

open Sveltish
open Sveltish.Stores
open Sveltish.Styling
open Sveltish.Attr
open Sveltish.DOM
open Sveltish.Bindings
open Browser.Types

open Sveltish.Transition

type Todo = {
        Id : int
        mutable Done: bool
        Description: string
    }

// List helpers
let listCount f list = list |> List.filter f |> List.length

// Todo helpers
let isDone t = t.Done
let todoKey r = r.Id

type Model = {
    Todos : Store<List<Todo>>
}

type Message =
    |AddTodo of desc:string
    |ToggleTodo of id:int
    |DeleteTodo of id:int
    |CompleteAll

let makeExampleTodos() = makeStore [
    { Id = 1; Done = false; Description = "1:write some docs" }
    { Id = 2; Done = false; Description = "2:start writing JSConf talk" }
    { Id = 3; Done =  true; Description = "3:buy some milk" }
    { Id = 4; Done = false; Description = "4:mow the lawn" }
    { Id = 5; Done = false; Description = "5:feed the turtle" }
    { Id = 6; Done = false; Description = "6:fix some bugs" }
]

let newUid = CodeGeneration.makeIdGeneratorFrom(7)

let styleSheet = [
    rule ".new-todo" [
        fontSize "1.4em"
        width "100%"
        margin "2em 0 1em 0"
    ]

    rule ".board" [
        maxWidth "36em"
        margin "0 auto"
    ]

    rule ".left, .right" [
        //float' "left"
        width "50%"
        padding "0 1em 0 0"
        boxSizing "border-box"
    ]

    rule "h2" [
        fontSize "2em"
        fontWeight  "200"
        userSelect  "none"
    ]

    rule "label"  [
        top "0"
        left "0"
        display "block"
        fontSize "1em"
        lineHeight "1"
        padding "0.5em"
        margin "0 auto 0.5em auto"
        borderRadius "2px"
        backgroundColor "#eee"
        userSelect "none"
    ]

    rule "input" [  margin "0" ]

    rule ".right label" [
        backgroundColor "rgb(180,240,100)"
    ]

    rule "label>button" [
        float' "right"
        height "1em"
        boxSizing "border-box"
        padding "0 0.5em"
        lineHeight "1"
        backgroundColor "transparent"
        border "none"
        color "rgb(170,30,30)"
        opacity "0"
        Attr.transition "opacity 0.2s"
    ]

    rule "label:hover button" [
        opacity "1"
    ]

    rule ".row" [
        display "flex"
    ]

    rule ".welldone" [
        marginTop "12px"
        marginBottom "12px"
    ]

    rule ".complete-all" [
        border "1px solid transparent"
        borderRadius "4px"
        boxShadow "none"
        fontSize "1rem"
        height "2.5em"
        position "relative"
        verticalAlign "top"

        backgroundColor "#fff"
        borderColor "#dbdbdb"
        borderWidth "1px"
        color "#363636"
        cursor "pointer"
        paddingBottom "calc(.5em - 1px)"
        paddingLeft "1em"
        paddingRight "1em"
        paddingTop "calc(.5em - 1px)"
        textAlign "center"
        whiteSpace "nowrap"
    ]
]

let update (message : Message) (model : Model) : unit =

    match message with
    | AddTodo desc ->
        let todo = {
            Id = newUid() + 10
            Done = false
            Description = desc
        }
        model.Todos <~ (model.Todos |-> (fun x -> x @ [ todo ])) // Mutation of model
    | ToggleTodo id ->
        match (storeFetchByKey todoKey id model.Todos) with
        |None -> ()
        |Some todo ->
            todo.Done <- not todo.Done
            forceNotify model.Todos // People will forget to do this
    | DeleteTodo id ->
        model.Todos <~ (model.Todos |-> List.filter (fun t -> t.Id <> id) )
    | CompleteAll ->
        model.Todos <~ (model.Todos |-> List.map (fun t -> { t with Done = true }) )


let lotsDone model = model.Todos |%>  (fun x -> x |> (listCount isDone) >= 3)

let fader  x = transition <| Both (Transition.fade,[ Duration 2000.0 ]) <| x
let slider x = transition <| Both (Transition.slide,[ Duration 2000.0 ])  <| x


let todosList cls title filter tin tout model dispatch =

    Html.div [
        class' cls
        Html.h2 [ text title ]

        Bindings.each model.Todos (fun (x:Todo) -> x.Id) filter (InOut (tin,tout) ) (fun todo ->
            Html.label [
                Html.input [
                    attr ("type","checkbox")
                    on "change" (fun e -> todo.Id |> ToggleTodo |> dispatch)
                    bindAttrIn "checked" (model.Todos |~> (makePropertyStore todo "Done"))
                    //Bindings.bindAttr "checked" ((makePropertyStore todo "Done") <~| model.Todos)
                ]
                text " "
                text todo.Description
                Html.button [
                    on "click" (fun _ -> todo.Id |> DeleteTodo |> dispatch)
                    text "x"
                ]
            ]
        )
    ]

let init() = { Todos = makeExampleTodos() }

let view (model : Model) dispatch : NodeFactory =
    let (send,recv) = Transition.crossfade [ ]
    let tsend = send, []
    let trecv = recv, []

    style styleSheet <| Html.div [
        class' "board"
        Html.p [ text "Note: initial transitions are weird, but subsequent ones are fine. Initialization bug, I'm on it" ]
        Html.input [
            class' "new-todo"
            placeholder "what needs to be done?"
            onKeyDown (fun e ->
                if e.key = "Enter" then (e.currentTarget :?> HTMLInputElement).value |> AddTodo |> dispatch
            )
        ]

        Html.div [
            class' "row"
            todosList "left" "todo" (fun t -> not t.Done) trecv tsend model dispatch
            todosList "right" "done" (fun t -> t.Done) trecv tsend model dispatch
        ]

        Html.div [
            Html.button [
                class' "complete-all"
                text "Complete All"
                on "click" (fun _ -> dispatch CompleteAll)
            ]
        ]

        Html.div [
            class' "welldone"
            text <| (model.Todos |-> (fun x -> sprintf "%d tasks completed! Good job!" x.Length))
        ] |> fader (model |> lotsDone)
    ]
