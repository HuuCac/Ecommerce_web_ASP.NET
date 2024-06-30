namespace EcommerceWebsite.ViewModels
{
    public class ChiTietHoaDonVM
    {
        public int MaSanPham { get; set; }
        public string TenSanPham { get; set; }
        public int SoLuong { get; set; }
        public double DonGia { get; set; }
        public double TongTien { get; set; }
    }

    public class HoaDonVM
    {
        public int MaHoaDon { get; set; }
        public int MaTrangThai { get; set; }
        public string TenTrangThai { get; set; }

        public DateTime NgayDat { get; set; }
        public List<ChiTietHoaDonVM> ChiTietHoaDons { get; set; }
    }

    public class TrangThaiVM
    {
        public int MaTrangThai { get; set; }
        public string TenTrangThai { get; set; }
    }

    public class KhachHangVM
    {
        public string MaKh { get; set; } = null!;
        public string HoTen { get; set; } = null!;
        public string? DiaChi { get; set; }
        public string? DienThoai { get; set; }
        public DateTime NgaySinh { get; set; }
        public bool GioiTinh { get; set; }
        public string Email { get; set; } = null!;
        public string? Hinh { get; set; }
        public bool HieuLuc { get; set; }
        public int VaiTro { get; set; }
        public string? RandomKey { get; set; }
        public List<HoaDonVM> HoaDons { get; set; } = new List<HoaDonVM>();
    }
}
