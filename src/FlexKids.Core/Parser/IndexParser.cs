namespace FlexKids.Core.Parser
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using FlexKids.Core.Parser.Helper;
    using FlexKids.Core.Scheduler.Model;
    using HtmlAgilityPack;
    using Microsoft.Extensions.Logging;

    internal class IndexParser
    {
        private readonly ILogger _logger;

        public IndexParser(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IndexContent Parse(string content)
        {
            var document = new HtmlDocument();
            document.LoadHtml(content);

            return new IndexContent
                {
                    Email = ExtractEmailFromContent(document),
                    Weeks = ExtractWeeksFromContent(document),
                };
        }

        private Dictionary<int, WeekItem> ExtractWeeksFromContent(HtmlDocument document)
        {
            var weekSelections = document.DocumentNode.Descendants()
                                          .Where(x => x.IsSelect() && x.IdEquals("week_selectie"))
                                          .ToList();
            if (weekSelections.Count != 1)
            {
                var s = $"Nr of weekselections is {weekSelections.Count} but should be equal to 1.";
                throw new InvalidDataException(s);
            }

            HtmlNode weekSelection = weekSelections.First();

            // select options
            var options = weekSelection.ChildNodes.Where(x => x.IsOption()).ToList();

            var weeks = new Dictionary<int, WeekItem>(options.Count);

            if (!options.Any())
            {
                return weeks;
            }

            foreach (HtmlNode option in options)
            {
                if (option.Attributes?["value"] == null)
                {
                    throw new FlexKidsParseException();
                }

                if (!int.TryParse(option.Attributes["value"].Value, out var nr))
                {
                    throw new FlexKidsParseException();
                }

                if (option.NextSibling == null)
                {
                    throw new FlexKidsParseException();
                }

                // Week 09 - 2015
                var weekText = option.NextSibling.InnerText.Trim();
                weekText = weekText.Replace("Week", string.Empty).Trim();
                var split = weekText.Split('-');

                if (split.Length != 2)
                {
                    continue;
                }

                var sWeek = split[0].Trim(); // 09
                var sYear = split[1].Trim(); // 2015

                if (int.TryParse(sWeek, out var weekNr) && int.TryParse(sYear, out var year))
                {
                    var w = new WeekItem(weekNr, year);
                    weeks.Add(nr, w);
                }
                else
                {
                    throw new FlexKidsParseException();
                }
            }

            return weeks;
        }

        private string ExtractEmailFromContent(HtmlDocument document)
        {
            var logins = document.DocumentNode.Descendants().Where(x => x.IsDiv() && x.ClassContains("username")).ToList();
            if (logins.Count != 1)
            {
                _logger.LogError("Cannot find email");
                return string.Empty;
            }

            HtmlNode loginEmailAddress = logins.First();
            return loginEmailAddress.InnerText.Replace("&nbsp;", string.Empty).Trim();
        }
    }
}