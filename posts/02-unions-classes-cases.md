---
layout: post
title: Unions, Cases, and Types (oh my)
author: @laenas
published: 2021-02-03
---

## Type Safety is difficult (or at least irritating)

One of the challenges in moving to a language like F#, especially when your point of destination is a language like C#, is in coping with what seems to be a fixation on types.  Whereas in C# it is commonplace and idiomatic to cast types to and fro, up and down their inheritence trees, such practices are discouraged and made more difficult in F#.  While not uniquely the cause of confusion, being well-practiced in Object Oriented patterns appears to have a problematic effect when a developer approaches programming involving algebraic data types, especially sum types - such as Discriminated Unions in F#.  They provide an enormous boost to type safety - reducing our need to worry about the permutations on data state - but that does come at a cost of actually needing to ensure that safety exists.  It's a major stumbling block that I see again, and again, and again with F# novices, so let's take a slightly in-depth look at unions and how they can be used to solve familiar problems in different ways.

## Everything old is new again

F#'s discriminated unions are, despite their simple nature, an unbelievably powerful tool in modelling a domain, because it allows us to cleanly model the 'either-or' nature of much of our data.

```fs
type Hypothesis =
    | True
    | False
```

Here, we can see how we can immediately constrain a `Hypothesis` to being either true or false.  Immediately, we can see that this could also be modelled as a `bool` - which is true.  OO-savvy readers will additionally notice that this looks a lot like an `enum` - and that is also true.  But hang on, it gets better:

```fs
type Hypothesis =
    | True of Proof
    | False of CounterExample
```

We can define the cases with different data!  In our example, we can acknowledge that a `True` `Hypothesis` consists of a `Proof` and a `False` `Hypothesis` consists of a `CounterExample`.  

We should really stop to think for a moment about what this means, especially if we're used to Object Oriented type hierarchies.

## Unions as alternative inheritence

Spend almost any time modelling a domain in an object-oriented fashion and you'll end up with examples of that leaky abstraction, or that hanging method, due to inheritence.  At which point we go back to the drawing board and consider *long and hard* how to restructure things to ensure that we don't accidentally call the wrong method at the wrong time.  We end up thinking about everything, not necessarily in terms of their own types - but of their base types, as well as their child types.  What data properties should be `private` or `protected` or `public` - what is `virtual` or perhaps `abstract`?

Unions allow us to gracefully sidestep these issues of trying to keep track of what data bleeds to where in a class hierarchy.  Each case defines its own data.  Consider for a moment the parallels, in an ideal world:  In C#, we often use inheritence as a means to pass child instances with different types by handling them as a shared ancestor class.  `Apple` and `Orange` both inherit from `Fruit`.  But with unions, we say clearly:  A `Fruit` is either an `Apple` or an `Orange` - and there is nothing else.  

While this initially seems constraining, to understand how it actually simplifies and solidifies our domain requires us to look a little more at the usage of unions.

## Cases != Types

Consider the following definition:
```fs
type Fruit =
    | Apple
    | Orange
```

Here, we have defined **one** type.  It's important to understand that.  Neither `Apple` nor `Orange` are types.  An extremely common mistake made by novices is to attempt to use unions as a form of shorthand for describing inheritence trees:

```fs
let core (apple:Apple) = //"The type 'Apple' is not defined."
```

We can't write a function that only handles Apples because, as the compiler tells us, `Apple` is not a defined type.  This confusion is often magnified when using a union with cases named after other types (often records):
```fs
type Apple = {Type: string; Cored: bool}
type Orange = {Type: string}
type Fruit =
    | Apple of Apple
    | Orange of Orange

let core (apple:Apple) =
    {apple with Cored = true}

let myFruit = Apple {Type = "Ambrosia"; Cored = false}

core myFruit 
(* This expression was expected to have type
    'Apple'    
but here has type
    'Fruit' *)
```

In this instance, `core` takes an `Apple`(the type) but we give it a `Fruit` (of case `Apple`), and the compiler throws an error.  And without understanding the critical difference between the case and the type, even the function signature of `core` (`Apple -> Apple`) doesn't immediately help us.  We've given it an Apple!  What more does it want!?  

## Unions are not just glorified Enums

One common pattern seems to follow developers with experience with other languages:
```fs
type FruitType = 
    | Apple
    | Orange

type Fruit = {Type: FruitType}
``` 

Which seems logical, in isolation:  We *want* a Fruit as a record type, which looks and feels familiar like a class - and then we just use the union to indicate which type of Fruit any given instance is. 

The problem here is that in - I would estimate - the majority of cases we've sacrified everything to be in this state:

If we want to do anything meaningful with the distinction between types of Fruit, we now end up needing even more pattern matching code - either by dotting into `Fruit` or by writing helpers to identify types for us: `if IsApple someFruit then //..`.  Both being ultimately redundant, since they're just hiding the union matching.
We lose in the complex case as well:  Since we now have to be very careful about how we extend our `Fruit` type - all common traits must be shared across all `Fruit` types, and we introduce the need to do very careful error handling to track states.  Apples have Cores, Oranges have rinds, Cherries have pits (shared with Peaches, let us not forget), and so forth.  

## A remedy worse than the ailment?

It's at about this point the realization dawns:  In order to work with `Fruit` in the abstract - we have to pattern match it to do anything meaningful with it.

```fs
//For the sake of this example, we're not going to attach data
type Fruit = 
    | Apple
    | Orange

let prepare fruit =
    match fruit with
    | Apple -> //core it
    | Orange -> //peel it

let juice fruit =
    match fruit with
    | Apple  -> //This example just gave me a twenty minute long distraction
    | Orange -> //Into researching how to make apple juice at home
```

And so on.  It's understandable that after some amount of this repetition, something starts to feel like it needs be reduced.  But it's important to note that we're playing by slightly different rules here - because of the earlier point:  this is an alternative to type hierarchies.  So whereas in that traditional inherited model, we'd hide away our differing behavior inside the child classes themselves, with unions it stands at the forefront and we gain compiler support to ensure that we always handle all our cases.

## The Prestige

So with a lot of boilerplate out of the way, here's the real why and how of the power of unions - we just need to move our example to something a little more involved:
```fs
//Let's imagine we're writing a game and want to model equipment that benefits a character
type Equipment =
    | Base of name: string * wisdomStat: int
    | Prefixed of equipment: Equipment * wisdomBonus: int * prefix: string
```

Do you see it?  Yes?  No?  Let's pull that trick one more time.
```fs
type Equipment =
    | Base of name: string * wisdomStat: int
    | Prefixed of equipment: Equipment * wisdomBonus: int * prefix: string
    | Suffixed of equipment: Equipment * wisdomBonus: int * suffix: string
```

The power comes from that most terrifying *r*-word:  *recursion*.  Deep breath, it's alright, let's just reason about this for a moment!  
In this example we'll see that there is (and must be, as with all recursion) our base case:  `Base` - which specifically **does not** include other `Equipment`.  The other two cases do, however, and if we think about how we'd go about creating those cases, we can work from a position of pure logic:  In order to create a `Prefixed` we need to have some other `Equipment`...if we have some other `Prefixed` then...we're back where we started.  And if we have `Suffixed` then we end up with a similar problem.  But I can create `Prefixed` `Equipment` by passing some form of `Base` in, and that's that:
```fs
let heroicSocks = Prefixed(Base("Socks",0),3,"Heroic")
//Heroic Socks (+3 Wisdom)
```
And to step back to that logic, we can make `Suffixed` equipment in the same way.  Even better, we can do both, and in either order!
```fs
let heroicSocksOfBar = Prefixed(Suffixed(Base("Socks",0),2, "of Bar"),3,"Heroic")
let heroicSocksOfFoo = Suffixed(Prefixed(Base("Socks",0),3, "Heroic"),2,"of Foo")
```

And note that nothing here stops us from continuing onwards: `Fancy Oversized Blue Socks of Astonishment and Woe` is just a matter of stacking `Prefixed` and `Suffixed` cases on top of one another, using the same, simple, elegant pattern.  
It's not to say this is impossible to handle without unions - just more complex, or difficult to work with.  Consider how you'd model this behavior with a class hierarchy.  Trying to subclass `PrefixedEquipment : Equipment` makes standalone `SuffixedEquipment : Equipment` an exclusive choice.  Adding `IPrefixedEquipment` interfaces demands handling those interfaces and types separately from the `ISuffixedEquipment`.  Perhaps even an `IAugmentedEquipment` - and then writing complicated (and leaky) logic to indicate whether a given implementation is a suffix or a prefix.  

Even moreso, consider how you'd model this, even in F#, but without that recursive union.  It's not impossible, just not as clean and - importantly - typesafe through-and-through.  And not just now, but as easily as we can add more enhancements in the same way, beyond just the scope of a name and stat calculation:  `| Scripted of equipment: Equipment * behavior: (EquipmentEvent -> Equipment)` - by adding a case that contains a function that returns equipment given some known type - we can end up with ad-hoc behavior.  And even better, just as with the name, we can do this *repeatedly* so that individual snippets of behavior can be just that - small snippet functions - rather than a single enormous function with complicated logic to handle all possible cases.

## What's the cost?  Where's the struggle?  Just more types.

So we can have these nested recursive unions, but unwrapping them and processing all of those layers every time I just want to know 'what is the name of this item' is annoying - even if we have a helper function.  Entirely true!  But by simply creating more types to suit those needs, we can retain both the flexibility we have and also augment it with simpler handling:
```fs
type Equipment =
    | Base of name: string * wisdomStat: int
    | Prefixed of equipment: Equipment * wisdomBonus: int * prefix: string

type Practical = {Name: string; WisdomBonus: int; RawEquipment: Equipment}

let makePractical e =
    let rec build equip (pName,pWis)= 
        match equip with
        | Base (name,wis) -> 
            ($"%s{pName}%s{name}",wis + pWis)
        | Prefixed (eq,wis,prefix) -> 
            build eq ($"%s{prefix} %s{pName}", wis + pWis)

    let (name,wis) = build e ("",0)

    {Name = name; WisdomBonus = wis; RawEquipment = e}

let heroicSocks = Prefixed(Base("Socks",0),3,"Heroic")

let practicalSocks = makePractical heroicSocks
```

We hold that reference to our raw stats, both so that we can augment and modify and recalculate on the fly:
```fs
let cursedSocks = Prefixed(practicalSocks.RawEquipment, -5, "Cursed") |> makePractical
```

I leave, knowingly, [the sequencing of these augmentations](https://twitter.com/MattAndersonNYT/status/772002757222002688) as an exercise for the reader.

## In conclusion

What can in addition be said, relative to what stands above?  We've looked at how unions aren't just enums.  We've built up the need to do complicated pattern matching by using unions - but then also reduced them back to simply handled, but strictly typesafe, records again.  We've seen how they can take what would normally be a complicated, interwoven jumble of interfaces and inherited types, and distill them down to simple, high-level abstractions.  We've talked about fruit and socks.  

And, hopefully, we've learned something.