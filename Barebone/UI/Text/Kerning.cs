namespace Barebone.UI.Text;

public readonly struct Kerning
{
    public readonly int First;
    public readonly int Second;
    public readonly int Amount;

    public Kerning(int first, int second, int amount)
    {
        First = first;
        Second = second;
        Amount = amount;
    }
}