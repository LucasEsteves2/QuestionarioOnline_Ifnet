namespace QuestionarioOnline.Domain.ValueObjects;

public sealed class PeriodoColeta : IEquatable<PeriodoColeta>
{
    public DateTime DataInicio { get; private set; }
    public DateTime DataFim { get; private set; }

    private PeriodoColeta(DateTime dataInicio, DateTime dataFim)
    {
        DataInicio = dataInicio;
        DataFim = dataFim;
    }

    public static PeriodoColeta Create(DateTime dataInicio, DateTime dataFim)
    {
        if (dataInicio >= dataFim)
            throw new ArgumentException("Data de início deve ser anterior à data de término");

        return new PeriodoColeta(dataInicio, dataFim);
    }

    public bool EstaAtivo()
    {
        var agora = DateTime.UtcNow;
        return agora >= DataInicio && agora <= DataFim;
    }

    public bool JaIniciou()
    {
        return DateTime.UtcNow >= DataInicio;
    }

    public bool JaEncerrou()
    {
        return DateTime.UtcNow > DataFim;
    }

    public bool Equals(PeriodoColeta? other)
    {
        if (other is null) return false;
        return DataInicio == other.DataInicio && DataFim == other.DataFim;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as PeriodoColeta);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(DataInicio, DataFim);
    }

    public override string ToString()
    {
        return $"{DataInicio:dd/MM/yyyy HH:mm} - {DataFim:dd/MM/yyyy HH:mm}";
    }
}
