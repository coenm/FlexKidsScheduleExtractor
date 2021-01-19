namespace FlexKids.Core.Parser.Helper
{
    using System;
    using System.IO;

    public static class ParseDate
    {
        // time = 09:00
        public static DateTime AddStringTimeToDate(DateTime date, string time)
        {
            var split = time.Trim().Split(':');
            if (split.Length != 2)
            {
                throw new InvalidDataException("Not a valid time format.");
            }

            if (!int.TryParse(split[0].Trim(), out var hour))
            {
                throw new InvalidDataException("No hours found");
            }

            if (hour is < 0 or > 23)
            {
                throw new InvalidDataException("Hours not in range");
            }

            if (!int.TryParse(split[1].Trim(), out var min))
            {
                throw new InvalidDataException("No minutes found");
            }

            if (min is < 0 or >= 60)
            {
                throw new InvalidDataException("Minutes not in range");
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
                throw new InvalidDataException("Split character '-' not found or found multiple times.");
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
                throw new InvalidDataException("Not a valid format.");
            }

            var spitDate = splitInput[1].Trim().Split('-');
            if (spitDate.Length != 2)
            {
                throw new InvalidDataException("Split character '-' not found or found multiple times.");
            }

            if (!int.TryParse(spitDate[0].Trim(), out var day))
            {
                throw new InvalidDataException("Not an integer.");
            }

            if (day <= 0)
            {
                throw new InvalidDataException($"Found day ({day}) not in range");
            }

            if (day > 31)
            {
                throw new InvalidDataException($"Found day ({day}) not in range");
            }

            var monthTxt = RemoveLastCharIfDot(spitDate[1].Trim());

            var month = monthTxt switch
            {
                "jan" => 1,
                "feb" => 2,
                "mrt" => 3,
                "apr" => 4,
                "mei" => 5,
                "jun" => 6, // unchecked
                "jul" => 7, // unchecked
                "aug" => 8, // unchecked
                "sep" or "sept" => 9, // unchecked
                "okt" => 10, // unchecked
                "nov" => 11, // unchecked
                "dec" => 12,
                _ => throw new InvalidDataException(monthTxt + " is not catched"),
            };

            var result = new DateTime(year, month, day, 0, 0, 0);
            return result;
        }
    }
}