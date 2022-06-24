using System.Collections.Generic;
using System.Reflection;

namespace Int19h.Bannerlord.PettyKingdoms {
    internal static class Extensions {
        public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> pair, out TKey key, out TValue value) {
            key = pair.Key;
            value = pair.Value;
        }

        public static void SetProperty(this object obj, string name, object value) =>
            obj.GetType().InvokeMember(
                "set_" + name,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod,
                null,
                obj,
                new object[] { value }
            );
    }
}
