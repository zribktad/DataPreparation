using Microsoft.Extensions.Logging;
using NUnit.Framework.Internal;

namespace DataPreparation.Testing;

public interface IDataPreparationLoggerInitializer
{
    static abstract ILoggerFactory InitializeDataPreparationTestLogger();
}