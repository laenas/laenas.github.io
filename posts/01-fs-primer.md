---
layout: post
title: An F# Primer for curious C# developers.
author: @laenas
published: 2020-12-19
---
## Foreword: ## 
My adoption of F# as a language-of-choice was slightly rocky.  After around a decade of nearly exclusive C# work, my curiosity was piqued with an uptick in hearing about this other #-lang.  My initial reaction was one I've since seen in other C# developers - dismissal - C# is a good language and I was comfortable with it so why bother with the effort of learning a different one?  But the curiosity remained - and at least a few times I decided I'd set aside an evening to go through a basic introduction post and try to write some katas in F#.  It didn't stick because I just felt lost and couldn't translate my experience with C# into feeling even remotely comfortable with F#.  Easy enough to drop the curly braces, a little bit of a hiccup to remember to `let` instead of `var` - but how to *do* what I wanted?  
  
I didn't realize it then, but what I was observing is, I think, a potential gap in the way that F# developers talk about, describe, and introduce their language to the outside world.  There's a thorough library of materials about all the features and functionality of F#: Algebraic Data Types, Exhaustive Matching, Type Inference, the works.  There's a lot of articles handling how to solve a wide range of problems with F#.  But what's missing is, I think, something like what follows here:  Some guideposts about how to take what you are already comfortable with in C# and translate them into F#.  So, I wonder if we can't close that gap somewhat.
  
Doing so expects just a little bit from the reader - a passing familiarity with three main points of F# syntax:  `let` is used like `var` in C# - to declare a variable.  `|>` is F#'s piping operator, which takes the left side and uses it as the final argument to the right side.  F# uses lowercase and a tick for generic type annotations, so `SomeType<T>` is represented as `SomeType<'a>`.
  
The rest should be understandable from usage and context as we go.  This isn't meant to be a comprehensive, no stone left unturned, guide - but enough information to cover most initial questions and get people off on the right foot.  A primer, if you will.

# Table of Contents
[I need to:]
* [Work with collections]
  * [Choose a collection type]
    * [Something like `Array<T>`] 
    * [Something like `List<T>`]
    * [Something like `Dictionary<TKey,TValue>`]
  * [Choose a function]
    * [I just want my LINQ]
    * [I'm not sure which function I need. I have]
      * [A collection, and want:]
        * [A single value or element:]
        * [An equal number of elements:]
        * [A possibly smaller number of elements:]
        * [A possibly greater number of elements:]
        * [To change the shape of the collection:]
        * [To iterate it without changing it:]
      * [A single value, and want:]
        * [It to be part of a collection:]
      * [Multiple collections, and want:]
        * [To combine them:]
* [Work Asynchronously]
* [Signal an error or control the program flow]
* [Use a C# library in F#]  
   
# I need to:

## Work with collections
F#'s core collection types (mostly) tend to look a lot like C#'s, but often with (sometimes subtle) behavioral differences to enforce immutability.  In most cases, functions that operate on these collections will return references and will not modify the original reference's contents.

## Choose a collection type
### Something like `Array<T>`
You're in luck!  Arrays in F# are the same as Arrays in C#.  A few points to be made, however:  
1. Arrays in F# generally use the `[|element|]` notation - because `[]` is the notation for F# Lists. 
2. Separating collection elements in F# involves a semicolon, rather than a comma: `[|elementA;elementB|]`
3. Accessing by index in F# requires a prefixed dot before the braces:
```fs
let myArray = [|1;2;3|]
myArray.[1] //2
```
4. F# also offers multidimensional arrays of up to 4 dimensions, through the `Array2<'a>`, `Array3<'a>`, and `Array4<'a>` types.

### Something like `List<T>`
The default list type in F# is slightly different than the `List<T>` type in C#.  
  
Here's what you need to know:  

1. Lists in F# generally use the `[element]` notation instead of arrays.
2. Lists, just like arrays, separate elements with semicolons instead of commas: `[elementA;elementB]`
3. F# Lists are implemeneted as singly-linked lists - which means that appending individual elements is at the front of the list with the `@` operator:
```fs
let myList = [1;2;3]
4 @ myList //[4;1;2;3]
``` 
5. If we need to append at the end, we can use the `::` operator to join two lists:
```fs
let listA = [1;2]
let listB = [3;4]
listA :: listB //[1;2;3;4]
```

### Something like `Dictionary<TKey,TValue>`
Along with the looks-similar-but-isn't motif of `list` - F# provides a default `Map<'key,'value>` type that isn't C#'s native `Dictionary<TKey,TValue>`, but does implement the usual group of .NET interfaces such as `IDictionary<TKey,TValue>` and `IEnumerable<T>`

Here's what you need to know:
1. Maps can be created from any collection of 2-item tuples, where the first item is the key and the second is the value:
```fs
[(1,2);(3,4)] |> Map.ofList //[1] = 2, [3] = 4
```  
2. If there are duplicates when we create from a sequence like this, the last value for a given key is what the Map contains:  
```fs
[(1,2);(1,3)] |> Map.ofList |> Map.find 1 = 3 //true
```
3. The reverse process is also true: Maps can be easily turned into collections of 2-item tuples:  
```fs
[(1,2);(3,4)] |> Map.ofList |> Map.toList //[(1,2);(3,4)]
```
4. F#'s native `Map` type isn't especially well suited to consumption by C#, in cases of interop, we can create a more C#-friendly `IDictionary` by utilizing the `dict` function with any collection of 2-item tuples.  But do note, this is still an immutable structure, and will throw an exception on attempts to add elements to it.
```fs
[(1,2);(3,4)] |> dict
```

## Choose a function
One important distinction between F# and C# when it comes to working with Collections is that in C# you tend to operate *on* an instance of a collection - by dotting into methods on that type; while F# prefers to offer families of functions in modules that take instances as an argument.  So C#'s `myDictionary.Add(someKey,someValue)` in F# would be `Map.add someKey someValue myMap`. 

### I just want my LINQ
F# offers functions that are analogous to those that C# programmers will be familiar with from LINQ, but the names are often different, as F# uses nomenclature that is more in alignment with the terminology used in the rest of the functional programming world.  Rest assured, they mostly behave as you would expect.  Rather than be exhaustive - LINQ is huge - I'll list what in my experience are the most common LINQ methods and their F# analogues:

* `.Aggregate()` is called `.fold` or `.reduce` depending on whether or not you're providing an initial state or just using the first element, respectively.
* `.Select()` is called `.map` 
* `.SelectMany()` is called `.collect` 
* `.Where()` is called `.where` or `.filter` (same thing, two names, long story)
* `.All()` is called `.forall`
* `.Any()` is called `.exists` if we are supplying a predicate, or `.isEmpty` if we just want to know if the collection has any elements
* `.Distinct()` is still `.distinct` - or `.distinctBy` if we are supplying a projection function.
* `.GroupBy()` is still `.groupBy`
* `.Min()` and `.Max()` are still `.min` and `.max` - with `.minBy` and `.maxBy` alternatives for using a projection.
* `.OrderBy()` is called `.sortBy` - and similarly, `.OrderByDescending()` is `.sortbyDescending`
* `.Reverse()` is called `.rev`
* `.First()` is called `.head` if we want the first element - or `.find` if we want the first element that matches a predicate.  Similarly, instead of `.FirstOrDefault()` we use `.tryHead` and `.tryFind` - which will return an Option of either `Some matchingValue` or `None` if not found, instead of throwing an exception.
* `.Single()` is called `.exactlyOne` - and similarly, `.SingleOrDefault()` is `.tryExactlyOne`

### I'm not sure which function I need. I have

* #### A collection, and want:
  * ##### A single value or element:
    * `.min`, `.minBy`, `.max`, and `.maxBy` will get an element of your collection relative to the others
    * `.sum`, `.sumBy`, `.average`, `.averageBy`, 
    * `.find`, `.tryFind`, `.pick`, and `.tryPick` will allow you to get a single specific element of your collection
    * `.head`, `.tryHead`, `.last`, and `.tryLast` will get you items from the front or back of your collection
    * `.fold` and `.reduce` will allow you to apply logic and use every element of your collection to create a single value
    * `.foldBack` and `.reduceBack` do the same, but from the end of the collection
  * ##### An equal number of elements:
    * `.map` will allow you to transform each element of your collection.
    * `.indexed` will turn each element of your collection into a tuple, whose first item is its index: `[1]` would become `[(0,1)]`, for example.
    * `.mapi` does this implicitly, by providing the index as an additional first argument to the mapping function.
    * `.sort`, `.sortDescending`, `sortBy`, and `.sortByDescending` allow you to change the order of your collection.
  * ##### A possibly smaller number of elements:
    * `.filter` will give you back a collection only containing elements that match the predicate provided.
    * `.choose` is like `.filter` - but allows you to map the elements at the same time.
    * `.skip` will return the remaining elements after ignoring the first `n`
    * `.take` and `.truncate` will return up to the first `n` items and either throw or not, respectively.
    * `.distinct` and `distinctBy` will allow you to remove duplicates from the collection
  * ##### A possibly greater number of elements:
    * `.collect` will apply a collection-generating function to each element of your collection, and concatenate all the results together.
  * ##### To change the shape of the collection:
    * `.windowed` will return a new collection of all `n` sized groups from the original collection: `[1;2,3]` would become `[[1;2];[2;3]]` when `n = 2`, for example.
    * `.groupBy` will return a new collection of tuples, where the first item is the projection key, and the second is a collection of starting elements that matched the projection: `[1;2;3]` projected by `(fun i -> i % 2)` would result in `[(0, [2]); (1, [1; 3])]`, for example.
    * `.chunkBySize` will return a new collection of up to `n` sized collections of your original.  `[1;2;3]` would become `[[1;2];[3]]` when `n = 2`, for example.
    * `.splitInto` will return a new collection containing `n` equally sized collections from your original.  `[1;2;3]` would become `[[1];[2];[3]]` when `n = 3`, for example.
  * ##### To iterate it without changing it:
    * `.iter` and `.iteri` take a function and apply each element of your collection to it, but not return any value.

* #### A single value, and want:
  * ##### It to be part of a collection:
    * `.singleton` can be used to create a one-item collection from the value
    * `.init` will take a size and an initializer function and create a new collection of that size.

* #### Multiple collections, and want:
  * ##### To combine them:
    * `.append` takes two collections and creates a new single collection containing all the elements of both.
    * `.concat` does the same but for a collection of collections.
    * `.map2` and `.fold2` act like `map` and `fold` from above, but will provide items from the same index in two source collections to the mapping/folding function.
    * `.allPairs` takes two collections and provides all 2-item permutations between both.
    * `.zip` and `.zip3` take 2(or 3) collections and produce a single collection consisting of tuples of items from the same index in the sources.

## Work Asynchronously
F#'s asynchronicity model resembles C#'s but has a few important differences that will occasionally catch out C# developers:  
1. F# has a separate `Async<'t>` type that is similar to C#'s `Task<T>`
2. Due to F#'s type system requiring returns, it uses `Async<unit>` instead of `Task` for cases where we don't return an actual value
3. F# can generate and consume `Task<T>` with the `Async.StartAsTask` and `Async.AwaitTask` functions from the core library.

F# has one other very notable difference from C# with regards to asynchronous code:  C# 'enables' the `await` keyword inside a method by applying the `async` keyword to that method's signature;  F# uses a language feature called a `computation expression` - which results in the `async` being part of the function body instead.  This also comes with some implications for how you write the code inside that function body:
```fs
let timesTwo i = i * 2 // We have our basic function definition

//And now we can make it async

let timesTwoAsync i = async { //Note that when working with computation expressions, we start with our keyword, and then the function itself inside curly braces
    return i * 2 //We also use the `return` keyword to end the expression
}

let timesFour i = async {
    let! doubleOnce = timesTwoAsync i //Note the ! in our let! - this is like `await` in C# - the function we call on the right side has to be something that returns an Async<'a>
    //After we have bound the result of an Async function with let! - we can use it afterwards just like normal
    let doubleTwice = timesTwo doubleOnce //In the case of non-Async functions, we can write our code like usual

    return doubleTwice
}
```

4. Keep in mind that `let!` in Async blocks only work when calling Async-producing functions - similar to how C#'s `await` can only be used on `Task` returning methods.
5. Differently, however, is that since F# handles async purely in the body of functions - there's no requirement about which functions you can `let!` bind - anything returning `Async<'a>` is acceptable.  This is in contrast to C#'s requirement that you can only `await` methods flagged as `async`

## Signal an error or control the program flow
First, a definition:  When we talk about error signalling and program flow, I don't mean exceptions - F# has those and they work very similarly to C#.   What I mean is predictable and potentially recoverable errors;  because this is an area where F# can seem like C# at a glance, but very quickly it becomes apparent how different it is.  Specifically, this turns up in the use of `null` as a common error signal in C#.  It isn't an uncommon pattern in C# that looks something like this:
```cs
public Foo DoSomething(Bar bar)
{
    if(bar.IsInvalid)
    {
        return null;
    }

    return new Foo(bar.Value);
}
```
And then, the caller of `DoSomething` can check the return for `null` and if so, does something similar to either handle it or pass it on.  One area where this pops up often, in my experience, is around LINQ's `FirstOrDefault()` - which gets used to avoid the exception in the case of an empty `IEnumerable<T>` - but often ends up just propagating the `null`.  
  
F# initially appears to offer a translation of this with its `Option<'a>` type - and the question tends to arise: isn't `None` just a shortcut for `null` except now it's more difficult to get at the value, now wrapped in `Some`?  Because that's going to require pattern matching or checking `.HasValue` on the option - and is that really better?  It isn't, and that's why F# by way of functional programming offers a cleaner solution:  writing the majority of your code without worrying about checking for existing errors, and instead only worrying about signalling potential new ones specific to a given function.  We can do this by writing most of our functions as though the inputs have already been validated for us, and then by using the `map` or `bind` functions to chain our happy-path functions together.  Let's look at these in the context of `Option`:  

`map` wants two arguments:  a `'a -> 'b` function, and an `Option<'a>`, from which it will produce an `Option<'b>`  
`bind` also wants two arguments:  a `'a -> Option<'b>` function, and an `Option<'a>`, from which it will produce an `Option<'b>`  

Let's consider what these can do for us:  
```fs
// string -> Option<string>
let getConfigVariable varName =
    Private.configFile
    |> Map.tryFind varName

// string -> Option<string[]>
let readFile filename = 
    if File.Exists(filename) 
        then Some File.ReadLines(filename)
        else None

// string[] -> int
let countLines textRows = Seq.length file

getConfigVariable "storageFile"                 // 1
|> Option.bind readFile                         // 2
|> Option.map countLines                        // 3
```

So what's going on there? 
1. We try to grab a variable from our configuration.  Maybe it exists, maybe it doesn't, but it only matters to that single function.
2. Then we pipe into `Option.bind` - which implicitly handles the safety logic for us:  if the previous step has `Some` value - use it as an argument to this function - otherwise keep it as `None` and move on
3. `Option.map` does the same - if there is `Some` value, use it with this function, otherwise just move on.
   
The astute observer here will notice that there doesn't appear to be an immediate difference between `bind` and `map` at step 3 - they're both just automatically handling the same thing, right?  But note the different signatures between `readFile` and `countLines` - `bind` has an additional step that `flatten`s the `Option` that its function outputs.  Consider the alternative:  If we had used `map` then at the end of line 2 we would have an `Option<Option<string[]>>` - and so on line 3 we would need to `Option.map (Option.map countLines)`!  

But, the question stands, how do I actually get the value, if there is one, *out* of that `Option`?  And it's a fair question.  And the answer is to avoid doing so as long as possible.  Because the later you wait to try to unwrap an Option, the less code you have to write that has any idea an error is even possible.  And at a point when you finally, absolutely, need to get a value out - you have two options:  
`Option.defaultValue` takes an `'a` and an `Option<'a>` - if the `Option` has a value, it returns that, otherwise it returns the `'a` you've given it.  
`Option.defaultWith` is the same, but instead of a value, it takes a `unit -> 'a` function to generate a value.  

Coincidentally, this same logic applies with F#'s built-in `Result<'a,'b>` type, which also offers `bind` and `map` (and `mapError` if you need it) - but instead of `None` you have the `Error` case, which you can use to store information about what went wrong - be it a `string` or a custom error type of your choosing.
  
    

## Use a C# library in F#  
One of the great benefits to F# - and probably why a C# developer looks at it first rather than something like Haskell - is that it is part of the greater .NET ecosystem and supports interop with all of the C# libraries that a developer is already familiar with.  C# code can (mostly) be consumed by F# - but some rough edges tend to crop up, but generally with easy workarounds:

* When calling C# methods, the F# compiler treats the method as a single-argument tuple.  Because of this, partial application is strictly not available and piping can be difficult due to overload resolution:
```fs
"1" |> Int32.Parse                          //Works like Int32.Parse("1")
("1", NumberStyles.Integer) |> Int32.Parse  //Works like Int32.Parse("1", NumberStyles.Integer)
NumberStyles.Integer |> Int32.Parse "1"     //Won't compile, because it's expecting a tupled argument, not two separate args.
```

* C# libraries - specifically those that involve serialization or reflection - are often not equipped for understanding native F# types.  The most common case here is JSON libraries - who can struggle with serialization and/or deserialization of Unions and Records - it's strongly advisable in cases such as this to check for an extension library that provides F# specific functionality.  `Newtonsoft.Json` has the `Newtonsoft.Json.FSharp` package, for example - `System.Text.Json` has `FSharp.SystemTextJson` - alternately these cases may also make for a good time to check out the native F# libraries for the same work, like `Thoth` or `Chiron`.  
  
* Owing to C#'s ability to produce `null`s for any reference type - and no (at time of writing) native interop for C#'s nullable `?` type notation for reference types - it's helpful to try to isolate C# code on the outside edge of your logic, and use helpers such as `Option.ofNullable` (for Nullable<T>) or `Option.ofObj` (for reference types) to quickly provide type safety for your own code.

* C# methods that expect delegate types such as `Action<T>` or `Func<T>` can be given an F# lambda of the appropriate signature, and the compiler will handle the conversion.  Remember:  `unit` fills in for `void` in F# - and its value is `()` - so an `Action<T>` would expect a `'T -> unit`, such as `(fun _ -> printfn "I'm a lambda!")`; and likewise, `Func<T>` would expect a `unit -> 'T`, such as `(fun () -> 123)`.
  
* In cases where a C# library expects things to be decorated with Attributes, they can be used almost identically with the tricky catch that F# uses `<>` inside the square brackets - so `[Serializable]` in C# would become `[<Serializable>]` in F#.  Arguments work the same:  `[<DllImport("user32.dll", CharSet = CharSet.Auto)>]`.  And, just like with collections above, multiple attributes are separated with a semicolon, not a comma:  `[<AttributeOne; AttributeTwo>]`, for example.