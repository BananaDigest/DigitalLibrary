using System;

namespace Domain.Enums
{
    [Flags]
    public enum BookType
    {
        None = 0,
        Paper = 1 << 0,
        Electronic = 1 << 1,
        Audio = 1 << 2
    }
}
