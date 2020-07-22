using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace SimpleSearchBar
{
    public class SearchUtility
    {
        private static string searchKeyword = string.Empty;
        private static string regulatedKeyword = string.Empty;
        public static string Keyword
        {
            get
            {
                return searchKeyword;
            }
            set
            {
                searchKeyword = value.TrimStart(' ');
                regulatedKeyword = RegulateString(value);
            }
        }

        private static string RegulateString(String label)
        {
            return label.ToLower().Replace(" ", string.Empty);
        }

        public static bool CheckVisible(ThingDef thing)
        {
            return SearchUtility.regulatedKeyword.Length <= 0 || SearchUtility.RegulateString(thing.LabelCap).Contains(SearchUtility.regulatedKeyword);
        }

        public static bool CheckVisible(TransferableOneWay transferable)
        {
            return SearchUtility.regulatedKeyword.Length <= 0 || SearchUtility.RegulateString(transferable.LabelCap).Contains(SearchUtility.regulatedKeyword);
        }

        public static bool CheckVisible(Tradeable tradable)
        {
            return SearchUtility.regulatedKeyword.Length <= 0 || SearchUtility.RegulateString(tradable.LabelCap).Contains(SearchUtility.regulatedKeyword);
        }

        public static void Reset()
        {
            searchKeyword = string.Empty;
            regulatedKeyword = string.Empty;
        }
    }
}
