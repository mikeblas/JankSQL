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

// StyleCop fires this warning for []? nullable arrays
// see https://github.com/DotNetAnalyzers/StyleCopAnalyzers/issues/2927
[assembly: SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1011:Closing square bracket should be followed by a space", Justification = "StyleCop bug")]

// incompatible with TODO and REVIEW comments, which are useful and built-in to Visual Studio
[assembly: SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1005:Single line comment should begin with a space", Justification = "Not my style")]

// not using file headers
[assembly: SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1633:The file header XML is invalid", Justification = "not using file headers just yet")]

// I freely use single-line comments
[assembly: SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1512:Single-line comments should not be followed by blank line", Justification = "Not my style")]
[assembly: SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1515:Single-line comment should be preceded by blank line", Justification = "Not my style")]

// regions seem nice to me
[assembly: SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1124:Do not use regions", Justification = "You're not the boss of me")]
