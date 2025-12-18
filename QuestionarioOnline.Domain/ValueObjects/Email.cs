namespace QuestionarioOnline.Domain.ValueObjects;

public sealed class Email : IEquatable<Email>
{
    public string Address { get; private set; }

    private Email(string address)
    {
        Address = address;
    }

    public static Email Create(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Email não pode ser vazio", nameof(address));

        if (!IsValidEmail(address))
            throw new ArgumentException("Email inválido", nameof(address));

        return new Email(address.ToLowerInvariant().Trim());
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    public bool Equals(Email? other)
    {
        if (other is null) return false;
        return Address == other.Address;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Email);
    }

    public override int GetHashCode()
    {
        return Address.GetHashCode();
    }

    public override string ToString()
    {
        return Address;
    }

    public static implicit operator string(Email email) => email.Address;
}
