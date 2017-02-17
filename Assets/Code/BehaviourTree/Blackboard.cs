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
        object GetData(uint inKey)
        {
            return _members[inKey];
        }
        object GetData(string inKeyString)
        {
            uint key = FNV1Hash.Calculate(inKeyString);
            return _members[key];
        }
        // As specific type
        T GetAs<T>(uint inKey)
        {
            Debug.Assert(_members[inKey].GetType() == typeof(T), string.Format("Tried to get a {} as a {}", _members[inKey].GetType(), typeof(T)));
            return (T)_members[inKey];
        }
        T GetAs<T>(string inKeyString)
        {
            uint key = FNV1Hash.Calculate(inKeyString);
            return GetAs<T>(key);
        }

        // Mutators
        void Set(uint inKey, object inValue)
        {
            _members[inKey] = inValue;
        }
        void Set(string inKeyString, object inValue)
        {
            uint key = FNV1Hash.Calculate(inKeyString);
            Set(key, inValue);
        }
        void Remove(uint inKey)
        {
            _members.Remove(inKey);
        }
        void Remove(string inKeyString)
        {
            uint key = FNV1Hash.Calculate(inKeyString);
            Remove(key);
        }
        //// Privates
        //Dictionary<uint, object> _GetTreeMemory(uint inTreeIDHash)
        //{
        //    object value;
        //    if (!_members.TryGetValue(inTreeIDHash, out value))
        //    {
        //        value = new Dictionary<uint, object>();
        //        _members[inTreeIDHash] = value;
        //    }
        //    return (Dictionary<uint, object>)value ;
        //}
        //Dictionary<uint, object> _GetNodeMemory(Dictionary<uint, object> )

        //void _Set(uint key, object value, System.Guid treeID)
        //{
        //    uint treeIDHash = (uint)treeID.GetHashCode();
        //    Dictionary<uint, object> treeMembers = _members[treeIDHash] as Dictionary<uint, object>;
        //    if (treeMembers == null)
        //    {
        //        treeMembers = new Dictionary<uint, object>();
        //    }
        //    treeMembers[key] = value;
        //}
        //void _Set(uint key, object value, System.Guid treeID, System.Guid nodeID)
        //{
        //    uint treeIDHash = (uint)treeID.GetHashCode();
        //    Dictionary<uint, object> treeMembers = _members[treeIDHash] as Dictionary<uint, object>;
        //    if (treeMembers == null)
        //    {
        //        treeMembers = new Dictionary<uint, object>();
        //    }
        //    treeMembers[key] = value;
        //}
    }
