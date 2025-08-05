
namespace backend.Models
{

    public class PaymentStatusResponse
    {
        public long Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusDetail { get; set; } = string.Empty;
        public DateTime? DateApproved { get; set; }
        public PaymentPointOfInteraction? PointOfInteraction { get; set; }
        public PaymentTransactionDetails? TransactionDetails { get; set; }
        public string? NotificationUrl { get; set; }
        public string? CallbackUrl { get; set; }
    }



    public class PaymentPointOfInteraction
    {
        public string? Type { get; set; }
        public PaymentTransactionData? TransactionData { get; set; }
    }

    public class PaymentTransactionData
    {
        public string? QrCode { get; set; }
        public string? QrCodeBase64 { get; set; }
        public string? TicketUrl { get; set; }
        public string? BankTransferId { get; set; }
        public string? FinancialInstitution { get; set; }
    }

    public class PaymentTransactionDetails
    {
        public string? ExternalResourceUrl { get; set; }
        public decimal TotalPaidAmount { get; set; }
        public decimal InstallmentAmount { get; set; }
        public string? FinancialInstitution { get; set; }
        public string? PaymentMethodReferenceId { get; set; }
    }

}

