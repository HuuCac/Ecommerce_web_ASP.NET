namespace EcommerceWebsite.ViewModels
{
    public class HangHoaVM
    {
        public int MaHh { get; set; }

        public string TenHh { get; set; } = null!;

        public int MaLoai { get; set; }
        public string TenLoai { get; set; }

        public string? MoTaDonVi { get; set; }

        public double? DonGia { get; set; }

        public string? Hinh { get; set; }

        public int SoLuong { get; set; }

        public string? MoTa { get; set; }

        public List<MenuLoaiVM> Categories { get; set; } // List of categories

        public HangHoaVM()
        {
            Categories = new List<MenuLoaiVM>();
        }
    }
}
