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
        /// The node represents a list of <see cref="ByamlPathPoint"/> instances (internally referenced by index).
        /// Only present in Mario Kart 8's BYAML files.
        /// </summary>
        PathIndex = 0xA1,
        
        /// <summary>
        /// The node represents binary data.
        /// Reuses the same node type as <see cref="PathIndex"/>, but only present in BYAML version 4.
        /// </summary>
        BinaryData = PathIndex,

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
        /// The node represents an array of lists of <see cref="ByamlPathPoint"/> instances.
        /// Only present in Mario Kart 8's BYAML files.
        /// </summary>
        PathArray = 0xC3,

        /// <summary>
        /// The node represents a <see cref="bool"/>.
        /// </summary>
        Boolean = 0xD0,

        /// <summary>
        /// The node represents an <see cref="int"/>.
        /// </summary>
        Integer = 0xD1,

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

    /// <summary>
    /// Represents extension methods for <see cref="ByamlNodeType"/> instances.
    /// </summary>
    internal static class ByamlNodeTypeExtensions
    {
        /// <summary>
        /// Gets the corresponding, instantiatable <see cref="Type"/> for the given <paramref name="nodeType"/>.
        /// </summary>
        /// <param name="nodeType">The <see cref="ByamlNodeType"/> which should be instantiated.</param>
        /// <returns>The <see cref="Type"/> to instantiate for the node.</returns>
        internal static Type GetInstanceType(this ByamlNodeType nodeType, ByamlVersion version)
        {
            switch (nodeType)
            {
                case ByamlNodeType.StringIndex:
                    return typeof(string);
                case ByamlNodeType.PathIndex:
                    if (version == ByamlVersion.Four)
                    {
                        return typeof(byte[]);
                    }
                    else if (version == ByamlVersion.One)
                    {
                        return typeof(List<ByamlPathPoint>);
                    }

                    throw new ByamlException($"Invalid node type {nodeType} for version {version}.");
                case ByamlNodeType.Array:
                    // TODO: Check if this could be loaded as an object array.
                    throw new ByamlException("Cannot instantiate an array of unknown element type.");
                case ByamlNodeType.Dictionary:
                    // TODO: Check if this could be loaded as a string-object dictionary.
                    throw new ByamlException("Cannot instantiate an object of unknown type.");
                case ByamlNodeType.Boolean:
                    return typeof(bool);
                case ByamlNodeType.Integer:
                    return typeof(int);
                case ByamlNodeType.Float:
                    return typeof(float);
                case ByamlNodeType.Null:
                    return typeof(object);
                default:
                    throw new ByamlException($"Unknown node type {nodeType}.");
            }
        }
    }
}
