using FluentValidation;
using HoaTheoMua.Repository.Enum;

namespace HoaTheoMua.Service.Order.Validate;

public class CreateOrderRequestValidator: AbstractValidator<Request.CreateOrderRequest>  
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.OrderTime)
            .Must(BeValidOrderTime)
            .WithMessage("OrderTime must be 0 or within the next 7 days.");

        When(x => x.IsExportInvoice == true, () =>
        {
            RuleFor(x => x.InvoiceType)
                .NotNull()
                .WithMessage("InvoiceType is required when exporting invoice.");

            // Doanh nghiệp: TaxCode, ReceiptEmail, ReceiptPhoneNumber
            When(x => x.InvoiceType == InvoiceType.Business, () =>
            {
                RuleFor(x => x.TaxCode)
                    .NotEmpty()
                    .WithMessage("TaxCode is required for business invoice.");

                RuleFor(x => x.ReceiptEmail)
                    .NotEmpty()
                    .WithMessage("ReceiptEmail is required for business invoice.");

                RuleFor(x => x.ReceiptPhoneNumber)
                    .NotEmpty()
                    .WithMessage("ReceiptPhoneNumber is required for business invoice.");
            });

            // Cá nhân: IdentityNumber, ReceiptFullName, ReceiptAddress, ReceiptEmail, ReceiptPhoneNumber
            When(x => x.InvoiceType == InvoiceType.Personal, () =>
            {
                RuleFor(x => x.IdentityNumber)
                    .NotEmpty()
                    .WithMessage("IdentityNumber is required for personal invoice.");

                RuleFor(x => x.ReceiptFullName)
                    .NotEmpty()
                    .WithMessage("ReceiptFullName is required for personal invoice.");

                RuleFor(x => x.ReceiptAddress)
                    .NotEmpty()
                    .WithMessage("ReceiptAddress is required for personal invoice.");

                RuleFor(x => x.ReceiptEmail)
                    .NotEmpty()
                    .WithMessage("ReceiptEmail is required for personal invoice.");

                RuleFor(x => x.ReceiptPhoneNumber)
                    .NotEmpty()
                    .WithMessage("ReceiptPhoneNumber is required for personal invoice.");
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