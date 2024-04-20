namespace PuzzleGame.Gameplay.DataStructures
{
    public enum GridType
    {
        Default = -1,
        Cube,
        Vase,
        Stone,
        Box,
        VerticalRocket,
        HorizontalRocket,
        Tnt
    }
    
    public enum GridColor
    {
        Default = -1,
        Red = 0,
        Green = 1,
        Blue = 2,
        Yellow = 3
    }

    public enum GridState
    {
        Default,
        NonInteractable,
        Interactable,
        Rocket,
        Tnt
    }
    
    public enum Direction
    {
        Vertical,
        Horizontal
    }
}