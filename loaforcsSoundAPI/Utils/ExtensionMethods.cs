using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace loaforcsSoundAPI.Utils {
    public static class ExtensionMethods {
        public static T GetValueOrDefault<T>(this JObject jsonObject, string key, T defaultValue = default) {
            JToken token = jsonObject?.GetValue(key, StringComparison.OrdinalIgnoreCase);
            return token != null ? token.ToObject<T>() : defaultValue;
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue) {
            return dictionary.TryGetValue(key, out var value) ? value : defaultValue;
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, Func<TValue> defaultValueProvider) {
            return dictionary.TryGetValue(key, out var value) ? value : defaultValueProvider();
        }

        public static bool IsNumber(this object @object) {
            return @object is int || @object is double || @object is float;
        }
    }
}
