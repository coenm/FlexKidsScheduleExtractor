namespace FlexKids.Core.Parser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FlexKids.Core.Parser.Helper;
    using FlexKids.Core.Scheduler.Model;
    using HtmlAgilityPack;
    using Microsoft.Extensions.Logging;

    internal class ScheduleParser
    {
        private const int NUMBER_OF_WORKDAYS = 5;
        private readonly ILogger _logger;

        public ScheduleParser(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IReadOnlyList<ScheduleItem> GetScheduleFromContent(string content, int year)
        {
            var document = new HtmlDocument();
            document.LoadHtml(content);

            var result = new List<ScheduleItem>();

            HtmlNode divHourRegistration = GetHourRegistration(document);
            HtmlNode locationWeekOverview = GetWeekScheduleTable(divHourRegistration);

            IReadOnlyList<HtmlNode> cols = GetColumns(locationWeekOverview);
            DateTime[] dates = GetDatesInWeek(cols, year).ToArray();

            IReadOnlyList<HtmlNode> rows = GetLocationSchedulesHtml(locationWeekOverview);

            foreach (HtmlNode row in rows)
            {
                var room = GetRoomFromSingleScheduleRow(row);
                IEnumerable<ScheduleItem> results = ProcessSingleRow(row, room, dates);
                result.AddRange(results);
            }

            return result;
        }

        private string GetRoomFromSingleScheduleRow(HtmlNode row)
        {
            // get first column as it contains the info
            HtmlNode infoColumn = row.ChildNodes.FirstOrDefault(x => x.IsTd());

            if (infoColumn == null)
            {
                throw new FlexKidsParseException();
            }

            // .. should containing 4 divs
            var infoTdDivs = infoColumn.ChildNodes.Where(x => x.IsDiv()).ToList();

            // to improve, use class='groep' (when not exist, use class='taak').
            return infoTdDivs[3].InnerText;
        }

        private IEnumerable<DateTime> GetDatesInWeek(IReadOnlyList<HtmlNode> cols, int year)
        {
            const int OFFSET = 1;

            for (var day = OFFSET; day < NUMBER_OF_WORKDAYS + OFFSET; day++)
            {
                var divs = cols[day].Descendants().Where(x => x.IsDiv()).ToList();
                var dateString = divs[0].InnerText.Trim();
                DateTime dateWithoutTime = ParseDate.StringToDateTime(dateString, year);
                yield return dateWithoutTime;
            }
        }

        private IReadOnlyList<HtmlNode> GetLocationSchedulesHtml(HtmlNode locationWeekOverview)
        {
            // first column is nothing..
            // second till 6th are Monday till Friday
            var tbodys = locationWeekOverview.ChildNodes.Where(x => x.IsTbody()).ToList();
            HtmlNode tbody = tbodys.First();
            return tbody.ChildNodes.Where(x => x.IsTr()).ToList();
        }

        private IReadOnlyList<HtmlNode> GetColumns(HtmlNode locationWeekOverview)
        {
            // get head (containing days and dates)
            var tableHeads = locationWeekOverview.Descendants().Where(x => x.IsThead()).ToList();

            if (tableHeads.Count == 0)
            {
                throw new FlexKidsParseException();
            }

            if (tableHeads.Count > 1)
            {
                _logger.LogWarning("Multiple {count} heads found. Resume with first one.", tableHeads.Count);
            }

            // get columns from first head row.
            var rows = tableHeads.First().Descendants().Where(x => x.IsTr()).ToList();
            if (rows.Count == 0)
            {
                throw new FlexKidsParseException();
            }

            // not sure
            if (rows.Count > 1)
            {
                _logger.LogWarning("Multiple {count} rows. Resume with first one.", rows.Count);
            }

            // get columns
            var cols = rows.First().Descendants().Where(x => x.IsTh()).ToList();

            if (cols.Count < NUMBER_OF_WORKDAYS + 1)
            {
                throw new FlexKidsParseException();
            }

            if (cols.Count > NUMBER_OF_WORKDAYS + 1)
            {
                _logger.LogWarning("Expecting " + NUMBER_OF_WORKDAYS + 1 + " columns but {count} found. Only use the first ones.", rows.Count);
            }

            return cols.Take(NUMBER_OF_WORKDAYS + 1).ToList();
        }

        private HtmlNode GetWeekScheduleTable(HtmlNode document)
        {
            var tableWeekOverview = document.Descendants()
                                            .Where(x => x.IdEquals("locatie_weekoverzicht"))
                                            .ToList();

            if (tableWeekOverview.Count == 0)
            {
                throw new FlexKidsParseException();
            }

            if (tableWeekOverview.Count > 1)
            {
                _logger.LogWarning("Multiple {count} week overviews found. Resume with first one.", tableWeekOverview.Count);
            }

            return tableWeekOverview.First();
        }

        private HtmlNode GetHourRegistration(HtmlDocument document)
        {
            var divsHourRegistration = document.DocumentNode.Descendants()
                                               .Where(x => x.IsDiv() && x.IdEquals("urenregistratie"))
                                               .ToList();

            if (divsHourRegistration.Count == 0)
            {
                throw new FlexKidsParseException();
            }

            if (divsHourRegistration.Count > 1)
            {
                _logger.LogWarning("Multiple {count} hour registration divs found. Resume with first one.", divsHourRegistration.Count);
            }

            return divsHourRegistration.First();
        }

        private IEnumerable<ScheduleItem> ProcessSingleRow(HtmlNode tr, string location, DateTime[] dates)
        {
            // get columns
            var rowColumns = tr.ChildNodes.Where(x => x.IsTd()).ToList();

            // get schedule for each day for given location and given date
            const int OFFSET = 1;
            for (var day = 0; day < NUMBER_OF_WORKDAYS; day++)
            {
                HtmlNode dayRowColumn = rowColumns[day + OFFSET];
                DateTime date = dates[day];

                foreach (ScheduleItem item in ExtractScheduleItemFromSingleItem(dayRowColumn, date, location))
                {
                    yield return item;
                }
            }
        }

        private IEnumerable<ScheduleItem> ExtractScheduleItemFromSingleItem(HtmlNode dayRowColumn, DateTime date, string location)
        {
            if (!dayRowColumn.HasChildNodes)
            {
                yield break;
            }

            if (dayRowColumn.ChildNodes.Count(x => x.IsElement()) < 2)
            {
                yield break;
            }

            IEnumerable<HtmlNode> locationSchedules = dayRowColumn.ChildNodes.Where(x => x.IsTable() && x.ClassContains("locatieplanning_2colommen"));

            foreach (HtmlNode locationSchedule in locationSchedules)
            {
                if (!locationSchedule.IsTable() || !locationSchedule.ClassContains("locatieplanning_2colommen"))
                {
                    // strange behavior.// check
                    continue;
                }

                HtmlNode[] elementRows = locationSchedule.ChildNodes.Where(x => x.IsElement()).ToArray();
                if (elementRows.Length is < 2 or > 3)
                {
                    continue;
                }

                HtmlNode dataRow = elementRows[1];

                if (dataRow.ChildNodes.Count(x => x.IsElement()) != 2)
                {
                    continue;
                }

                HtmlNode tdStartEndTime = dataRow.ChildNodes.First(x => x.IsElement()); // <td class="left">09:00-18:30</td>
                var times = tdStartEndTime.InnerText.Trim(); // i.e. 09:00-18:00

                (DateTime start, DateTime end) = ParseDate.CreateStartEndDateTimeTuple(date, times);

                yield return new ScheduleItem
                    {
                        Start = start,
                        End = end,
                        Location = location,
                    };
            }
        }
    }
}