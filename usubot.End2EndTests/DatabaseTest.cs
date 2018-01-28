using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using MySql.Data.MySqlClient;
using NUnit.Framework;

namespace usubot.End2EndTests
{
    [TestFixture]
    public class BabyStepsAssignmentTest
    {
        private const string CONNECTION_STRING = "Server=localhost;Database=ucubot;Uid=root;Pwd=1qaz2wsx";
        
        [Test, Order(1)]
        public async Task TestDatabaseWasCreated()
        {
            // create database test
            var dbScript = await ReadMysqlScript("db");
            using (var conn = new MySqlConnection(CONNECTION_STRING))
            {
                var command = conn.CreateCommand();
                command.CommandText = dbScript;
                await command.ExecuteNonQueryAsync();

                var databases = MapDataTableToStringCollection(await ExecuteDataTable("SHOW DATABASES;", conn)).ToArray();
                databases.Should().Contain("ucubot");
                
                var users = MapDataTableToStringCollection(await ExecuteDataTable("SELECT User FROM mysql.user;", conn)).ToArray();
                users.Length.Should().BeGreaterThan(1); // we don't know actual name of the user...
            }
        }
        
        [Test, Order(2)]
        public async Task TestTableWasCreated()
        {
            // create database test
            var dbScript = await ReadMysqlScript("lesson_signal");
            using (var conn = new MySqlConnection(CONNECTION_STRING))
            {
                var command = conn.CreateCommand();
                command.CommandText = dbScript;
                await command.ExecuteNonQueryAsync();
            }
        }

        private Task<string> ReadMysqlScript(string scriptName)
        {
            using (var reader = new StreamReader(File.OpenRead($"/app/ucubot/Scripts/{scriptName}.sql")))
            {
                return reader.ReadToEndAsync();
            }
        }

        private async Task<DataTable> ExecuteDataTable(string sqlCommand, MySqlConnection conn)
        {
            var adapter = new MySqlDataAdapter(sqlCommand, conn);
            
            var dataset = new DataSet();

            await adapter.FillAsync(dataset);

            return dataset.Tables[0];
        }

        private IEnumerable<string> MapDataTableToStringCollection(DataTable table)
        {
            foreach (DataRow row in table.Rows)
            {
                yield return row[0].ToString();
            }
        }
    }
}