using System.Collections.Generic;
using UnityEngine;


namespace BehaviourTree
{
    class Blackboard
    {
        Dictionary<uint, object> _members;
        Blackboard()
        {
            _members = new Dictionary<uint, object>();
        }

        // Accessors
        // As object
        object GetData(uint key)
        {
            return _members[key];
        }
        object GetData(string keyString)
        {
            uint key = FNV1Hash.Calculate(keyString);
            return _members[key];
        }
        // As specific type
        T GetAs<T>(uint key)
        {
            Debug.Assert(_members[key].GetType() == typeof(T), string.Format("Tried to get a {} as a bool", _members[key].GetType()));
            return (T)_members[key];
        }
        T GetAs<T>(string keyString)
        {
            uint key = FNV1Hash.Calculate(keyString);
            return GetAs<T>(key);
        }
        // Mutators
        void Set(uint key, object value)
        {
            _members[key] = value;
        }
        void Set(string keyString, object value)
        {
            uint key = FNV1Hash.Calculate(keyString);
            Set(key, value);
        }
        void Remove(uint key)
        {
            _members.Remove(key);
        }
        void Remove(string keyString)
        {
            uint key = FNV1Hash.Calculate(keyString);
            Remove(key);
        }
    }
}
