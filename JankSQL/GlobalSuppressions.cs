// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

#pragma warning disable IDE0077 // Avoid legacy format target in 'SuppressMessageAttribute'

// see https://josefpihrt.github.io/docs/roslynator/how-to-suppress-diagnostic

// bickering about whitespace is too much
[assembly: SuppressMessage("Roslynator",
    "RCS1036:Remove unnecessary blank line",
    Justification = "Nobody tells me where to put whitespace",
    Scope = "namespaceanddescendants",
    Target = "JankSQL")]
[assembly: SuppressMessage("Roslynator",
    "RCS1036:Remove unnecessary blank line",
    Justification = "Nobody tells me where to put whitespace",
    Scope = "namespaceanddescendants",
    Target = "JankSQL.Expressions")]
[assembly: SuppressMessage("Roslynator",
    "RCS1036:Remove unnecessary blank line",
    Justification = "Nobody tells me where to put whitespace",
    Scope = "namespaceanddescendants",
    Target = "JankSQL.Engines")]
[assembly: SuppressMessage("Roslynator",
    "RCS1036:Remove unnecessary blank line",
    Justification = "Nobody tells me where to put whitespace",
    Scope = "namespaceanddescendants",
    Target = "JankSQL.Contexts")]
[assembly: SuppressMessage("Roslynator",
    "RCS1036:Remove unnecessary blank line",
    Justification = "Nobody tells me where to put whitespace",
    Scope = "namespaceanddescendants",
    Target = "JankSQL.Operators.Aggregations")]

// I like to use += 1 (for example)
[assembly: SuppressMessage("Roslynator",
    "RCS1089:Use --/++ operator instead of assignment",
    Justification = "Nobody tells me where to put whitespace",
    Scope = "namespaceanddescendants",
    Target = "JankSQL.Expressions")]
[assembly: SuppressMessage("Roslynator",
    "RCS1089:Use --/++ operator instead of assignment",
    Justification = "This is my style",
    Scope = "namespaceanddescendants",
    Target = "JankSQL.Expressions")]
[assembly: SuppressMessage("Roslynator",
    "RCS1089:Use --/++ operator instead of assignment",
    Justification = "This is my style",
    Scope = "namespaceanddescendants",
    Target = "JankSQL.Operators.Aggregations")]

// one-line controlled statements don't need brackets
[assembly: SuppressMessage("Roslynator",
    "RCS1003:Add braces to if-else (when expression spans over multiple lines)",
    Justification = "Mixing brackets is preferred",
    Scope = "namespaceanddescendants",
    Target = "JankSQL")]

#pragma warning restore IDE0077 // Avoid legacy format target in 'SuppressMessageAttribute'
