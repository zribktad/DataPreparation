namespace DataPreparation.Testing;

public class AttributeUsing(IList<Attribute> dataPreparationAttributes)
{
    int AttributesUseUpCount { get; set; }
    
    internal int IncrementAttrributeCountUp()
    {
        if(IsAllUpAttributesRun())
        {
            throw new InvalidOperationException("All attributes are used up");
        }
        return ++AttributesUseUpCount;
    }

    internal bool IsAllUpAttributesRun()
    {
        return dataPreparationAttributes.Count == AttributesUseUpCount;
    }
}