using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BehaviourTree
{
    class FNV1Hash
    {
        public static uint Calculate(string inHashString)
        {
            const uint fnvPrime = 0x01000193;
            const uint offsetBasis = 0x811C9DC5;
            uint hash = offsetBasis;

            foreach (char character in inHashString)
            {
                hash *= fnvPrime;
                hash ^= character;
            }
            return hash;
        }
    }
}
