// File: Controllers/ProfilesController.cs

using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RedshiftApi.Models;
using Microsoft.Extensions.Configuration;

namespace RedshiftApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProfilesController : ControllerBase
    {
        private readonly string _redshiftConnectionString;

        public ProfilesController(IConfiguration configuration)
        {
            _redshiftConnectionString = configuration.GetConnectionString("RedshiftConnection");
        }

        [HttpGet("GetProfiles")]
        public async Task<IActionResult> GetProfiles()
        {
            var profiles = new List<Profile>();

            try
            {
                await using (var connection = new NpgsqlConnection(_redshiftConnectionString))
                {
                    await connection.OpenAsync();

                    await using (var command = new NpgsqlCommand("SELECT * FROM public.firehose_test_table", connection))
                    {
                        await using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var profile = new Profile
                                {
                                    TickerSymbol = reader.GetString(reader.GetOrdinal("sector"))
                                    // Map other fields as necessary
                                };

                                profiles.Add(profile);
                            }
                        }
                    }
                }

                return Ok(profiles);
            }
            catch (Exception ex)
            {
                // Log exception (you can inject a logger to do this)
                return StatusCode(500, ex.ToString());
            }
        }
    }
}
