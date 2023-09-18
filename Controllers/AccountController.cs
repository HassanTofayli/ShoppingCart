using Microsoft.AspNetCore.Mvc;

namespace ShoppingCart.Controllers
{
	public class AccountController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
