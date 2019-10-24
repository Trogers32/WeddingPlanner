using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WeddingPlanner.Models;
using Microsoft.AspNetCore.Http; ///////////////////////added for session
using Microsoft.AspNetCore.Identity; ///////////password hashing
using Microsoft.EntityFrameworkCore; 				//////////entity import

namespace WeddingPlanner.Controllers
{
    public class HomeController : Controller
    {
        private MyContext dbContext;
     
        // here we can "inject" our context service into the constructor
        public HomeController(MyContext context)
        {
            dbContext = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////Register/////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        [HttpPost("create")]
        public IActionResult CreateUser(User newUser)
        {
            // We can take the User object created from a form submission
            // And pass this object to the .Add() method
            if(ModelState.IsValid)
            {
                if(dbContext.Users.Any(u => u.Email == newUser.Email))
                {
                    ModelState.AddModelError("Email", "Email already in use!");
                } else {
                    PasswordHasher<User> Hasher = new PasswordHasher<User>();
                    newUser.Password = Hasher.HashPassword(newUser, newUser.Password);
                    dbContext.Add(newUser);
                    // OR dbContext.Users.Add(newUser);
                    dbContext.SaveChanges();
                    HttpContext.Session.SetString("Email", newUser.Email);
                    return RedirectToAction("Dashboard");
                }
            } 
            return View("Index");
        }
        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////Login/////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        [HttpPost("login")]
        public IActionResult Login(string Email, string Password)
        {
            // If a User exists with provided email
            if(dbContext.Users.Any(u => u.Email == Email))
            {
                User logger = dbContext.Users.FirstOrDefault(User => User.Email == Email);
                // Initialize hasher object
                var hasher = new PasswordHasher<User>();
                // verify provided password against hash stored in db
                var result = hasher.VerifyHashedPassword(logger, logger.Password, Password);
                if(result != 0){
                    HttpContext.Session.SetString("Email", Email);
                    return RedirectToAction("Dashboard");
                }
            }
            ViewBag.fail = "Incorrect email or password.";
            return View("Index");
        }
        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////Logout/////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        [HttpGet("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.SetString("Email", "");
            return RedirectToAction("Index");
        }

        [HttpGet("dashboard")]
        public IActionResult Dashboard()
        {
            if(HttpContext.Session.GetString("Email") == "" || HttpContext.Session.GetString("Email")==null){
                ViewBag.fail = "Incorrect email or password.";
                return RedirectToAction("Index");
            }
            IEnumerable<Wedding> weds = dbContext.Weddings.Include(a=>a.Guests).ThenInclude(b=>b.User).ToList();
            ViewBag.Weddings = weds;
            foreach(var i in ViewBag.Weddings){
                int x = 0;
                if(i.Guests != null){
                    foreach(var s in i.Guests){
                        x++;
                    }
                }
                i.GuestCount = x;
            }
            User cUser = dbContext.Users.FirstOrDefault(a=>a.Email == HttpContext.Session.GetString("Email"));
            ViewBag.User = cUser;
            return View("Dashboard", weds);
        }
        [HttpGet("wedding")]
        public IActionResult Wedding()
        {
            if(HttpContext.Session.GetString("Email") == "" || HttpContext.Session.GetString("Email")==null){
                ViewBag.fail = "Incorrect email or password.";
                return RedirectToAction("Index");
            }
            return View("Wedding");
        }
        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////Create Wedding/////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        [HttpPost("CreateWedding")]
        public IActionResult CreateWedding(Wedding newWedding)
        {
            if(HttpContext.Session.GetString("Email") == "" || HttpContext.Session.GetString("Email")==null){
                ViewBag.fail = "Incorrect email or password.";
                return RedirectToAction("Index");
            }
            if(newWedding.WeddingDate <= DateTime.Now){
                ModelState.AddModelError("WeddingDate", "Wedding must be in the future.");
            }else if(ModelState.IsValid){
                User cUser = dbContext.Users.FirstOrDefault(a=>a.Email == HttpContext.Session.GetString("Email"));
                Console.WriteLine(cUser.Email);
                newWedding.Creator = cUser;
                dbContext.Add(newWedding);
                Associations newAss = new Associations();
                newAss.UserId = cUser.UserId;
                newAss.WeddingId = newWedding.WeddingId;
                dbContext.Add(newAss);
                dbContext.SaveChanges();
                return RedirectToAction("Dashboard");
            }
            return View("Wedding");
        }
        [HttpGet("wedding/{Id}")]
        public IActionResult SWedding(int Id)
        {
            if(HttpContext.Session.GetString("Email") == "" || HttpContext.Session.GetString("Email")==null){
                ViewBag.fail = "Incorrect email or password.";
                return RedirectToAction("Index");
            }
            List<User> gs = new List<User>();
            Wedding wed = dbContext.Weddings.Include(a=>a.Guests).ThenInclude(b=>b.User).FirstOrDefault(y=>y.WeddingId == Id);
            ViewBag.Wedding = wed;
            if(wed.Guests != null){
                foreach(var s in wed.Guests){
                    gs.Add(s.User);
                }
            }
            ViewBag.Guests = gs;
            return View("SWedding");
        }
        [HttpGet("RSVP/{WeddingId}/{UserId}")]
        public IActionResult RSVP(int WeddingId, int UserId)
        {
            if(HttpContext.Session.GetString("Email") == "" || HttpContext.Session.GetString("Email")==null){
                ViewBag.fail = "Incorrect email or password.";
                return RedirectToAction("Index");
            }
            Associations newAss = new Associations();
            newAss.UserId = UserId;
            newAss.WeddingId = WeddingId;
            dbContext.Add(newAss);
            dbContext.SaveChanges();
            return RedirectToAction("Dashboard");
        }
        [HttpGet("UnRSVP/{WeddingId}/{UserId}")]
        public IActionResult UnRSVP(int WeddingId, int UserId)
        {
            if(HttpContext.Session.GetString("Email") == "" || HttpContext.Session.GetString("Email")==null){
                ViewBag.fail = "Incorrect email or password.";
                return RedirectToAction("Index");
            }
            Associations newAss = dbContext.Associations.FirstOrDefault(y=>y.UserId == UserId && y.WeddingId == WeddingId);
            dbContext.Remove(newAss);
            dbContext.SaveChanges();
            return RedirectToAction("Dashboard");
        }
        [HttpGet("delete/{WeddingId}")]
        public IActionResult RemoveWedding(int WeddingId)
        {
            if(HttpContext.Session.GetString("Email") == "" || HttpContext.Session.GetString("Email")==null){
                ViewBag.fail = "Incorrect email or password.";
                return RedirectToAction("Index");
            }
            Wedding rWed = dbContext.Weddings.FirstOrDefault(y=>y.WeddingId == WeddingId);
            dbContext.Remove(rWed);
            dbContext.SaveChanges();
            return RedirectToAction("Dashboard");
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
