using System;

namespace Reservations.Functions.Utils
{
    public interface IDateTimeProvider
    {
        DateTime Now { get; }
        DateTime UtcNow { get; }
    }

    public class DateTimeProvider : IDateTimeProvider
    {
        private static IDateTimeProvider _current;

        private DateTimeProvider()
        {
        }

        public static IDateTimeProvider Current
        {
            get => _current ?? new DateTimeProvider();
            set => _current = value;
        }

        public DateTime Now => DateTime.Now;
        public DateTime UtcNow => DateTime.UtcNow;
    }
}