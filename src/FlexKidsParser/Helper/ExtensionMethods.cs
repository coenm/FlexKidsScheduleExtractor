namespace FlexKidsParser.Helper
{
    using HtmlAgilityPack;

    public static class ExtensionMethods
    {
        public static bool IsTbody(this HtmlNode item)
        {
            return item.Name == "tbody";
        }

        public static bool IsThead(this HtmlNode item)
        {
            return item.Name == "thead";
        }

        public static bool IsTr(this HtmlNode item)
        {
            return item.Name == "tr";
        }

        public static bool IsTh(this HtmlNode item)
        {
            return item.Name == "th";
        }

        public static bool IsTd(this HtmlNode item)
        {
            return item.Name == "td";
        }

        public static bool IsTable(this HtmlNode item)
        {
            return item.Name == "table";
        }

        public static bool IsDiv(this HtmlNode item)
        {
            return item.Name == "div";
        }

        public static bool IsSelect(this HtmlNode item)
        {
            return item.Name == "select";
        }

        public static bool IsOption(this HtmlNode item)
        {
            return item.Name == "option";
        }

        public static string Class(this HtmlNode item)
        {
            if (item.Attributes["class"] != null)
            {
                return item.Attributes["class"].Value;
            }

            return string.Empty;
        }

        public static bool ClassContains(this HtmlNode item, string classValue)
        {
            return item.Attributes["class"] != null && item.Attributes["class"].Value.Contains(classValue);
        }

        public static bool IdEquals(this HtmlNode item, string idValue)
        {
            return item.Attributes["id"] != null && item.Attributes["id"].Value.Equals(idValue);
        }

        public static bool IsElement(this HtmlNode item)
        {
            return item.Name != "#text";
        }

        public static bool IsJustText(this HtmlNode item)
        {
            return item.Name == "#text";
        }
    }
}