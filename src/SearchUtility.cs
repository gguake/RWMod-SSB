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
            if (regulatedKeyword.Length > 0)
            {
                return RegulateString(thing.LabelCap).Contains(regulatedKeyword);
            }

            return true;
        }

        public static bool CheckVisible(Transferable transferable)
        {
            if (regulatedKeyword.Length > 0)
            {
                return RegulateString(transferable.LabelCap).Contains(regulatedKeyword);
            }

            return true;
        }

        public static void Reset()
        {
            searchKeyword = string.Empty;
            regulatedKeyword = string.Empty;
        }
    }
}
