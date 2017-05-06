﻿using Enigma.D3.MemoryModel.Collections;
using Enigma.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enigma.D3.ApplicationModel
{
    public class ContainerObserver<T>
    {
        public int State = 0;
        public Container<T> Container;
        public byte[] CurrentData = new byte[0];
        public byte[] PreviousData = new byte[0];
        public int[] PreviousMapping = new int[0];
        public int[] CurrentMapping = new int[0];
        public T[] PreviousItems;
        public T[] CurrentItems;

        public ContainerObserver(Container<T> container)
        {
            Container = container ?? throw new ArgumentNullException(nameof(container));
        }

        public event Action<int, T> ItemRemoved;
        public event Action<int, T> ItemAdded;

        public void Update()
        {
            Container.TakeSnapshot();

            if (PreviousData.Length != CurrentData.Length)
                Array.Resize(ref PreviousData, CurrentData.Length);
            Buffer.BlockCopy(CurrentData, 0, PreviousData, 0, CurrentData.Length);
            Container.GetAllocatedBytes(ref CurrentData);


            if (PreviousMapping.Length != CurrentMapping.Length)
                Array.Resize(ref PreviousMapping, CurrentMapping.Length);
            Buffer.BlockCopy(CurrentMapping, 0, PreviousMapping, 0, CurrentMapping.Length * sizeof(int));


            var count = CurrentData.Length / Container.ItemSize;
            var mr = new BufferMemoryReader(CurrentData);
            if (CurrentMapping.Length != count)
                Array.Resize(ref CurrentMapping, count);
            for (int i = 0; i < count; i++)
                CurrentMapping[i] = mr.Read<int>(i * Container.ItemSize);


            if (PreviousMapping.Length == CurrentMapping.Length)
            {
                for (int i = 0; i < CurrentMapping.Length; i++)
                {
                    if (CurrentMapping[i] != PreviousMapping[i])
                    {
                        if (PreviousMapping[i] != -1)
                            OnItemRemoved(i);
                        if (CurrentMapping[i] != -1)
                            OnItemAdded(i);
                    }
                }
            }


            //var state = (Container.NextHash << 16) + Container.NextIndex;
            //if (state != State)
            //	OnCollectionModified();
            //State = state;
        }

        private void OnItemRemoved(int index)
        {
            ItemRemoved?.Invoke(index, default(T));
        }

        private void OnItemAdded(int index)
        {
            ItemAdded?.Invoke(index, default(T));
        }

        private void OnCollectionModified()
        {
            Console.WriteLine(Container.Name + " modified");
        }
    }
}
