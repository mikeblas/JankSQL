// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

// see https://josefpihrt.github.io/docs/roslynator/how-to-suppress-diagnostic
[assembly: SuppressMessage("Roslynator",
    "RCS1036:Remove unnecessary blank line",
    Justification = "Nobody tells me where to put whitespace", Scope = "namespaceanddescendants", Target = "JankSQL")]
[assembly: SuppressMessage("Roslynator",
    "RCS1036:Remove unnecessary blank line",
    Justification = "Nobody tells me where to put whitespace", Scope = "namespaceanddescendants", Target = "JankSQL.Expressions")]
[assembly: SuppressMessage("Roslynator",
    "RCS1036:Remove unnecessary blank line",
    Justification = "Nobody tells me where to put whitespace", Scope = "namespaceanddescendants", Target = "JankSQL.Engines")]
[assembly: SuppressMessage("Roslynator",
    "RCS1036:Remove unnecessary blank line",
    Justification = "Nobody tells me where to put whitespace", Scope = "namespaceanddescendants", Target = "JankSQL.Contexts")]
[assembly: SuppressMessage("Roslynator",
    "RCS1036:Remove unnecessary blank line",
    Justification = "Nobody tells me where to put whitespace", Scope = "namespaceanddescendants", Target = "JankSQL.Listeners")]

[assembly: SuppressMessage("Roslynator",
    "RCS1003:Add braces to if-else (when expression spans over multiple lines)",
    Justification = "Mixing brackets is preferred", Scope = "namespaceanddescendants", Target = "JankSQL")]
