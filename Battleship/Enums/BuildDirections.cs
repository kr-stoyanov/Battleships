using System;

namespace Battleships.Enums
{
    [Flags]
    public enum BuildDirections
    {
        Left = 2,
        Right = 4,
        Up = 8,
        Down = 16
    }
}
