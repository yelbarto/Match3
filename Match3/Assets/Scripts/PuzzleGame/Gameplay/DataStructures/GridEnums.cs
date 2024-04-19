namespace PuzzleGame.Gameplay.DataStructures
{
    public enum GridType
    {
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
}