namespace HonzaBotner.Discord.Services.Helpers;

internal class StandUpStats
{
    private int _normal;
    private int _must;

    private const string Must = "!";

    public void Increment(string priority)
    {
        if (priority.Contains(Must))
        {
            _must++;
        }
        else
        {
            _normal++;
        }
    }

    private int Sum => _normal + _must;

    public override string ToString()
    {
        return $"{Sum} ({_normal} + {_must}!)";
    }

    public StandUpStats Add(StandUpStats other)
    {
        return new StandUpStats { _normal = _normal + other._normal, _must = _must + other._must };
    }
}
