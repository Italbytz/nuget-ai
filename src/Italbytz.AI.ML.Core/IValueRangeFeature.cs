namespace Italbytz.AI.ML.Core;

public interface IValueRangeFeature<TValue> : IFeature
{
    List<TValue> ValueRange { get; set; }
}
