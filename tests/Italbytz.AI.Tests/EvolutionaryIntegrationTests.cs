using System.Collections;
using System.Text.Json;
using Italbytz.AI.Evolutionary.Fitness;
using Italbytz.AI.Evolutionary.Individuals;
using Italbytz.AI.Evolutionary.SearchSpace;

namespace Italbytz.AI.Tests;

[TestClass]
public class EvolutionaryIntegrationTests
{
    [TestMethod]
    public void Set_literal_supports_multiple_rendering_variants()
    {
        var categories = new List<string> { "1", "2", "3" };

        Assert.AreEqual("(F0 ∈ {1,3})", new SetLiteral<string>(0, categories, 5).ToString());
        Assert.AreEqual("(F0 = 2)", new SetLiteral<string>(0, categories, 2, SetLiteralType.Dussault).ToString());
        Assert.AreEqual("(F0 < 3)", new SetLiteral<string>(0, categories, 3, SetLiteralType.LessGreater).ToString());
        Assert.AreEqual("(F0 ∈ [2,3])", new SetLiteral<string>(0, categories, 6, SetLiteralType.Su).ToString());
    }

    [TestMethod]
    public void Weighted_polynomial_reports_size_and_survives_json_roundtrip()
    {
        var literal1 = new SetLiteral<int>(0, [0, 1, 2], 6);
        var literal2 = new SetLiteral<int>(1, [0, 1, 2], 1);
        var literal3 = new SetLiteral<int>(2, [0, 1, 2], 1);

        var monomial1 = new WeightedMonomial<SetLiteral<int>, int>([literal1, literal2]);
        var monomial2 = new WeightedMonomial<SetLiteral<int>, int>([literal3]);
        var polynomial = new WeightedPolynomial<SetLiteral<int>, int>([monomial1, monomial2]);

        Assert.AreEqual(3, polynomial.Size);

        var json = JsonSerializer.Serialize(polynomial);
        var deserialized = JsonSerializer.Deserialize<WeightedPolynomial<SetLiteral<int>, int>>(json);

        Assert.IsNotNull(deserialized);
        Assert.AreEqual(polynomial.ToString(), deserialized.ToString());
    }

    [TestMethod]
    public void One_max_counts_ones_in_bitstring_genotype()
    {
        var genotype = new BitStringGenotype(new BitArray(new[] { true, false, true, true }), 4);
        var individual = new Individual(genotype, null);

        var fitness = new OneMax().Evaluate(individual);

        Assert.AreEqual(3d, fitness.ConsolidatedValue);
    }

    [TestMethod]
    public void Weighted_polynomial_genotype_predicts_class_using_fixed_weights()
    {
        var literal0 = new SetLiteral<int>(0, [0, 1], 1, SetLiteralType.Dussault);
        var literal1 = new SetLiteral<int>(0, [0, 1], 2, SetLiteralType.Dussault);

        var monomial0 = new WeightedMonomial<SetLiteral<int>, int>([literal0]) { Weights = [2f, 0f] };
        var monomial1 = new WeightedMonomial<SetLiteral<int>, int>([literal1]) { Weights = [0f, 3f] };
        var polynomial = new WeightedPolynomial<SetLiteral<int>, int>([monomial0, monomial1]) { Weights = [1f, 1f] };
        var genotype = new WeightedPolynomialGenotype<SetLiteral<int>, int>(polynomial, [literal0, literal1], Weighting.Fixed);

        Assert.AreEqual(0, genotype.PredictClass([0]));
        Assert.AreEqual(1, genotype.PredictClass([1]));
    }
}
