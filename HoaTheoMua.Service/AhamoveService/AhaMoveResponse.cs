namespace HoaTheoMua.Service.AhamoveService;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class ServiceResponse
{
    [JsonPropertyName("service_id")]
    public string ServiceId { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public ServiceData Data { get; set; } = new();
}

public class ServiceData
{
    [JsonPropertyName("distance")]
    public double Distance { get; set; }

    [JsonPropertyName("duration")]
    public int Duration { get; set; }

    [JsonPropertyName("distance_fee")]
    public decimal DistanceFee { get; set; }

    [JsonPropertyName("request_fee")]
    public decimal RequestFee { get; set; }

    [JsonPropertyName("stop_fee")]
    public decimal StopFee { get; set; }

    [JsonPropertyName("vat_fee")]
    public decimal VatFee { get; set; }

    [JsonPropertyName("discount")]
    public decimal Discount { get; set; }

    [JsonPropertyName("total_fee")]
    public decimal TotalFee { get; set; }

    [JsonPropertyName("total_price")]
    public decimal TotalPrice { get; set; }

    // Initialized as an empty list to avoid null reference issues on services without requests
    [JsonPropertyName("requests")]
    public List<ServiceRequest> Requests { get; set; } = new();
}

public class ServiceRequest
{
    [JsonPropertyName("_id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("num")]
    public int Num { get; set; }

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("tier_code")]
    public string? TierCode { get; set; }
}