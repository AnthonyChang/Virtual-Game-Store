using System;

namespace CVGS.Enums
{
    [Flags]
    public enum GameCategoryOptions
    {
        None = 0,
        Action = 1,
        Adventure = 2,
        RolePlaying = 4,
        Simulation = 8,
        Strategy = 16,
        Puzzle = 32
    }
}