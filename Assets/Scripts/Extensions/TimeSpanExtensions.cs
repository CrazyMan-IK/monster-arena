using System;
using System.Text;

namespace MonsterArena.Extensions
{
    public static class TimeSpanExtensions
    {
        public static string ToCustomString(this TimeSpan span)
        {
            StringBuilder builder = new StringBuilder();

            if (span.Hours > 0)
            {
                builder.Append(span.Hours);
                builder.Append("h ");
            }

            if (span.Minutes > 0)
            {
                builder.Append(span.Minutes);
                builder.Append("m ");
            }

            if (span.Seconds > 0)
            {
                builder.Append(span.Seconds);
                builder.Append("s ");
            }

            return builder.ToString();
        }
    }
}
