namespace FlexKids.Core.Parser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FlexKids.Core.Parser.Helper;
    using FlexKids.Core.Scheduler.Model;
    using HtmlAgilityPack;

    internal class ScheduleParser
    {
        private readonly string _content;
        private readonly int _year;

        public ScheduleParser(string content, int year)
        {
            _year = year;
            _content = content;
        }

        public List<ScheduleItem> GetScheduleFromContent()
        {
            const int NUMBER_OF_WORKDAYS = 5;

            var document = new HtmlDocument();
            document.LoadHtml(_content);

            var result = new List<ScheduleItem>();

            var divsHourRegistration = document.DocumentNode.Descendants()
                                                .Where(x => x.IsDiv() && x.IdEquals("urenregistratie"))
                                                .ToList();

            if (divsHourRegistration.Count != 1)
            {
                // _logger.Error("urenregistratieDiv");
                return null;
            }

            var tablesIdLocatieWeekoverzicht = divsHourRegistration.First()
                                                                   .Descendants()
                                                                   .Where(x => x.IdEquals("locatie_weekoverzicht"))
                                                                   .ToList();
            if (tablesIdLocatieWeekoverzicht.Count != 1)
            {
                // _logger.Error("tableLocaties");
                return null;
            }

            HtmlNode tableIdLocatieWeekOverzicht = tablesIdLocatieWeekoverzicht.First();

            // get head (hierin zitten de dagen en de datums)
            var theads = tableIdLocatieWeekOverzicht.Descendants().Where(x => x.IsThead()).ToList();
            if (theads.Count != 1)
            {
                // _logger.Error("theads");
                return null;
            }

            // get tr
            var rows = theads.First().Descendants().Where(x => x.IsTr()).ToList();
            if (rows.Count != 1)
            {
                // _logger.Error("rows");
                return null;
            }

            // get columns
            var cols = rows.First().Descendants().Where(x => x.IsTh()).ToList();

            // Additional column contains info.
            if (cols.Count != NUMBER_OF_WORKDAYS + 1)
            {
                // _logger.Error("cols");
                return null;
            }

            // first column is nothing..
            // second till 6th are Monday till Friday
            var tbodys = tableIdLocatieWeekOverzicht.ChildNodes.Where(x => x.IsTbody()).ToList();
            HtmlNode tbody = tbodys.First();

            var trs2 = tbody.ChildNodes.Where(x => x.IsTr()).ToList();

            foreach (HtmlNode tr in trs2)
            {
                IEnumerable<ScheduleItem> results = ProcessSingleRow(tr, cols);
                result.AddRange(results);
            }

            return result;
        }

        private IEnumerable<ScheduleItem> ProcessSingleRow(HtmlNode tr, IReadOnlyList<HtmlNode> cols)
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

                            DateTime dateWithoutTime = ParseDate.StringToDateTime(dateString, _year);
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