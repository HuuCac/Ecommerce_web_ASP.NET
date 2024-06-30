using EcommerceWebsite.Data;
using EcommerceWebsite.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceWebsite.ViewComponents
{
    public class MenuLoaiViewComponent : ViewComponent
    {
        private readonly DbEcommerceContext db;

        public MenuLoaiViewComponent(DbEcommerceContext context) => db = context;

        public IViewComponentResult Invoke()
        {
            var data = db.Loais.Select(lo => new MenuLoaiVM
            {
                MaLoai = lo.MaLoai,
                TenLoai = lo.TenLoai,
                SoLuong = lo.HangHoas.Count
            }).OrderBy(p => p.TenLoai);

            return View(data); // Default.cshtml
                               //return View("Default", data);
        }
    }
}
