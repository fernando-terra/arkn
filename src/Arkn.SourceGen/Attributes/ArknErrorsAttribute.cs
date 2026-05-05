// This file is compiled into the source generator assembly (netstandard2.0).
// A copy is also emitted into consumer projects via the generator's Initialize method.
using System;
namespace Arkn.SourceGen;

/// <summary>
/// Marks a static partial class as an Arkn error group.
/// The source generator will implement all partial methods annotated with <see cref="ArknErrorCodeAttribute"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class ArknErrorsAttribute : Attribute { }
