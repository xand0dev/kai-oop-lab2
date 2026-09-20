using System;
using System.Collections.Generic;

namespace Lab2.Domain
{
    public sealed class VectorColorComparer : IComparer<Vector>
    {
        private static readonly StringComparer ColorOrder =
            StringComparer.Create(System.Globalization.CultureInfo.GetCultureInfo("uk-UA"), true);

        public int Compare(Vector? first, Vector? second)
        {
            if (ReferenceEquals(first, second)) return 0;
            if (first == null) return -1;
            if (second == null) return 1;

            int result = ColorOrder.Compare(first.LineColor, second.LineColor);
            return result == 0 ? first.CompareTo(second) : result;
        }
    }
}
