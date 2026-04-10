namespace Italbytz.AI.ML.UciDatasets;

public static class Data
{
    public static IDataset Iris { get; } = new IrisDataset();

    public static IDataset HeartDisease { get; } = new HeartDiseaseDataset();

    public static IDataset WineQuality { get; } = new WineQualityDataset();

    public static IDataset BreastCancerWisconsinDiagnostic { get; } = new BreastCancerWisconsinDiagnosticDataset();

    public static IDataset Lenses { get; } = new LensesDataset();

    public static IDataset BalanceScale { get; } = new BalanceScaleDataset();
}
