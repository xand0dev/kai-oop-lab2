using System;
using System.Collections;
using System.Collections.Generic;

namespace Lab2.Collections
{
    public sealed class BinarySearchTree<T> : IEnumerable<T> where T : class
    {
        private sealed class Node
        {
            public T Value { get; }
            public Node? Left { get; set; }
            public Node? Right { get; set; }

            public Node(T value) { Value = value; }
        }

        private Node? _root;
        private readonly IComparer<T> _comparer;
        private int _version;
        public int Count { get; private set; }

        public BinarySearchTree(IComparer<T>? comparer = null)
        {
            if (comparer == null && !typeof(IComparable<T>).IsAssignableFrom(typeof(T))
                && !typeof(IComparable).IsAssignableFrom(typeof(T)))
                throw new ArgumentException("Для цього типу потрібно передати IComparer<T>.", nameof(comparer));
            _comparer = comparer ?? Comparer<T>.Default;
        }

        public void Add(T item)
        {
            ArgumentNullException.ThrowIfNull(item);
            Node node = new Node(item);
            if (_root == null)
                _root = node;
            else
            {
                Node current = _root;
                while (true)
                {
                    if (_comparer.Compare(item, current.Value) < 0)
                    {
                        if (current.Left == null) { current.Left = node; break; }
                        current = current.Left;
                    }
                    else
                    {
                        // Рівні за критерієм об'єкти зберігаються в правій гілці.
                        if (current.Right == null) { current.Right = node; break; }
                        current = current.Right;
                    }
                }
            }
            Count++;
            _version++;
        }

        public bool Contains(T item)
        {
            ArgumentNullException.ThrowIfNull(item);
            Node? current = _root;
            while (current != null)
            {
                int result = _comparer.Compare(item, current.Value);
                if (result == 0) return true;
                current = result < 0 ? current.Left : current.Right;
            }
            return false;
        }

        public T[] ToSortedArray()
        {
            T[] result = new T[Count];
            int index = 0;
            foreach (T item in this)
                result[index++] = item;
            Array.Sort(result, _comparer);
            return result;
        }

        public IEnumerator<T> GetEnumerator() { return new PreorderEnumerator(this); }
        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

        private sealed class PreorderEnumerator : IEnumerator<T>
        {
            private readonly BinarySearchTree<T> _tree;
            private readonly int _version;
            private readonly Stack<Node> _pending = new Stack<Node>();
            private Node? _current;

            public PreorderEnumerator(BinarySearchTree<T> tree)
            {
                _tree = tree;
                _version = tree._version;
                Reset();
            }

            public T Current
            {
                get
                {
                    CheckVersion();
                    if (_current == null)
                        throw new InvalidOperationException("Ітератор не вказує на елемент.");
                    return _current.Value;
                }
            }

            object IEnumerator.Current { get { return Current; } }

            public bool MoveNext()
            {
                CheckVersion();
                if (_pending.Count == 0)
                {
                    _current = null;
                    return false;
                }

                _current = _pending.Pop();
                // Стек LIFO: права гілка кладеться першою, щоб ліву прочитати раніше.
                if (_current.Right != null) _pending.Push(_current.Right);
                if (_current.Left != null) _pending.Push(_current.Left);
                return true;
            }

            public void Reset()
            {
                CheckVersion();
                _current = null;
                _pending.Clear();
                if (_tree._root != null) _pending.Push(_tree._root);
            }

            public void Dispose()
            {
                _pending.Clear();
                _current = null;
            }

            private void CheckVersion()
            {
                if (_version != _tree._version)
                    throw new InvalidOperationException("Дерево змінено під час обходу.");
            }
        }
    }
}
