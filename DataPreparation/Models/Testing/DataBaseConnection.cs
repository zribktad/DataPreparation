

using DataPreparation.Unums.Testing;

namespace DataPreparation.Testing
{
    public class DataBaseConnection
    {
        public required string ConnectionString { get; set; }
        public required DatabaseType DatabaseType { get; set; }
    }
}