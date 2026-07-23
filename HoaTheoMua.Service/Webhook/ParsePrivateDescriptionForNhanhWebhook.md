# Parse `privateDescription` ngắn gọn

## Ý chính

- `description` => giữ nguyên map vào `Remarks`
- `privateDescription` => bóc các field xuất hoá đơn
- hard code theo prefix:
  - `MST:` => doanh nghiệp
  - `Tên:` / `CCCD:` / `ĐC:` => cá nhân
  - `Email:`
  - `SĐT:`

## Format đang được đẩy lên Nhanh

### Doanh nghiệp
```text
...
MST:0123456789
Email:a@gmail.com
SĐT:0909123456
```

### Cá nhân
```text
...
Tên:Nguyễn Văn A
CCCD:012345678999
ĐC:123 ABC
Email:a@gmail.com
SĐT:0909123456
```

## Code hard code ngắn

```csharp
using HoaTheoMua.Repository.Enum;

var privateDescription = item.Info.PrivateDescription ?? string.Empty;

string? taxCode = null;
string? identityNumber = null;
string? receiptFullName = null;
string? receiptPhoneNumber = null;
string? receiptAddress = null;
string? receiptEmail = null;
bool? isExportInvoice = false;
InvoiceType? invoiceType = null;

var lines = privateDescription
    .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

foreach (var line in lines)
{
    if (line.StartsWith("MST:"))
    {
        isExportInvoice = true;
        invoiceType = InvoiceType.Business;
        taxCode = line.Replace("MST:", "").Trim();
    }
    else if (line.StartsWith("Tên:"))
    {
        isExportInvoice = true;
        invoiceType = InvoiceType.Personal;
        receiptFullName = line.Replace("Tên:", "").Trim();
    }
    else if (line.StartsWith("CCCD:"))
    {
        isExportInvoice = true;
        invoiceType = InvoiceType.Personal;
        identityNumber = line.Replace("CCCD:", "").Trim();
    }
    else if (line.StartsWith("ĐC:"))
    {
        isExportInvoice = true;
        invoiceType = InvoiceType.Personal;
        receiptAddress = line.Replace("ĐC:", "").Trim();
    }
    else if (line.StartsWith("Email:"))
    {
        isExportInvoice = true;
        receiptEmail = line.Replace("Email:", "").Trim();
    }
    else if (line.StartsWith("SĐT:"))
    {
        isExportInvoice = true;
        receiptPhoneNumber = line.Replace("SĐT:", "").Trim();
    }
}
```

## Gắn vào `WebhookNhanhVnOrderAdd`

Trong `order = new ...` thì map:

```csharp
TaxCode = taxCode,
IdentityNumber = identityNumber,
ReceiptFullName = receiptFullName,
ReceiptPhoneNumber = receiptPhoneNumber,
ReceiptAddress = receiptAddress,
ReceiptEmail = receiptEmail,
IsExportInvoice = isExportInvoice,
InvoiceType = invoiceType,
```
