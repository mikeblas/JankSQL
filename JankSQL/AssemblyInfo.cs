using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

// In SDK-style projects such as this one, several assembly attributes that were historically
// defined in this file are now automatically added during build and populated with
// values defined in project properties. For details of which attributes are included
// and how to customise this process see: https://aka.ms/assembly-info-properties

// Setting ComVisible to false makes the types in this assembly not visible to COM
// components.  If you need to access a type in this assembly from COM, set the ComVisible
// attribute to true on that type.

[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM.

// [assembly: Guid("a68a6abe-295b-4aaa-a7f1-5ede4dbad7a6")]

[assembly: CLSCompliant(false)]



// make internal things in JankSQL visible to the "Tests" assembly
// see here:
//  https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.internalsvisibletoattribute?view=net-6.0
//  https://softwareengineering.stackexchange.com/questions/126545/would-you-rather-make-private-stuff-internal-public-for-tests-or-use-some-kind

[assembly: InternalsVisibleToAttribute("Tests")] 
