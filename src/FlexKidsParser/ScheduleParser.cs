namespace FlexKidsParser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FlexKidsParser.Helper;
    using FlexKidsScheduler.Model;
    using HtmlAgilityPack;
    using NLog;

    internal class ScheduleParser
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly string _content;
        private readonly int _year;
        private HtmlDocument _document;

        public ScheduleParser(string content, int year)
        {
            _year = year;
            _content = content;
        }

        private HtmlDocument Document
        {
            get
            {
                if (_document == null)
                {
                    _document = new HtmlDocument();
                    _document.LoadHtml(_content);
                }

                return _document;
            }
        }

        public List<ScheduleItem> GetScheduleFromContent()
        {
            var result = new List<ScheduleItem>();

            var divsIdUrenregistratie = Document.DocumentNode.Descendants()
                                                .Where(x => x.IsDiv() && x.IdEquals("urenregistratie"))
                                                .ToList();

            if (divsIdUrenregistratie.Count != 1)
            {
                _logger.Error("urenregistratieDiv");
                return null;
            }

            HtmlNode divIdUrenregistratie = divsIdUrenregistratie.First();

            var tablesIdLocatieWeekoverzicht = divIdUrenregistratie.Descendants()
                                                                   .Where(x => x.IdEquals("locatie_weekoverzicht"))
                                                                   .ToList();
            if (tablesIdLocatieWeekoverzicht.Count != 1)
            {
                _logger.Error("tableLocaties");
                return null;
            }

            HtmlNode tableIdLocatieWeekoverzicht = tablesIdLocatieWeekoverzicht.First();

            // get head (hierin zitten de dagen en de datums)
            var theads = tableIdLocatieWeekoverzicht.Descendants().Where(x => x.IsThead()).ToList();
            if (theads.Count != 1)
            {
                _logger.Error("theads");
                return null;
            }

            HtmlNode thead = theads.First();

            // get tr
            var rows = thead.Descendants().Where(x => x.IsTr()).ToList();
            if (rows.Count != 1)
            {
                _logger.Error("rows");
                return null;
            }

            HtmlNode row = rows.First();

            // get columns
            var cols = row.Descendants().Where(x => x.IsTh()).ToList();
            if (cols.Count != 5 + 1) // 5 days + first column is info
            {
                _logger.Error("cols");
                return null;
            }

            // first column is nothing..
            // second till 6th are Monday till Friday
            var tbodys = tableIdLocatieWeekoverzicht.ChildNodes.Where(x => x.IsTbody()).ToList();
            HtmlNode tbody = tbodys.First();

            var trs2 = tbody.ChildNodes.Where(x => x.IsTr()).ToList();

            foreach (HtmlNode tr in trs2)
            {
                // get columns
                var tds = tr.ChildNodes.Where(x => x.IsTd()).ToList();

                // first column is info
                HtmlNode infoTd = tds.First();

                // deze heeft 4 divs
                var infoTdDivs = infoTd.ChildNodes.Where(x => x.IsDiv()).ToList();

                // for (int i = 0; i < infoTdDivs.Count; i++)
                // {
                //     Console.WriteLine(infoTdDivs[i].Attributes.First().Value);
                //     Console.WriteLine(infoTdDivs[i].InnerText);
                //     Console.WriteLine("+++++++++++++++++++++");
                // }

                // days
                for (var i = 1; i < 6; i++)
                {
                    if (!tds[i].HasChildNodes)
                    {
                        continue;
                    }

                    // at least one locatieplanning_2colommen en 1 div met totaal
                    // kunnen meerdere locatieplanning_2collomen zijn
                    if (tds[i].ChildNodes.Count(x => x.IsElement()) < 2)
                    {
                        continue;
                    }

                    IEnumerable<HtmlNode> locatieplanningen = tds[i].ChildNodes.Where(x => x.IsTable() && x.ClassContains("locatieplanning_2colommen"));
                    foreach (HtmlNode firstItem in locatieplanningen)
                    {
                        // table
                        // var firstItem = tds[i].ChildNodes.First(x => x.IsElement());
                        // firstItem.Class() = "";
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
                                // var firstRow = firstItem.ChildNodes.First(x => x.IsElement());
                                HtmlNode lastRow = rowsx[1];

                                if (lastRow.ChildNodes.Count(x => x.IsElement()) == 2)
                                {
                                    HtmlNode firstTd = lastRow.ChildNodes.First(x => x.IsElement()); // <td class="left">09:00-18:30</td>
                                    HtmlNode lastTd = lastRow.ChildNodes.Last(x => x.IsElement()); // <td class="right">(09:00)</td>

                                    var times = firstTd.InnerText.Trim(); // ie. 09:00-18:00
                                    var divs = cols[i].Descendants().Where(x => x.IsDiv()).ToList();
                                    var dateString = divs[0].InnerText.Trim();

                                    var locationString = infoTdDivs[3].InnerText;

                                    DateTime dateWithoutTime = ParseDate.StringToDateTime(dateString, _year);
                                    Tuple<DateTime, DateTime> startEndDateTimeTuple = ParseDate.CreateStartEndDateTimeTuple(dateWithoutTime, times);

                                    result.Add(new ScheduleItem
                                        {
                                            Start = startEndDateTimeTuple.Item1,
                                            End = startEndDateTimeTuple.Item2,
                                            Location = locationString,
                                        });
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }
    }
}