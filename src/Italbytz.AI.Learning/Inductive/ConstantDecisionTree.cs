namespace Italbytz.AI.Learning.Inductive;

public class ConstantDecisionTree : DecisionTree
{
    public ConstantDecisionTree(string value)
    {
        Value = value;
    }

    public string Value { get; set; }

    public override object Predict(IExample example)
    {
        return Value;
    }
}
