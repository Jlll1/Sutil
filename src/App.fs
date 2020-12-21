module App

open Sveltish
open Sveltish.Attr
open Sveltish.DOM

module BulmaStyling =
    open Styling

    let bulmaStyleSheet = [
        rule "body" [
            fontFamily "sans-serif"
            color "#4a4a4a"
            fontSize "1em"
            fontWeight "400"
            lineHeight "1.5"
        ]

        rule ".container" [
            margin "0 auto"
            position "relative"
            width "auto"
            maxWidth "960px"
        ]


    ]

module App =
    open BulmaStyling

    //let count = Stores.makeStore 0

    let testApp model dispatch : NodeFactory =
        Styling.style bulmaStyleSheet <| Html.div [
            className "container"
            //Html.p [ text "Sveltish is running" ]
            //Html.p [ text "Counter" ]
            //Counter.Counter { InitialCounter = 0; Label = "Click Me"; ShowHint = true }
            Html.h1 [ text "Sveltish Todos" ]
            Html.div [ Todos.view model dispatch ]

            Html.pre [
                text """


                """
            ]
        ]

Sveltish.Program.makeProgram "sveltish-app" Todos.init Todos.update App.testApp