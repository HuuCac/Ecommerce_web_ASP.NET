using EcommerceWebsite.Data;
using EcommerceWebsite.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcommerceWebsite.Controllers
{
    public class HangHoaController : Controller
    {
        private readonly DbEcommerceContext db;

        public HangHoaController(DbEcommerceContext context)
        {
            db = context;
        }

        public IActionResult Index(int? loai, int currentPage = 1)
        {
            var hangHoas = db.HangHoas.AsQueryable();

            if (loai.HasValue)
            {
                hangHoas = hangHoas.Where(p => p.MaLoai == loai.Value);
            }

            var pageSize = 9;
            var total = hangHoas.Count();
            var totalPage = (int)Math.Ceiling((double)total / pageSize);

            var result = hangHoas
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new HangHoaVM
                {
                    MaHh = p.MaHh,
                    TenHh = p.TenHh,
                    DonGia = p.DonGia ?? 0,
                    Hinh = p.Hinh ?? "",
                    MoTa = p.MoTa ?? "",
                    TenLoai = p.MaLoaiNavigation.TenLoai
                })
                .ToList();

            var viewModel = new PaginatedList<HangHoaVM>
            {
                Items = result,
                CurrentPage = currentPage,
                TotalPages = totalPage
            };

            return View(viewModel);
        }


        public IActionResult Search(string? query)
        {
            var hangHoas = db.HangHoas.AsQueryable();

            if (query != null)
            {
                hangHoas = hangHoas.Where(p => p.TenHh.Contains(query));
            }

            var result = hangHoas.Select(p => new HangHoaVM
            {
                MaHh = p.MaHh,
                TenHh = p.TenHh,
                DonGia = p.DonGia ?? 0,
                Hinh = p.Hinh ?? "",
                MoTa = p.MoTa ?? "",
                TenLoai = p.MaLoaiNavigation.TenLoai
            });
            return View(result);
        }

        public IActionResult Detail(int id)
        {
            var data = db.HangHoas
                .Include(p => p.MaLoaiNavigation)
                .SingleOrDefault(p => p.MaHh == id);
            if (data == null)
            {
                TempData["Message"] = $"Không thấy sản phẩm có mã {id}";
                return Redirect("/404");
            }
            var result = new HangHoaVM
            {
                MaHh = data.MaHh,
                TenHh = data.TenHh,
                DonGia = data.DonGia ?? 0,
                MoTa = data.MoTa ?? string.Empty,
                Hinh = data.Hinh ?? string.Empty,
                MoTaDonVi = data.MoTaDonVi ?? string.Empty,
                TenLoai = data.MaLoaiNavigation.TenLoai,
                SoLuong = data.SoLuong,
            };
            return View(result);
        }
    }
}
