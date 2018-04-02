using System.Web.Mvc;

namespace TestProject.Controllers
{
	public class HomeController : Controller
	{
		// GET: Default
		public ActionResult Index()
		{
			return View();
		}
	}
}