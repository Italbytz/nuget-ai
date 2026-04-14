// The original version of this file is part of 
// <see href="https://sourceforge.net/projects/freak427/"/> which is released 
// under GPLv2
// Copyright (c) 2003 Patrick Briest, Dimo Brockhoff, Sebastian Degener, 
// Matthias Englert, Christian Gunia, Oliver Heering, Thomas Jansen, 
// Michael Leifhelm, Kai Plociennik, Heiko Roeglin, Andrea Schweer, 
// Dirk Sudholt, Stefan Tannenbaum, Ingo Wegener

namespace Italbytz.EA.Individuals;

/// <summary>
///     Represents an individual in a genetic programming (GP) search algorithm.
/// </summary>
/// <remarks>
///     An individual contains a genotype and fitness information, and can be
///     compared with other
///     individuals for dominance in multi-objective optimization.
/// </remarks>
internal interface IIndividual : global::Italbytz.AI.Evolutionary.Individuals.IIndividual
{
	public new IGenotype Genotype { get; }
}