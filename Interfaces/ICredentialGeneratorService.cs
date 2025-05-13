namespace backend.Interfaces;

public interface ICredentialGeneratorService
{
    string GenerateRandomLoginId(int minLength = 5, int maxLength = 8); // Adicione valores padrão
    string GenerateRandomPassword(int minLength = 8); // Adicione valor padrão
}