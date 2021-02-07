#r "../_lib/Fornax.Core.dll"
#load "layout.fsx"

open Html

let generate' (ctx : SiteContents) (page: string) =  
    ctx.TryGetValues<Postloader.Post> ()
    |> Option.bind (Seq.tryFind (fun n -> n.file = page))
    |> Option.map (fun post ->
        let siteInfo = ctx.TryGetValue<Globalloader.SiteInfo> ()
        let desc =
            siteInfo
            |> Option.map (fun si -> si.description)
            |> Option.defaultValue ""

        Layout.layout ctx post.title [
            section [Class "hero is-info is-medium is-bold"] [
                div [Class "hero-body"] [
                    div [Class "container has-text-centered"] [
                        h1 [Class "title"] [!!desc]
                    ]
                ]
            ]
            div [Class "container"] [
                section [Class "articles"] [
                    div [Class "column is-10 is-offset-1"] [
                        Layout.postLayout false post
                    ]
                ]
            ]
        ]
    )
    |> Option.defaultValue (div [] [])    

let generate (ctx : SiteContents) (projectRoot: string) (page: string) =
    generate' ctx page
    |> Layout.render ctx