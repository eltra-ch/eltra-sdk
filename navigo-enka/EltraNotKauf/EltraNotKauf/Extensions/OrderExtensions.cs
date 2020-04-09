using EltraCloudContracts.Enka.Orders;
using System;

namespace EltraNotKauf.Extensions
{
    public static class OrderExtensions
    {
        public static string RemainingTime(this Order order)
        {
            string result = string.Empty;

            if (order != null)
            {
                var durationInSec = (int)(DateTime.Now - order.Modified).TotalSeconds;
                var timeout = order.Timeout;
                var remainingSec = timeout - durationInSec;

                var remains = new TimeSpan(remainingSec * TimeSpan.TicksPerSecond);

                var totalDays = (int)Math.Round(remains.TotalDays);
                var totalHours = (int)Math.Round(remains.TotalHours);
                var totalMinutes = (int)Math.Round(remains.TotalMinutes);
                var totalSeconds = (int)Math.Round(remains.TotalSeconds);

                if (totalDays > 0)
                {
                    if (totalDays == 1)
                    {
                        result = $"{totalDays} Tag";
                    }
                    else
                    {
                        result = $"{totalDays} Tage";
                    }
                }
                else if (totalHours > 0)
                {
                    if (totalHours == 1)
                    {
                        result = $"{totalHours} Stunde";
                    }
                    else
                    {
                        result = $"{totalHours} Stunden";
                    }
                }
                else if (totalMinutes > 0)
                {
                    if (totalMinutes == 1)
                    {
                        result = $"{totalMinutes} Minute";
                    }
                    else
                    {
                        result = $"{totalMinutes} Minuten";
                    }
                }
                else if (totalSeconds > 0)
                {
                    if (totalSeconds == 1)
                    {
                        result = $"{totalSeconds} Sekunde";
                    }
                    else
                    {
                        result = $"{totalSeconds} Sekunden";
                    }
                }
            }

            return result;
        }
    }
}
