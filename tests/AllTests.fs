module Program

open Fable.Mocha
open System

Mocha.runTests  [
        Test.DOM.tests
        Test.Store.tests
        Test.Observable.tests
        ]
