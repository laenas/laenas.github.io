---
layout: post
title: Reasoning about Signatures
author: @laenas
published: 2021-03-07
---

## I promise not to talk about LEGO

We've all heard that one before, and I think people might be getting a little tired of it.  (Even if it's a good metaphor)
But it's undeniable that, in the world of F# (and from my perusing, statically-typed functional programming as a whole), the signatures of things are given an markedly more central role.  Immensely moreso than in our sibling language C#, to use the reliable old point of comparison.  I've reflected upon this a fair bit, recently, and come to the conclusion that it boils down to two primary differences:  
First, foremost, we can actually express them without the patience of a monk and the memory of a pub quiz champion.  Consider the differences between these two equivalent things (and don't worry too much about making sense of either, we'll get there later, this is just a run-on sentence in a run-on introduction):
```fs
('a -> 'b -> 'a) -> 'a -> seq<'b> -> 'a
```
```cs
TAccumulate Aggregate<TSource,TAccumulate> (this System.Collections.Generic.IEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate,TSource,TAccumulate> func)
```

Second, there's a much stronger compositional inertia in a language like F#; and part of that composition is being able to quickly and easily understand how pieces fit together.  There's a reason why tooling like [Ionide](https://ionide.io/) adds signature hints liberally, and FSI prints them for all bindings.

So let's walk through this together, starting simple, and working upwards.  And each step of the way, let's consider how to reason about what we're doing in a way that will develop our intuition about how to work with unfamiliar code and libraries.

## No, no, not the T word, I thought we weren't going to talk about the T word

***Types.***  Whoo, been holding that in for the entire introduction.  
When we talk about *signatures*  what we're talking about is *types* - or, more clearly, a specific *subset* of types.  The types of functions.  But before I get there, I want to start from the ground up, to make sure we're really clear about the distinction between a *Type*  and a *Value* - because that's going to be important to understand before moving on.
```fs
let ten = 10
```
We have three elements in this snippet:
* A binding name: `ten`
* A value: `10`
* The type of that value, and thereby, of `ten`:  `int`
  
We can phrase this more conversationally as:  `ten` is an `int` with a value of `10`.
`"hello"` is a `string` - `'x'` is a `char` - `(1,2)` is a `int * int` and `()` is `unit` 

That last point is one to linger on for just a moment:
`()` is a *value* of the type `unit` - a type that has only that one value.  There are no magic `void`s in the F# type system, everything has a clearly defined type and value.

This rule has two implications, beyond the simple values above, which I suspect everyone can reason easily about.

The first is with regards to generics:
```fs
let abc = ['a';'b';'c']
```
`abc` is a `char list` - or put another way - a `List<char>` and the type parameter there is important.  Because you can also have `List<int>` or `List<string>` - but they all behave the same, they are all, fundamentally, a list.  Or, to wit, `'a list`.  Because in F# we denote type parameters for our generic types as `'a` and `'b` and `'anyNameReally` and so forth.  It just needs to start with that tick.  

The second is with regards to functions:
```fs
let addOne i = i + 1
```
`addOne` is a `int -> int`.  The `->` notation in F# indicates a function.  The final type in the signature is the output type of the function, the rest are the parameters that are sent as input to the function.  So in this case, exactly as we see in the code - we want one input (an `int`) and from that the function will produce an output (an `int`).  

## Thinking our way through the basics

So we have looked at simple values.  They have types.  Probably not much confusion there.
And when dealing with generic types, like `list`, we can have types that generically operate on other types, their values dependent on that other type.
And with functions, they have types - signatures - also.

But stop and read that through again.  There's a logical association at play.  
Values have types.  `2` is an `int`
Generic values have types.  `[|1;2;3|]` is an `int array`
And functions have types.  But where did the *value*  in that statement go?  What is the *value* of a function?

It's not its output.  Its not its binding name.  It is ***the function itself***

Consider for a moment how we think about values and types otherwise:  Consider `3`.  What can we say about it?  It's an `int`, yes.  It's value is *three* - as in the natural number.  But we can't necessarily *do* much with just `3`.  It's a fine value, but it's just a single value.  Consider `"shanty town faded sub-orbital city range-rover rain singularity artisanal modem"` - it's a [fine string](http://loremgibson.com/), but do we *really* want to pass that around everywhere we might need it?  Nope, so we *bind* it to a name that's easier to work with:  
```fs 
let lorem = "shanty town faded sub-orbital city range-rover rain singularity artisanal modem"
```

And we can now use these interchangeably, because for all intents and purposes, they are the same thing.  We put the name beside `let` and then a value after the `=` and now we have associative equivalence.

So when we look at a function like this:
```fs
let addOne i = i + 1
```

We can say that `addOne` has the signature `int -> int`.  But the ***value*** of it is the entire function itself - more easily seen (but please don't write it this way in day-to-day code) by simply our writing our function more like every other value we work with:
```fs
let addOne = (fun i -> i + 1)
```

Pause there.  Both of those `addOne` declarations are functionally equivalent.  Both can be called by simply doing `addOne 1` (producing `2`).  And this is because - and this is critical for folks not used to functional programming:  ***Functions Are Values***

Ok one more time, because this is going to get very important very soon.

Functions are values.  They can be bound to a name (often are!) but they can also be used in their 'value' form - generally just called a lambda(as in other languages as well).  

In exactly the same way that we can assign the value `3` to a name, and then use either interchangeably, we can do the same with functions.

## Higher order readership

So to briefly recap:
All values have a type, and can be bound to a name.
Functions are values, and by previous definition, have a type signature.
That signature is a definition that describes the input(s) and output by type.
We can call that function, to produce a new value, by sending values as input(s).

There's a logical loop in our definitions there.  If you aren't already familiar with it, I strongly encourage thinking about it.  Functions are values and functions take values to produce new values.  This means, very simply, that functions can accept *other functions* as arguments.  When you see the term 'higher order functions' this is (part) of what that means.

Let's build one for ourselves!
```fs
let transformInt i (f: int -> int) = f i
```

Surprisingly simple, right?  The type of `transformInt` is `int -> (int -> int) -> int` - note the parenthesis there that distinguish the second parameter as a function, as compared to an `int -> int -> int -> int` which is a function taking 3 `int` arguments.  Depending on your experience, this can be a *significant* insight, and may not settle immediately.  Make a cup of coffee, think about it, consider any problems you've worked on that perhaps may have benefitted from being able to change the behavior of a function by using a function parameter.

The subtlety here is that *any* function of the form `int -> int` can be used here.  And we can use their values directly, as well!
```fs
let transformInt i (f: int -> int) = f i
let addOne i = i + 1
transformInt 2 addOne //produces '3'

transformInt 3 (fun i -> i - 2) //produces '1'
```

## Reasoning about Signatures

Hey, that's the title of the article!

So we know now what signatures are, and how to read them.  But that's trivia, right?  I mean, we still need to stare at a mountain of documentation to understand what they do, don't we?  While partially true, and while samples are always helpful, understanding how to reduce unfamiliar code to raw signatures - combined with helpful names on the functions themselves - is a powerful tool to help every developer.  Especially in a non-top-5 language like F#, not every library is documented to the gills and with rich samples for every use case.  Indeed, several times a week there are people in the various F# communities asking How-To questions about libraries that have reasonable documentation, but simply lack fully-featured sample code.  Understanding how to sift through the signatures in a library - either in API documentation or via Intellisense - leads to rapid productivity gains.

But how do we do it?  Well, the primary trouble seems to be around generics.  When we talk about an `int -> int` there's something easy to grasp - give an `int`, get an `int`.  Even when we have different inputs and outputs: `int -> char -> string` gives us something I think we can reason about easily.  But I suspect something starts to short circuit for a lot of people when we move to a signature that looks like `('a -> 'b) -> ('b -> 'c) -> 'a -> 'c`

That looks *indecipherable*  at first.  And oftentimes our first instinct internally when faced with something like that is just to throw up our hands, panic, shut down, and just decide it's too much.  But hold yourself steady for a minute, because we're going to apply our thinking powers and we are going to conquer this!

As with any problem, let's start small.

`'a`

We know this is a generic, but unlike the ones we've seen previously it's not attached to anything.  There's no `list` or `option`.  It's entirely alone.  So in that sense, it can be...anything.  Any type whatsoever.  Let's just scribble in something simple, to help ourselves think.  And remember, we're using *types*  and not *values* right now.  A forewarning:  this is about helping that panic to subside, and introduces a fatal logical flaw to our reasoning, that we'll need to remove later by going back to the generic signature, but for now, let's get our mind right.

`(int -> 'b) -> ('b -> 'c) -> int -> 'c`

Ok, one thing down.  Now it's the same problem again.  Let's choose another type for `'b`, just to mix things up.

`(int -> string) -> (string -> 'c) -> int -> 'c`

And, once more, for completeness:

`(int -> string) -> (string -> char array) -> int -> char array`
`('a -> 'b) -> ('b -> 'c) -> 'a -> 'c`

There they are, side-by-side.  And while the top one feels a little more approachable, filling in the types like that is just a means to reason a little bit more about what a function like this is doing.  Don't forget that those types are chosen arbitrarily and could be anything!

Now, with things simplified for the sake of understanding, we can continue methodically and logically thinking about this function.

We know that it has three parameters:
`(int -> string)` and `(string -> char array)` and `int`
And that it produces a `char array`

We can tell that the first two parameters are functions.  This can lead us, through logic, to a conclusion that this function on the whole must be reasonably simple - because if it's taking functions as parameters, then it is altering its behavior based on those functions, rather than holding the logic itself.  There's a hypothetically large number of ways that a function like `int -> string` can be implemented, and so by taking that signature as a parameter, we can't know which one is going to be used.  Or, put another way, we've declared with this overall signature that *any* `int -> string` function will do.  That's a wide net to cast, and we can work backwards from it to our conclusion that there must not be 'very much' concrete logic happening in this function.

The other logical conclusion we can draw is that because of that abstracted behavior, there must be some relationship between the parameters.  Since we're taking in a function, we're probably intending to call it somehow, and in the case of `int -> string` that means we are going to need an `int`.  We can find it as the third parameter - which logic demands must be used as an input to the first parameter.

Wait.  That's wrong.  There could be a hardcoded `int` inside the function that is used.  And here's the fatal flaw in thinking about generic signatures from the standpoint of concrete types:  

`(int -> string) -> (string -> char array) -> int -> char array` can have unknown, hard-coded behavior that we can't make strong logical assumptions about.
`('a -> 'b) -> ('b -> 'c) -> 'a -> 'c` cannot.  You cannot write a function that holds this signature and hides away a (meaningful) hardcoded `'a` to be used with the function parameters because you can't know what the type of `'a` might be.  It's this very logic that lets us presume that the `'a` used as a parameter is used as input to the `'a -> 'b`

So we can see that there's a strong association between the first and third parameters, and as we scan over the rest of the signature, we can draw some other logical conclusions:  Our second parameter is a `'b -> 'c` which means we need to get a `'b` from somewhere.  Unlike our `'a` - there isn't one sent into our function alone.  But from the signature itself we see one is the output of the first parameter.  So it's reasonable to conclude that the output of the `'a -> 'b` is used as input to `'b -> 'c`.

And then our final output, a `'c` is produced by our function, and that's also the output of the second parameter.  So that makes sense.

So what can we reason out about this function's behavior solely from its signature?
`'a` is passed into the `('a -> 'b)` to give us a `'b`.  That `'b` is passed into the `('b -> 'c)` to give us a `'c`.  And that `'c` is the output of this very function.

`('a -> 'b) -> ('b -> 'c) -> 'a -> 'c` is the signature of function composition - present in F# via the `>>` operator.

Stop and take a moment to reflect on what we've just done.  Imagine looking at `>>` in your IDE and seeing that signature and just...gasping.  But we've walked through it, step by step, and made sense of exactly what it's doing.  That's huge!  We've done it without needing to see examples of it being used.  We've done it without needing to reach for reliable old metaphors about Danish plastic.  Well done!

## Once more, with less flourish.

There's an awkward flaw in written tuition like this - I'm victim to it as often as you.  It's so easy to *read* something, especially when it is in a teaching pace and tone, and feel very good about understanding it, but then walk away not really able to apply that knowledge.  Part of that, I think, is that so many of these sort of articles in development tend to make their point once and move on, understanding assured.  Instead, I want to walk through approaching signatures using reasoning with you here, with slightly less boilerplate, to show how this isn't about a single magical `>>` operator - but that it works universally and really can be applied.  

Up at the start, we glanced at this one:
`('a -> 'b -> 'a) -> 'a -> seq<'b> -> 'a`

Uff!  It's a weird one!  But is it really?  Our parameterized function uses an `'a` and a `'b` to produce an `'a`.  We once again see that we send in a `'a` as another parameter.  So let's use the same logic - that it's used as input to that function.  We also take in a `seq<'b>` and that `'b` matches our function...but not the `seq`.  It's notable that there's no other `seq` in the signature.  So we take in this collection of `'b` values but we don't use them with our function, nor do we output a `seq`.  Logic leads us to consider that we must be iterating over all the values in that `seq` - because if we were using only a single value, then this function would be better stated as `('a -> 'b -> 'a) -> 'a -> 'b -> 'a` (itself nonsensical, since we could just call the function parameter directly);  and if we wanted some subset of values from the `seq` - then we'd still be handling a `seq` and so the matter of selecting a subset would be better handled in its own function (or in yet another function-as-parameter that somehow indicated how many to use, like an `int` or a `'a -> bool`).  We can also explore the idea that we keep using our single `'a` with each `'b` in the function...but that would give us a `seq<'a>`, which doesn't match our output, either.  So we can break it down:  We call our function with our `'a` parameter and the first `'b` and that gives us...an `'a` - which we can use with the second `'b` to get an `'a`.  Once we're out of `'b` then we would be left with just the last `'a`. 

And that's **exactly** what this function does.  `Seq.fold` (which has siblings in `List` and `Array`, amongst others).  We've sussed out the behavior without seeing the code in action, or even the (very helpful) documentation about it.

Here's one that I have, prior to this, never actually used - just to level the playing field.

`Seq.scan` has a signature of `('a -> 'b -> 'a) -> 'a -> seq<'b> -> seq<'a>`

That panic I mentioned earlier?  Yeah, I just felt a rush of it.  But almost all our logic from up above still stands, up until the very end.  That's helpful.  Instead of a single `'a` we have a `seq<'a>`.  Wait, with `fold`, we had reasoned that the `'a` parameter must be used to bootstrap the chaining of our function, since we didn't get a `seq<'a>` on the output.  But now we do.  Does that mean it's what it does?  Well no, that's flawed.  If we had a `seq<'b>` that we wanted to transform into a `seq<'a>` then we would just use `Seq.map` (`(('a -> 'b) -> seq<'a> -> seq<'b>`) - so this must be doing something else.  The other significant logical thing it could be doing is collecting all the output `'a` from the function we pass in.  And sure enough, that's *precisely* what it does.

## More than just a neat trick

None of this is to say that there is *infallibility* or that a signature can tell you *everything*.  `Seq.foldBack` is `('a -> 'b -> 'b) -> seq<'a> -> 'b -> 'b` - and without the hint in the name - or in the documentation - the signature alone doesn't clue you into the fact that it's operating from the end of the collection instead of the front.  (Even though the raw behavior we reasoned out is correct).  
  
But when we look at the F# ecosystem, there's a strong sense in projects and libraries that this sort of thing is implicitly understood.  We talk a lot about the signatures of functions, because in so little text it can communicate so much.  A relatively comprehensive amount of API Reference documentation - with modules, function names, and their signatures - but much smaller amounts of sample code covering all the possible functions and how to call them and use them.  We, as developers, can bridge that divide by learning skills such as this in order to empower ourselves.  These sorts of things need not remain mysteries, or esoteric corners we avoid until illuminated by some other hand.

Let us use our own reasoning faculties to illuminate these dark corners.  Let us realize that we are already capable of understanding and working through the code before us.  We just need, sometimes, to sit down and consider it step by logical step.  But we'll come back to that in a later article.