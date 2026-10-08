using System.Security.Cryptography;

namespace HoaTheoMua.Service.VoucherProgram;

public interface IVoucherCodeGenerator
{
    string Generate(string? prefix, int length, string? suffix);
}

public sealed class VoucherCodeGenerator : IVoucherCodeGenerator
{
    private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public string Generate(string? prefix, int length, string? suffix)
    {
        var characters = new char[length];
        for (var i = 0; i < characters.Length; i++)
            characters[i] = Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)];

        return string.Concat(prefix, new string(characters), suffix);
    }
}
