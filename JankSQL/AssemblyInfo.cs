using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// In SDK-style projects such as this one, several assembly attributes that were historically
// defined in this file are now automatically added during build and populated with
// values defined in project properties. For details of which attributes are included
// and how to customize this process see: https://aka.ms/assembly-info-properties

// Setting ComVisible to false makes the types in this assembly not visible to COM
// components.  If you need to access a type in this assembly from COM, set the ComVisible
// attribute to true on that type.
[assembly: ComVisible(false)]

// make internal things in JankSQL visible to the "Tests" assembly
// see here:
//  https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.internalsvisibletoattribute?view=net-6.0
//  https://softwareengineering.stackexchange.com/questions/126545/would-you-rather-make-private-stuff-internal-public-for-tests-or-use-some-kind
[assembly: InternalsVisibleToAttribute("Tests")]
[assembly: InternalsVisibleToAttribute("ScratchWork")]
