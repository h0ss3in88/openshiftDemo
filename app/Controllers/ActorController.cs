using app.Models;
using Microsoft.AspNetCore.Mvc;

namespace app.Controllers
{
  public class ActorController : Controller
  {
    private readonly IRunner _runner;
    public ActorController(IRunner runner)
    {
      _runner = runner;
    }
    // GET
    public IActionResult Index()
    {
      var sql =
        @"select actor_id as Id,first_name as FirstName,last_name as LastName,last_update as LastUpdate from actor order by actor_id;";
      var result = _runner.Execute<Actor>(sql, null);
      return View(result);
    }
  }
}
