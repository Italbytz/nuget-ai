namespace Italbytz.AI.ML.UciDatasets;

public static class Data
{
    public static IDataset AdultIncome { get; } = new AdultIncomeDataset();

    public static IDataset Automobile { get; } = new AutomobileDataset();

    public static IDataset Iris { get; } = new IrisDataset();

    public static IDataset HeartDisease { get; } = new HeartDiseaseDataset();

    public static IDataset HeartDiseaseBinary { get; } = new HeartDiseaseBinaryDataset();

    public static IDataset WineQuality { get; } = new WineQualityDataset();

    public static IDataset BreastCancerWisconsinDiagnostic { get; } = new BreastCancerWisconsinDiagnosticDataset();

    public static IDataset CarEvaluation { get; } = new CarEvaluationDataset();

    public static IDataset SolarFlare { get; } = new SolarFlareDataset();

    public static IDataset NPHA { get; } = new NPHADataset();

    public static IDataset CDCDiabetes { get; } = new CDCDiabetesDataset();

    public static IDataset ObesityLevels { get; } = new ObesityLevelsDataset();

    public static IDataset Wine { get; } = new WineDataset();

    public static IDataset Lenses { get; } = new LensesDataset();

    public static IDataset BalanceScale { get; } = new BalanceScaleDataset();

    public static IDataset PageBlocks { get; } = new PageBlocksDataset();

    public static IDataset BanknoteAuthentication { get; } = new BanknoteAuthenticationDataset();

    public static IDataset StudentPerformance { get; } = new StudentPerformanceDataset();

    public static IDataset Multiplexer6 { get; } = new Multiplexer6Dataset();

    public static IDataset Multiplexer11 { get; } = new Multiplexer11Dataset();

    public static IDataset Multiplexer20 { get; } = new Multiplexer20Dataset();
}
