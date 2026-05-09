internal class Room
{
    public string Type { get; }
    public string Label { get; }

    public Room(string type, string label)
    {
        Type = type;
        Label = label;
    }
}
