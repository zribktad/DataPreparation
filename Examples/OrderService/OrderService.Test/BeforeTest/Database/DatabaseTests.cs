using Microsoft.Extensions.Configuration;
using Npgsql;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace OrderService.DataTest.Database
{
    
    [Explicit]
    public class DatabaseTests
    {

        private static string _connectionString;


        private static  string _databaseName = "OrderServiceBddTest"; // Original database name
        private static  string _snapshotName = "OrderService_Snapshot"; // Name for the snapshot

        public DatabaseTests()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            var configuration = configurationBuilder.Build();
            _connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION") ??
                                   configuration.GetConnectionString("DefaultConnection");

        }

        private NpgsqlConnection connection;
        [SetUp]
        public async Task SetUp()
        {
            connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
        }
        [TearDown]
        public async Task TearDown()
        {
            await connection.CloseAsync();
            await connection.DisposeAsync();
            connection = null;
        }
        [Test]
        public void DBConnectionTest()
        {
            using var command = new NpgsqlCommand("SELECT * FROM \"Complaints\" LIMIT 10", connection);

            using var reader = command.ExecuteReader();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                Console.Write($"{reader.GetName(i)}\t"); // Print column name with a tab space
            }

            Console.WriteLine();
            while (reader.Read())
            {
                var rowValues = new List<string>();

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    // Convert each value to string
                    var value = reader.IsDBNull(i) ? "NULL" : reader.GetValue(i).ToString();
                    rowValues.Add(value);
                }

                // Join all values in a row into a single string for display
                string rowString = string.Join(", ", rowValues);
                Console.WriteLine(rowString);
            }

        }

        [Test]
        public void CreateSnapshot_Should_Create_Snapshot_Successfully_PG()
        {
 
            // Act
            CreateSnapshotPG();
            // Assert
            // Check if snapshot exists (assuming a way to verify snapshot existence)
            using var command = new NpgsqlCommand($"SELECT COUNT(*) FROM pg_catalog.pg_database WHERE datname = '{_snapshotName}';", connection);
            var result = (long)command.ExecuteScalar();
            Assert.That(result, Is.EqualTo(1), "Snapshot should exist after creation.");
        }
        
        [Test]
        public void CreateSnapshot_And_Restore_Successfully_PG()
        {
 
            // Act
            CreateSnapshotPG();
            // Assert
            // Check if snapshot exists (assuming a way to verify snapshot existence)
            using var command = new NpgsqlCommand($"SELECT COUNT(*) FROM pg_catalog.pg_database WHERE datname = '{_snapshotName}';", connection);
            var result = (long)command.ExecuteScalar();
            Assert.That(result, Is.EqualTo(1), "Snapshot should exist after creation.");


            PrintDataFromAllTablesinSnapchotDatabase();
            RestoreDatabaseFromSnapshotPG();
        }

        [Test, Explicit]
        public void CreateBackup_Should_Create_Snapshot_Successfully_PG()
        {

            // Act
            //NpgSql();
            // Assert
            // Check if snapshot exists (assuming a way to verify snapshot existence)
            using var command = new NpgsqlCommand($"SELECT COUNT(*) FROM pg_catalog.pg_database WHERE datname = '{_snapshotName}';", connection);
            var result = (long)command.ExecuteScalar();
            Assert.That(result, Is.EqualTo(1), "Snapshot should exist after creation.");
        }

        [Test, Explicit]
        public void RestoreDatabaseFromSnapshot_Should_Restore_Database_Successfully()
        {
            // Act
            RestoreDatabaseFromSnapshot();

            // Assert
            // Verify the original database state (this would depend on your setup)
            using var command = new NpgsqlCommand("SELECT COUNT(*) FROM \"YourTable\";", connection);
            var originalDataCount = (long)command.ExecuteScalar(); // Adjust table name accordingly
            Assert.That(originalDataCount, Is.GreaterThan(0), "Database should have records after restoration.");
        }

        [Test]
        public void TransactionTest()
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            long countExpected = 0;
            using (var command = new NpgsqlCommand("SELECT COUNT(*) FROM \"Customers\" WHERE \"Name\" = 'Test Customer for Rollback';", connection))
            {
                 countExpected = (long)command.ExecuteScalar();
            
            }
            using var transaction = connection.BeginTransaction();
            try
            {
                // Example operation 1: Insert a record into Customers
                using var command = new NpgsqlCommand("INSERT INTO \"Customers\" (Name, Email, Phone, AddressId) VALUES ('Test Customer for Rollback', 'rollback@example.com', '987-654-3210', 1);", connection, transaction);
                command.ExecuteNonQuery();

                // Simulate an error by throwing an exception
                throw new Exception("Simulated error to trigger rollback.");

                // If we reach here, it means the error did not occur
                transaction.Commit();
            }
            catch (Exception)
            {
                // Rollback the transaction on error
                transaction.Rollback();
            }

            // Verify that the record was not inserted
            using (var command = new NpgsqlCommand("SELECT COUNT(*) FROM \"Customers\" WHERE \"Name\" = 'Test Customer for Rollback';", connection))
            {
                var count = (long)command.ExecuteScalar();
                Assert.That(count, Is.EqualTo(countExpected), "Args should not be present after rollback.");
            }

        }
        private void CreateSnapshot()
        {

            using var command = new NpgsqlCommand($"CREATE DATABASE {_snapshotName} AS SNAPSHOT OF {_databaseName};", connection);
            command.ExecuteNonQuery();
        }
        public void CreateSnapshotPG()
        {
            // Drop the snapshot database if it already exists
            using (var dropCommand = new NpgsqlCommand($"DROP DATABASE IF EXISTS \"{_snapshotName}\";", connection))
            {
                dropCommand.ExecuteNonQuery();
            }

            // Create a new snapshot by cloning the original database
            using (var cloneCommand = new NpgsqlCommand($"CREATE DATABASE \"{_snapshotName}\" WITH TEMPLATE \"{_databaseName}\";", connection))
            {
                cloneCommand.ExecuteNonQuery();
            }
        }

        public void RestoreDatabaseFromSnapshotPG()
        {

            // Drop the original database
            // using (var dropCommand = new NpgsqlCommand($"DROP DATABASE IF EXISTS \"{_databaseName}\";", connection))
            // {
            //     dropCommand.ExecuteNonQuery();
            // }

            // Recreate the original database from the snapshot
            using (var restoreCommand = new NpgsqlCommand($"CREATE DATABASE \"{_databaseName}\" WITH TEMPLATE \"{_snapshotName}\";", connection))
            {
                restoreCommand.ExecuteNonQuery();
            }
        }
        
        public void PrintDataFromAllTablesinSnapchotDatabase()
        {
            using var command = new NpgsqlCommand("SELECT table_name FROM information_schema.tables WHERE table_schema = 'public';", connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var tableName = reader.GetString(0);
                Console.WriteLine($"Table: {tableName}");

                using var dataCommand = new NpgsqlCommand($"SELECT * FROM \"{tableName}\";", connection);
                using var dataReader = dataCommand.ExecuteReader();

                for (int i = 0; i < dataReader.FieldCount; i++)
                {
                    Console.Write($"{dataReader.GetName(i)}\t"); // Print column name with a tab space
                }

                Console.WriteLine();
                while (dataReader.Read())
                {
                    var rowValues = new List<string>();

                    for (int i = 0; i < dataReader.FieldCount; i++)
                    {
                        // Convert each value to string
                        var value = dataReader.IsDBNull(i) ? "NULL" : dataReader.GetValue(i).ToString();
                        rowValues.Add(value);
                    }

                    // Join all values in a row into a single string for display
                    string rowString = string.Join(", ", rowValues);
                    Console.WriteLine(rowString);
                }
            }
           
        }

        private void RestoreDatabaseFromSnapshot()
        {

            // Note: You usually don't use "USE master;" in PostgreSQL, remove this if it's not valid
            // using var masterCommand = new NpgsqlCommand("USE master;", connection);
            // masterCommand.ExecuteNonQuery();

            // Restore the database from the snapshot
            using var restoreCommand = new NpgsqlCommand($"RESTORE DATABASE {_databaseName} FROM DATABASE_SNAPSHOT = '{_databaseName}';", connection);
            restoreCommand.ExecuteNonQuery();
        }
        
        public void RestoreDatabaseFromSnapshot(string backupFilePath, string newDatabaseName, NpgsqlConnection connection)
        {
            var restoreQuery = $"psql -U username -h localhost -d {newDatabaseName} < {_databaseName}";
            using var command = new NpgsqlCommand(restoreQuery, connection);
            command.ExecuteNonQuery();
        }


    }
}
