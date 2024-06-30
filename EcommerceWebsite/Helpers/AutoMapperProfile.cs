using AutoMapper;
using EcommerceWebsite.Data;
using EcommerceWebsite.ViewModels;

namespace EcommerceWebsite.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<RegisterVM, KhachHang>();
            //.ForMember(kh => kh.HoTen, option => option.MapFrom(RegisterVM => RegisterVM.HoTen))
            //.ReverseMap();
            CreateMap<HangHoaVM, HangHoa>()
            .ForMember(dest => dest.TenHh, opt => opt.MapFrom(src => src.TenHh))
            .ForMember(dest => dest.MoTaDonVi, opt => opt.MapFrom(src => src.MoTaDonVi))
            .ForMember(dest => dest.Hinh, opt => opt.MapFrom(src => src.Hinh))
            .ForMember(dest => dest.DonGia, opt => opt.MapFrom(src => src.DonGia))
            .ForMember(dest => dest.MoTa, opt => opt.MapFrom(src => src.MoTa))
            .ForMember(dest => dest.MaLoai, opt => opt.MapFrom(src => src.MaLoai))
            .ForMember(dest => dest.SoLuong, opt => opt.MapFrom(src => src.SoLuong))
            .ReverseMap();

        }
    }
}
