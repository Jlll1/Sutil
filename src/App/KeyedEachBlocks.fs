module KeyedEachBlocks

// Adapted from
// https://svelte.dev/examples

open Sutil
open Sutil.Attr
open Sutil.DOM
open Sutil.Styling
open System
open Feliz

type Thing = { Id : int; Color : string }
let Color t = t.Color
let Id t = t.Id

let ThingView (viewId : int) (thing : IObservable<Thing>) : NodeFactory =
        let initialColor = thing |> Store.current |> Color

        let thingStyle = [
            rule "span" [
                Css.display.inlineBlock
                CssXs.padding "0.2em 0.5em"
                CssXs.margin "0 0.2em 0.2em 0"
                Css.width (length.em 8)
                Css.textAlign.center
                Css.borderRadius (length.em 0.2)
                Css.color "#eeeeee"
            ]
        ]

        Html.div [
            Bind.fragment thing <| fun t ->
                Html.p [
                    Html.span [ style [ Css.backgroundColor t.Color ]; text $"{t.Id} {t.Color} #{viewId}" ]
                    Html.span [ style [ Css.backgroundColor initialColor ]; text "initial" ]
                ] |> withStyle thingStyle
        ]

let view() =
    let nextId = Helpers.makeIdGenerator()

    let makeThing thing = ThingView (nextId()) thing

    let things = Store.make [
        { Id = 1; Color = "darkblue" }
        { Id = 2; Color = "indigo" }
        { Id = 3; Color = "deeppink" }
        { Id = 4; Color = "salmon" }
        { Id = 5; Color = "gold" }
    ]

    let handleClick _ =
        things |> Store.modify List.tail

    Html.div [
        disposeOnUnmount [things]

        Html.button [
            onClick handleClick []
            text "Remove first thing"
        ]

        Html.div [
            style [ Css.display.grid; Css.gridTemplateColumns [length.fr 1; length.fr 1]; CssXs.gridGap "1em" ]

            Html.div [
                Html.h2 [ text "Keyed" ]
                eachiko things (snd>>makeThing) (snd>>Id) []
            ]

            Html.div [
                Html.h2 [ text "Unkeyed" ]
                eachio things (snd>>makeThing) []
            ]
        ]
    ]