---
layout: post
title: Functions.  Just functions.
author: @laenas
published: 2021-03-20
---

## Three words
If there's one thing, one rule, that I hope to get across in this pile of words, it's that ***functions are values***.

Functions.
Are.
Values.

These three words are a key principle that unlock a door to a different way of solving problems, an important and significant step in solving problems and writing simpler, more functional code.  But there often seems to be a divide between being able to strictly read this - which everyone gets;  the ability to use it when presented with an API expecting it - something I think most developers these days do, even if they don't realize it;  and the ability to *wield* the idea to design our own code with this concept in mind.  The pause appears to occur most severely on that latter step - as though standing on the bridge over the chasm has given pause and a wondering which side is closer.  Let's help cross that bridge, as to better add another tool to our bags.

## A value or a name

What do we mean when we say 'functions are values'?  Let's attack this as specifically as possible, and ensure that we've defined our terms.  
First:  `values`.  Every developer understands values, as they're so critical to our work that to need to define or think about it, is like trying to be conscious about the act of breathing.
```fs
let x = 3
let y = "foo"
let z = [9;8;7]
```

Here, we have some values.  `x` is an `int` with a value of `3`.  And so on.  **Left hand side is a name, right hand side is the value.**

These also mean that there is referential equivalence between using the name and the value itself.  In the above, anywhere I can use the raw value `3`, I can now use `x` and achieve the same result.

Then: `functions`.  Every developer understands functions, they're also fundamental to our work, regardless of language or paradigm.  They allow us to name and capture a subset of our code in such a way that we can both split up our program textually, but also reuse parts of the code from multiple places.  Functions have *one* defining characteristic:  They take some values as arguments and produce some other value as a result.
```fs
let double x = x * 2
let add x y = x + y
```

We have a few arithmatic functions:  `double` takes a single argument and multiplies it by two.  `add` takes two arguments and sums them.  We're all perfectly familiar with this:  functions taking values and producing values.

## Left hand side: name.

We're all used to declaring functions.  We're all used to calling them.  And in what seems to be most language these days, we're even comfortable using them as arguments:
```fs
[1;2;3]
|> List.map (fun i -> i * 2) //[2;4;6]
```

This sort of lambda syntax comes naturally once you've used it for even short while.  I just have to provide a little transformation function and it'll call it with each thing in my list.  Sure, this makes sense.

But here's the question:  If functions are defined because they take values and produce other values, and `List.map` is a function, then what are the **values** that we give it as arguments?  Definitely our list of `[1;2;3]` sure - but...also that lambda.

Let's rewrite the above, slightly:
```fs
let double x = x * 2

[1;2;3]
|> List.map double
```

See it?

## Right hand side: value.  

A quick recap of points.  
Values can be given a name: `let x = 3`  
And afterwards, we can use either the basic value or the name interchangeably: `double x` and `double 3` will both produce the same result.  
Functions take values as arguments to produce new values: `let double x = x * 2`  
  
And now, with `List.map` as our go-to example function, we've seen that the mapping function we use can be passed in as either a lambda, or a previously declared and named function.  Repeated with emphasis:  It can be passed in as either *a lambda*  or *a named function*.  A value or a name.  

## Now, the weeds

Let's look at the same simple function, but written three separate ways:
```fs
let add x y = x + y

let add x = (fun y -> x + y)

let add = (fun x y -> x + y)
```

These are all conceptually equivalent - and all can be called identically: `add 1 2` will produce `3`.  But it's worth it to pause for a moment and consider the three forms:  
  
Everyone is familiar with the first form - we declare the name, we name the parameters, and we do something with them.  It's the baseline way of working with functions[1]

The second form is one that we tend to see while learning F# - oftentimes in examples involving partial application.  But it's here that we also may start feeling a fog set in:  Because we tend to learn, by default, that functions have to return *solid* things, *values*, something - for lack of a better word - tangible.  We're used to the concept of something like `add` giving us a number back.  So there's something a little weird and alien at the idea that this...thing...isn't a number, it's a function.  But it's also here where we probably have that lightbulb moment, at least in part, that 'ok, cool, so I can make lambdas and return them so that they can be called!'.  But that's like learning to walk, without learning to run.

The third form, and I want to be perfectly crystal clear I sincerely don't advocate for writing functions in this way, I do so solely to unify some examples.  But here we have something that looks more like what we see in all non-function cases in the language - a name on the left, a value on the right.  It lays bare the fact that our function *is* just 'a lambda' with a name.  Equivalency, the same as `let x = 3`.

## Put to good use

While this is trivial - in the case of `add` - or familiar to a point of acceptance, in the case of `List.map` - there's something more powerful lurking here that deserves a little bit more attention.  It's the power to dramatically shift the overall flow of an application without needing to dramatically alter the primary flow path of the code.

Consider an example:  We have an application that will search an `InputDir` for files with `ExtensionSuffix` and we'll output whatever work we're doing to some `OutputDir`.  We also have some defaults.
```fs
type AppConfig = {
    InputDir: string
    ExtensionSuffix: string
    OutputDir: string
}

let defaultConfig = {
    InputDir = "./search"
    ExtensionSuffix = ".log"
    OutputDir = "./results"
}
```

But we want to allow users to override these settings using some form of runtime config.  For the sake of brevity, we'll ignore *where* this configuration comes from, as well as *how* it gets parsed.  But we'll assume that what we have to work with is something in the form of:
```fs
type ConfigOption =
    | InputDir of string
    | ExtensionSuffix of string
    | OutputDir of string
```

And we'll assume these are parsed into a list.

Now, the first way newcomers to FP will tend to approach such a problem has the air of other languages to it - especially those without first-class functions:
```fs
let configureApp (options: ConfigOption list) =
    let config = defaultConfig

    let inputDir =
        options
        |> List.tryFind (fun opt -> 
            match opt with
            | InputDir _ -> true
            | _ -> false)

    let config = 
        if Option.isSome inputDir then
            {config with InputDir = Option.get inputDir}
        else
            config

    //Snip - you get the picture    
```

Do note, none of the above is 'real' code, it's just an off-the-cuff amalgamation of the sort of things that seem to come up rather repeatedly.  And somewhere around here, the question rightfully tends to come up as well: "How can I easily test which case a union type is?"  (Which is generally a smell in its own right.)  As well as having to deal better with the Option that's caused by dealing safely with the list, branching in general - it's painful.  I earnestly think it is likely to push some people away - because in this sense, something like C# *looks* cleaner - you aren't *forced* to `else` the `if` and can use null-coalescing with LINQ to avoid the Option.  And those feelings will also make us potentially lean towards trying to model all our individual options as their own types - and write helper functions for them - but then we start to recognize the duplication of logic down that road as well.  But what about if we take what we've learned about functions and apply it here, what can we do?
```fs
let configureApp (options: ConfigOption list) =
    options
    |> List.map (fun opt ->
        match opt with
        | InputDir dir -> (fun cfg -> {cfg with InputDir = dir})
        | ExtensionSuffix ext -> (fun cfg -> {cfg with ExtensionSuffix = ext})
        | OutputDir dir -> (fun cfg -> {cfg with OutputDir = dir}))
    |> List.fold (|>) defaultConfig
```

What's going on here?  The major difference here is in the recognition of three key details:  First is that our options - conceptually, not in code - just want to update the config;  thinking about this in F#, pretending to write that function itself, we can notice how the shape `AppConfig -> AppConfig` just feels natural.  Take a config, return an altered config.  Second is that what we're trying to do:  take an `AppConfig`(our default), apply it to our first function, then take that function's output and apply it to the second, and so forth.  That's pipelining - but since we can't know at compile-time which functions to call we can't just *write* a pipeline, so we need a way to create a 'dynamic' pipeline.  The third and final piece of the puzzle, that ties the whole thing together, is that *functions are values* - as we saw above - and so it's possible in the first place to have something like an `(AppConfig -> AppConfig) list` - a list of *functions*.  So we have a list of functions that by signature compose themselves, we just need a starting point, and some way to combine them sequentially - and that sure sounds pretty much spot-on for `List.fold`, doesn't it?  

So we take our list of options.  We transform each one into a function that updates a config - keep in mind that they don't have to be lambdas:  they can be named functions.  It's entirely the same!  But now that we have a list of functions that update a config, we fold them over our default config.  Admittedly, using the `|>` operator in such a way is a flourish, but it conceptually pairs better with that previous realization that what we want *is* to pipeline.  We could have used an explicit folder function as well: 
```fs
//snip
|> List.fold (fun cfg optF -> optF cfg) defaultConfig
```

When people say that F# is a terse language, and packs a lot of power per LOC relative to C#, this is the sort of thing that seems to be the case.  Yes - C# is shorter and nicer when we compare direct, naive solutions - but when you lean into the power of the language, F# starts to be shorter, more expressive, and still somehow more typesafe - the union means we know that all config options get checked, and not needing to do an equivalent of `tryFind` to figure out if an option exists means we can just directly map whatever we do have and work with it.  And while you can achieve a similar behavior in C# with `Func<TConfig,TConfig>` and LINQ's `Aggregate`, both of those will result in much less comprehensible code - to say nothing about the trouble in trying to model the options themselves without a union.

## In summary

Functions are values and behave similarly to all other values we use in the language.
The left hand side of a `let` is a name (and some parameter names, for functions) - and the right hand side is the value (and what we do with those params, for functions)
This goes from ints to lists to classes to functions.  And in cases where we can deal with more primitive types, we can also have function types themselves:
```fs
//Record fields (though Interfaces can be recommended)
type FuncRecord = {F: int -> int}

//Union cases
type FuncUnion = FuncUnion of (int -> int)

//Type argument to generic types
type FuncList = (int -> int) list

//Tuple members
type FuncTuple = (int * (int -> int))
```

And in the same way that we're all familiar that things like LINQ's `Select` (F#'s `map`) is more expressive than writing a full `foreach` or list expression to do the same work - we can also use that same elegance, that same power, in our own code outside the BCL itself.  Because whether they come from the BCL, or our own code, functions are just values.  So let's use them as such!