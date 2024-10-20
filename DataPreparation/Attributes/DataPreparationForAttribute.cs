using System.Reflection;

namespace DataPreparation.Testing
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DataPreparationFor: Attribute
    {
        public Type ClassType { get; }
        public DataPreparationFor(Type type)
        {
            ClassType = type;
        }
    }

}
