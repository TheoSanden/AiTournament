using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI
{
    public class Blackboard : Dictionary<string, object>
    {
        #region Properties

        public IEnumerable<KeyValuePair<string, object>> Items => this;

        #endregion

        public T GetValue<T>(string key, T defaultValue)
        {
            object value;
            if (!string.IsNullOrEmpty(key))
            {
                if (TryGetValue(key, out value))
                {
                    if (value is T returnValue)
                    {
                        return returnValue;
                    }
                }
            }

            return defaultValue;
        }

        public void SetValue<T>(string key, T value)
        {
            if (!string.IsNullOrEmpty(key))
            {
                if (value == null)
                {
                    if (ContainsKey(key))
                    {
                        Remove(key);
                    }
                }
                else
                {
                    this[key] = value;
                }
            }
        }

        public void RemoveValue(string key)
        {
            if (!string.IsNullOrEmpty(key) && ContainsKey(key))
            {
                Remove(key);
            }
        }
    }
}
