using HtmlAgilityPack;

namespace Forto4kiParser.Helpers
{
    public static class HtmlNodeExtensions
    {
        public static HtmlNodeCollection GetNextElementsWithParent(this HtmlNode node, int n)
        {
            HtmlNodeCollection nextElements = new HtmlNodeCollection(node.ParentNode);
            nextElements.Add(node);

            HtmlNode nextElement = node.NextSibling;
            int count = 0;

            while (nextElement != null && count < n)
            {
                nextElements.Add(nextElement);
                nextElement = nextElement.NextSibling;
                count++;
            }

            return nextElements;
        }
    }
}
