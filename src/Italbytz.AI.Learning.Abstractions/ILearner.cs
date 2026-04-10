namespace Italbytz.AI.Learning;

public interface ILearner
{
    void Train(IDataSet ds);

    string[] Predict(IDataSet ds);

    string Predict(IExample e);

    int[] Test(IDataSet ds);
}
