using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using ucubot.Model;

namespace ucubot.Controllers
{
    [Route("api/[controller]/:[id]")]
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
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                var adapter = new MySqlDataAdapter("SELECT * FROM lesson_signal", conn);
                
                var dataset = new DataSet();
                
                adapter.Fill(dataset, "lesson_signal");

                foreach (DataRow row in dataset.Tables[0].Rows)
                {
                    var signalDto = new LessonSignalDto
                    {
                        Id = (long) row["id"],
                        Timestamp = (DateTime) row["timestamp_"],
                        Type = (LessonSignalType) row["signal_type"],
                        UserId = (string) row["user_id"]
                    };
                    yield return signalDto;
                }
            }
        }
        
        [HttpGet]
        public LessonSignalDto ShowSignal(long id)
        {
            using (var conn = new MySqlConnection(_configuration.GetConnectionString("BotDatabase")))
            {
                conn.Open();
                var command = new MySqlCommand("SELECT * FROM lesson_signal WHERE id = @id", conn);
                command.Parameters.Add("id", id);
                var adapter = new MySqlDataAdapter(command);
                
                var dataset = new DataSet();
                
                adapter.Fill(dataset, "lesson_signal");
                if (dataset.Tables[0].Rows.Count < 1)
                    return null;
                
                var row = dataset.Tables[0].Rows[0];
                var signalDto = new LessonSignalDto
                {
                    Timestamp = (DateTime) row["timestamp_"],
                    Type = (LessonSignalType) row["signal_type"],
                    UserId = (string) row["user_id"]
                };
                return signalDto;
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> CreateSignal(SlackMessage message)
        {
            var userId = message.UserId;
            var signalType = message.Text.ConvertSlackMessageToSignalType();

            using (var conn = new MySqlConnection(_configuration.GetConnectionString("BotDatabase")))
            {
                conn.Open();
                var command = conn.CreateCommand();
                command.CommandText =
                    "INSERT INTO lesson_signal (user_id, signal_type) VALUES (@userId, @signalType);";
                command.Parameters.AddRange(new[]
                {
                    new MySqlParameter("userId", userId),
                    new MySqlParameter("signalType", signalType)
                });
                await command.ExecuteNonQueryAsync();
            }
            
            return Accepted();
        }
        
        [HttpDelete]
        public async Task<IActionResult> RemoveSignal(long id)
        {
            using (var conn = new MySqlConnection(_configuration.GetConnectionString("BotDatabase")))
            {
                conn.Open();
                var command = conn.CreateCommand();
                command.CommandText =
                    "DELETE FROM lesson_signal WHERE ID = @id;";
                command.Parameters.Add(new MySqlParameter("id", id));
                await command.ExecuteNonQueryAsync();
            }
            
            return Accepted();
        }
    }
}
