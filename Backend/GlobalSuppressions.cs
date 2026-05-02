// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Usage", "CA2254:Template should be a static expression", Justification = "Potential performance hit is negligible for our purposes", Scope = "namespaceanddescendants", Target = "N:Backend")]
[assembly: SuppressMessage("Performance", "CA1873:Avoid potentially expensive logging", Justification = "Potential performance hit is negligible for our purposes", Scope = "namespaceanddescendants", Target = "N:Backend")]
