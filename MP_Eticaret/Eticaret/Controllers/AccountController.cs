using Eticaret.Entity;
using Eticaret.Identity;
using Eticaret.Models;
using Eticaret.Services.Senders;
using Eticaret.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Eticaret.Controllers
{
    public class AccountController : Controller
    {
        DataContext db = new DataContext();
        private UserManager<ApplicationUser> UserManager;
        private RoleManager<ApplicationRole> RoleManager;
        private IdentityDataContext identityDb = new IdentityDataContext();
        public AccountController()
        {
            var userStore = new UserStore<ApplicationUser>(identityDb);
            UserManager = new UserManager<ApplicationUser>(userStore);
            var roleStore = new RoleStore<ApplicationRole>(identityDb);
            RoleManager = new RoleManager<ApplicationRole>(roleStore);
        }
  
        // GET: Account
        public ActionResult Register()
        {
            return View();
        }
        public ActionResult Index()
        {
            var username = User.Identity.Name;
            var orders = db.Orders.Where(i => i.UserName == username).Select(i => new UserOrderModel
            {
                Id = i.Id,
                OrderNumber = i.OrderNumber,
                OrderDate = i.OrderDate,
                OrderState = i.OrderState,
                Total = i.Total
            }).OrderByDescending(i => i.OrderDate).ToList();//azalan olarak sıraladık yani en son verilen sipariş en başa gelir.
            return View(orders);
            //orders tablosundaki bilgileri userOrderModel tablosunun içerisine paketledik
        }
  

        public ActionResult Details(int id)
        {
            var entity = db.Orders.Where(i => i.Id == id).Select(i => new OrderDetailsModel()
            {
                OrderId=i.Id,
                OrderNumber=i.OrderNumber,
                Total=i.Total,
                OrderDate=i.OrderDate,
                OrderState=i.OrderState,
                AdresBasligi=i.AdresBasligi,
                Adres=i.Adres,
                Il=i.Il,
                Ilce=i.Ilce,
                Mahalle=i.Mahalle,
                PostaKodu=i.PostaKodu,
                /*kart bilgileri*/
                CartNumber=i.CartNumber,
                SecurityNumber=i.SecurityNumber,
                CartHasName=i.CartHasName,
                ExpYear=i.ExpYear,
                ExpMonth=i.ExpMonth,
                /**/
                OrderLines=i.OrderLines.Select(x=> new OrderLineModel()
                {
                    ProductId=x.ProductId,
                    Image=x.Product.Image,
                    ProductName=x.Product.Name,
                    Quantity=x.Quantity,
                    Price=x.Price
                }).ToList()
            }).FirstOrDefault();
            return View(entity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(Register model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser();
                user.Name = model.Name;
                user.Surname = model.Surname;
                user.Email = model.Email;
                user.UserName = model.Username;
                user.DateOfBirth = model.DateOfBirth;
                user.Gender = model.Gender;

                var result = UserManager.Create(user, model.Password);

                if (result.Succeeded)
                {
                    //mail gönder
                    var emailService = new EmailService();
                    var body = $"Merhaba <b>{user.Name}  {user.Surname}</b> <br/> <p>Web sitemize üye Olduğun için teşekkür ederiz.</p>";
                    await emailService.SendAsync(new IdentityMessage() { Body = body, Subject = "Sitemize Hoşgeldiniz" }, user.Email);

                    if (RoleManager.RoleExists("user"))
                    {
                        UserManager.AddToRole(user.Id, "user");
                    }
                    return RedirectToAction("Login", "Account");
                   
                }
                else
                {
                    ModelState.AddModelError("RegisterUserError", "Kullanıcı Oluşturma Hatası...");
                }
            }
            return View();
        }


        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(Login model,string ReturnUrl)
        {
            if (ModelState.IsValid)
            {
                var user = UserManager.Find(model.Username, model.Password);//modelden alınan kullanıcıadı ve şifreye göre kullanıcı arar
                if (user!=null)//kullanıcı sistemde var ise
                {
                    var autManager = HttpContext.GetOwinContext().Authentication;
                    var identityclaims = UserManager.CreateIdentity(user, "ApplicationCookie");
                    var authProperties = new AuthenticationProperties();
                    authProperties.IsPersistent = model.RememberMe;//sisteme beni dahil et
                    autManager.SignIn(authProperties, identityclaims);
                    if (!String.IsNullOrEmpty(ReturnUrl))
                    {
                        return Redirect(ReturnUrl);
                    }
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("LoginUserError", "Böyle bir kullanıcı yok..");
                }
            }
            return View();
        }
        public ActionResult LogOut()
        {
            var autManager = HttpContext.GetOwinContext().Authentication;
            autManager.SignOut();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Authorize]
        public ActionResult UserProfile()
        {
            var id = HttpContext.GetOwinContext().Authentication.User.Identity.GetUserId();
            var user = UserManager.FindById(id);
            var data = new ProfilePasswordViewModel()
            {
                UserProfileViewModel = new UserProfileViewModel()
                {
                    Email = user.Email,
                    Id = user.Id,
                    Name = user.Name,
                    /**/
                    DateOfBirth=user.DateOfBirth,
                    Gender=user.Gender,
                    Adres=user.Adres,
                    AdresBasligi=user.AdresBasligi,
                    Il=user.Il,
                    Ilce=user.Ilce,
                    Mahalle=user.Mahalle,
                    PostaKodu=user.PostaKodu,
                    CartNumber=user.CartNumber,
                    SecurityNumber=user.SecurityNumber,
                    CartHasName=user.CartHasName,
                    ExpYear=user.ExpYear,
                    ExpMonth=user.ExpMonth,
                    /**/
                    PhoneNumber = user.PhoneNumber,
                    Surname = user.Surname,
                    Username = user.UserName
                }
            };
            return View(data);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<ActionResult> UpdateProfile(ProfilePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("UserProfile", model);
            }
            var userManager = UserManager;

            var user = await UserManager.FindByIdAsync(model.UserProfileViewModel.Id);

            user.Name = model.UserProfileViewModel.Name;
            user.Surname = model.UserProfileViewModel.Surname;
            user.PhoneNumber = model.UserProfileViewModel.PhoneNumber;
            user.Email = model.UserProfileViewModel.Email;
            /**/
            user.DateOfBirth = model.UserProfileViewModel.DateOfBirth;
            user.Gender = model.UserProfileViewModel.Gender;
            user.AdresBasligi = model.UserProfileViewModel.AdresBasligi;
            user.Adres = model.UserProfileViewModel.Adres;
            user.Il = model.UserProfileViewModel.Il;
            user.Ilce = model.UserProfileViewModel.Ilce;
            user.Mahalle = model.UserProfileViewModel.Mahalle;
            user.PostaKodu = model.UserProfileViewModel.PostaKodu;

            user.CartNumber = model.UserProfileViewModel.CartNumber;
            user.SecurityNumber = model.UserProfileViewModel.SecurityNumber;
            user.CartHasName = model.UserProfileViewModel.CartHasName;
            user.ExpYear = model.UserProfileViewModel.ExpYear;
            user.ExpMonth = model.UserProfileViewModel.ExpMonth;
            /**/

            await userManager.UpdateAsync(user);
            TempData["mesaj"] = "Bilgileriniz kaydedildi";
            return RedirectToAction("UserProfile");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<ActionResult> ChangePassword (ProfilePasswordViewModel model)
        {
            var userManager = UserManager;
            var id = HttpContext.GetOwinContext().Authentication.User.Identity.GetUserId();
            var user = UserManager.FindById(id);
            var data = new ProfilePasswordViewModel()
            {
                UserProfileViewModel = new UserProfileViewModel()
                {
                    Email = user.Email,
                    Id = user.Id,
                    Name = user.Name,
                    PhoneNumber = user.PhoneNumber,
                    Surname = user.Surname,
                    Username = user.UserName
                }
            };
            model.UserProfileViewModel = data.UserProfileViewModel;
            if (!ModelState.IsValid)
            {
                return View("UserProfile", model);
            }

            var result = await userManager.ChangePasswordAsync(HttpContext.GetOwinContext().Authentication.User.Identity.GetUserId(),
                model.changePasswordView.OldPassword,
                model.changePasswordView.NewPassword);
            if (result.Succeeded)
            {//mail gönder
                var emailService = new EmailService();
                var body = $"Merhaba <b>{user.Name}  {user.Surname}</b> <br/> <p>Hesabınızın şifresi güncellenmiştir.</p>";
                await emailService.SendAsync(new IdentityMessage() { Body = body, Subject = "Sitemize Hoşgeldiniz" }, user.Email);
                return RedirectToAction("Logout","Account");
            }
            return View("UserProfile",model);

        }
        public ActionResult UserPassword()
        {
            return View();
        }


    }
}