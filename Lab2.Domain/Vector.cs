using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Lab2.Domain
{
    public sealed class Vector : IComparable<Vector>
    {
        public string LineColor { get; }
        public double X { get; }
        public double Y { get; }
        public double Length { get; }

        public Vector(string lineColor, double x, double y)
        {
            if (string.IsNullOrWhiteSpace(lineColor))
                throw new ArgumentException("Вкажіть колір лінії.", nameof(lineColor));

            string color = lineColor.Trim();
            if (color.Length > 32 || !Regex.IsMatch(color, @"\A\p{L}+(?:[ -]\p{L}+)*\z"))
                throw new ArgumentException("Колір: до 32 літер, із пробілами або дефісами між словами.", nameof(lineColor));

            if (!double.IsFinite(x) || !double.IsFinite(y))
                throw new ArgumentException("Координати повинні бути скінченними числами.");

            // Масштабування не дає квадратам великих координат переповнитися.
            double scale = Math.Max(Math.Abs(x), Math.Abs(y));
            double length = scale == 0 ? 0 : scale * Math.Sqrt((x / scale) * (x / scale) + (y / scale) * (y / scale));
            if (!double.IsFinite(length))
                throw new ArgumentException("Довжина вектора завелика для типу double.");

            LineColor = color;
            X = x;
            Y = y;
            Length = length;
        }

        public Vector Extend(double distance)
        {
            if (!double.IsFinite(distance) || distance < 0)
                throw new ArgumentOutOfRangeException(nameof(distance), "Відрізок має бути скінченним невід'ємним числом.");
            if (distance == 0)
                return this;
            if (Length == 0)
                throw new InvalidOperationException("Нульовий вектор не має напрямку для збільшення.");

            double newLength = Length + distance;
            if (!double.IsFinite(newLength))
                throw new ArgumentOutOfRangeException(nameof(distance), "Нова довжина завелика.");

            // Новий об'єкт не змінює ключ об'єкта, який уже може перебувати в дереві.
            double scale = Math.Max(Math.Abs(X), Math.Abs(Y));
            double scaledX = X / scale;
            double scaledY = Y / scale;
            double scaledLength = Math.Sqrt(scaledX * scaledX + scaledY * scaledY);
            return new Vector(LineColor, (scaledX / scaledLength) * newLength, (scaledY / scaledLength) * newLength);
        }

        public int CompareTo(Vector? other)
        {
            if (other == null)
                return 1;

            int result = Length.CompareTo(other.Length);
            if (result == 0) result = X.CompareTo(other.X);
            if (result == 0) result = Y.CompareTo(other.Y);
            if (result == 0) result = StringComparer.Ordinal.Compare(LineColor, other.LineColor);
            return result;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.GetCultureInfo("uk-UA"),
                "{0,-14} X = {1,8:F2}; Y = {2,8:F2}; довжина = {3:F2}", LineColor, X, Y, Length);
        }
    }
}
