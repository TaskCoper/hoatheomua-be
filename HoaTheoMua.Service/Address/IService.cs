namespace HoaTheoMua.Service.Address;

public interface IService
{
    // Danh sách địa chỉ của user hiện tại (mặc định lên đầu) — dùng để chọn khi đặt hàng.
    Task<List<Response.AddressResponse>> GetMyAddresses();

    Task<Response.AddressResponse> GetAddressById(Guid id);

    Task<string> CreateAddress(Request.CreateAddressRequest request);

    Task<string> UpdateAddress(Guid id, Request.UpdateAddressRequest request);

    Task<string> DeleteAddress(Guid id);

    // Đặt một địa chỉ làm mặc định (tự bỏ mặc định các địa chỉ còn lại).
    Task<string> SetDefaultAddress(Guid id);
}
