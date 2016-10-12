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

// The namespace Gsiot.Contracts provides a Contract class that supports a
// simple form of code contracts. Code contracts, originally proposed by
// Bertrand Meyer under the name "design by contract", allow to explicitly
// and clearly specify the "rights" and "duties" of methods: what a method
// can assume when it is called (a right, also called "precondition"), and
// what it must establish when it completes (a duty, also called
// "postcondition").
// Contracts serve as unambiguous documentation of a method's semantics on
// the one hand. On the other hand, they enable automatic checks, thereby
// making it easier to unearth bugs as early as possible.

//Developed for the book
//  "Getting Started with the Internet of Things", by Cuno Pfister.
//  Copyright 2011 Cuno Pfister, Inc., 978-1-4493-9357-1.
//
//Version 0.9 (beta release)

using System;

namespace Gsiot.Contracts
{

    public static class Contract
    {
        /// <summary>
        /// Throws an exception if a precondition is violated.
        /// </summary>
        /// <param name="condition">Precondition to be checked.</param>
        public static void Requires(bool condition)
        {
            if (!condition)
            {
                throw new ArgumentException("Precondition violated");
            }
        }

        /// <summary>
        /// Throws an exception if a postcondition is violated.
        /// </summary>
        /// <param name="condition">Postcondition to be checked.</param>
        public static void Ensures(bool condition)
        {
            if (!condition)
            {
                throw new Exception("Postcondition violated");
            }
        }

        /// <summary>
        /// Throws an exception if a condition is violated,
        /// e.g., a loop invariant or an object invariant.
        /// </summary>
        /// <param name="condition">Condition to be checked, e.g.,
        /// loop or object invariant.</param>
        public static void Assert(bool condition)
        {
            if (!condition) { throw new Exception("Assertion violated"); }
        }
    }
}
