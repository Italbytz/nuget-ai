namespace Italbytz.AI.ML.UciDatasets;

public record TrainValidateTestFileNames
{
    public required string TrainFileName { get; set; }
    public required string ValidateFileName { get; set; }
    public required string TrainValidateFileName { get; set; }
    public required string TestFileName { get; set; }
}
