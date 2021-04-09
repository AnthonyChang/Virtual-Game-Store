using System;

namespace CVGS.Enums
{
    [Flags]
    public enum FavoritePlatforms
    {
        None = 0,
        PC = 1,
        Xbox = 2,
        PlayStation = 4,
        Nintendo = 8,
        Mobile = 16
    }
}