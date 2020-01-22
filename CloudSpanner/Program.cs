using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Google.Cloud.Spanner;
using Google.Cloud.Spanner.Data;


namespace CloudSpanner
{
    public class Program
    {
        public static async Task Main(string[] args)
        {

            string connectionString =
               $"Data Source=projects/{"ingka-recovery-cloud-dev"}/instances/{"myspanner"}";

            // public string projectId;
            // public string instanceId;

            // public string databaseId;

            //var reponse = CreateAsync(projectId,instanceId,databaseId);

            // var verbMap = new VerbMap<object>();
            //verbMap
            //    .Add((CreateOptions opts) => Create(
            //        opts.projectId, opts.instanceId, opts.databaseId))
            //.Add((InsertOptions opts) => Insert(
            //    opts.projectId, opts.instanceId, opts.databaseId, opts.insertType))
            //.Add((QueryOptions opts) => Query(
            //    opts.projectId, opts.instanceId, opts.databaseId, opts.timespan))
            //.NotParsedFunc = (err) => 1;
            //return (int)verbMap.Run(args);


            using (SpannerConnection connection = new SpannerConnection(connectionString))
            {
                SpannerCommand createDbCmd = connection.CreateDdlCommand($"CREATE DATABASE {"example-db"}");
                await createDbCmd.ExecuteNonQueryAsync();

                SpannerCommand createTableCmd = connection.CreateDdlCommand(
                    @"CREATE TABLE TestTable (
                                Key                STRING(MAX) NOT NULL,
                                StringValue        STRING(MAX),
                                Int64Value         INT64,
                              ) PRIMARY KEY (Key)");
                await createTableCmd.ExecuteNonQueryAsync();
            }

        }


        public static async Task CreateAsync(
            string projectId, string instanceId, string databaseId)
        {
            // [START spanner_create_custom_database]
            // Initialize request connection string for database creation.
            string connectionString =
                $"Data Source=projects/{projectId}/instances/{instanceId}";
            // Make the request.
            using (var connection = new SpannerConnection(connectionString))
            {
                string createStatement = $"CREATE DATABASE `{databaseId}`";
                var cmd = connection.CreateDdlCommand(createStatement);
                await cmd.ExecuteNonQueryAsync();
            }
            // [END spanner_create_custom_database]
        }


        public static async Task CreateSampleDatabaseAsync(string projectId, string instanceId, string databaseId)
        {
            // [START spanner_create_database]
            // Initialize request connection string for database creation.
            string connectionString =
                $"Data Source=projects/{projectId}/instances/{instanceId}";
            // Make the request.
            using (var connection = new SpannerConnection(connectionString))
            {
                string createStatement = $"CREATE DATABASE `{databaseId}`";
                var cmd = connection.CreateDdlCommand(createStatement);
                try
                {
                    await cmd.ExecuteNonQueryAsync();
                }
                catch (SpannerException e) when (e.ErrorCode == ErrorCode.AlreadyExists)
                {
                    // OK.
                }
            }
            // Update connection string with Database ID for table creation.
            connectionString = connectionString + $"/databases/{databaseId}";
            using (var connection = new SpannerConnection(connectionString))
            {
                // Define create table statement for table #1.
                string createTableStatement =
               @"CREATE TABLE Singers (
                     SingerId INT64 NOT NULL,
                     FirstName    STRING(1024),
                     LastName STRING(1024),
                     ComposerInfo   BYTES(MAX)
                 ) PRIMARY KEY (SingerId)";
                // Make the request.
                var cmd = connection.CreateDdlCommand(createTableStatement);
                await cmd.ExecuteNonQueryAsync();
                // Define create table statement for table #2.
                createTableStatement =
                @"CREATE TABLE Albums (
                     SingerId     INT64 NOT NULL,
                     AlbumId      INT64 NOT NULL,
                     AlbumTitle   STRING(MAX)
                 ) PRIMARY KEY (SingerId, AlbumId),
                 INTERLEAVE IN PARENT Singers ON DELETE CASCADE";
                // Make the request.
                cmd = connection.CreateDdlCommand(createTableStatement);
                await cmd.ExecuteNonQueryAsync();
            }
            // [END spanner_create_database]
        }


        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
