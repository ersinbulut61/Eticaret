using Eticaret.Entity;
using Eticaret.Identity;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Eticaret.Controllers
{
    public class UserController : Controller
    {
        DataContext db = new DataContext();
        private UserManager<ApplicationUser> UserManager;
        private RoleManager<ApplicationRole> RoleManager;
        private IdentityDataContext identityDb = new IdentityDataContext();
        public UserController()
        {
            var userStore = new UserStore<ApplicationUser>(identityDb);
            UserManager = new UserManager<ApplicationUser>(userStore);
            var roleStore = new RoleStore<ApplicationRole>(identityDb);
            RoleManager = new RoleManager<ApplicationRole>(roleStore);
        }


        // GET: User
        [Authorize(Roles = "admin")]//sadece rolu admin olanlar bu sayfaya gidebilir
        public ActionResult Index()
        {
            //var users = identityDb.Users.Include(u => u.Roles).ToList();
            var users = identityDb.Users.ToList();
            
            return View(users);
        }
    }
}