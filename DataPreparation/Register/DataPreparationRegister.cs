using System.Reflection;
using DataPreparation.Testing.Attributes;
using DataPreparation.Testing.Interfaces;

namespace DataPreparation.Testing.Register
{
    public static class DataPreparationRegister
    {
        private static readonly Dictionary<Type, Type> DataRegister = new Dictionary<Type, Type>();
        private static bool _registered;

        public static void Register<TDataPreparation, TClass>()
            where TDataPreparation : IDataPreparation
        {
            DataRegister[typeof(TClass)] = typeof(TDataPreparation);
        }

        public static Type? GetDataPreparationType(Type classType)
        {
            return DataRegister.TryGetValue(classType, out var dataPreparationType) ? dataPreparationType : null;
        }
        public static void RegisterFromAttributes()
        {
            if (_registered) return;

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
                            return new Type[] { }; // Vráť prázdne pole typov
                        }
                    })
                    .Where(type => type.GetCustomAttributes(typeof(DataPreparationFor), false).Length > 0);

                foreach (var dataPreparationType in dataPreparationTypes)
                {
                    var attribute = (DataPreparationFor)Attribute.GetCustomAttribute(dataPreparationType, typeof(DataPreparationFor));
                    if (attribute != null)
                    {
                        DataRegister[attribute.ClassType] = dataPreparationType;
                    }
                }
                _registered = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: Unable to register data preparation types. {ex.Message}");
            }
        }
    }
}
