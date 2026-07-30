using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;

namespace HoaTheoMua.Service.Test.TestSupport;

// Tạo IMapper thật (không mock) từ các Profile của Service layer, để test chạy đúng
// logic map thật sự. Dùng Mock<IMapper> sẽ khiến Map() không làm gì và test pass giả.
public static class MapperFactory
{
    private static readonly Lazy<IMapper> Instance = new(() =>
        new MapperConfiguration(
            cfg => cfg.AddProfile<HoaTheoMua.Service.Product.ProductMapping>(),
            NullLoggerFactory.Instance).CreateMapper());

    public static IMapper Create() => Instance.Value;
}
