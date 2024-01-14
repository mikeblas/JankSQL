// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

// see https://github.com/dotnet/roslyn/issues/71644
#pragma warning disable IDE0077 // Avoid legacy format target in 'SuppressMessageAttribute'

// see https://josefpihrt.github.io/docs/roslynator/how-to-suppress-diagnostic
[assembly: SuppressMessage("Roslynator",
    "RCS1036:Remove unnecessary blank line",
    Justification = "Nobody tells me where to put whitespace",
    Scope = "NamespaceAndDescendants",
    Target = "Tests")]

[assembly: SuppressMessage("Roslynator",
    "RCS1003:Add braces to if-else (when expression spans over multiple lines)",
    Justification = "Mixing brackets is preferred",
    Scope = "NamespaceAndDescendants",
    Target = "Tests")]

[assembly: SuppressMessage("Roslynator",
    "RCS1036:Remove unnecessary blank line",
    Justification = "Nobody tells me where to put whitespace",
    Scope = "namespaceanddescendants",
    Target = "Tests")]

[assembly: SuppressMessage("Roslynator",
    "RCS1089:Use --/++ operator instead of assignment",
    Justification = "This is my style",
    Scope = "namespaceanddescendants",
    Target = "Tests")]

#pragma warning restore IDE0077 // Avoid legacy format target in 'SuppressMessageAttribute'
