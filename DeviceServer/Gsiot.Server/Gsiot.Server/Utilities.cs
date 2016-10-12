//Copyright 2011 Oberon microsystems, Inc.
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

//Developed for the book
//  "Getting Started with the Internet of Things", by Cuno Pfister.
//  Copyright 2011 Cuno Pfister, Inc., 978-1-4493-9357-1.
//
//Version 0.9 (beta release)

// The internal class Utilities provides a string-to-integer conversion
// implementation for unsigned 32-bit integers.

// TODO is there already an implementation in the NETMF core classes?

using Gsiot.Contracts;

namespace Gsiot.Server
{
    internal class Utilities
    {
        internal static bool TryParseUInt32(string s, out int result)
        {
            Contract.Requires(s != null);
            result = 0;
            if (s.Length > 0)
            {
                var r = 0;
                foreach (char c in s)
                {
                    if ((c < '0') || (c > '9')) { return false; }
                    var n = (int)(c - '0');
                    r = (r * 10) + n;
                }
                result = r;
                return true;
            }
            return false;
        }
    }
}
