using DataPreparation.Testing.Stores;

namespace DataPreparation.Testing
{
    public static class CaseDataRegister
    {
        private static readonly Dictionary<Type, TestCaseStore> Register = new();
    }
}
