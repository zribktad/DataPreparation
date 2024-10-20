using System.Reflection;

namespace DataPreparation.Testing
{
 
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class DataMethodPreparationForAttribute : Attribute
    {
    
        public MethodInfo MethodInfo { get; }

        public DataMethodPreparationForAttribute(Type baseTestClass, string methodNmae)
        {
            MethodInfo = baseTestClass.GetMethod(methodNmae) ?? throw new ArgumentNullException(nameof(methodNmae));
        }
    }
}