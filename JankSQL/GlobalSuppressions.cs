// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

// I'd rather not
[assembly: SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1101:Prefix local calls with this", Justification = "Not my style")]

// StyleCop forces bracing that I don't like, and can't be configured
[assembly: SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1503:Braces should not be omitted", Justification = "Cant be controlled to represent my style")]
[assembly: SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1520:Use braces consistently", Justification = "can't be controlled to represent my style")]

// multiple newlines at the end of a file are inconsequential
// the configuration only "allows" a single newline
[assembly: SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1518:Use line endings correctly at end of file", Justification = "silly")]

// no need for line nannying
[assembly: SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1508:Closing braces should not be preceded by blank line", Justification = "silly")]
[assembly: SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1507:Code should not contain multiple blank lines in a row", Justification = "silly")]
