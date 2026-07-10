using FluentValidation;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.AhaMoveService.Validate;
using HoaTheoMua.Service.Common;

namespace HoaTheoMua.Service.Order.Validate;

public class CreateOrderRequestValidator: AbstractValidator<Request.CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        Include(new EstimateShippingFeeRequestValidator());
        
        RuleFor(x => x.TagIds)
            .NotEmpty().WithMessage("TAG_IDS_REQUIRED");

        RuleFor(x => x.TaxCode).MaximumLength(20).WithMessage("TAX_CODE_LENGTH_INVALID");
        RuleFor(x => x.IdentityNumber).MaximumLength(20).WithMessage("IDENTITY_NUMBER_LENGTH_INVALID");
        RuleFor(x => x.ReceiptFullName).MaximumLength(100).WithMessage("RECEIPT_FULL_NAME_LENGTH_INVALID");
        RuleFor(x => x.ReceiptAddress).MaximumLength(255).WithMessage("RECEIPT_ADDRESS_LENGTH_INVALID");
        RuleFor(x => x.ReceiptPhoneNumber).OptionalPhoneNumberVn();
        RuleFor(x => x.ReceiptEmail).OptionalEmail();
        
        RuleFor(x => x.IsExportInvoice).NotEmpty().WithMessage("IS_EXPORT_FORCE");
        
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
    
}
