using System;
using System.Collections;
using System.Collections.Generic;
using Lab2.Collections;
using Lab2.Domain;

namespace Lab2.App
{
    internal static class Demonstration
    {
        public static Vector[] CreateVectors()
        {
            return new Vector[]
            {
                new Vector("Червоний", 3, 4),
                new Vector("Зелений", 0, 2),
                new Vector("Синій", 6, 8)
            };
        }

        public static void RunAll(Vector[] source)
        {
            Console.WriteLine("Лабораторна робота 3.2. Варіант 7: Вектор");
            Print("Початкові 3 вектори", source);
            Vector original = source[0];
            try
            {
                Console.WriteLine("\nЗбільшення першого вектора на 2 зі збереженням напрямку:");
                Console.WriteLine(original.Extend(2));
            }
            catch (InvalidOperationException error)
            {
                Console.WriteLine(error.Message);
            }
            catch (ArgumentException error)
            {
                Console.WriteLine(error.Message);
            }
            ShowCollections(source);
            ShowTree(source);
        }

        public static void ShowCollections(Vector[] source)
        {
            Vector extra = new Vector("Жовтий", 5, 12);
            Vector replacement = new Vector("Білий", 0, 3);
            Vector target = source[0];

            Console.WriteLine("\n=== Масив Vector[] ===");
            Vector[] array = (Vector[])source.Clone();
            Print("Початковий масив", array);
            Array.Resize(ref array, array.Length + 1);
            array[array.Length - 1] = extra;
            Print("Додавання: Array.Resize і запис у останню комірку", array);
            array[1] = replacement;
            Print("Оновлення: array[1] = новий вектор", array);
            Console.WriteLine($"Пошук початкового першого об'єкта через Array.IndexOf: індекс {Array.IndexOf(array, target)}");
            int removeIndex = Array.IndexOf(array, extra);
            for (int i = removeIndex; i < array.Length - 1; i++)
                array[i] = array[i + 1];
            Array.Resize(ref array, array.Length - 1);
            Print("Видалення доданого об'єкта: зсув і Array.Resize", array);

            Console.WriteLine("\n=== Неузагальнена колекція ArrayList ===");
            ArrayList nonGeneric = new ArrayList(source);
            Print("Початкова колекція", nonGeneric);
            nonGeneric.Add(extra);
            Print("Додавання: Add", nonGeneric);
            nonGeneric[1] = replacement;
            Print("Оновлення: nonGeneric[1] = новий вектор", nonGeneric);
            Console.WriteLine($"Пошук через IndexOf: індекс {nonGeneric.IndexOf(target)}");
            Vector found = (Vector)nonGeneric[0]!;
            Console.WriteLine($"Доступ до Length потребує приведення object до Vector: {found.Length:F2}");
            nonGeneric.Remove(extra);
            Print("Видалення: Remove", nonGeneric);

            Console.WriteLine("\n=== Узагальнена колекція List<Vector> ===");
            List<Vector> generic = new List<Vector>(source);
            Print("Початкова колекція", generic);
            generic.Add(extra);
            Print("Додавання: Add", generic);
            generic[1] = replacement;
            Print("Оновлення: generic[1] = новий вектор", generic);
            Console.WriteLine($"Пошук через IndexOf: індекс {generic.IndexOf(target)}");
            Console.WriteLine($"Доступ без приведення типу: {generic[0].Length:F2}");
            generic.Remove(extra);
            Print("Видалення: Remove", generic);

            Console.WriteLine("\nМасив має фіксований розмір: Array.Resize створює інший масив і копіює елементи.");
            Console.WriteLine("ArrayList зберігає object: тип елемента перевіряється під час виконання.");
            Console.WriteLine("List<Vector> приймає лише Vector: тип перевіряється під час компіляції.");
            Console.WriteLine("Обидва списки самі керують місткістю; Count не обов'язково дорівнює Capacity.");
            Console.WriteLine("Vector є класом, тому тут зберігаються посилання, без boxing.");
            Console.WriteLine("Boxing виникне, наприклад, якщо додати int до ArrayList.");
            Console.WriteLine("IndexOf шукає саме передане посилання: Equals у Vector не перевизначено.");
            Console.WriteLine("Усі наведені набори виводяться через foreach; початковий масив не змінено.");
        }

        public static void ShowTree(Vector[] source)
        {
            BinarySearchTree<Vector> tree = new BinarySearchTree<Vector>();
            foreach (Vector vector in source)
                tree.Add(vector);

            Console.WriteLine("\n=== Узагальнене бінарне дерево ===");
            Console.WriteLine("Порівняння за довжиною; за однакової довжини - X, Y, колір.");
            Console.WriteLine("Менші значення ліворуч, більші або рівні праворуч.");
            Print("Власний ітератор, preorder: корінь -> ліве піддерево -> праве", tree);
            Console.WriteLine($"Кількість: {tree.Count}; пошук першого вектора: {tree.Contains(source[0])}");

            Print("ToSortedArray: Array.Sort із критерієм дерева (за довжиною)", tree.ToSortedArray());
            List<Vector> sorted = new List<Vector>(source);
            sorted.Sort();
            Print("List.Sort(): IComparable<Vector>, за довжиною", sorted);
            sorted.Sort(new VectorColorComparer());
            Print("List.Sort(IComparer<Vector>): за кольором", sorted);

            BinarySearchTree<Vector> colorTree = new BinarySearchTree<Vector>(new VectorColorComparer());
            foreach (Vector vector in source)
                colorTree.Add(vector);
            Print("Інший IComparer у конструкторі дерева: сортування за кольором", colorTree.ToSortedArray());

            BinarySearchTree<string> words = new BinarySearchTree<string>(StringComparer.Ordinal);
            words.Add("B");
            words.Add("A");
            words.Add("C");
            Print("Той самий клас дерева для string, preorder", words);
            Console.WriteLine("Preorder визначає порядок обходу, а Sort - порядок сортування. Це різні операції.");
        }

        public static void Print(string title, IEnumerable items)
        {
            Console.WriteLine($"\n{title}:");
            int index = 0;
            foreach (object? item in items)
                Console.WriteLine($"  [{index++}] {item}");
        }
    }
}
