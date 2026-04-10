namespace Italbytz.AI.Learning.Inductive;

public interface ICrossValidation
{
    IParameterizedLearner CrossValidationWrapper(IParameterizedLearner learner, int k, IDataSet examples);
}
