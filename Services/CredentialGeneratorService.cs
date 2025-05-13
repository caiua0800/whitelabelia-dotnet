// CredentialGeneratorService.cs
using System.Security.Cryptography;
using System.Text.Json;
using backend.Interfaces;

namespace backend.Services;

public class CredentialGeneratorService : ICredentialGeneratorService
{
    private const string Numbers = "0123456789";
    private const string AllChars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ!@#$%^&*";

    public string GenerateRandomLoginId(int minLength = 5, int maxLength = 8)
    {
        if (minLength < 5) minLength = 5;
        if (maxLength > 8) maxLength = 8;
        if (maxLength < minLength) maxLength = minLength;

        int length = RandomNumberGenerator.GetInt32(minLength, maxLength + 1);
        
        var chars = new char[length];
        for (int i = 0; i < length; i++)
        {
            chars[i] = Numbers[RandomNumberGenerator.GetInt32(Numbers.Length)];
        }

        return new string(chars);
    }

    public string GenerateRandomPassword(int minLength = 8)
    {
        if (minLength < 8) minLength = 8;

        var chars = new char[minLength];
        
        // Garantir pelo menos um de cada tipo
        chars[0] = Numbers[RandomNumberGenerator.GetInt32(Numbers.Length)];
        chars[1] = AllChars[RandomNumberGenerator.GetInt32(26) + 10]; // Letra minúscula
        chars[2] = AllChars[RandomNumberGenerator.GetInt32(26) + 36]; // Letra maiúscula
        chars[3] = AllChars[RandomNumberGenerator.GetInt32(8) + 62]; // Caractere especial

        // Preencher o resto
        for (int i = 4; i < minLength; i++)
        {
            chars[i] = AllChars[RandomNumberGenerator.GetInt32(AllChars.Length)];
        }

        // Embaralhar
        for (int i = 0; i < minLength; i++)
        {
            int r = i + RandomNumberGenerator.GetInt32(minLength - i);
            (chars[r], chars[i]) = (chars[i], chars[r]);
        }

        return new string(chars);
    }
}