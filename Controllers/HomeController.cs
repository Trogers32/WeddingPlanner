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
            dbContext = context;   ///Database variable link
        }
        public IActionResult Index()
        {
            return View();
        }

        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////Register////////////////////////////////////////////////////
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
        /////////////////////Login///////////////////////////////////////////////////////
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
        /////////////////////Logout//////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        [HttpGet("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.SetString("Email", "");
            return RedirectToAction("Index");
        }

        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////Dashboard///////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        [HttpGet("dashboard")]
        public IActionResult Dashboard()
        {//HttpContext.Session.GetString("Email") == "" || HttpContext.Session.GetString("Email")==null
            if(!dbContext.Users.Any(e => e.Email == HttpContext.Session.GetString("Email"))){
                return RedirectToAction("Index");
            }
            Wrapper data = new Wrapper();
            data.weddings = dbContext.Weddings
                .Include(a=>a.Guests)
                .ThenInclude(b=>b.User)
                .ToList();
            data.user = dbContext.Users.FirstOrDefault(a=>a.Email == HttpContext.Session.GetString("Email"));
            return View("Dashboard", data);
        }

        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////Wedding creation page///////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        [HttpGet("wedding")]
        public IActionResult Wedding()
        {
            if(!dbContext.Users.Any(e => e.Email == HttpContext.Session.GetString("Email"))){
                return RedirectToAction("Index");
            }
            return View("Wedding");
        }

        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////Create Wedding method///////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        [HttpPost("CreateWedding")]
        public IActionResult CreateWedding(Wedding newWedding)
        {
            if(!dbContext.Users.Any(e => e.Email == HttpContext.Session.GetString("Email"))){
                return RedirectToAction("Index");
            }
            if(newWedding.WeddingDate <= DateTime.Now){
                ModelState.AddModelError("WeddingDate", "Wedding must be in the future.");
            }else if(ModelState.IsValid){
                User cUser = dbContext.Users
                    .FirstOrDefault(a=>a.Email == HttpContext.Session.GetString("Email"));
                newWedding.Creator = cUser;                                                 //assign current user as the creator of the wedding
                dbContext.Add(newWedding);                                                  //add the new wedding to the database
                Associations newAssociation = new Associations();                           //create a new rsvp entry and automatically rsvp the creator
                newAssociation.UserId = cUser.UserId;
                newAssociation.WeddingId = newWedding.WeddingId;
                dbContext.Add(newAssociation);
                dbContext.SaveChanges();                                                    //save the database changes
                return RedirectToAction("Dashboard");
            }
            return View("Wedding");
        }

        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////Individual wedding page/////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        [HttpGet("wedding/{Id}")]
        public IActionResult SWedding(int Id)
        {
            if(!dbContext.Users.Any(e => e.Email == HttpContext.Session.GetString("Email"))){
                return RedirectToAction("Index");
            }
            List<User> gs = new List<User>();
            Wedding wed = dbContext.Weddings
                .Include(a=>a.Guests)
                .ThenInclude(b=>b.User)
                .FirstOrDefault(y=>y.WeddingId == Id);
            ViewBag.Wedding = wed;
            if(wed.Guests != null){
                foreach(var s in wed.Guests){
                    gs.Add(s.User);
                }
            }
            ViewBag.Guests = gs;
            return View("SWedding");
        }

        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////RSVP method/////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        [HttpGet("RSVP/{WeddingId}/{UserId}")]
        public IActionResult RSVP(int WeddingId, int UserId)
        {
            if(!dbContext.Users.Any(e => e.Email == HttpContext.Session.GetString("Email"))){
                return RedirectToAction("Index");
            }
            Associations newAss = new Associations();
            newAss.UserId = UserId;
            newAss.WeddingId = WeddingId;
            dbContext.Add(newAss);
            dbContext.SaveChanges();
            return RedirectToAction("Dashboard");
        }

        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////UnRSVP method///////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        [HttpGet("UnRSVP/{WeddingId}/{UserId}")]
        public IActionResult UnRSVP(int WeddingId, int UserId)
        {
            if(!dbContext.Users.Any(e => e.Email == HttpContext.Session.GetString("Email"))){
                return RedirectToAction("Index");
            }
            Associations newAss = dbContext.Associations.FirstOrDefault(y=>y.UserId == UserId && y.WeddingId == WeddingId);
            dbContext.Remove(newAss);
            dbContext.SaveChanges();
            return RedirectToAction("Dashboard");
        }

        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////Delete a wedding////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        [HttpGet("delete/{WeddingId}")]
        public IActionResult RemoveWedding(int WeddingId)
        {
            if(!dbContext.Users.Any(e => e.Email == HttpContext.Session.GetString("Email"))){
                return RedirectToAction("Index");
            }
            Wedding rWed = dbContext.Weddings.FirstOrDefault(y=>y.WeddingId == WeddingId);
            dbContext.Remove(rWed);
            dbContext.SaveChanges();
            return RedirectToAction("Dashboard");
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
