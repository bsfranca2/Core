namespace Bsfranca2.Core;

public abstract record TaxId
{
    protected static string Sanitize(string input)
    {
        return new string(input?.Where(char.IsDigit).ToArray() ?? []);
    }

    public abstract string GetFormattedValue();
}

public sealed record Cpf : TaxId
{
    public string Value { get; }

    public Cpf(string value)
    {
        if (value.Length != 11)
        {
            throw new ArgumentException("CPF deve ter 11 dígitos");
        }

        Value = value;
    }

    public override string GetFormattedValue()
    {
        return $"{Value[..3]}.{Value[3..6]}.{Value[6..9]}-{Value[9..]}";
    }

    public static Cpf Parse(string value)
    {
        return new Cpf(Sanitize(value));
    }
}

public sealed record Cnpj : TaxId
{
    public string Value { get; }

    public Cnpj(string value)
    {
        if (value.Length != 14)
        {
            throw new ArgumentException("CNPJ deve ter 14 dígitos");
        }

        Value = value;
    }

    public override string GetFormattedValue()
    {
        return $"{Value[..2]}.{Value[2..5]}.{Value[5..8]}/{Value[8..12]}-{Value[12..]}";
    }

    public static Cnpj Parse(string value)
    {
        return new Cnpj(Sanitize(value));
    }
}

public static class TaxIdExtensions
{
    public static TResult Map<TResult>(this TaxId taxId, Func<Cpf, TResult> cpf, Func<Cnpj, TResult> cnpj)
    {
        return taxId switch
        {
            Cpf c => cpf(c),
            Cnpj c => cnpj(c),
            _ => throw new ArgumentException($"Unknown tax id kind: {taxId}")
        };
    }
}