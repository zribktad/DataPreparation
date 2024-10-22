using System.Reflection;
using DataPreparation.Data;

namespace DataPreparation.Testing
{
    public static class DataPreparationRegister
    {
        private static readonly Dictionary<Type, Type> DataRegister = new();
       

        public static void Register<TDataPreparation, TClass>()
            where TDataPreparation : IDataPreparation
        {
            DataRegister[typeof(TClass)] = typeof(TDataPreparation);
        }

        public static Type? GetDataPreparationType(Type classType)
        {
            return DataRegister.GetValueOrDefault(classType);
        }
        public static void RegisterFromAttributes()
        {
            
            try
            {
                var dataPreparationTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(assembly =>
                    {
                        try
                        {
                            return assembly.GetTypes();
                        }
                        catch (ReflectionTypeLoadException ex)
                        {
                            Console.WriteLine($"Warning: Unable to load types from assembly {assembly.FullName}: {ex.Message}");
                            return []; 
                        }
                    })
                    .Where(type => type.GetCustomAttributes(typeof(DataClassPreparationForAttribute), false).Length > 0);

                foreach (var dataPreparationType in dataPreparationTypes)
                {
                    var attribute = (DataClassPreparationForAttribute)Attribute.GetCustomAttribute(dataPreparationType, typeof(DataClassPreparationForAttribute));
                    if (attribute != null)
                    {
                        DataRegister[attribute.ClassType] = dataPreparationType;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: Unable to register data preparation types. {ex.Message}");
            }
        }
    }
}
