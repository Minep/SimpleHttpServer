using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace HttpServer.Common
{
    public class Heap<T> : IEnumerable<T> where T : IComparable
    {
        private readonly object heapLock = new object();

        List<T> heapArray = new List<T>();

        bool isMinHeap = false;

        public Heap(T[] array, bool isMinHeap = true)
        {
            heapArray.AddRange(array);
            this.isMinHeap = isMinHeap;
            BuildHeap();
        }

        public Heap(List<T> array, bool isMinHeap = true)
        {
            heapArray.AddRange(array);
            this.isMinHeap = isMinHeap;
            BuildHeap();
        }

        public Heap(bool isMinHeap, params T[] args)
        {
            heapArray.AddRange(args);
            this.isMinHeap = isMinHeap;
            BuildHeap();
        }

        public Heap(bool isMinHeap)
        {
            this.isMinHeap = isMinHeap;
        }

        public int Count => heapArray.Count;

        public void Append(T element)
        {
            lock(heapLock)
            {
                heapArray.Add(element);
                //Swim(heapArray.Count);
            }
        }

        public T ExtractTop()
        {
            return ExtractAt(0);
        }

        public List<T> ExtractSuccessiveTop(int n)
        {
            lock(heapLock)
            {
                List<T> result = new List<T>();
                BuildHeap();
                while (n > 0 && heapArray.Count > 0)
                {
                    result.Add(heapArray[0]);
                    RemoveOne();
                    n--;
                }
                return result;
            }
        }

        public T RemoveWhere(Predicate<T> predicate) {
            int index = heapArray.FindIndex(predicate);
            if (index >= 0) {
                return ExtractAt(index);
            }
            return default;
        }

        public T ExtractAt(int index)
        {
            lock (heapLock)
            {
                BuildHeap();
                T top = heapArray[index];
                RemoveOne(index);
                return top;
            }
        }

        private void RemoveOne(int index = 0)
        {
            int last = heapArray.Count - 1;
            heapArray[index] = heapArray[last];
            heapArray.RemoveAt(last);
            Sink(index + 1);
        }

        private void Swim(int index)
        {
            if (index > heapArray.Count)
            {
                return;
            }

            int current = index;
            while (current > 1 && compare(current / 2, current))
            {
                swap(current, current / 2);
                current = current / 2;
            }
        }

        private void Sink(int index, int N = -1)
        {
            N = N == -1 ? heapArray.Count : N;
            if (index > N)
            {
                return;
            }

            int current = index;
            while (2 * current <= N)
            {
                int inx = current * 2;
                if (inx < N && compare(inx, inx + 1))
                {
                    inx++;
                }
                if (!compare(current, inx))
                {
                    break;
                }
                swap(current, inx);
                current = inx;
            }
        }

        private void BuildHeap()
        {
            for (int i = heapArray.Count / 2; i >= 1; i--)
            {
                Sink(i);
            }
        }

        private bool compare(int a, int b)
        {
            if (isMinHeap)
            {
                return heapArray[a - 1].CompareTo(heapArray[b - 1]) > 0;
            }
            else
            {
                return heapArray[a - 1].CompareTo(heapArray[b - 1]) < 0;
            }
        }

        private void swap(int a, int b)
        {
            T temp = heapArray[a - 1];
            heapArray[a - 1] = heapArray[b - 1];
            heapArray[b - 1] = temp;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return heapArray.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return heapArray.GetEnumerator();
        }
    }
}
