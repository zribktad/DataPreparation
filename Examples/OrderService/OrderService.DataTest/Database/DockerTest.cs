﻿using DataPreparation.Database.Helpers;
using Microsoft.Extensions.Configuration;
using Npgsql;
using NUnit.Framework;
using FluentAssertions;

namespace OrderService.DataTest.Database
{
    public class DockerTest
    {

        private static string _connectionString;


        private static  string _databaseName = "OrderService"; // Original database name
        private static  string _snapshotName = "OrderService_Snapshot"; // Name for the snapshot

        public DockerTest()
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
        private DockerHelper _dockerHelper;

        [SetUp]
        public async Task SetUp()
        {
            
            _dockerHelper = new DockerHelper("0163d3066a87", "OrderService", "ear", "ear");
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
        public async Task BackupDatabaseInDocker()
        {
            var retbackup = _dockerHelper.BackupDatabaseInDocker();
            retbackup.Should().BeTrue();
         
            
            var retrestore = _dockerHelper.RestoreDatabaseInDocker();
            retrestore.Should().BeTrue();
            
        }


    }
}
