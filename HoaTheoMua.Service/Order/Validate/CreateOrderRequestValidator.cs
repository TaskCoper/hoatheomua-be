using FluentValidation;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Common;

namespace HoaTheoMua.Service.Order.Validate;

public class CreateOrderRequestValidator: AbstractValidator<Request.CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        // ----- Estimate fields (§5.1) -----
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("FULL_NAME_REQUIRED")
            .Length(1, 100).WithMessage("FULL_NAME_LENGTH_INVALID");

        RuleFor(x => x.Phone).PhoneNumberVn();

        RuleFor(x => x.Remarks)
            .MaximumLength(500).WithMessage("REMARKS_LENGTH_INVALID");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("ADDRESS_REQUIRED")
            .Length(1, 255).WithMessage("ADDRESS_LENGTH_INVALID");
        
        RuleFor(x => x.ShortAddress)
            .NotEmpty().WithMessage("ADDRESS_REQUIRED")
            .Length(1, 255).WithMessage("ADDRESS_LENGTH_INVALID");


        RuleFor(x => x.Latitude)
            .NotEmpty().WithMessage("LATITUDE_REQUIRED");

        RuleFor(x => x.Longitude)
            .NotEmpty().WithMessage("LONGITUDE_REQUIRED");
        
        
        // cityCode / wardCode: -1 là sentinel "chưa chọn" ở FE.
        RuleFor(x => x.City).GreaterThan(0).WithMessage("CITY_CODE_REQUIRED");
        RuleFor(x => x.Ward).GreaterThan(0).WithMessage("WARD_CODE_REQUIRED");
        
        RuleFor(x => x.OrderTime)
            .Must(BeValidOrderTime)
            .WithMessage("ORDER_TIME_INVALID");
        
        RuleFor(x => x.TaxCode).MaximumLength(20).WithMessage("TAX_CODE_LENGTH_INVALID");
        RuleFor(x => x.IdentityNumber).MaximumLength(20).WithMessage("IDENTITY_NUMBER_LENGTH_INVALID");
        RuleFor(x => x.ReceiptFullName).MaximumLength(100).WithMessage("RECEIPT_FULL_NAME_LENGTH_INVALID");
        RuleFor(x => x.ReceiptAddress).MaximumLength(255).WithMessage("RECEIPT_ADDRESS_LENGTH_INVALID");
        RuleFor(x => x.ReceiptPhoneNumber).OptionalPhoneNumberVn();
        RuleFor(x => x.ReceiptEmail).OptionalEmail();

        // Cross-field: chỉ áp dụng khi xuất hóa đơn.
        When(x => x.IsExportInvoice == true, () =>
        {
            RuleFor(x => x.InvoiceType)
                .NotNull()
                .WithMessage("INVOICE_TYPE_REQUIRED");

            // Doanh nghiệp: TaxCode, ReceiptEmail, ReceiptPhoneNumber
            When(x => x.InvoiceType == InvoiceType.Business, () =>
            {
                RuleFor(x => x.TaxCode)
                    .NotEmpty()
                    .WithMessage("TAX_CODE_REQUIRED");

                RuleFor(x => x.ReceiptEmail)
                    .NotEmpty()
                    .WithMessage("RECEIPT_EMAIL_REQUIRED");

                RuleFor(x => x.ReceiptPhoneNumber)
                    .NotEmpty()
                    .WithMessage("RECEIPT_PHONE_NUMBER_REQUIRED");
            });

            // Cá nhân: IdentityNumber, ReceiptFullName, ReceiptAddress, ReceiptEmail, ReceiptPhoneNumber
            When(x => x.InvoiceType == InvoiceType.Personal, () =>
            {
                RuleFor(x => x.IdentityNumber)
                    .NotEmpty()
                    .WithMessage("IDENTITY_NUMBER_REQUIRED");

                RuleFor(x => x.ReceiptFullName)
                    .NotEmpty()
                    .WithMessage("RECEIPT_FULL_NAME_REQUIRED");

                RuleFor(x => x.ReceiptAddress)
                    .NotEmpty()
                    .WithMessage("RECEIPT_ADDRESS_REQUIRED");

                RuleFor(x => x.ReceiptEmail)
                    .NotEmpty()
                    .WithMessage("RECEIPT_EMAIL_REQUIRED");

                RuleFor(x => x.ReceiptPhoneNumber)
                    .NotEmpty()
                    .WithMessage("RECEIPT_PHONE_NUMBER_REQUIRED");
            });
        });
    }

    private bool BeValidOrderTime(long orderTime)
    {
        if (orderTime == 0)
            return true;

        var orderDate = DateTimeOffset
            .FromUnixTimeSeconds(orderTime)
            .UtcDateTime;

        var now = DateTime.UtcNow;

        return orderDate >= now &&
               orderDate <= now.AddDays(7);
    }
}
