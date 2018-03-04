﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using ucubot.Model;

namespace ucubot.Controllers
{
    [Route("api/[controller]")]
    public class LessonSignalEndpointController : Controller
    {
        private readonly IConfiguration _configuration;

        public LessonSignalEndpointController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        
        public IEnumerable<LessonSignalDto> ShowSignals()
        {
            var connectionString = _configuration.GetConnectionString("BotDatabase");
            var connection = new MySqlConnection(connectionString);   
            var command = new MySqlCommand("SELECT * FROM lessonsignal;", connection);
            connection.Open();
            var dataTable = new DataTable();
            var da = new MySqlDataAdapter(command);
            da.Fill(dataTable);
            foreach(DataRow row in dataTable.Rows)
            {
                var id = (int) row["id"];
                var timestamp = (DateTime) row["Timestamp"];
                var signalType = (LessonSignalType)(int)row["SignalType"];
                var userId = (string) row["UserId"];
                var lessonSignalDto = new LessonSignalDto
                {
                    Id = id,
                    Timestamp = timestamp,
                    SignalType = signalType,
                    UserId = userId
                };
                yield return lessonSignalDto;
            }
            connection.Close();
            da.Dispose();
        }
        
        [HttpGet("{id}")]
        public LessonSignalDto ShowSignal(long id)
        {
            var connectionString = _configuration.GetConnectionString("BotDatabase");
            var connection = new MySqlConnection(connectionString);   
            var command = new MySqlCommand(string.Format("SELECT * FROM lessonsignal WHERE id = {0};", id), connection);
            connection.Open();
            var dataTable = new DataTable();
            var da = new MySqlDataAdapter(command);
            da.Fill(dataTable);
            if (dataTable.Rows.Count == 0)
            {
                connection.Close();
                da.Dispose();
                return null;
            }

            var row = dataTable.Rows[0];
            var timestamp = (DateTime) row["Timestamp"];
            var signalType = (LessonSignalType)(int)row["SignalType"];
            var userId = (string) row["UserId"];
            var lessonSignalDto = new LessonSignalDto
            {
                Id = (int)id,
                Timestamp = timestamp,
                SignalType = signalType,
                UserId = userId
            };
            connection.Close();
            da.Dispose();
            return lessonSignalDto;

        }
        
        [HttpPost]
        public async Task<IActionResult> CreateSignal(SlackMessage message)
        {
            var userId = message.user_id;
            var signalType = message.text.ConvertSlackMessageToSignalType();
            var connectionString = _configuration.GetConnectionString("BotDatabase");
            var connection = new MySqlConnection(connectionString);              
            connection.Open();

            var command = new MySqlCommand(string.Format("INSERT lessonsignal " +
                                           "(Timestamp, SignalType, Userid)" +
                                           "VALUES (CURRENT_TIMESTAMP, {0}, {1});", signalType, userId), connection);
            await command.ExecuteNonQueryAsync();
            connection.Close();
            return Accepted();
        }
        
        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveSignal(long id)
        {
            var connectionString = _configuration.GetConnectionString("BotDatabase");         
            var connection = new MySqlConnection(connectionString);              
            connection.Open();

            var command = new MySqlCommand(string.Format("DELETE FROM lessonsignal WHERE id = {0};", id), connection);
            await command.ExecuteNonQueryAsync();
            connection.Close();
            return Accepted();
        }
    }
}
