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
            IReadOnlyList<HtmlNode> rows = GetRows(locationWeekOverview);

            foreach (HtmlNode row in rows)
            {
                IEnumerable<ScheduleItem> results = ProcessSingleRow(row, cols, year);
                result.AddRange(results);
            }

            return result;
        }

        private IReadOnlyList<HtmlNode> GetRows(HtmlNode locationWeekOverview)
        {
            // first column is nothing..
            // second till 6th are Monday till Friday
            var tbodys = locationWeekOverview.ChildNodes.Where(x => x.IsTbody()).ToList();
            HtmlNode tbody = tbodys.First();
            return tbody.ChildNodes.Where(x => x.IsTr()).ToList();
        }

        private IReadOnlyList<HtmlNode> GetColumns(HtmlNode locationWeekOverview)
        {
            // get head (hierin zitten de dagen en de datums)
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

            // check
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

        private IEnumerable<ScheduleItem> ProcessSingleRow(HtmlNode tr, IReadOnlyList<HtmlNode> cols, int year)
        {
            // get columns
            var rowColumns = tr.ChildNodes.Where(x => x.IsTd()).ToList();

            // first column is info
            HtmlNode infoColumn = rowColumns.First();

            // deze heeft 4 divs
            var infoTdDivs = infoColumn.ChildNodes.Where(x => x.IsDiv()).ToList();

            // days
            for (var i = 1; i < 6; i++)
            {
                if (!rowColumns[i].HasChildNodes)
                {
                    continue;
                }

                // at least one locatieplanning_2colommen en 1 div met totaal
                // kunnen meerdere locatieplanning_2collomen zijn
                if (rowColumns[i].ChildNodes.Count(x => x.IsElement()) < 2)
                {
                    continue;
                }

                IEnumerable<HtmlNode> locatieplanningen =
                    rowColumns[i].ChildNodes.Where(x => x.IsTable() && x.ClassContains("locatieplanning_2colommen"));
                foreach (HtmlNode firstItem in locatieplanningen)
                {
                    if (firstItem.IsTable() && firstItem.ClassContains("locatieplanning_2colommen"))
                    {
                        /*
                                <table class="locatieplanning_2colommen dienst" width="100%">
                                    <tr>
                                        <td class="locatieplanning_naam bold" colspan=2>
                                            Dienst
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="left">09:00-18:30</td>
                                        <td class="right">(09:00)</td>
                                    </tr>
                                </table>
                                */
                        HtmlNode[] rowsx = firstItem.ChildNodes.Where(x => x.IsElement()).ToArray();
                        if (rowsx.Length is >= 2 and <= 3)
                        {
                            HtmlNode lastRow = rowsx[1];

                            if (lastRow.ChildNodes.Count(x => x.IsElement()) != 2)
                            {
                                continue;
                            }

                            HtmlNode tdStartEndTime =
                                lastRow.ChildNodes.First(x => x.IsElement()); // <td class="left">09:00-18:30</td>

                            var times = tdStartEndTime.InnerText.Trim(); // i.e. 09:00-18:00
                            var divs = cols[i].Descendants().Where(x => x.IsDiv()).ToList();
                            var dateString = divs[0].InnerText.Trim();

                            var locationString = infoTdDivs[3].InnerText;

                            DateTime dateWithoutTime = ParseDate.StringToDateTime(dateString, year);
                            (DateTime start, DateTime end) = ParseDate.CreateStartEndDateTimeTuple(dateWithoutTime, times);

                            yield return new ScheduleItem
                                {
                                    Start = start,
                                    End = end,
                                    Location = locationString,
                                };
                        }
                    }
                }
            }
        }
    }
}