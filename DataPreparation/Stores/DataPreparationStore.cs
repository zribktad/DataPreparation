using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DataPreparation.Data;

namespace DataPreparation.Testing.Stores
{
    internal class DataPreparationStore
    {

        private readonly Dictionary<Type, Type> DataRegister = new();
        public void Register<IMethod, TClass>()
            where IMethod : IDataPreparation
        {
            DataRegister[typeof(IMethod)] = typeof(TClass);
        }

        public  Type? GetDataPreparationType(Type methodClassType)
        {
            return DataRegister.GetValueOrDefault(methodClassType);
        }
        public void RegisterFromAttributes(Type tw)
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
