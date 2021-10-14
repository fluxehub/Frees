open System
open ThreesAI
open type SDL2.SDL

type Textures =
    { Tiles: Texture }

let screenWidth = 32 * 2 * 4 * 2
let screenHeight = 48 * 2 * 4 * 2

let pollEvents ()  =
    let rec pollLoop events =
        let mutable event = Unchecked.defaultof<SDL_Event>
        if (SDL_PollEvent &event) <> 0 then
            pollLoop <| event :: events
        else
            events
    pollLoop []

let gameLoop (window: Rendering.Window) (renderer: Rendering.Renderer) textures =
    
    let rec eventLoop () =
        SDL_RenderClear renderer |> ignore
        Texture.render textures.Tiles renderer (0, 0) (Some { X = 64; Y = 0; W = 32; H = 48 }) |> ignore
        Texture.render textures.Tiles renderer (32 * 4, 0) (Some { X = 32; Y = 0; W = 32; H = 48 }) |> ignore
        Texture.render textures.Tiles renderer (32 * 4, 48 * 4) (Some { X = 0; Y = 0; W = 32; H = 48 }) |> ignore
        SDL_RenderPresent renderer
        
        let events = pollEvents ()
        
        let matchQuit (event: SDL_Event) =
            event.``type`` = SDL_EventType.SDL_QUIT
        
        if List.exists matchQuit events then
            ()
        else
            eventLoop ()
        
    eventLoop ()
    SDL_DestroyRenderer renderer
    SDL_DestroyWindow window
    SDL_Quit ()
    
let init () = ResultBuilder.resultBuilder {
    let! window, renderer = Rendering.init ("ThreesAI", screenWidth, screenHeight)
    let! tiles = Texture.create renderer "assets/tiles.png" 4
    
    do! Rendering.setDrawColor renderer (0xB6uy, 0xCDuy, 0xF0uy, 0xFFuy)
    
    return (window, renderer, { Tiles = tiles })
}

[<EntryPoint>]
let main _ =
    match init () with
    | Ok (window, renderer, textures) -> gameLoop window renderer textures
    | Error e -> printfn $"SDL Error: {e}"
    |> ignore 
    0
    