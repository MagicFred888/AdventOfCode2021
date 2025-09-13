// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Blocker Code Smell", "S2368:Public methods should not have multidimensional array parameters", Justification = "Want keep it for debug", Scope = "member", Target = "~M:AdventOfCode2021.Tools.SmallTools.DebugPrint(System.Object[,],System.Collections.Generic.Dictionary{System.String,System.String},System.String)")]
[assembly: SuppressMessage("Blocker Code Smell", "S2368:Public methods should not have multidimensional array parameters", Justification = "Want keep it for debug", Scope = "member", Target = "~M:AdventOfCode2021.Tools.SmallTools.DebugPrint(System.Char[,])")]
[assembly: SuppressMessage("Major Code Smell", "S2234:Arguments should be passed in the same order as the method parameters", Justification = "Normal, it's how we do a transposition", Scope = "member", Target = "~M:AdventOfCode2021.Tools.QuickMatrix.Transpose")]