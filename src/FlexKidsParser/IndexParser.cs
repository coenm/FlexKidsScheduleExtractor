namespace FlexKidsParser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FlexKidsParser.Helper;
    using FlexKidsScheduler.Model;
    using HtmlAgilityPack;
    using NLog;

    internal class IndexParser
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly HtmlDocument _document;

        public IndexParser(string content)
        {
            _document = new HtmlDocument();
            _document.LoadHtml(content);
        }

        public IndexContent Parse()
        {
            var result = new IndexContent
                {
                    Email = ExtractEmailFromContent(),
                };
            result.IsLoggedin = !string.IsNullOrWhiteSpace(result.Email);
            result.Weeks = ExtractWeeksFromContent();
            return result;
        }

        private Dictionary<int, WeekItem> ExtractWeeksFromContent()
        {
            var weekselections = _document.DocumentNode.Descendants()
                                          .Where(x => x.IsSelect() && x.IdEquals("week_selectie"))
                                          .ToList();
            if (weekselections.Count != 1)
            {
                var s = $"Nr of weekselections is {weekselections.Count} but should be equal to 1.";
                _logger.Error(s);
                throw new ApplicationException(s);
            }

            HtmlNode weekselection = weekselections.First();

            // select options
            var options = weekselection.ChildNodes.Where(x => x.IsOption()).ToList();

            var weeks = new Dictionary<int, WeekItem>();
            if (options.Any())
            {
                foreach (HtmlNode option in options)
                {
                    if (option.Attributes?["value"] == null)
                    {
                        throw new Exception();
                    }

                    if (!int.TryParse(option.Attributes["value"].Value, out var nr))
                    {
                        throw new Exception();
                    }

                    if (option.NextSibling == null)
                    {
                        throw new Exception();
                    }

                    // Week 09 - 2015
                    var weekText = option.NextSibling.InnerText.Trim();
                    weekText = weekText.Replace("Week", string.Empty).Trim();
                    var split = weekText.Split('-');

                    if (split.Length == 2)
                    {
                        var sWeek = split[0].Trim(); // 09
                        var sYear = split[1].Trim(); // 2015

                        if (int.TryParse(sWeek, out var weekNr) && int.TryParse(sYear, out var year))
                        {
                            var w = new WeekItem(weekNr, year);
                            weeks.Add(nr, w);
                        }
                        else
                        {
                            throw new Exception();
                        }
                    }
                }
            }

            return weeks;
        }

        private string ExtractEmailFromContent()
        {
            var logins = _document.DocumentNode.Descendants().Where(x => x.IsDiv() && x.ClassContains("username")).ToList();
            if (logins.Count != 1)
            {
                throw new Exception();
            }

            HtmlNode login = logins.First();
            var email = login.InnerText.Replace("&nbsp;", string.Empty).Trim();
            return email;
        }
    }
}