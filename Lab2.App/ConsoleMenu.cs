using System;
using System.Globalization;
using Lab2.Domain;

namespace Lab2.App
{
    internal sealed class ConsoleMenu
    {
        private Vector[] _vectors = Demonstration.CreateVectors();

        public void Run()
        {
            Console.WriteLine("ЛР 3.2 | Варіант 7 | Вектор");
            try
            {
                while (true)
                {
                    Console.WriteLine("\n1 - Повна демонстрація\n2 - Ввести 3 вектори\n3 - Показати вектори\n4 - Збільшити вектор на відрізок\n5 - Операції з колекціями\n6 - Дерево та сортування\n7 - Відновити приклад\n0 - Вихід");
                    string choice = ReadText("Оберіть пункт: ");
                    switch (choice)
                    {
                        case "1": Demonstration.RunAll(_vectors); break;
                        case "2": ReadVectors(); break;
                        case "3": Demonstration.Print("Вектори", _vectors); break;
                        case "4": ExtendVector(); break;
                        case "5": Demonstration.ShowCollections(_vectors); break;
                        case "6": Demonstration.ShowTree(_vectors); break;
                        case "7": _vectors = Demonstration.CreateVectors(); Console.WriteLine("Приклад відновлено."); break;
                        case "0": return;
                        default: Console.Error.WriteLine("Введіть номер пункту від 0 до 7."); break;
                    }
                }
            }
            catch (System.IO.EndOfStreamException)
            {
                Console.WriteLine("\nВведення завершено.");
            }
        }

        private void ReadVectors()
        {
            Vector[] entered = new Vector[3];
            for (int i = 0; i < entered.Length; i++)
            {
                while (true)
                {
                    Console.WriteLine($"\nВектор {i + 1}, початок у (0; 0)");
                    string color = ReadText("Колір лінії: ");
                    double x = ReadNumber("X: ");
                    double y = ReadNumber("Y: ");
                    try
                    {
                        entered[i] = new Vector(color, x, y);
                        break;
                    }
                    catch (ArgumentException error)
                    {
                        Console.Error.WriteLine(error.Message);
                    }
                }
            }
            _vectors = entered;
            Demonstration.Print("Збережено 3 вектори", _vectors);
        }

        private void ExtendVector()
        {
            Demonstration.Print("Вектори (індекси 0, 1, 2)", _vectors);
            string text = ReadText("Індекс вектора: ");
            if (!int.TryParse(text, out int index) || index < 0 || index >= _vectors.Length)
            {
                Console.Error.WriteLine("Індекс має бути 0, 1 або 2.");
                return;
            }
            double distance = ReadNumber("На скільки збільшити довжину: ");
            try
            {
                _vectors[index] = _vectors[index].Extend(distance);
                Console.WriteLine(_vectors[index]);
            }
            catch (ArgumentException error) { Console.Error.WriteLine(error.Message); }
            catch (InvalidOperationException error) { Console.Error.WriteLine(error.Message); }
        }

        private static string ReadText(string prompt)
        {
            Console.Write(prompt);
            string? text = Console.ReadLine();
            if (text == null) throw new System.IO.EndOfStreamException();
            return text.Trim();
        }

        private static double ReadNumber(string prompt)
        {
            while (true)
            {
                string text = ReadText(prompt).Replace(',', '.');
                if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double value)
                    && double.IsFinite(value))
                    return value;
                Console.Error.WriteLine("Введіть скінченне число, наприклад 3,5 або -2.5.");
            }
        }
    }
}
