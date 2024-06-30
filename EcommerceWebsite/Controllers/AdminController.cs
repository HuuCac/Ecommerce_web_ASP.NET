using AutoMapper;
using EcommerceWebsite.Data;
using EcommerceWebsite.Helpers;
using EcommerceWebsite.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcommerceWebsite.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly DbEcommerceContext db;
        private readonly IMapper _mapper;


        public AdminController(DbEcommerceContext context)
        {
            db = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult GetCustomers(int currentPage = 1)
        {
            var customers = db.KhachHangs.AsQueryable();

            var pageSize = 10; // Set the page size
            var total = customers.Count();
            var totalPages = (int)Math.Ceiling((double)total / pageSize);

            var result = customers
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new KhachHangVM
                {
                    MaKh = p.MaKh,
                    HoTen = p.HoTen,
                    DiaChi = p.DiaChi,
                    DienThoai = p.DienThoai,
                    Email = p.Email,
                    Hinh = p.Hinh ?? "",
                    HieuLuc = p.HieuLuc
                })
                .ToList();

            var viewModel = new PaginatedList<KhachHangVM>
            {
                Items = result,
                CurrentPage = currentPage,
                TotalPages = totalPages
            };

            return View("Customers", viewModel);
        }


        [HttpPost]
        public async Task<IActionResult> DeleteCustomer(string customerId)
        {
            if (string.IsNullOrEmpty(customerId))
            {
                return BadRequest("Customer ID cannot be null or empty.");
            }

            var khachHang = await db.KhachHangs.FindAsync(customerId);
            if (khachHang == null)
            {
                return NotFound("Customer not found.");
            }

            try
            {
                // Find all related invoices (HoaDon) for the customer
                var invoices = await db.HoaDons.Where(hd => hd.MaKh == customerId).ToListAsync();

                // Delete all related invoice details (ChiTietHD) for each invoice
                foreach (var invoice in invoices)
                {
                    var details = await db.ChiTietHds.Where(ct => ct.MaHd == invoice.MaHd).ToListAsync();
                    db.ChiTietHds.RemoveRange(details);
                }

                // Delete all invoices (HoaDon) for the customer
                db.HoaDons.RemoveRange(invoices);

                // Finally, delete the customer (KhachHang)
                db.KhachHangs.Remove(khachHang);

                // Save changes to the database
                await db.SaveChangesAsync();

                return RedirectToAction("GetCustomers");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        public IActionResult GetProducts(int currentPage = 1)
        {
            var hangHoas = db.HangHoas.AsQueryable();

            var pageSize = 10; // Set the page size
            var total = hangHoas.Count();
            var totalPages = (int)Math.Ceiling((double)total / pageSize);

            var result = hangHoas
                .Select(p => new HangHoaVM
                {
                    MaHh = p.MaHh,
                    TenHh = p.TenHh,
                    DonGia = p.DonGia ?? 0,
                    Hinh = p.Hinh ?? "",
                    MoTa = p.MoTa ?? "",
                    TenLoai = p.MaLoaiNavigation.TenLoai,
                    SoLuong = p.SoLuong,
                })
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var viewModel = new PaginatedList<HangHoaVM>
            {
                Items = result,
                CurrentPage = currentPage,
                TotalPages = totalPages
            };

            return View("Products", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleCustomerStatus(string customerId)
        {
            if (string.IsNullOrEmpty(customerId))
            {
                return BadRequest("Customer ID cannot be null or empty.");
            }

            var khachHang = await db.KhachHangs.FindAsync(customerId);
            if (khachHang == null)
            {
                return NotFound("Customer not found.");
            }

            try
            {
                khachHang.HieuLuc = !khachHang.HieuLuc; // Toggle the status
                db.KhachHangs.Update(khachHang);
                await db.SaveChangesAsync();

                return RedirectToAction("GetCustomers");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProduct(int productId)
        {
            if (productId <= 0)
            {
                return BadRequest("Invalid product ID.");
            }

            var product = await db.HangHoas.FindAsync(productId);
            if (product == null)
            {
                return NotFound("Product not found.");
            }

            try
            {
                // Delete related records in ChiTietHoaDon
                var relatedDetails = db.ChiTietHds.Where(ct => ct.MaHh == productId).ToList();
                db.ChiTietHds.RemoveRange(relatedDetails);

                // Delete the product
                db.HangHoas.Remove(product);
                await db.SaveChangesAsync();

                return RedirectToAction("GetProducts");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet]
        public IActionResult EditProduct(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid product ID.");
            }

            var product = db.HangHoas
                .Where(p => p.MaHh == id)
                .Select(p => new HangHoaVM
                {
                    MaHh = p.MaHh,
                    TenHh = p.TenHh,
                    DonGia = p.DonGia ?? 0,
                    Hinh = p.Hinh ?? "",
                    MoTaDonVi = p.MoTaDonVi ?? "",
                    MoTa = p.MoTa ?? "",
                    SoLuong = p.SoLuong,
                    MaLoai = p.MaLoai,
                    TenLoai = p.MaLoaiNavigation.TenLoai
                })
                .FirstOrDefault();

            if (product == null)
            {
                return NotFound("Product not found.");
            }

            var categories = db.Loais
                .Select(l => new MenuLoaiVM
                {
                    MaLoai = l.MaLoai,
                    TenLoai = l.TenLoai
                })
                .ToList();

            ViewBag.Categories = categories;

            return View(product);
        }

        // Phương thức POST EditProduct để lưu các thay đổi của sản phẩm
        [HttpPost]
        public async Task<IActionResult> EditProduct(HangHoaVM model, IFormFile Hinh)
        {
            var product = await db.HangHoas.FindAsync(model.MaHh);
            if (product == null)
            {
                return NotFound("Product not found.");
            }

            try
            {
                // Update product fields
                product.TenHh = model.TenHh;
                product.DonGia = model.DonGia;
                product.MoTaDonVi = model.MoTaDonVi;
                product.MoTa = model.MoTa;
                product.SoLuong = model.SoLuong;
                product.MaLoai = model.MaLoai;

                // Check if a new image is provided
                if (Hinh != null)
                {
                    // Delete the old image if it exists
                    if (!string.IsNullOrEmpty(product.Hinh))
                    {
                        var oldImagePath = Path.Combine("wwwroot", "Hinh", "HangHoa", product.Hinh);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    // Upload the new image and update the product's image field
                    product.Hinh = MyUtil.UploadHinh(Hinh, "HangHoa");
                }

                db.HangHoas.Update(product);
                await db.SaveChangesAsync();

                return RedirectToAction("GetProducts");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpGet]
        public IActionResult AddProduct()
        {
            var categories = db.Loais
                .Select(l => new MenuLoaiVM
                {
                    MaLoai = l.MaLoai,
                    TenLoai = l.TenLoai
                })
                .ToList();

            ViewData["Categories"] = categories;

            return View();
        }


        // POST: Admin/AddProduct
        [HttpPost]
        public async Task<IActionResult> AddProduct(HangHoaVM model, IFormFile Hinh)
        {
            try
            {
                var product = new HangHoa
                {
                    TenHh = model.TenHh,
                    MoTaDonVi = model.MoTaDonVi,
                    DonGia = model.DonGia,
                    MoTa = model.MoTa,
                    MaLoai = model.MaLoai,
                    SoLuong = model.SoLuong
                };

                // Upload hình ảnh nếu có
                if (Hinh != null && Hinh.Length > 0)
                {
                    product.Hinh = MyUtil.UploadHinh(Hinh, "HangHoa");
                }

                db.HangHoas.Add(product);
                await db.SaveChangesAsync();

                return RedirectToAction("GetProducts", "Admin");
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ và hiển thị thông báo lỗi
                ModelState.AddModelError("", "An error occurred while saving the product. Please try again.");
                var categories = db.Loais.ToList();
                ViewBag.Categories = categories;
                return View(model);
            }
        }

        public IActionResult GetOrders(int currentPage = 1)
        {
            var orders = db.HoaDons.AsQueryable();

            var pageSize = 10; // Set the page size
            var total = orders.Count();
            var totalPages = (int)Math.Ceiling((double)total / pageSize);

            var result = orders
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new HoaDonVM
                {
                    MaHoaDon = o.MaHd,
                    NgayDat = o.NgayDat,
                    MaTrangThai = o.MaTrangThai,
                    TenTrangThai = o.MaTrangThaiNavigation.TenTrangThai,
                    ChiTietHoaDons = o.ChiTietHds.Select(ct => new ChiTietHoaDonVM
                    {
                        MaSanPham = ct.MaHh,
                        TenSanPham = ct.MaHhNavigation.TenHh,
                        SoLuong = ct.SoLuong,
                        DonGia = ct.DonGia,
                        TongTien = ct.SoLuong * ct.DonGia
                    }).ToList()
                })
                .ToList();

            var viewModel = new PaginatedList<HoaDonVM>
            {
                Items = result,
                CurrentPage = currentPage,
                TotalPages = totalPages
            };

            return View("Orders", viewModel);
        }

        public IActionResult EditOrder(int id)
        {
            var order = db.HoaDons
                .Where(o => o.MaHd == id)
                .Select(o => new HoaDonVM
                {
                    MaHoaDon = o.MaHd,
                    NgayDat = o.NgayDat,
                    MaTrangThai = o.MaTrangThai,
                    TenTrangThai = o.MaTrangThaiNavigation.TenTrangThai,
                    ChiTietHoaDons = o.ChiTietHds.Select(ct => new ChiTietHoaDonVM
                    {
                        MaSanPham = ct.MaHh,
                        TenSanPham = ct.MaHhNavigation.TenHh,
                        SoLuong = ct.SoLuong,
                        DonGia = ct.DonGia,
                        TongTien = ct.SoLuong * (ct.DonGia)
                    }).ToList()
                })
                .FirstOrDefault();

            if (order == null)
            {
                return NotFound("Order not found.");
            }

            var statuses = db.TrangThais
                .Select(tt => new TrangThaiVM
                {
                    MaTrangThai = tt.MaTrangThai,
                    TenTrangThai = tt.TenTrangThai
                })
                .ToList();

            ViewBag.Statuses = statuses;

            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> EditOrder(HoaDonVM model)
        {
            var order = await db.HoaDons.FindAsync(model.MaHoaDon);
            if (order == null)
            {
                return NotFound("Order not found.");
            }

            order.MaTrangThai = model.MaTrangThai;
            db.HoaDons.Update(order);
            await db.SaveChangesAsync();

            return RedirectToAction("GetOrders");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteOrder(int orderId)
        {
            if (orderId <= 0)
            {
                return BadRequest("Invalid order ID.");
            }

            var order = await db.HoaDons.FindAsync(orderId);
            if (order == null)
            {
                return NotFound("Order not found.");
            }
            try
            {
                // Delete all related order details (ChiTietHds)
                var details = await db.ChiTietHds.Where(ct => ct.MaHd == orderId).ToListAsync();
                db.ChiTietHds.RemoveRange(details);

                // Delete the order itself
                db.HoaDons.Remove(order);

                // Save changes to the database
                await db.SaveChangesAsync();

                return RedirectToAction("GetOrders");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

    }
}
