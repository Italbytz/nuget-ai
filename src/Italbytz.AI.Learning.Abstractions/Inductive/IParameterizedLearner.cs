namespace Italbytz.AI.Learning.Inductive;

public interface IParameterizedLearner : ILearner
{
    int ParameterSize { get; set; }

    void Train(int size, IDataSet dataSet);
}
