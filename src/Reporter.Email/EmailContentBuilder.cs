namespace Reporter.Email
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using FlexKidsScheduler.Model;

    internal static class EmailContentBuilder
    {
        public static string ScheduleToPlainTextString(ScheduleDiff[] schedule)
        {
            if (schedule == null || schedule?.Length == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            foreach (ScheduleDiff item in schedule)
            {
                _ = sb.Append(StatusToString(item))
                      .Append(" ")
                      .Append(item.Schedule.StartDateTime.ToString("dd-MM HH:mm"))
                      .Append("-")
                      .Append(item.Schedule.EndDateTime.ToString("HH:mm"))
                      .Append(" ")
                      .Append(item.Schedule.Location)
                      .Append(Environment.NewLine);
            }

            return sb.ToString();
        }

        public static string ScheduleToHtmlString(ScheduleDiff[] schedule)
        {
            if (schedule == null || schedule.Length == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            _ = sb.AppendLine($"<p>Hier is het rooster voor week {schedule.First().Schedule.Week.WeekNr}:</p>");
            _ = sb.AppendLine("<table style='border: 1px solid black; border-collapse:collapse;'>");

            //header
            _ = sb.AppendLine($"<tr style='{StyleString("left")}'>");
            _ = sb.AppendLine($"<td style='{StyleString("center")}'></td>");
            _ = sb.AppendLine($"<td colspan=2 style='{StyleString("left")}'><b>Dag</b></td>");
            _ = sb.AppendLine($"<td colspan=3 style='{StyleString("left")}'><b>Tijd</b></td>");
            _ = sb.AppendLine($"<td style='{StyleString("left")}'><b>Locatie</b></td>");
            _ = sb.AppendLine("</tr>");

            foreach (var item in schedule)
            {
                _ = sb.AppendLine($"<tr style='{StyleString("left")}'>");
                _ = sb.AppendLine($"<td style='{StyleString("center")}'>{StatusToString(item)}</td>");
                _ = sb.AppendLine(
                    $"<td style='{StyleString("left")}{LineThrough(item.Status)} border-right:hidden;'>{item.Schedule.StartDateTime.ToString("ddd", CultureInfo.CreateSpecificCulture("nl-NL"))}</td>");
                _ = sb.AppendLine($"<td style='{StyleString("left")}{LineThrough(item.Status)}'>{item.Schedule.StartDateTime.ToString("dd-MM")}</td>");
                _ = sb.AppendLine($"<td style='{StyleString("left")}{LineThrough(item.Status)} text-align: right; padding-right:0px;'>{item.Schedule.StartDateTime.ToString("HH:mm")}</td>");
                _ = sb.AppendLine($"<td style='{StyleString("center")} border-left: hidden; border-right: hidden;'>-</td>");
                _ = sb.AppendLine($"<td style='{StyleString("left")}{LineThrough(item.Status)} padding-left:0px;'>{item.Schedule.EndDateTime.ToString("HH:mm")}</td>");
                _ = sb.AppendLine($"<td style='{StyleString("left")}{LineThrough(item.Status)}'>{item.Schedule.Location}</td>");
                _ = sb.AppendLine("</tr>");
            }

            _ = sb.AppendLine("</table>");
            _ = sb.AppendLine("</p>");

            return sb.ToString();
        }

        private static string StyleString(string textAlign)
        {
            return $"text-align:{textAlign}; padding:0px 5px; border: 1px solid black;";
        }

        private static string LineThrough(ScheduleStatus status)
        {
            if (status == ScheduleStatus.Removed)
            {
                return "text-decoration: line-through;";
            }

            return string.Empty;
        }

        private static string StatusToString(ScheduleDiff item)
        {
            switch (item.Status)
            {
                case ScheduleStatus.Added:
                    return "+";
                case ScheduleStatus.Removed:
                    return "-";
                case ScheduleStatus.Unchanged:
                    return "=";
                default:
                    return String.Empty;
            }
        }
    }
}
