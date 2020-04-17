using System;

namespace NCoreUtils.OAuth2
{
    internal static class DateTimeOffsetExtensions
    {
        private static readonly long _halfSecond = TimeSpan.TicksPerSecond / 2;

        internal static DateTimeOffset Normalize(this DateTimeOffset value)
        {
            var reminder = value.UtcTicks % TimeSpan.TicksPerSecond;
            var ticks = value.UtcTicks - reminder;
            if (reminder > _halfSecond)
            {
                ticks += TimeSpan.TicksPerSecond;
            }
            return new DateTimeOffset(ticks, TimeSpan.Zero);
        }
    }
}