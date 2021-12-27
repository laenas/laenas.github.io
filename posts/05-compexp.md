---
layout: post
title: There is No Magic - Computation Expressions
author: @laenas
published: 2021-12-27
---

## There is No Magic ##

It's my catchphrase, if I were to have one in the technical realm.  There is No Magic.  It's a mantra and a truism and a guiding principle.  
All too often, in a technical context, I see developers behave like primitive man during an eclipse - their dependencies and frameworks and languages, some unassailable force of nature that is beyond the reckoning of mankind.

The confusion seems to stem from a misjudgment - that something one doesn't understand in the moment must be driven by forces beyond understanding.  It's evil omen thinking.  But we're developers, our core skill is problem solving, and yet we don't always reach for it in the moment.  We perhaps get too focused on the goal, or on our own lack of knowledge, and leave that skill behind.  We don't break it down, we don't do the analysis, we don't trust ourselves.

Let's start changing that.

## Computation Expressions ##

One of the features that makes F# super neat, but also confuses the hell out of newcomers to the language, is Computation Expressions.  In some cases we'll see them called `workflows` or `builders` - for reasons we'll end up at later.

We tend to be first introduced to them via F#'s core `async` and `task` workflows, as it's the strongly preferred way to work with asynchronous operations in F#.  A direct example being a workflow for making an HTTP request using the standard .NET `HttpClient`:
```fs
let httpGet (url:string) = task {
    use http = new HttpClient()
    let! response = http.GetAsync(url)
    return! response.Content.ReadAsStringAsync()
}
```

There are two clues that we are working with a Computation Expression - the first is the combination of an identifier: `task` being followed by a block of curly-braced code;  the second is that block containing `!` versions of familiar terms:  `let!` for example.

Regardless, we can all read this code, and broadly understand what it's doing.  Especially if we're familiar with languages that do similar things with `await` as an inline keyword construct.  We have asynchronous operations like `GetAsync` and something here is doing something to wait for them to finish before moving onwards to the next step in the code;  but also in a non-blocking way.  And we'd be correct in that assumption.

But then we're going to stumble across a custom Computation Expression:
```fs
let unsafeAsyncOperation x = asyncResult {
    let! step1 = doUnsafeThing x
    let step2 = thenSomething step1

    return getResult step1 step2
}
```

And we try to modify it by adding a third step, using that previous HTTP workflow:
```fs
let unsafeAsyncOperation x = asyncResult {
    let! step1 = doUnsafeThing x
    let step2 = thenSomething step1
    let! step3 = httpGet "http://laenas.github.io"

    return getResult step1 step2
}
```

The compiler suddenly complains, it wants an `Async<'a>` but we're giving it a `Task<string>`.
And this is one, but not the only, place where people seem to get conceptually stuck.  What is going on, why doesn't it just work?  They enter a spiral of removing the `!`s, which causes even more arcane compiler errors, or adding them to other places - to the same effect.  The panic sets in, we retreat from our place of logic and principle, and resort to just throwing keypresses at the problem, hoping something will just compile.

We can work with CEs like this, but we can work with them more effectively - especially with regards to building our own - if we understand their machinery.  [MSDN](https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/computation-expressions) has documentation covering much of this, but I think it skips (rightfully) a more foundational step to thinking about the sugaring the compiler applies to Computation Expressions.

## Back to the Beginning ##

The F# language(and compiler) is *constantly* sugaring things for us, in ways that we forget about - especially if we haven't been with the language since its birth - or don't have experience with sibling languages.

One direct example being functions - the mathematical-theoretical underpinnings of F# (and functional programming more generally) are based in the mathematical concept of functions - mappings from some single input value to some output value.  The key point there is single input, even if that single input is a structure like a tuple or a list, it is still logically a single input.  When we write functions with multiple parameters - what we're 'actually' doing is writing a chain of single-input functions that produce single-input functions:
```fs
let f x y = x >>> y
//Is conceptually equivalent to
let g x = (fun y -> x >>> y)
```

We don't have to think about this with F# - we can do the more natural thing by just listing all parameters and, if anything, we end up needing to learn from a different direction the nature of partial application that aligns with this form.  The language has our backs, without sacrificing the foundational concept at play.

In another slightly more dated example, F# has - though not used often - a so-called verbose syntax.  One notable feature of it is it's use of the keyword `in`:
```fs
let x = 1 in
let y = 2 in
x + y
```

I draw attention to it because it poses a curious question:  Why `in`?  What about that order of operations puts `x` "in" the code that follows?  And herein lies a curious link between this and the previous point - If F# is ostensibly grounded in mathematics - input/output functions - then where is the *function* in that snippet?  With experience elsewhere, we might casually shrug this question off as wordplay, but it's got a curious, subtle power.  Those three lines might reside inside a function, certainly, but there is still no function amongst them.  (let us disregard the operator, dear reader)

We can rewrite that snippet in a more conceptually pure way as follows:
```fs
1 |> (fun x -> 
  2 |> (fun y ->
    x + y
  )
)
```

It looks weird, and isn't going to pass for day to day code, but it's logically what's happening.  The value we bind to `x` becoming an 'external' value that gets pushed into a function whereby `x` is the input binding name and the rest of the code being the body of that function.  Allowing us to write it the other way not only helps reduce the burden of increasingly arrowed code, but also structures it in ways that are more familiar and similar to other languages.

This is all well and good, it's curious, but how does it help us demystify Computation Expressions?  We have one more brief detour to make.

## The other Bind ##

It likely seems as a curiosity that when talking about code like `let x = 1` we refer to them as bindings.  That we have bound a value to a name.  As we see above, there's a logic to it.  But we are also familiar with the same word being used in module functions: `Option.bind`, for example.  How this fits together is another, important, part of the overall picture we are attempting to assemble.  When we see code such as this:
```fs

Some 1
|> Option.bind (fun x -> Some (x + 2))
```

It's visible that we are binding the contextual value and assigning a name to it in the function that follows.  In this case, `x` is `1` and `Option.bind` is just acting as a removal of the boilerplate code of matching the input value and in the `Some` case, applying the value, and in the `None` case, continuing onwards.

There's a parallel here:
```fs
Some 1
|> Option.bind (fun x ->
  Some 2 
  |> Option.bind (fun y ->
    Some (x + y)
  )
)
```

There's that arrow again - the same as above, except that we're now dealing with `Option` values, rather than primitives.  And this code is entirely reasonable.  Your codebase is probably filled with situations where you will have chains of functions following this pattern.  It's entirely predictable and normal.  But it's **the same arrow** and wouldn't it be nice if we could flatten it, in the same way that we can with primitive types.

Enter Computation Expressions.

F# doesn't ship with one for `Option` - and from a practicality standpoint it seems like overhead, since `Option` tends to be relatively short-lived in the context of workflows - either getting defaulted or converted to a richer type like `Result` in short order.  But that doesn't mean we can't make one.

## What makes it tick ##

Computation Expressions are based on creating a type - generally called a `Builder` - that implements by convention (some combination)[https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/computation-expressions] of methods of specific known signatures.  And while that page may seem unassailable to begin with, we can demystify it by looking at our own use case and its implementation.  

The `let!` keyword, for example, requires a `Bind` method to be present, and in the table we can see it expects a signature of `M<'T> * ('T -> M<'U>) -> M<'U>` - and it initially looks frightening, just a soup of letters.  But we can quickly make sense of it by substituting in our specific example: `Option<'T> * ('T -> Option<'U>) -> Option<'U>` and that's much more understandable!  Indeed, while it's using tupled params, it's clearly just `Option.bind`!  

```fs
type MaybeBuilder() =
  member _.Bind(x,f) = Option.bind f x

let maybe = MaybeBuilder()

let flat = maybe {
  let! x = Some 1
  let! y = Some 2
}
```

Note here that the term `Maybe` comes up in this(and all other similar examples) both because it's a friendly alternate name for `Option` - but also because the CE style keyword - `option` is already used as a postfix type notation, so `maybe` keeps it simple.

Along with that, a note of the curiosity:  `maybe` is just an alias for an instance of our Builder type.  There's no magic here either - and indeed the consistency remains - you can use the full class name as well (but why would you?):
```fs
let flat = MaybeBuilder() {
  let! x = Some 1
  let! y = Some 2
}
```

But we haven't finished yet, our arrow isn't entirely flattened, we still need the sum of these two values.  To do so, we just need to add support for the `return` keyword, via the `Return` method (`'T -> M<'T>`).  We can logic this one out:  If I give you a value, then clearly you have `Some` value, and that's what we want `Return` to do:

```fs
type MaybeBuilder() =
  member _.Bind(x,f) = Option.bind f x
  member _.Return(x) = Some x

let maybe = MaybeBuilder()

let flat = maybe {
  let! x = Some 1
  let! y = Some 2

  return x + y
}
```

And thus, we've flattened the arrow.  It's all still just that chain of function calls under the hood, but it looks like it isn't.  It's easier to read, it doesn't arrow, and we can ignore some of the complexities.

## And thus armed with this knowledge... ##

Let's return to our earlier example:
```fs
let unsafeAsyncOperation x = asyncResult {
    let! step1 = doUnsafeThing x
    let step2 = thenSomething step1
    let! step3 = httpGet "http://laenas.github.io"

    return getResult step1 step2
}
```

Where our compiler was complaining about wanting an Async, getting a Task, all that good stuff.  With what we now know, can we puzzle out a reasoning to that error, and in doing so, a solution?

We first need to think about what it is that the CE is doing, with all that binding.  We might be inclined to handwave it away as some sort of magic, we've learned the incantation, we're good to go.  But it's important for us to connect the dots, to think it through, and see the bigger picture for what it is.  Let's do so by looking at the CE in the above example:  `asyncResult` - we don't need to know the exact implementation - can be inferred to work with the type `Async<Result<'T>>` - but something very curious is happening here, at the head of which we might ask the question:  Why not just use `async {}`?  And the answer can be drafted quickly enough:

```fs
let asyncResultX : Async<Result<int,unit>> = async {return (Ok 1)}
let asyncResultY : Async<Result<int,unit>> = async {return (Ok 2)}

let func = async {
  let! x = asyncResultX
  let! y = asyncResultY

  return x + y //ERROR
}
```

We get an error here because you cannot directly add, with the `+` infix operator, two `Result`s.  They themselves would need to be bound. So how can something like `asyncResult {}` get around this?

Consider how CE's work:  `let!` is not magically, presciently, stripping away the context of `Async` or `Option` - nor is it automatically calling to `Option.bind` - the builder type is something in our control, and the compiler desugars `let!` into a call to our `Bind` method.  It's all in the control of the developer of the CE!  And since code is just code, we can do what we need.

How can `asyncResult {}` allow you to `let!` bind `Async<Result<>>` to get at the inner value?  Well, it might look something like this:
```fs
//snip
member _.Bind(x,f) = async {
  match! x with
  | Ok x'' -> return! f x''
  | Error _ -> return! x }
```

We use an `async` CE to let us `match!` (bind, then match on the inner value) and that lets us unwrap the `Result` to get at the innermost value to pass it to our continuation function.  As an aside, `return!` does exactly what you'd expect from a `!` - it binds and then returns.  Since our continuation function by necessity must return an `AsyncResult` - if we were to `return` from the `async {}` we would have an `Async<Async<Result<>>>`.

The key here is to recognize that since it's all just code, it's possible to tweak and adjust this code to suit our use cases.  We're able to create CEs for composed cases like `AsyncResult` - just as we could do so for `AsyncOption` - what would that look like?  Give it a shot!  But I draw upon the greater impression:  it's all just code, and we can customize it in other ways as well, using our foundational principles!

Let's return to our `maybe {}` builder.  Let's watch it in motion!

```fs
type MaybeBuilder(format) =
  member _.Bind(x,f) = 
    printfn format x
    Option.bind f x
  member _.Return(x) = Some x

let maybe format = MaybeBuilder(format)

let flat = maybe "--binding: %A" {
  let! x = Some 1
  let! y = Some 2

  return x + y
}
```

Take a moment and consider the somewhat unexpected composition here, don't just understand *what* it is doing, but *how* it is possible to combine things in this manner.  The tool here isn't the pattern of how to debug print in a builder - it's how you can *customize* a builder, and therefore a CE; from 'outside' the CE itself, as a consumer of it.

`MaybeBuilder()` isn't magic, it's just a class type, and like any other class type it can have constructor parameters that it uses elsewhere in its internal logic.

`maybe` isn't magic, it's just a binding, and we've simply changed it from a value binding to a function that is parameterized.

And now our CE prints out the incoming binding values when our `Bind` method is called.

What other compositional logic can you, reader, envision being useful and interesting to add to a CE?

## And thus, our solution ##

We return, finally, to that initial error.  Confused as we were when the compiler barked about type mismatches, and we wrestled with exclamation points, do we see it more clearly now?  

Inside our hypothetical `asyncResult {}` we've attempted to bind a `Task` - perhaps even a `Task<Result<>>`!  But what the compiler is hinting towards is that the builder for our CE doesn't operate against that type.  A conclusion forms:  We need to turn our `Task` into an `Async` - and thus a call to `Async.AwaitTask` resolves our problem. 

When we see our own code arrowing rightwards across the indents, we might be able to stop and consider building our own CE, to not only improve readability, but declutter and reduce opportunities for error and confusion when handling increasingly nested functions.

There's a lot more to be said about CEs, and perhaps in the future the topic can be revisited, but in this instance they're a proxy, a show pony, for the greater lesson I'm trying to deliver:  There Is No Magic.  Everything makes sense, and when faced with the strange, the unknown, the confusing, or the magical - we should use those impressions as a clue to ourselves to strap in and analyze the problem.

It'll save you a lot of time in the long run.