using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace Eticaret.Entity
{
    public class DataInitializer:DropCreateDatabaseIfModelChanges<DataContext>
    {
        protected override void Seed(DataContext context)
        {
            var kategoriler = new List<Category>()
            {
                new Category() {Name="KAMERA", Description="Kamera Ürünleri"},


                new Category() {Name="BİLGİSAYAR", Description="Bilgisayar Ürünleri"},
            };
            foreach (var item in kategoriler)
            {
                context.Categories.Add(item);
            }
         
            context.SaveChanges();

            var urunler = new List<Product>()
            {
                new Product() {Name="Canon",Brand="Canon",Pattern="CS200",Releasedon="2017",Description="Kamera",Price=2500,Stock=50,IsHome=true,CategoryId=1,Image="kamera1.jpg",IsApproved=true,IsFeatured=false,Features="DENEME DENEME" },

                 new Product() { Name="Asus Bilgisayar",Brand="Asus",Pattern="K555U",Releasedon="2018",Description="Asus Bilgisayar Ürünleri",Price=500,Stock=10,IsHome=false,CategoryId=2,Image="pc1.jpg",IsApproved=true,IsFeatured=true,Features="DENEME1 DENEME1" },//IsHome false old için anasayfada gözükmez

                  new Product() { Name="Casper Bilgisayar",Brand="Casper",Pattern="CA500",Releasedon="2019",Description="Casper Bilgisayar Ürünleri",Price=2000,Stock=150,IsHome=true,CategoryId=2,Image="pc2.jpg",IsApproved=true,IsFeatured=false,Features="DENEME2 DENEME2" },
            };

            foreach (var item in urunler)
            {
                context.Products.Add(item);
            }
            context.SaveChanges();



            base.Seed(context); 
        }
    }
}