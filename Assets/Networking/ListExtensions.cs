using System.Collections;
using System.Collections.Generic;
using Unity.Collections;

namespace MarsTS.Networking
{
    public static class ListExtensions
    {
        public static NativeArray<FixedString32Bytes> ToNativeArray32(this ICollection<string> collection)
        {
            NativeArray<FixedString32Bytes> serialized = new NativeArray<FixedString32Bytes>(collection.Count, Allocator.Temp);
            IEnumerator<string> yeet = collection.GetEnumerator();

            int index = 0;

            while (yeet.MoveNext())
            {
                serialized[index] = yeet.Current;
                index++;
            }
            
            yeet.Dispose();
            return serialized;
        }

        public static List<string> ToList(this NativeArray<FixedString32Bytes> nativeArray)
        {
            List<string> output = new(nativeArray.Length);

            for (int i = 0; i < nativeArray.Length; i++)
            {
                output[i] = nativeArray[i].ToString();
            }

            return output;
        }
    }
}