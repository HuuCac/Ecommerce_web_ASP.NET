using AutoMapper;
using EcommerceWebsite.Data;
using EcommerceWebsite.Helpers;
using EcommerceWebsite.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EcommerceWebsite.Controllers
{
	public class KhachHangController : Controller
	{
		private readonly DbEcommerceContext db;
		private readonly IMapper _mapper;


		public KhachHangController(DbEcommerceContext context, IMapper mapper)
		{
			db = context;
			_mapper = mapper;
		}

		#region Register
		[HttpGet]
		public IActionResult DangKy()
		{
			return View();
		}

		[HttpPost]
		public IActionResult DangKy(RegisterVM model, IFormFile Hinh)
		{
			if (ModelState.IsValid)
			{
				try
				{
					var khachHang = _mapper.Map<KhachHang>(model);
					khachHang.RandomKey = MyUtil.GenerateRamdomKey();
					khachHang.MatKhau = model.MatKhau;
					khachHang.HieuLuc = true;//sẽ xử lý khi dùng Mail để active
					khachHang.VaiTro = 0;

					if (Hinh != null)
					{
						khachHang.Hinh = MyUtil.UploadHinh(Hinh, "KhachHang");
					}

					db.Add(khachHang);
					db.SaveChanges();
					return RedirectToAction("Index", "HangHoa");
				}
				catch (Exception ex)
				{
					var mess = $"{ex.Message} shh";
				}
			}
			return View();
		}
		#endregion


		#region Login
		[HttpGet]
		public IActionResult DangNhap(string? ReturnUrl)
		{
			ViewBag.ReturnUrl = ReturnUrl;
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> DangNhap(LoginVM model, string? ReturnUrl = "/KhachHang/Profile")
		{
			ViewBag.ReturnUrl = ReturnUrl;
			if (ModelState.IsValid)
			{
				var khachHang = db.KhachHangs.SingleOrDefault(kh => kh.MaKh == model.UserName);
				if (khachHang == null)
				{
					ModelState.AddModelError("loi", "Không có khách hàng này");
				}
				else
				{
					if (!khachHang.HieuLuc)
					{
						ModelState.AddModelError("loi", "Tài khoản đã bị khóa. Vui lòng liên hệ Admin.");
					}
					else
					{
						if (khachHang.MatKhau != model.Password)
						{
							ModelState.AddModelError("loi", "Sai thông tin đăng nhập");
						}
						else
						{
							var role = khachHang.VaiTro == 2 ? "Admin" : "Customer";
							var claims = new List<Claim> {
								new Claim(ClaimTypes.Email, khachHang.Email),
								new Claim(ClaimTypes.Name, khachHang.HoTen),
								new Claim("CustomerID", khachHang.MaKh),

								//claim - role động
								new Claim(ClaimTypes.Role, role)
							};

							var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
							var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

							await HttpContext.SignInAsync(claimsPrincipal);

							if (khachHang.VaiTro == 2)
							{
								return Redirect("/Admin");
							}

							if (Url.IsLocalUrl(ReturnUrl))
							{
								return Redirect(ReturnUrl);
							}
							else
							{
								return Redirect("/");
							}
						}
					}
				}
			}
			return View();
		}
		#endregion

		[Authorize]
		public IActionResult Profile()
		{
			var userId = User.FindFirst("CustomerID")?.Value;

			var khachHang = db.KhachHangs.SingleOrDefault(kh => kh.MaKh == userId);

			var hoaDons = db.HoaDons
				.Where(hd => hd.MaKh == userId)
				.Select(hd => new HoaDonVM
				{
					MaHoaDon = hd.MaHd,
					NgayDat = hd.NgayDat,
					MaTrangThai = hd.MaTrangThai,
					TenTrangThai = db.TrangThais
						.Where(tt => tt.MaTrangThai == hd.MaTrangThai)
						.Select(tt => tt.TenTrangThai)
						.FirstOrDefault(),
					ChiTietHoaDons = db.ChiTietHds
						.Where(ct => ct.MaHd == hd.MaHd)
						.Join(db.HangHoas,
							ct => ct.MaHh,
							hh => hh.MaHh,
							(ct, hh) => new ChiTietHoaDonVM
							{
								MaSanPham = ct.MaHh,
								TenSanPham = hh.TenHh,
								SoLuong = ct.SoLuong,
								DonGia = ct.DonGia,
								TongTien = ct.SoLuong * ct.DonGia
							}).ToList()
				}).ToList();

			var result = new KhachHangVM
			{
				MaKh = khachHang.MaKh,
				HoTen = khachHang.HoTen,
				DiaChi = khachHang.DiaChi,
				DienThoai = khachHang.DienThoai,
				NgaySinh = khachHang.NgaySinh,
				GioiTinh = khachHang.GioiTinh,
				Email = khachHang.Email,
				Hinh = khachHang.Hinh,
				HieuLuc = khachHang.HieuLuc,
				VaiTro = khachHang.VaiTro,
				RandomKey = khachHang.RandomKey,
				HoaDons = hoaDons
			};

			return View(result);
		}


		[Authorize]
		public async Task<IActionResult> DangXuat()
		{
			await HttpContext.SignOutAsync();
			return Redirect("/");
		}

		[HttpGet]
		public IActionResult UpdateKH(string id)
		{
			var customer = db.KhachHangs.Find(id);
			if (customer == null)
			{
				return NotFound();
			}

			var viewModel = new KhachHangVM
			{
				MaKh = customer.MaKh,
				HoTen = customer.HoTen,
				DiaChi = customer.DiaChi ?? "",
				DienThoai = customer.DienThoai ?? "",
				NgaySinh = customer.NgaySinh,
				GioiTinh = customer.GioiTinh,
				Email = customer.Email,
				Hinh = customer.Hinh ?? "",
			};

			return View(viewModel);
		}


		[HttpPost]
		public async Task<IActionResult> UpdateKH(KhachHangVM model, IFormFile Hinh)
		{

			var khachHang = await db.KhachHangs.FindAsync(model.MaKh);
			if (khachHang == null)
			{
				return NotFound();
			}

			khachHang.HoTen = model.HoTen;
			khachHang.DiaChi = model.DiaChi;
			khachHang.DienThoai = model.DienThoai;
			khachHang.NgaySinh = model.NgaySinh;
			khachHang.GioiTinh = model.GioiTinh;
			khachHang.Email = model.Email;
			if (Hinh != null)
			{
				if (!string.IsNullOrEmpty(khachHang.Hinh))
				{
					var oldImagePath = Path.Combine("wwwroot", "Hinh", "KhachHang", khachHang.Hinh);
					if (System.IO.File.Exists(oldImagePath))
					{
						System.IO.File.Delete(oldImagePath);
					}
				}
				khachHang.Hinh = MyUtil.UploadHinh(Hinh, "KhachHang");
			}

			db.KhachHangs.Update(khachHang);
			await db.SaveChangesAsync();

			return RedirectToAction("Profile");
		}

		[HttpPost]
		[Authorize]
		public IActionResult CancelOrder(int orderId)
		{
			var userId = User.FindFirst("CustomerID")?.Value;

			var order = db.HoaDons
				.Include(o => o.ChiTietHds)
				.SingleOrDefault(o => o.MaHd == orderId && o.MaKh == userId);

			if (order == null)
			{
				return NotFound();
			}

			// Update order status to "Cancelled"
			order.MaTrangThai = -1; // Replace with the actual status ID for "Cancelled"
			db.SaveChanges();

			return RedirectToAction("Profile");
		}
	}

}