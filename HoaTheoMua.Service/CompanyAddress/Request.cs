using System.ComponentModel.DataAnnotations;

namespace HoaTheoMua.Service.CompanyAddress;

public class Request
{
    public class CreateCompanyAddressRequest
    {
        [Required(ErrorMessage = "Địa chỉ không được để trống")]
        [StringLength(255, MinimumLength = 5, ErrorMessage = "Địa chỉ phải từ 5 đến 255 ký tự")]
        public required string AddressLine { get; set; }

        [StringLength(100, ErrorMessage = "Phường/Xã tối đa 100 ký tự")]
        public string? Ward { get; set; }

        [Required(ErrorMessage = "Tỉnh/Thành phố không được để trống")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Tỉnh/Thành phố phải từ 2 đến 100 ký tự")]
        public required string City { get; set; }

        [Required(ErrorMessage = "Quốc gia không được để trống")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Quốc gia phải từ 2 đến 100 ký tự")]
        public required string Country { get; set; }

        [Range(typeof(decimal), "-90", "90", ErrorMessage = "Vĩ độ phải nằm trong khoảng -90 đến 90")]
        public decimal? Latitude { get; set; }

        [Range(typeof(decimal), "-180", "180", ErrorMessage = "Kinh độ phải nằm trong khoảng -180 đến 180")]
        public decimal? Longitude { get; set; }
        
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ và tên phải từ 2 đến 100 ký tự")]
        public required string FullName { get; set; }
        public required string Phone { get; set; }
        public string? Remarks { get; set; } = null!;
        public required string MapId { get; set; }
    }

    public class UpdateCompanyAddressRequest
    {
        [Required(ErrorMessage = "Địa chỉ không được để trống")]
        [StringLength(255, MinimumLength = 5, ErrorMessage = "Địa chỉ phải từ 5 đến 255 ký tự")]
        public required string AddressLine { get; set; }

        [StringLength(100, ErrorMessage = "Phường/Xã tối đa 100 ký tự")]
        public required string Ward { get; set; }

        [Required(ErrorMessage = "Tỉnh/Thành phố không được để trống")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Tỉnh/Thành phố phải từ 2 đến 100 ký tự")]
        public required string City { get; set; }

        [Required(ErrorMessage = "Quốc gia không được để trống")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Quốc gia phải từ 2 đến 100 ký tự")]
        public required string Country { get; set; }

        [Range(typeof(decimal), "-90", "90", ErrorMessage = "Vĩ độ phải nằm trong khoảng -90 đến 90")]
        public decimal? Latitude { get; set; }

        [Range(typeof(decimal), "-180", "180", ErrorMessage = "Kinh độ phải nằm trong khoảng -180 đến 180")]
        public decimal? Longitude { get; set; }
        public required string FullName { get; set; }
        public required string Phone { get; set; }
        public string? Remarks { get; set; }
        public required string MapId { get; set; }
    }
}