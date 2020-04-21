using System;

namespace NCoreUtils.AspNetCore
{
    [Flags]
    public enum TokenHandlers
    {
        Bearer = 0x01,
        Cookie = 0x02,
        Query = 0x04,
        Form = 0x08
    }
}