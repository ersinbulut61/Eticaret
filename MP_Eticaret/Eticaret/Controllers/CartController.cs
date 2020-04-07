using Eticaret.Entity;
using Eticaret.Identity;
using Eticaret.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Eticaret.Controllers
{
    public class CartController : Controller
    {
        // GET: Cart

        DataContext db = new DataContext();
        public ActionResult Index()
        {
            return View(GetCart());
        }
        public ActionResult AddToCart(int Id)//karta ekle
        {
            var product = db.Products.FirstOrDefault(i => i.Id == Id);
            if (product!=null)//eğer ürün veri tabanında var ise
            {
                GetCart().AddProduct(product, 1);
            }
            return RedirectToAction("Index");
        }
        public ActionResult RemoveFromCart(int Id)//karttan sil
        {
            var product = db.Products.FirstOrDefault(i => i.Id == Id);
            if (product != null)//eğer ürün veri tabanında var ise
            {
                GetCart().DeleteProduct(product);
            }
            return RedirectToAction("Index");
        }
       
        public Cart GetCart()
        {
            var cart = (Cart)Session["Cart"];
            if (cart==null)//kullanıcı kart oluşturmamışsa
            {
                cart = new Cart();
                Session["Cart"] = cart;
            }
            return cart;
        }
        public PartialViewResult Summary()
        {
            return PartialView(GetCart());
        }
        /**/

        //public ActionResult AddressList()
        //{
        //    var addres = db.Addres.Where(u=>u.UserName == User.Identity.Name).ToList();
        //    return View(addres);
        //}
        //public ActionResult CreateUserAddress()
        //{
        //    return View();
        //}
        //[HttpPost]
        //public ActionResult CreateUserAddress(Addres entity)
        //{
        //    entity.UserName = User.Identity.GetUserName();

        //    db.Addres.Add(entity);
        //    db.SaveChanges();
        //    return RedirectToAction("AddressList");
        //}
        //public ActionResult CreateOrder(int id, Cart cart, Addres entity)
        //{
        //    Order order = new Order();

        //    if (cart.Cartlines.Count == 0)
        //    {
        //        ModelState.AddModelError("UrunYokError", "Sepetinizde ürün bulunmamaktadır..");
        //    }
        //    if (ModelState.IsValid)//sepette ürün var ise
        //    {
        //        order.UserAddressID = id;
        //        order.OrderNumber = "A" + (new Random()).Next(1111, 9999).ToString();//a harfi ile başlayan 4 haneli bir sipariş numarası 
        //        order.Total = cart.Total();
        //        order.OrderDate = DateTime.Now;
        //        order.OrderState = EnumOrderState.Bekleniyor;
        //        order.UserName = User.Identity.Name;
        //        order.AdresBasligi = entity.AdresBasligi;
        //        order.Adres = entity.Adres;
        //        order.Il = entity.Il;
        //        order.Ilce = entity.Ilce;
        //        order.Mahalle = entity.Mahalle;
        //        order.PostaKodu = entity.PostaKodu;
        //        /*kart bilgileri*/
        //        //order.CartNumber = entity.CartNumber;
        //        //order.SecurityNumber = entity.SecurityNumber;
        //        //order.CartHasName = entity.CartHasName;
        //        //order.ExpYear = entity.ExpYear;
        //        //order.ExpMonth = entity.ExpMonth;
        //        /**/
        //        order.OrderLines = new List<OrderLine>();
        //        foreach (var pr in cart.Cartlines)
        //        {
        //            var orderline = new OrderLine();
        //            orderline.Quantity = pr.Quantity;
        //            orderline.Price = pr.Quantity * pr.Product.Price;
        //            orderline.ProductId = pr.Product.Id;
        //            order.OrderLines.Add(orderline);
        //        }
        //        db.Addres.Add(entity);
        //        db.Orders.Add(order);
        //        db.SaveChanges();
        //        //veri tabanına kaydet
        //        cart.Clear();
        //        //return RedirectToAction("Complated", new { id = order.Id });
        //        //return RedirectToAction("Complated");
        //        return View("Complated");
        //    }
        //    else
        //    {
        //        return View(entity);
        //    }

        //}
        ////public ActionResult Complated(int id)
        ////{
        ////    var data = db.Orders
        ////        .Where(x => x.Id == id).FirstOrDefault();
        ////    return View(data);
        ////}
        /**/
        public ActionResult Checkout()
        {
            return View(new ShippingDetails());
        }

        [HttpPost]
        public ActionResult Checkout(ShippingDetails entity)
        {
            var cart = GetCart();
            if (cart.Cartlines.Count == 0)
            {
                ModelState.AddModelError("UrunYokError", "Sepetinizde ürün bulunmamaktadır..");
            }
            if (ModelState.IsValid)//sepette ürün var ise
            {
                SaveOrder(cart, entity);
                //veri tabanına kaydet
                cart.Clear();
                return View("Complated");
            }
            else
            {
                return View(entity);
            }

        }

        private void SaveOrder(Cart cart, ShippingDetails entity)
        {
            var order = new Order();
            order.OrderNumber = "A" + (new Random()).Next(1111, 9999).ToString();//a harfi ile başlayan 4 haneli bir sipariş numarası 
            order.Total = cart.Total();
            order.OrderDate = DateTime.Now;
            order.OrderState = EnumOrderState.Bekleniyor;
            order.UserName = User.Identity.Name;
            order.AdresBasligi = entity.AdresBasligi;
            order.Adres = entity.Adres;
            order.Il = entity.Il;
            order.Ilce = entity.Ilce;
            order.Mahalle = entity.Mahalle;
            order.PostaKodu = entity.PostaKodu;
            /*kart bilgileri*/
            order.CartNumber = entity.CartNumber;
            order.SecurityNumber = entity.SecurityNumber;
            order.CartHasName = entity.CartHasName;
            order.ExpYear = entity.ExpYear;
            order.ExpMonth = entity.ExpMonth;
            /**/
            order.OrderLines = new List<OrderLine>();
            foreach (var pr in cart.Cartlines)
            {
                var orderline = new OrderLine();
                orderline.Quantity = pr.Quantity;
                orderline.Price = pr.Quantity * pr.Product.Price;
                orderline.ProductId = pr.Product.Id;
                order.OrderLines.Add(orderline);
            }
            db.Orders.Add(order);
            db.SaveChanges();
        }
    }
}