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
            using (var conn = new MySqlConnection(_configuration.GetConnectionString("BotDatabase")))
            {
                var adapter = new MySqlDataAdapter("SELECT * FROM lesson_signal", conn);
                
                var dataset = new DataSet();
                
                adapter.Fill(dataset, "lesson_signal");

                foreach (DataRow row in dataset.Tables[0].Rows)
                {
                    var signalDto = new LessonSignalDto
                    {
                        Timestamp = (DateTime) row["timestamp_"],
                        Type = (LessonSignalType) row["signal_type"],
                        UserId = (string) row["user_id"]
                    };
                    yield return signalDto;
                }
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> PostSignal(SlackMessage message)
        {
            var userId = message.UserId;
            var signalType = message.Text.ConvertSlackMessageToSignalType();

            using (var conn = new MySqlConnection(_configuration.GetConnectionString("BotDatabase")))
            {
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
    }
}
