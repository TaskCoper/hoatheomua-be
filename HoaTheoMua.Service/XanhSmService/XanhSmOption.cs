using System.ComponentModel.DataAnnotations;

namespace HoaTheoMua.Service.XanhSmService;

/// <summary>Cấu hình API đối tác Xanh SM; secret được cấp qua biến môi trường khi deploy.</summary>
public sealed class XanhSmOption
{
    [Required] public string BaseUrl { get; set; } = string.Empty;
    [Required, Url] public string TokenUrl { get; set; } = string.Empty;
    [Required] public string ClientId { get; set; } = string.Empty;
    [Required] public string ClientSecret { get; set; } = string.Empty;
    [Required] public string Scope { get; set; } = string.Empty;
    [Required] public string GrantType { get; set; } = "client_credentials";
    public string AcceptLanguage { get; set; } = "vi";
    public string AvailableServicesPath { get; set; } = "/list-available-services";
    public string EstimateOrderPath { get; set; } = "/estimate/order";
    public string CreateOrderPath { get; set; } = "/create-order";
    public string CancelOrderPath { get; set; } = "/cancel-order";
    [Range(0, 3600)] public int TokenRefreshBufferSeconds { get; set; } = 300;
    [Range(1, 120)] public int TimeoutSeconds { get; set; } = 30;
}
