using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FocusDataBridge
{
    class Constant
    {
        public const int INVALID_ID = -99999;

        public static List<string> getTimes(int start, int end, int length)
        {
            List<string> results = new List<string>();
            for (int i = start; i < end; i += length)
            {
                TimeSpan time = TimeSpan.FromSeconds(i);

                //here backslash is must to tell that colon is
                //not the part of format, it just a character that we want in output
                string str = time.ToString(@"hh\:mm\:ss");
                results.Add(str);
            }
            return results;
        }

        public static List<DateTime> getDays(int d, DateTime start, DateTime end)
        {
            DayOfWeek dayOfWeek = DayOfWeek.Sunday;

            switch (d)
            {
                case 1:
                    dayOfWeek = DayOfWeek.Sunday;
                    break;
                case 2:
                    dayOfWeek = DayOfWeek.Monday;
                    break;
                case 3:
                    dayOfWeek = DayOfWeek.Tuesday;
                    break;
                case 4:
                    dayOfWeek = DayOfWeek.Wednesday;
                    break;
                case 5:
                    dayOfWeek = DayOfWeek.Thursday;
                    break;
                case 6:
                    dayOfWeek = DayOfWeek.Friday;
                    break;
                case 7:
                    dayOfWeek = DayOfWeek.Saturday;
                    break;
                default:
                    break;

            }
            List<DateTime> results = new List<DateTime>();
            int intMonth = DateTime.Now.Month;
            int intYear = DateTime.Now.Year;
            int intDay = DateTime.Now.Day;

            for (int i = 0; i < 30; i++)
            {
                if (d != 0)
                {
                    if (DateTime.Now.AddDays(i).DayOfWeek == dayOfWeek)
                    {
                        if (DateTime.Now.AddDays(i) >= start && DateTime.Now.AddDays(i) <= end)
                            results.Add(DateTime.Now.AddDays(i));
                    }

                }
                else
                {
                    if (DateTime.Now.AddDays(i).DayOfWeek != DayOfWeek.Saturday &&
                        DateTime.Now.AddDays(i).DayOfWeek != DayOfWeek.Sunday)
                    {
                        if (DateTime.Now.AddDays(i) >= start && DateTime.Now.AddDays(i) <= end)
                            results.Add(DateTime.Now.AddDays(i));
                    }
                }

            }
            return results;
        }
    }
}
