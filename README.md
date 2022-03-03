<img src="docs/pics/logo.png" alt="Project Logo" width=200>

# JankSQL

This is a simple implementation of a SQL database server written in C#.

It's just me working on it, so it'll take forever to implement. As I go, I'll pick interesting tasks to work on and try to get them done, but at any moment the project is probably in some disarray -- but it *ought* to work for as much as is really done, despite any loose ends.

This project is completely silly: I have no goal aside from having something to do, and learning a bit as I go. JankSQL is *totally* [jank](https://www.urbandictionary.com/define.php?term=jank): redundant, un-finished, crooked, and a replacement of dubious quality.

Let's have even more fun: "JankSQL" is pronounced "janks queue ell".

## Motivation

I'm retired, and a bit bored. I've always wanted to try to glue together everything I know about software development, SQL, and databases into an actual database implementation -- but I've always been distracted by paying work. Now, I'm taking a run at it since I have some spare time. I don't have any particularly jarring insights into how anything might be faster or more efficient just yet. One idea I *do* have is eventually translating SQL into executable code rather than interpreted code. That's why I've chosen C# -- since I think I can emit IL, compile it, and host it in an environment that implements accessors for the rest of the structure.  Maybe I'll eventually get to that, but my approach is (as above) just to paw through the things that I think are personally interesting in the order I feel like attacking them.

And, really, doing work without a deadline and in the order that pleases me (instead of some PM or manager or external team or ...) is very refreshing and liberating.

Also, my wife said I'm not allowed to build a robot, so this project will have to suffice.

## Approach

### SQL Grammar

I've chosen to re-use an existing SQL grammar for parsing; even at my most ambitious, I can't hope to implement all the features of the grammar, but it seems better than struggling along with subsets of the grammar itself while I try to implement features.

Remarkably, I'll want to extend the grammar to support features unique to JankSQL.

### Storage

I'll expect to implement my own binary file format, but that will take time. For now, there's a simpler mechanism for storage which just uses CSV files. This means I'll eventually have pluggable engines; maybe something works against CSV files, maybe I'll have different binary implementations and formats.

### Tests

I'd like to be test-driven in development of the server, so I'll have a variety of unit tests. Strictly, the "unit" tests are a bit stretched -- they're probably a bit more integration tests, since they'll exercise multiple components in concert. 

# Setup 

This project uses .Net 6.0 and therefore requires Visual Studio 2022.

After that, the project has just two prerequisites: the Antlr tool and an external T-SQL grammar for Antlr.

## Installing Antlr
The first requirement is Antlr -- which, in turn, requires the Java run-time. The [Antlr installation instructions]( https://github.com/antlr/antlr4/blob/master/doc/getting-started.md) explain how to get Antlr going, and I just added the Antlr JAR file to my `\bin` directory, which is already on my path.

## The T-SQL Antlr Grammar
Rather than write my own grammar as I go, I started with [an available Antlr grammar for T-SQL](https://github.com/antlr/grammars-v4/tree/master/sql/tsql). 

The `grammars-v4` project on GitHub contains many grammars, including the [sql/tsql](https://github.com/antlr/grammars-v4/tree/master/sql/tsql) grammar I chose. It's more than adequate, though it's a annoyingly case-sensitive: `SELECT` is a token, but `select` is not.

The grammar's `*.g4` files can be copied from that project to the `$/grammar` directory.  Once landed there, build the grammar with Antlr while targeting C#:

```
    antlr4 -Dlanguage=CSharp TSqlLexer.g4 TSqlParser.g4 -o ..\Parsing -visitor
```

The resulting C# files end up in the `$\Parsing` directory of the project.

At this point, the project is buildable. Note that the `$\Parsing` directory is checked in with existing `*.cs` files, so maybe it's not necessary to build the grammar directly. Once Antlr add-ins are available for Visual Studio 2022, I can automate the whole thing.


# Licensing

This project is source-visible, but I retain full rights to its use and distribution. No warranty of any kind is given for someone who wants to reuse this code.

