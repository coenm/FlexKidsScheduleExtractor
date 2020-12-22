namespace FlexKidsParser.Helper
{
    using System;
    using NLog;

    public static class ParseDate
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        // time = 09:00
        public static DateTime AddStringTimeToDate(DateTime date, string time)
        {
            var split = time.Trim().Split(':');
            if (split.Length != 2)
            {
                throw new FormatException("Not a valid time format.");
            }

            if (!int.TryParse(split[0].Trim(), out var hour))
            {
                throw new Exception("No hours found");
            }

            if (hour is < 0 or > 23)
            {
                throw new Exception("Hours not in range");
            }

            if (!int.TryParse(split[1].Trim(), out var min))
            {
                throw new Exception("No minutes found");
            }

            if (min is < 0 or >= 60)
            {
                throw new Exception("Minutes not in range");
            }

            return new DateTime(date.Year, date.Month, date.Day, hour, min, 0);
        }

        // startEndTime = 09:00-13:30
        // startEndTime = 14:00-17:30
        public static Tuple<DateTime, DateTime> CreateStartEndDateTimeTuple(DateTime date, string startEndTime)
        {
            var splitBothTimes = startEndTime.Trim().Split('-');
            if (splitBothTimes.Length != 2)
            {
                throw new Exception();
            }

            return new Tuple<DateTime, DateTime>(
                AddStringTimeToDate(date, splitBothTimes[0]),
                AddStringTimeToDate(date, splitBothTimes[1]));
        }

        public static string RemoveLastCharIfDot(string s)
        {
            if (s.Length == 0)
            {
                return s;
            }

            if (s.Substring(s.Length - 1, 1) == ".")
            {
                return s.Substring(0, s.Length - 1);
            }

            return s;
        }

        public static DateTime StringToDateTime(string input, int year)
        {
            var splitInput = input.Split(' ');
            if (splitInput.Length != 2)
            {
                throw new FormatException("Not a valid format.");
            }

            var spitDate = splitInput[1].Trim().Split('-');
            if (spitDate.Length != 2)
            {
                throw new Exception();
            }

            if (!int.TryParse(spitDate[0].Trim(), out var day))
            {
                throw new Exception();
            }

            if (day <= 0)
            {
                throw new Exception();
            }

            if (day > 31)
            {
                throw new Exception();
            }

            int month;
            var monthTxt = RemoveLastCharIfDot(spitDate[1].Trim());

            switch (monthTxt)
            {
                case "jan": // unchecked
                    month = 1;
                    break;

                case "feb":
                    month = 2;
                    break;

                case "mrt":
                    month = 3;
                    break;

                case "apr":
                    month = 4;
                    break;

                case "mei":
                    month = 5;
                    break;

                case "jun":
                    month = 6; // unchecked
                    break;

                case "jul":
                    month = 7; // unchecked
                    break;

                case "aug":
                    month = 8; // unchecked
                    break;

                case "sep":
                case "sept":
                    month = 9; // unchecked
                    break;

                case "okt":
                    month = 10; // unchecked
                    break;

                case "nov":
                    month = 11; // unchecked
                    break;

                case "dec":
                    month = 12;
                    break;

                default:
                    _logger.Error(monthTxt + "  is not catched");
                    throw new Exception(monthTxt + " is not catched");
            }

            var result = new DateTime(year, month, day, 0, 0, 0);
            return result;
        }
    }
}