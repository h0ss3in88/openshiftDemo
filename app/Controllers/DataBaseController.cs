using System;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace app.Controllers
{
  public class DataBaseController : Controller
  {
    private IHostingEnvironment _environment;
    public DataBaseController(IHostingEnvironment environment)
    {
      _environment = environment;
    }
    // GET
    public IActionResult Index()
    {
      return View();
    }
    // GET
    public IActionResult CreateDataBase()
    {
      var connectionString = "server=postgresql;port=5432;user id=Hussein;password=123456;";
      NpgsqlConnection connection = new NpgsqlConnection(connectionString);
      var sql = System.IO.File.ReadAllText(Path.Combine(_environment.ContentRootPath, "Files","pg_chinook_mod_to_lower.sql"), Encoding.UTF8);
      var command = new NpgsqlCommand
      {
        Connection = connection,
        CommandText = sql
      };
      connection.Open();
      try
      {
        command.ExecuteNonQuery();
        return Ok();
      }
      catch (NpgsqlException e)
      {
        return BadRequest(e.Message);
        throw e;
      }
      finally
      {
        connection.Close();
      }
    }
  }
}
