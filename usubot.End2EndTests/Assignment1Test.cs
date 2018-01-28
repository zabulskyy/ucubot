using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MySql.Data.MySqlClient;
using NUnit.Framework;

namespace usubot.End2EndTests
{
    [TestFixture]
    public class Assignment1Test
    {
        private const string CONNECTION_STRING_NODB = "Server=db;Uid=root;Pwd=1qaz2wsx";
        private const string CONNECTION_STRING = "Server=db;Database=ucubot;Uid=root;Pwd=1qaz2wsx";
        
        [Test, Order(-10)]
        public void WaitForMysqlToStart()
        {
            // HACK: waits few seconds to give a time for mysql container to start
            Thread.Sleep(10000);
            Assert.That(true);
        }
        
        [Test, Order(0)]
        public void CleanData()
        {
            using (var conn = new MySqlConnection(CONNECTION_STRING_NODB))
            {
                conn.Open();
                var command = conn.CreateCommand();
                command.CommandText = "DROP DATABASE IF EXISTS ucubot;";
                command.ExecuteNonQuery();
                
                var users = ExecuteDataTable("SELECT User, Host FROM mysql.user;", conn);
                foreach (DataRow row in users.Rows)
                {
                    var name = (string) row["User"];
                    if (name == "root") continue;
                    var host = (string) row["Host"];

                    var cmd = $"DROP USER '{name}'@'{host}';";
                    var command2 = conn.CreateCommand();
                    command2.CommandText = cmd;
                    command2.ExecuteNonQuery();
                }
                
                var users2 = MapDataTableToStringCollection(ExecuteDataTable("SELECT User, Host FROM mysql.user;", conn)).ToArray();
                users2.Length.Should().Be(2);
            }
        }
        
        [Test, Order(1)]
        public void TestDatabaseWasCreated()
        {
            // create database test
            var dbScript = ReadMysqlScript("db");
            using (var conn = new MySqlConnection(CONNECTION_STRING_NODB))
            {
                conn.Open();
                
                var users1 = MapDataTableToStringCollection(ExecuteDataTable("SELECT User FROM mysql.user;", conn)).ToArray();
                users1.Length.Should().BeGreaterThan(1); // we don't know actual name of the user...
                
                var command = conn.CreateCommand();
                command.CommandText = dbScript;
                command.ExecuteNonQuery();

                var databases = MapDataTableToStringCollection(ExecuteDataTable("SHOW DATABASES;", conn)).ToArray();
                databases.Should().Contain("ucubot");

                var users = MapDataTableToStringCollection(ExecuteDataTable("SELECT User FROM mysql.user;", conn)).ToArray();
                users.Length.Should().Be(3); // we don't know actual name of the user, and there is only root exists after cleanup
            }
        }

        [Test, Order(2)]
        public void TestTableWasCreated()
        {
            // create database test
            var dbScript = ReadMysqlScript("lesson-signal");
            using (var conn = new MySqlConnection(CONNECTION_STRING))
            {
                conn.Open();
                var command = conn.CreateCommand();
                command.CommandText = dbScript;
                command.ExecuteNonQuery();
                
                var tables = MapDataTableToStringCollection(ExecuteDataTable("SHOW TABLES;", conn)).ToArray();
                tables.Should().Contain("lesson_signal");
            }
        }

        private string ReadMysqlScript(string scriptName)
        {
            using (var reader = new StreamReader(File.OpenRead($"/app/ucubot/Scripts/{scriptName}.sql")))
            {
                return reader.ReadToEnd();
            }
        }

        private DataTable ExecuteDataTable(string sqlCommand, MySqlConnection conn)
        {
            var adapter = new MySqlDataAdapter(sqlCommand, conn);

            var dataset = new DataSet();

            adapter.Fill(dataset);

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
