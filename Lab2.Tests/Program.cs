using System;
using System.Collections;
using System.Collections.Generic;
using Lab2.Collections;
using Lab2.Domain;

namespace Lab2.Tests
{
    internal static class Program
    {
        private static int _passed;
        private static int _failed;

        private static int Main()
        {
            Run("Довжина, від'ємні координати та збільшення зі збереженням напрямку", () =>
            {
                Vector vector = new Vector("Червоний", -3, 4);
                Vector extended = vector.Extend(2);
                Near(vector.Length, 5);
                Near(extended.Length, 7);
                Near(extended.X, -4.2);
                Near(extended.Y, 5.6);
                Near(vector.X, -3);
                Require(extended.LineColor == vector.LineColor);
            });
            Run("Нульовий вектор і недопустимий відрізок", () =>
            {
                Vector zero = new Vector("Синій", 0, 0);
                Near(zero.Length, 0);
                Near(zero.Extend(0).Length, 0);
                Throws<InvalidOperationException>(() => zero.Extend(1));
                Throws<ArgumentOutOfRangeException>(() => zero.Extend(-1));
                Throws<ArgumentOutOfRangeException>(() => zero.Extend(double.NaN));
                Throws<ArgumentOutOfRangeException>(() => zero.Extend(double.PositiveInfinity));
            });
            Run("Перевірка кольору та координат", () =>
            {
                Throws<ArgumentException>(() => new Vector("", 1, 2));
                Throws<ArgumentException>(() => new Vector("Blue123", 1, 2));
                Throws<ArgumentException>(() => new Vector("Синій", double.NaN, 2));
                Throws<ArgumentException>(() => new Vector("Синій", 2, double.NegativeInfinity));
                Require(new Vector(" Темно-синій ", 1, 2).LineColor == "Темно-синій");
            });
            Run("Великі та малі скінченні координати", () =>
            {
                Vector large = new Vector("Blue", 3e200, 4e200);
                Require(Math.Abs(large.Length / 5e200 - 1) < 1e-12);
                Require(new Vector("Blue", 1e-300, 0).Length > 0);
                Vector tinyExtended = new Vector("Blue", double.Epsilon, double.Epsilon).Extend(1);
                Near(tinyExtended.Length, 1);
                Near(tinyExtended.X, tinyExtended.Y);
                Throws<ArgumentException>(() => new Vector("Blue", double.MaxValue, double.MaxValue));
                Throws<ArgumentOutOfRangeException>(() => new Vector("Blue", double.MaxValue, 0).Extend(double.MaxValue));
            });
            Run("IComparable і сортування масиву", () =>
            {
                Vector[] vectors = Vectors();
                Array.Sort(vectors);
                Require(vectors[0].Length == 2 && vectors[1].Length == 5 && vectors[2].Length == 10);
                Require(vectors[0].CompareTo(null) > 0);
                Vector equalLength = new Vector("Білий", 0, 5);
                Require(equalLength.CompareTo(vectors[1]) != 0);
                Require(Math.Sign(equalLength.CompareTo(vectors[1])) == -Math.Sign(vectors[1].CompareTo(equalLength)));
            });
            Run("IComparer і сортування списку за кольором", () =>
            {
                List<Vector> vectors = new List<Vector>(Vectors());
                vectors.Sort(new VectorColorComparer());
                Require(vectors[0].LineColor == "Зелений" && vectors[1].LineColor == "Синій" && vectors[2].LineColor == "Червоний");
                VectorColorComparer comparer = new VectorColorComparer();
                Require(comparer.Compare(null, vectors[0]) < 0 && comparer.Compare(null, null) == 0);
            });
            Run("Preorder і відмінність від сортування", () =>
            {
                BinarySearchTree<Vector> tree = Tree();
                Vector[] traversal = new List<Vector>(tree).ToArray();
                Require(traversal[0].Length == 5 && traversal[1].Length == 2 && traversal[2].Length == 10);
                Vector[] sorted = tree.ToSortedArray();
                Require(sorted[0].Length == 2 && sorted[1].Length == 5 && sorted[2].Length == 10);
                Require(tree.Count == 3);
            });
            Run("Глибші ліве і праве піддерева", () =>
            {
                BinarySearchTree<string> tree = new BinarySearchTree<string>(StringComparer.Ordinal);
                foreach (string item in new[] { "D", "B", "F", "A", "C", "E", "G" }) tree.Add(item);
                Require(string.Join("", tree) == "DBACFEG");
                Require(string.Join("", tree.ToSortedArray()) == "ABCDEFG");
            });
            Run("Пошук, дублікати, відсутній елемент та null", () =>
            {
                BinarySearchTree<Vector> tree = Tree();
                tree.Add(new Vector("Червоний", 3, 4));
                Require(tree.Count == 4);
                Require(tree.Contains(new Vector("Червоний", 3, 4)));
                Require(!tree.Contains(new Vector("Білий", 8, 15)));
                Require(tree.ToSortedArray().Length == 4);
                Throws<ArgumentNullException>(() => tree.Add(null!));
                Throws<ArgumentNullException>(() => tree.Contains(null!));
            });
            Run("Ітератор порожнього дерева і стан Current", () =>
            {
                BinarySearchTree<string> tree = new BinarySearchTree<string>();
                using (IEnumerator<string> iterator = tree.GetEnumerator())
                {
                    Throws<InvalidOperationException>(() => { _ = iterator.Current; });
                    Require(!iterator.MoveNext() && !iterator.MoveNext());
                    iterator.Reset();
                    Require(!iterator.MoveNext());
                }
                Require(tree.ToSortedArray().Length == 0 && !tree.Contains("A"));
            });
            Run("Reset, завершення та незалежні ітератори", () =>
            {
                BinarySearchTree<Vector> tree = Tree();
                using (IEnumerator<Vector> first = tree.GetEnumerator())
                using (IEnumerator<Vector> second = tree.GetEnumerator())
                {
                    Require(first.MoveNext() && first.Current.Length == 5);
                    Require(first.MoveNext() && first.Current.Length == 2);
                    Require(second.MoveNext() && second.Current.Length == 5);
                    first.Reset();
                    Throws<InvalidOperationException>(() => { _ = first.Current; });
                    Require(first.MoveNext() && first.Current.Length == 5);
                    while (first.MoveNext()) { }
                    Throws<InvalidOperationException>(() => { _ = first.Current; });
                    Require(!first.MoveNext());
                }
            });
            Run("Неузагальнений IEnumerable та IEnumerator", () =>
            {
                IEnumerator iterator = ((IEnumerable)Tree()).GetEnumerator();
                Require(iterator.MoveNext() && ((Vector)iterator.Current).Length == 5);
                iterator.Reset();
                Require(iterator.MoveNext() && ((Vector)iterator.Current).Length == 5);
                ((IDisposable)iterator).Dispose();
            });
            Run("Зміна дерева під час обходу", () =>
            {
                BinarySearchTree<Vector> tree = Tree();
                using (IEnumerator<Vector> iterator = tree.GetEnumerator())
                {
                    Require(iterator.MoveNext());
                    tree.Add(new Vector("Білий", 8, 15));
                    Throws<InvalidOperationException>(() => iterator.MoveNext());
                    Throws<InvalidOperationException>(() => iterator.Reset());
                    Throws<InvalidOperationException>(() => { _ = iterator.Current; });
                }
            });
            Run("Довільний клас із зовнішнім IComparer", () =>
            {
                Throws<ArgumentException>(() => new BinarySearchTree<Box>());
                BinarySearchTree<Box> tree = new BinarySearchTree<Box>(Comparer<Box>.Create((a, b) => a.Number.CompareTo(b.Number)));
                tree.Add(new Box(5)); tree.Add(new Box(2)); tree.Add(new Box(10));
                Require(tree.Contains(new Box(2)) && tree.ToSortedArray()[0].Number == 2);
                Require(typeof(BinarySearchTree<>).GetGenericArguments()[0].GenericParameterAttributes
                    .HasFlag(System.Reflection.GenericParameterAttributes.ReferenceTypeConstraint));
            });
            Run("Дерево використовує переданий критерій у пошуку і сортуванні", () =>
            {
                BinarySearchTree<string> tree = new BinarySearchTree<string>(StringComparer.OrdinalIgnoreCase);
                tree.Add("b"); tree.Add("A"); tree.Add("C");
                Require(tree.Contains("B") && tree.ToSortedArray()[0] == "A");
            });
            Run("Вироджене дерево без рекурсивного обходу", () =>
            {
                BinarySearchTree<string> tree = new BinarySearchTree<string>(StringComparer.Ordinal);
                for (int i = 0; i < 2000; i++) tree.Add(i.ToString("D4"));
                Require(new List<string>(tree).Count == 2000 && tree.Contains("1999"));
            });

            Console.WriteLine($"\nПройдено: {_passed}; помилок: {_failed}");
            return _failed == 0 ? 0 : 1;
        }

        private sealed class Box
        {
            public int Number { get; }
            public Box(int number) { Number = number; }
        }

        private static Vector[] Vectors()
        {
            return new[] { new Vector("Червоний", 3, 4), new Vector("Зелений", 0, 2), new Vector("Синій", 6, 8) };
        }

        private static BinarySearchTree<Vector> Tree()
        {
            BinarySearchTree<Vector> tree = new BinarySearchTree<Vector>();
            foreach (Vector vector in Vectors()) tree.Add(vector);
            return tree;
        }

        private static void Require(bool condition)
        {
            if (!condition) throw new Exception("Перевірка не пройшла.");
        }

        private static void Near(double actual, double expected)
        {
            Require(Math.Abs(actual - expected) < 1e-10);
        }

        private static void Throws<T>(Action action) where T : Exception
        {
            try { action(); }
            catch (T) { return; }
            throw new Exception($"Очікувався виняток {typeof(T).Name}.");
        }

        private static void Run(string name, Action action)
        {
            try { action(); _passed++; Console.WriteLine($"PASS: {name}"); }
            catch (Exception error) { _failed++; Console.Error.WriteLine($"FAIL: {name}: {error.Message}"); }
        }
    }
}
