// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Performance", "CA1862:Use the 'StringComparison' method overloads to perform case-insensitive string comparisons", Justification = "Entity Framework cannot translate this type of string comparison", Scope = "namespaceanddescendants", Target = "N:Backend.Database")]
[assembly: SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "Generated File, Code Cleanup attempts to edit file", Scope = "type", Target = "~T:Backend.Database.KecskeDatabaseContext")]
