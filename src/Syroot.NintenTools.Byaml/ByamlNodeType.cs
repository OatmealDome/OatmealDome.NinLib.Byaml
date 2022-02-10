using System;
using System.Collections.Generic;

namespace OatmealDome.NinLib.Byaml
{
    /// <summary>
    /// Represents the type of which a dynamic BYAML node can be.
    /// </summary>
    internal enum ByamlNodeType : byte
    {
        /// <summary>
        /// The node represents a <see cref="string"/> (internally referenced by index).
        /// </summary>
        StringIndex = 0xA0,

        /// <summary>
        /// The node represents an array of <see cref="byte"/>s.
        /// Only present in Mario Kart 8's BYAML files and BYAML versions 4 and newer.
        /// </summary>
        BinaryData = 0xA1,

        /// <summary>
        /// The node represents an array of dynamic child instances.
        /// </summary>
        Array = 0xC0,

        /// <summary>
        /// The node represents a dictionary of dynamic child instances referenced by a <see cref="string"/> key.
        /// </summary>
        Dictionary = 0xC1,

        /// <summary>
        /// The node represents an array of <see cref="string"/> instances.
        /// </summary>
        StringArray = 0xC2,

        /// <summary>
        /// The node represents an array of binary data.
        /// Only present in Mario Kart 8's BYAML files.
        /// </summary>
        BinaryDataArray = 0xC3,

        /// <summary>
        /// The node represents a <see cref="bool"/>.
        /// </summary>
        Boolean = 0xD0,

        /// <summary>
        /// The node represents an <see cref="int"/>.
        /// </summary>
        Int32 = 0xD1,

        /// <summary>
        /// The node represents a <see cref="float"/>.
        /// </summary>
        Float = 0xD2,
        
        /// <summary>
        /// The node represents a <see cref="uint"/>.
        /// </summary>
        UInt32 = 0xD3,
        
        /// <summary>
        /// The note represents a <see cref="long"/>.
        /// </summary>
        Int64 = 0xD4,
        
        /// <summary>
        /// The node represents a <see cref="ulong"/>.
        /// </summary>
        UInt64 = 0xD5,
        
        /// <summary>
        /// The node represents a <see cref="double"/>.
        /// </summary>
        Double = 0xD6,

        /// <summary>
        /// The node represents <c>null</c>.
        /// </summary>
        Null = 0xFF
    }
}
