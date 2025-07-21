namespace backend.DTOs;

public class PixPaymentRequest
{
    public double Transaction_amount { get; set; }
    public string Description { get; set; }
    public string Email { get; set; }
    public string IdentificationType { get; set; }
    public string Number { get; set; }

    public PixPaymentRequest() { }

    public PixPaymentRequest(double transaction_amount, string description, string email, string identificationType, string number)
    {
        Transaction_amount = transaction_amount;
        Description = description;
        Email = email;
        IdentificationType = identificationType;
        Number = number;
    }
}