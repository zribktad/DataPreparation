using Microsoft.Extensions.Logging;
using NUnit.Framework.Internal;

namespace DataPreparation.Testing;

public interface IDataPreparationLogger
{ 
    ILoggerFactory InitializeDataPreparationTestLogger();
}