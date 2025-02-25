using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DataPreparation.Testing
{
    
    /// <summary>
    /// Provides a store for data type preparation associated with test cases.
    /// </summary>
    internal  static class DataRelationStore
    {

        public static Type? GetClassDataPreparationType(Type classType)
        {
            return ClassDataRegister.GetValueOrDefault(classType);
        }
        public static Type? GetMethodDataPreparationType(MethodInfo methodInfo)
        {
            return MethodDataRegister.GetValueOrDefault(methodInfo);
        }
        public static bool HasMethodDataPreparationType(MethodInfo methodInfo)
        {
            return MethodDataRegister.ContainsKey(methodInfo);
        }

        public static void SetClassDataPreparationType(Type classType, Type data)
        {
           ClassDataRegister[classType] = data;
        }
        public static void SetMethodDataPreparationType(MethodInfo methodInfo, Type data)
        {
            MethodDataRegister[methodInfo] = data;
        }
        private static readonly ConcurrentDictionary<Type, Type> ClassDataRegister = new();
        private static readonly ConcurrentDictionary<MethodInfo, Type> MethodDataRegister = new();

    }
}
