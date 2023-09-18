using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShoppingCart.Data;
using ShoppingCart.Models;

namespace ShoppingCart.Areas.Admin.Controllers
{
	[Area("Admin")]
	public class ProductsController : Controller
	{
		private readonly AppDbContext _context;
		private readonly IWebHostEnvironment _webHostEnvironment;
		public ProductsController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
		{
			_context = context;
			_webHostEnvironment = webHostEnvironment;
		}

		public async Task<IActionResult> Index(int p = 1)
		{
			int pageSize = 3;
			ViewBag.PageNumber = p;
			ViewBag.PageRange = pageSize;
			ViewBag.TotalPages = (int)Math.Ceiling((decimal)_context.Products.Count() / pageSize);

			return View(await _context.Products
									  .OrderByDescending(p => p.Id)
									  .Include(p => p.Category)
									  .Skip((p - 1) * pageSize)
									  .Take(pageSize)
									  .ToListAsync());

		}

		public IActionResult Create()
		{
			ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");
			return View();
		}



		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(Product product)
		{
			product.Slug = product.Name.ToLower().Replace(" ", "-");

			ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);

			if (ModelState.IsValid)
			{

				var slug = await _context.Products.FirstOrDefaultAsync(p => p.Slug == product.Slug);
				if (slug != null)
				{
					ModelState.AddModelError("", "The product already exists.");
					return View(product);
				}

				if (product.ImageUpload != null)
				{
					string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
					string imageName = Guid.NewGuid().ToString() + "_" + product.ImageUpload.FileName;

					string filePath = Path.Combine(uploadsDir, imageName);

					FileStream fs = new FileStream(filePath, FileMode.Create);
					await product.ImageUpload.CopyToAsync(fs);
					fs.Close();

					product.Image = imageName;
				}

				_context.Add(product);
				await _context.SaveChangesAsync();

				TempData["Success"] = "The product has been created!";

				return RedirectToAction("Index");
			}

			return View(product);
		}


		public async Task<IActionResult> Delete(long id)
		{
			Product p = await _context.Products.FindAsync(id);
			if (p == null) { return new ContentResult { Content = "There was an error", ContentType = "text/plain" }; }

			if (!string.Equals(p.Image, "noimage.png"))
			{
				string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "media/products", p.Image);

				if (System.IO.File.Exists(imagePath))
				{
					System.IO.File.Delete(imagePath);
				}
			}

			_context.Products.Remove(p);
			await _context.SaveChangesAsync();

			TempData["Success"] = "The product has been Deleted!";
			return RedirectToAction("Index");
		}

		public async Task<IActionResult> Edit(long id)
		{
			Product p = await _context.Products.FindAsync(id);
			ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", p.CategoryId);
			return View(p);
		}

		[HttpPost]
		public async Task<IActionResult> Edit(long id, Product product)
		{
			Product existingProduct = await _context.Products.FindAsync(id);

			string newSlug = product.Name.ToLower().Replace(" ", "-");

			// Check for duplicate slugs
			var duplicateSlug = await _context.Products
									.Where(p => p.Id != id) // Exclude the current product
									.FirstOrDefaultAsync(p => p.Slug == newSlug);

			if (duplicateSlug != null)
			{
				ModelState.AddModelError("Name", "The product name produces a URL slug that is already in use.");
			}
			ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);

			if (ModelState.IsValid)
			{
				if (product.ImageUpload != null)
				{
					if (existingProduct.Image != "noimage.png")
					{
						string oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, "media/products", existingProduct.Image);
						if (System.IO.File.Exists(oldImagePath))
						{
							System.IO.File.Delete(oldImagePath);
						}
					}

					string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
					string newImageName = Guid.NewGuid().ToString() + "_" + product.ImageUpload.FileName;
					string newImagePath = Path.Combine(uploadsDir, newImageName);

					FileStream fs = new FileStream(newImagePath, FileMode.Create);
					await product.ImageUpload.CopyToAsync(fs);
					fs.Close();

					existingProduct.Image = newImageName;
				}

				existingProduct.Name = product.Name;
				existingProduct.Description = product.Description;
				existingProduct.Price = product.Price;
				existingProduct.CategoryId = product.CategoryId;
				existingProduct.Slug = product.Slug;

				_context.Update(existingProduct);
				await _context.SaveChangesAsync();

				TempData["Success"] = "The product has been Updated!";
			}

			return View(product);
		}


	}
}


