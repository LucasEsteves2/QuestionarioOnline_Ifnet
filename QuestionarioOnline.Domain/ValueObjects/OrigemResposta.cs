using System.Security.Cryptography;
using System.Text;

namespace QuestionarioOnline.Domain.ValueObjects;

public sealed class OrigemResposta : IEquatable<OrigemResposta>
{
    public string Hash { get; private set; }

    private OrigemResposta(string hash)
    {
        Hash = hash;
    }

    public static OrigemResposta Create(string ipAddress, string userAgent)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
            throw new ArgumentException("IP não pode ser vazio", nameof(ipAddress));

        if (string.IsNullOrWhiteSpace(userAgent))
            throw new ArgumentException("User Agent não pode ser vazio", nameof(userAgent));

        var combined = $"{ipAddress}|{userAgent}";
        var hash = ComputeHash(combined);

        return new OrigemResposta(hash);
    }

    private static string ComputeHash(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = SHA256.HashData(bytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    public bool Equals(OrigemResposta? other)
    {
        if (other is null) return false;
        return Hash == other.Hash;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as OrigemResposta);
    }

    public override int GetHashCode()
    {
        return Hash.GetHashCode();
    }

    public override string ToString()
    {
        return Hash;
    }

    public static implicit operator string(OrigemResposta origem) => origem.Hash;
}
