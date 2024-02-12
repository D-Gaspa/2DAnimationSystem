namespace Transformations;

public class KeyFrame(int frame)
{
    public int Frame
    {
        get => frame;
        set => frame = value;
    }

    public List<Figure>? Figures { get; set; }
    public bool IsVisible { get; init; } = true;
}