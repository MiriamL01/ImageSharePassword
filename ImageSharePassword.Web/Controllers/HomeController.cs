using ImageSharePassword.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using ImageSharePassword.Data;
using System.Text.Json;

namespace ImageSharePassword.Web.Controllers
{
    public class HomeController : Controller
    {
        private string _connectionString =
            @"Data Source=.\sqlexpress;Initial Catalog=ImageSharePassword;Integrated Security=true;Trust Server Certificate=true;";

        private IWebHostEnvironment _environment;

        public HomeController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public IActionResult UploadImage(IFormFile imageFile, string password)
        {
            var fileName = $"{Guid.NewGuid()}-{imageFile.FileName}";
            var FullImagePath = Path.Combine(_environment.WebRootPath, "images", fileName);
            using FileStream fs = new FileStream(FullImagePath, FileMode.Create);
            imageFile.CopyTo(fs);
            var db = new ImageShareManager(_connectionString);
            var image = new Image
            {
                FileName = fileName,
                Password = password
            };
            db.AddImage(image);

            var vm = new FileUploadViewModel
            {
                Id = image.Id,
                FileName = image.FileName,
                Password = image.Password
            };
            return View(vm);
        }
        public IActionResult ViewImage(int id)
        {
            var db = new ImageShareManager(_connectionString);
            var vm = new ImageViewModel();

            if (TempData["message"] != null)
            {
                vm.Message = (string)TempData["message"];
            }

            if (!HasPermissionToViewImage(id))
            {
                vm.HasPermission = false;
                vm.Image = new Image { Id = id};
            }
            else
            {
                vm.HasPermission = true;
                vm.Image = db.GetImageById(id);
                if (vm.Image == null)
                {
                    TempData["message"] = "Image not found!";
                    return RedirectToAction("index");
                }
                db.AddImageView(id);
            }
            return View(vm);
        }

        [HttpPost]
        public IActionResult ViewImage(int id, string Password)
        {
            var db = new ImageShareManager(_connectionString);
            var image = db.GetImageById(id);
            if (image == null)
            {
                return Redirect("/");
            }
            if (Password != image.Password)
            {
                TempData["message"] = "Invalid Password!";
            }
            else
            {
                var allowedIds = HttpContext.Session.Get<List<int>>("allowedIds");
                if (allowedIds == null)
                {
                    allowedIds = new List<int>();
                }
                allowedIds.Add(id);
                HttpContext.Session.Set("allowedIds", allowedIds);
            }
            return Redirect($"/home/viewimage?id={id}");

        }

        public bool HasPermissionToViewImage(int id)
        {
            var allowedIds = HttpContext.Session.Get<List<int>>("allowedIds");
            if (allowedIds == null)
            {
                return false;
            }
            if (!allowedIds.Contains(id))
            {
                return false;
            }
            return true;
        }

    }
    public static class SessionExtensions
    {
        public static void Set<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        public static T Get<T>(this ISession session, string key)
        {
            string value = session.GetString(key);

            return value == null ? default(T) :
                JsonSerializer.Deserialize<T>(value);
        }
    }
}
