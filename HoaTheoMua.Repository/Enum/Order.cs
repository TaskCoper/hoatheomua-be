namespace HoaTheoMua.Repository.Enum;

public enum PaymentMethod
{
    /// <summary>Virtual Account</summary>
    VA = 1,

    /// <summary>Buy Now Pay Later</summary>
    BNPL = 2,

    /// <summary>Thẻ tín dụng</summary>
    CreditCard = 3,

    /// <summary>Trả góp thẻ</summary>
    CardInstallment = 4,

    /// <summary>ATM (thẻ nội địa)</summary>
    ATM = 5,

    /// <summary>VNPay QR</summary>
    VNPayQR = 6
}


public enum PaymentGateway
{
    BaoKim = 1,
    Sepay = 2
}

public enum ShippingGateway
{
    Ahamove = 1,
    XanhSm = 2
}
