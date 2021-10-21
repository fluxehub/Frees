﻿module ThreesAI.Execution.GameProcessor

open type SDL2.SDL
open ThreesAI
open Controls
    
let newStateFromMessage message state =
    match message with
    | ShiftUp    -> state |> Game.shift Up
    | ShiftDown  -> state |> Game.shift Down
    | ShiftLeft  -> state |> Game.shift Left
    | ShiftRight -> state |> Game.shift Right
    | Restart    -> Game.create ()
    | _          -> state
    
// If I had more time for this project I would find a more functional way to do this,
// but I'm going to do it this way for now
type stateProcessor() =
    let mutable state = Game.create ()
    let mutable exit = false
    
    member this.AsyncRenderLoop (sdlData: Display.SDLData) = async {
        let rec renderLoop () =
            if not exit then 
                SDL_RenderClear sdlData.Renderer |> ignore
                Display.render sdlData.Renderer sdlData.Textures state
                SDL_RenderPresent sdlData.Renderer
                renderLoop ()
            else
                ()
        
        renderLoop ()
    }
    
    member this.StartRender (sdlData: Display.SDLData) =
        Async.Start(this.AsyncRenderLoop sdlData)
    
    member this.Start () =
        MailboxProcessor<Message>.Start(fun inbox ->
            let rec updateLoop () = async {
                // Wait for update message
                let! message = inbox.Receive()
                
                if message = Exit then
                    // Exit the loop cleanly
                    exit <- true
                    ()
                else
                    state <- state |> newStateFromMessage message
                    
                    
                    if state.GameOver then
                        printfn "Game over! Press r to restart."
                        
                    return! updateLoop ()
            }

            updateLoop ())
        