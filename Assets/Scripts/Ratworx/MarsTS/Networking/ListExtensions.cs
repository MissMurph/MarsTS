using System.Collections.Generic;
using System.Linq;
using Unity.Collections;

namespace Ratworx.MarsTS.Networking
{
    public static class ListExtensions
    {
        public static NativeArray<FixedString32Bytes> ToNativeArray32(this ICollection<string> collection)
        {
            var serialized = new NativeArray<FixedString32Bytes>(collection.Count, Allocator.Temp);
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

        public static List<string> ToStringList(this NativeArray<FixedString32Bytes> nativeArray) 
            => nativeArray.Select(fixedString => fixedString.ToString()).ToList();
    }
}