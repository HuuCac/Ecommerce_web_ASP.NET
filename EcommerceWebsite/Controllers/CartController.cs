using EcommerceWebsite.Data;
using EcommerceWebsite.Helpers;
using EcommerceWebsite.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceWebsite.Controllers
{
	public class CartController : Controller
	{
		private readonly DbEcommerceContext db;

		public CartController(DbEcommerceContext context)
		{
			db = context;
		}

		const string CART_KEY = "MYCART";
		public List<CartItem> Cart => HttpContext.Session.Get<List<CartItem>>(CART_KEY) ?? new List<CartItem>();

		public IActionResult Index()
		{
			return View(Cart);
		}

		public IActionResult AddToCart(int id, int quantity = 1)
		{
			var gioHang = Cart;
			var item = gioHang.SingleOrDefault(p => p.MaHh == id);
			if (item == null)
			{
				var hangHoa = db.HangHoas.SingleOrDefault(p => p.MaHh == id);
				if (hangHoa == null)
				{
					TempData["Message"] = $"Không tìm thấy hàng hóa có mã {id}";
					return Redirect("/404");
				}
				item = new CartItem
				{
					MaHh = hangHoa.MaHh,
					TenHH = hangHoa.TenHh,
					DonGia = hangHoa.DonGia ?? 0,
					Hinh = hangHoa.Hinh ?? string.Empty,
					SoLuong = quantity
				};
				gioHang.Add(item);
			}
			else
			{
				item.SoLuong += quantity;
			}

			HttpContext.Session.Set(CART_KEY, gioHang);

			return RedirectToAction("Index");
		}

		public IActionResult RemoveCart(int id)
		{
			var gioHang = Cart;
			var item = gioHang.SingleOrDefault(p => p.MaHh == id);
			if (item != null)
			{
				gioHang.Remove(item);
				HttpContext.Session.Set(CART_KEY, gioHang);
			}
			return RedirectToAction("Index");
		}

		public IActionResult IncreaseQuantity(int id)
		{
			var gioHang = Cart;
			var item = gioHang.SingleOrDefault(p => p.MaHh == id);

			if (item != null)
			{
				var hangHoa = db.HangHoas.SingleOrDefault(p => p.MaHh == id);

				if (hangHoa == null)
				{
					TempData["Message"] = $"Không tìm thấy hàng hóa có mã {id}";
					return RedirectToAction("Index");
				}

				if (item.SoLuong + 1 > hangHoa.SoLuong)
				{
					TempData["Message"] = $"Số lượng yêu cầu vượt quá số lượng tồn kho hiện có ({hangHoa.SoLuong})";
					return RedirectToAction("Index");
				}

				item.SoLuong += 1;
				HttpContext.Session.Set(CART_KEY, gioHang);
			}

			return RedirectToAction("Index");
		}


		// New method to decrease the quantity of a product in the cart
		public IActionResult DecreaseQuantity(int id)
		{
			var gioHang = Cart;
			var item = gioHang.SingleOrDefault(p => p.MaHh == id);
			if (item != null)
			{
				item.SoLuong -= 1;
				if (item.SoLuong <= 0)
				{
					gioHang.Remove(item);
				}
				HttpContext.Session.Set(CART_KEY, gioHang);
			}
			return RedirectToAction("Index");
		}

		[Authorize]
		[HttpGet]
		public IActionResult Checkout()
		{
			if (Cart.Count == 0)
			{
				return Redirect("/");
			}

			return View(Cart);
		}

		[Authorize]
		[HttpPost]
		public IActionResult Checkout(CheckoutVM model)
		{
			if (ModelState.IsValid)
			{
				var customerId = HttpContext.User.Claims.SingleOrDefault(p => p.Type == MySetting.CLAIM_CUSTOMERID).Value;
				var khachHang = new KhachHang();
				if (model.GiongKhachHang)
				{
					khachHang = db.KhachHangs.SingleOrDefault(kh => kh.MaKh == customerId);
				}

				var hoadon = new HoaDon
				{
					MaKh = customerId,
					HoTen = model.HoTen ?? khachHang.HoTen,
					DiaChi = model.DiaChi ?? khachHang.DiaChi,
					DienThoai = model.DienThoai ?? khachHang.DienThoai,
					NgayDat = DateTime.Now,
					CachThanhToan = "COD",
					CachVanChuyen = "GRAB",
					MaTrangThai = 0,
					GhiChu = model.GhiChu
				};

				using (var transaction = db.Database.BeginTransaction())
				{
					try
					{
						db.Add(hoadon);
						db.SaveChanges();

						var cthds = new List<ChiTietHd>();
						foreach (var item in Cart)
						{
							var hangHoa = db.HangHoas.SingleOrDefault(hh => hh.MaHh == item.MaHh);
							if (hangHoa != null)
							{
								hangHoa.SoLuong -= item.SoLuong;
								cthds.Add(new ChiTietHd
								{
									MaHd = hoadon.MaHd,
									SoLuong = item.SoLuong,
									DonGia = item.DonGia,
									MaHh = item.MaHh,
									GiamGia = 0
								});
							}
							else
							{
								throw new Exception("Product not found or out of stock");
							}
						}

						db.AddRange(cthds);
						db.SaveChanges();

						transaction.Commit();

						HttpContext.Session.Set<List<CartItem>>(MySetting.CART_KEY, new List<CartItem>());

						return View("Success");
					}
					catch (Exception ex)
					{
						transaction.Rollback();
						// Log the error (uncomment ex variable name and write a log.)
						ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
					}
				}
			}
			return View(Cart);
		}

	}
}
