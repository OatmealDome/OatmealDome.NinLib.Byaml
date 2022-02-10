using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using OatmealDome.BinaryData;

namespace OatmealDome.NinLib.Byaml.Dynamic
{
    /// <summary>
    /// Represents the loading and saving logic of BYAML files and returns the resulting file structure in dynamic
    /// objects.
    /// </summary>
    public class ByamlFile
    {
        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        private const ushort _magicBytes = 0x4259; // "BY"

        // ---- MEMBERS ------------------------------------------------------------------------------------------------

        private readonly ByamlSerializerSettings _settings;

        private ByamlVersion _currentReadVersion;

        private List<string> _nameArray;
        private List<string> _stringArray;
        private List<byte[]> _binaryDataArray;

        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        private ByamlFile(ByamlSerializerSettings settings)
        {
            _settings = settings;
        }

        // ---- METHODS (PUBLIC) ---------------------------------------------------------------------------------------
        
        /// <summary>
        /// Deserializes and returns the dynamic value of the BYAML node read from the given file.
        /// </summary>
        /// <param name="fileName">The name of the file to read the data from.</param>
        /// <param name="settings">The <see cref="ByamlSerializerSettings"/> used to configure how the BYAML will be
        /// deserialized.</param>
        public static dynamic Load(string fileName, ByamlSerializerSettings settings)
        {
            using (FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return Load(stream, settings);
            }
        }

        /// <summary>
        /// Deserializes and returns the dynamic value of the BYAML node read from the specified stream.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to read the data from.</param>
        /// <param name="settings">The <see cref="ByamlSerializerSettings"/> used to configure how the BYAML will be
        /// deserialized.</param>
        public static dynamic Load(Stream stream, ByamlSerializerSettings settings)
        {
            ByamlFile byamlFile = new ByamlFile(settings);
            return byamlFile.Read(stream);
        }

        /// <summary>
        /// Serializes the given dynamic value which requires to be an array or dictionary of BYAML compatible values
        /// and stores it in the given file.
        /// </summary>
        /// <param name="fileName">The name of the file to store the data in.</param>
        /// <param name="root">The dynamic value becoming the root of the BYAML file. Must be an array or dictionary of
        /// BYAML compatible values.</param>
        /// <param name="settings">The <see cref="ByamlSerializerSettings"/> used to configure how the BYAML will be
        /// serialized.</param>
        public static void Save(string fileName, dynamic root, ByamlSerializerSettings settings)
        {
            using (FileStream stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                Save(stream, root, settings);
            }
        }

        /// <summary>
        /// Serializes the given dynamic value which requires to be an array or dictionary of BYAML compatible values
        /// and stores it in the specified stream.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to store the data in.</param>
        /// <param name="root">The dynamic value becoming the root of the BYAML file. Must be an array or dictionary of
        /// BYAML compatible values.</param>
        /// <param name="settings">The <see cref="ByamlSerializerSettings"/> used to configure how the BYAML will be
        /// serialized.</param>
        public static void Save(Stream stream, dynamic root, ByamlSerializerSettings settings)
        {
            ByamlFile byamlFile = new ByamlFile(settings);
            byamlFile.Write(stream, root);
        }

        // ---- Helper methods ----

        /// <summary>
        /// Tries to retrieve the value of the element with the specified <paramref name="key"/> stored in the given
        /// dictionary <paramref name="node"/>. If the key does not exist, <c>null</c> is returned.
        /// </summary>
        /// <param name="node">The dictionary BYAML node to retrieve the value from.</param>
        /// <param name="key">The key of the value to retrieve.</param>
        /// <returns>The value stored under the given key or <c>null</c> if the key is not present.</returns>
        public static dynamic GetValue(IDictionary<string, dynamic> node, string key)
        {
            return node.TryGetValue(key, out dynamic value) ? value : null;
        }

        /// <summary>
        /// Sets the given <paramref name="value"/> in the provided dictionary <paramref name="node"/> under the
        /// specified <paramref name="key"/>. If the value is <c>null</c>, the key is removed from the dictionary node.
        /// </summary>
        /// <param name="node">The dictionary node to store the value under.</param>
        /// <param name="key">The key under which the value will be stored or which will be removed.</param>
        /// <param name="value">The value to store under the key or <c>null</c> to remove the key.</param>
        public static void SetValue(IDictionary<string, dynamic> node, string key, dynamic value)
        {
            if (value == null)
            {
                node.Remove(key);
            }
            else
            {
                node[key] = value;
            }
        }

        /// <summary>
        /// Casts all elements of the given array <paramref name="node"/> into the provided type
        /// <typeparamref name="T"/>. If the node is <c>null</c>, <c>null</c> is returned.
        /// </summary>
        /// <typeparam name="T">The type to cast each element to.</typeparam>
        /// <param name="node">The array node which elements will be casted.</param>
        /// <returns>The list of type <typeparamref name="T"/> or <c>null</c> if the node is <c>null</c>.</returns>
        public static List<T> GetList<T>(IEnumerable<dynamic> node)
        {
            return node?.Cast<T>().ToList();
        }

        // ---- METHODS (PRIVATE) --------------------------------------------------------------------------------------

        // ---- Loading ----

        private dynamic Read(Stream stream)
        {
            // Open a reader on the given stream.
            using (BinaryDataReader reader = new BinaryDataReader(stream, Encoding.UTF8, true))
            {
                reader.ByteOrder = _settings.ByteOrder;

                // Load the header, specifying magic bytes ("BY"), version and main node offsets.
                if (reader.ReadUInt16() != _magicBytes)
                {
                    throw new ByamlException("Invalid BYAML header.");
                }
                
                ushort version = reader.ReadUInt16();
                if (version > (ushort)ByamlVersion.Four)
                {
                    throw new ByamlException($"Unsupported BYAML version '{version}'.");
                }
                
                if (_settings.Version.HasValue && (ByamlVersion)version != _settings.Version)
                {
                    throw new ByamlException($"Unexpected BYAML version '{version}'.");
                }

                if (version != (ushort)ByamlVersion.One && _settings.SupportsBinaryData)
                {
                    throw new ByamlException("SupportsBinaryData is only used with BYAML version 1.");
                }

                _currentReadVersion = (ByamlVersion)version;
                
                uint nameArrayOffset = reader.ReadUInt32();
                uint stringArrayOffset = reader.ReadUInt32();
                uint dataArrayOffset = 0;
                if (_settings.SupportsBinaryData)
                {
                    dataArrayOffset = reader.ReadUInt32();
                }
                uint rootNodeOffset = reader.ReadUInt32();

                // Read the name array, holding strings referenced by index for the names of other nodes.
                reader.Seek(nameArrayOffset, SeekOrigin.Begin);
                _nameArray = ReadNode(reader);

                // Read the optional string array, holding strings referenced by index in string nodes.
                if (stringArrayOffset != 0)
                {
                    reader.Seek(stringArrayOffset, SeekOrigin.Begin);
                    _stringArray = ReadNode(reader);
                }
                
                // Read the optional binary data array, holding data referenced by index in binary data nodes.
                if (_settings.SupportsBinaryData && dataArrayOffset != 0)
                {
                    // The third offset is the root node, so just read that and we're done.
                    reader.Seek(dataArrayOffset, SeekOrigin.Begin);
                    _binaryDataArray = ReadNode(reader);
                }

                
                // Read the root node.
                reader.Seek(rootNodeOffset, SeekOrigin.Begin);
                return ReadNode(reader);
            }
        }

        private dynamic ReadNode(BinaryDataReader reader, ByamlNodeType nodeType = 0)
        {
            // Read the node type if it has not been provided yet.
            bool nodeTypeGiven = nodeType != 0;
            if (!nodeTypeGiven)
            {
                nodeType = (ByamlNodeType)reader.ReadByte();
            }
            if (nodeType >= ByamlNodeType.Array && nodeType <= ByamlNodeType.BinaryDataArray)
            {
                // Get the length of arrays.
                long? oldPos = null;
                if (nodeTypeGiven)
                {
                    // If the node type was given, the array value is read from an offset.
                    uint offset = reader.ReadUInt32();
                    oldPos = reader.Position;
                    reader.Seek(offset, SeekOrigin.Begin);
                }
                else
                {
                    reader.Seek(-1);
                }
                int length = (int)Get3LsbBytes(reader.ReadUInt32());
                dynamic value = null;
                switch (nodeType)
                {
                    case ByamlNodeType.Array:
                        value = ReadArrayNode(reader, length);
                        break;
                    case ByamlNodeType.Dictionary:
                        value = ReadDictionaryNode(reader, length);
                        break;
                    case ByamlNodeType.StringArray:
                        value = ReadStringArrayNode(reader, length);
                        break;
                    case ByamlNodeType.BinaryDataArray:
                        value = ReadBinaryDataArrayNode(reader, length);
                        break;
                    default:
                        throw new ByamlException($"Unknown node type '{nodeType}'.");
                }
                // Seek back to the previous position if this was a value positioned at an offset.
                if (oldPos.HasValue)
                {
                    reader.Seek(oldPos.Value, SeekOrigin.Begin);
                }
                return value;
            }
            else
            {
                // Read the following UInt32 which is representing the value directly.
                switch (nodeType)
                {
                    case ByamlNodeType.StringIndex:
                        return _stringArray[reader.ReadInt32()];
                    case ByamlNodeType.BinaryData:
                        return _binaryDataArray[reader.ReadInt32()];
                    case ByamlNodeType.Boolean:
                        return reader.ReadInt32() != 0;
                    case ByamlNodeType.Int32:
                        return reader.ReadInt32();
                    case ByamlNodeType.Float:
                        return reader.ReadSingle();
                    case ByamlNodeType.UInt32:
                        EnforceMinimumVersion(nodeType, ByamlVersion.Two);
                        return reader.ReadUInt32();
                    case ByamlNodeType.Int64:
                        EnforceMinimumVersion(nodeType, ByamlVersion.Three);
                        return ReadComplexValueNode(reader, r => r.ReadInt64());
                    case ByamlNodeType.UInt64:
                        EnforceMinimumVersion(nodeType, ByamlVersion.Three);
                        return ReadComplexValueNode(reader, r => r.ReadUInt64());
                    case ByamlNodeType.Double:
                        EnforceMinimumVersion(nodeType, ByamlVersion.Three);
                        return ReadComplexValueNode(reader, r => r.ReadDouble());
                    case ByamlNodeType.Null:
                        reader.Seek(0x4);
                        return null;
                    default:
                        throw new ByamlException($"Unknown node type '{nodeType}'.");
                }
            }
        }

        private List<dynamic> ReadArrayNode(BinaryDataReader reader, int length)
        {
            List<dynamic> array = new List<dynamic>(length);

            // Read the element types of the array.
            byte[] nodeTypes = reader.ReadBytes(length);
            // Read the elements, which begin after a padding to the next 4 bytes.
            reader.Align(4);
            for (int i = 0; i < length; i++)
            {
                array.Add(ReadNode(reader, (ByamlNodeType)nodeTypes[i]));
            }

            return array;
        }

        private Dictionary<string, dynamic> ReadDictionaryNode(BinaryDataReader reader, int length)
        {
            Dictionary<string, dynamic> dictionary = new Dictionary<string, dynamic>();

            // Read the elements of the dictionary.
            for (int i = 0; i < length; i++)
            {
                uint indexAndType = reader.ReadUInt32();
                int nodeNameIndex = (int)Get3MsbBytes(indexAndType);
                ByamlNodeType nodeType = (ByamlNodeType)Get1MsbByte(indexAndType);
                string nodeName = _nameArray[nodeNameIndex];
                dictionary.Add(nodeName, ReadNode(reader, nodeType));
            }

            return dictionary;
        }

        private List<string> ReadStringArrayNode(BinaryDataReader reader, int length)
        {
            List<string> stringArray = new List<string>(length);

            // Read the element offsets.
            long nodeOffset = reader.Position - 4; // String offsets are relative to the start of node.
            uint[] offsets = reader.ReadUInt32s(length);

            // Read the strings by seeking to their element offset and then back.
            long oldPosition = reader.Position;
            for (int i = 0; i < length; i++)
            {
                reader.Seek(nodeOffset + offsets[i], SeekOrigin.Begin);
                stringArray.Add(reader.ReadString(StringDataFormat.ZeroTerminated));
            }
            reader.Seek(oldPosition, SeekOrigin.Begin);

            return stringArray;
        }

        private List<byte[]> ReadBinaryDataArrayNode(BinaryDataReader reader, int length)
        {
            List<byte[]> dataArray = new List<byte[]>(length);

            // Read the element offsets.
            long nodeOffset = reader.Position - 4; // Binary data offsets are relative to the start of node.
            uint[] offsets = reader.ReadUInt32s(length + 1);

            // Read the data by seeking to their element offset and then back.
            long oldPosition = reader.Position;
            for (int i = 0; i < length; i++)
            {
                reader.Seek(nodeOffset + offsets[i], SeekOrigin.Begin);
                int size = (int)(offsets[i + 1] - offsets[i]);
                dataArray.Add(reader.ReadBytes(size));
            }
            reader.Seek(oldPosition, SeekOrigin.Begin);

            return dataArray;
        }

        private dynamic ReadComplexValueNode(BinaryDataReader reader, Func<BinaryDataReader, dynamic> readFunc)
        {
            long offset = reader.ReadUInt32();
            using (reader.TemporarySeek(offset, SeekOrigin.Begin))
            {
                return readFunc(reader);
            }
        }

        // ---- Saving ----

        private void Write(Stream stream, object root)
        {
            // Check if the root is of the correct type.
            if (root == null)
            {
                throw new ByamlException("Root node must not be null.");
            }

            if (!(root is IDictionary<string, dynamic> || root is IEnumerable))
            {
                throw new ByamlException($"Type '{root.GetType()}' is not supported as a BYAML root node.");
            }
            
            if (!_settings.Version.HasValue)
            {
                throw new ByamlException(
                    "Version must be specified in ByamlSerializerSettings when serializing a BYAML.");
            }

            if (_settings.Version != ByamlVersion.One && _settings.SupportsBinaryData)
            {
                throw new ByamlException("SupportsBinaryData is only used with BYAML version 1.");
            }

            // Generate the name, string and data array nodes.
            _nameArray = new List<string>();
            _stringArray = new List<string>();
            _binaryDataArray = new List<byte[]>();
            CollectNodeArrayContents(root);
            _nameArray.Sort(StringComparer.Ordinal);
            _stringArray.Sort(StringComparer.Ordinal);

            // Open a writer on the given stream.
            using (BinaryDataWriter writer = new BinaryDataWriter(stream, Encoding.UTF8, true))
            {
                writer.ByteOrder = _settings.ByteOrder;

                // Write the header, specifying magic bytes, version and main node offsets.
                writer.Write(_magicBytes);
                writer.Write((ushort)_settings.Version);
                Offset nameArrayOffset = writer.ReserveOffset();
                Offset stringArrayOffset = writer.ReserveOffset();
                Offset dataArrayOffset = _settings.SupportsBinaryData ? writer.ReserveOffset() : null;
                Offset rootOffset = writer.ReserveOffset();

                // Write the main nodes.
                WriteValueContents(writer, nameArrayOffset, ByamlNodeType.StringArray, _nameArray);
                if (_stringArray.Count == 0)
                {
                    writer.Write(0);
                }
                else
                {
                    WriteValueContents(writer, stringArrayOffset, ByamlNodeType.StringArray, _stringArray);
                }

                // Include a data array offset if requested.
                if (_settings.SupportsBinaryData)
                {
                    if (_binaryDataArray.Count == 0)
                    {
                        writer.Write(0);
                    }
                    else
                    {
                        WriteValueContents(writer, dataArrayOffset, ByamlNodeType.BinaryDataArray, _binaryDataArray);
                    }
                }

                // Write the root node.
                WriteValueContents(writer, rootOffset, GetNodeType(root), root);
            }
        }

        private void CollectNodeArrayContents(dynamic node)
        {
            if (node is string)
            {
                if (!_stringArray.Contains((string)node))
                {
                    _stringArray.Add((string)node);
                }
            }
            else if (node is byte[])
            {
                _binaryDataArray.Add((byte[])node);
            }
            else if (node is IDictionary<string, dynamic>)
            {
                foreach (KeyValuePair<string, dynamic> entry in node)
                {
                    if (!_nameArray.Contains(entry.Key))
                    {
                        _nameArray.Add(entry.Key);
                    }
                    CollectNodeArrayContents(entry.Value);
                }
            }
            else if (node is IEnumerable)
            {
                foreach (dynamic childNode in node)
                {
                    CollectNodeArrayContents(childNode);
                }
            }
        }

        private Offset WriteValue(BinaryDataWriter writer, dynamic value)
        {
            // Only reserve and return an offset for the complex value contents, write simple values directly.
            ByamlNodeType type = GetNodeType(value);
            switch (type)
            {
                case ByamlNodeType.StringIndex:
                    WriteStringIndexNode(writer, value);
                    return null;
                case ByamlNodeType.BinaryData:
                    WriteBinaryDataIndexNode(writer, value);
                    return null;
                case ByamlNodeType.Dictionary:
                case ByamlNodeType.Array:
                    return writer.ReserveOffset();
                case ByamlNodeType.Boolean:
                    writer.Write(value ? 1 : 0);
                    return null;
                case ByamlNodeType.Int32:
                case ByamlNodeType.Float:
                    writer.Write(value);
                    return null;
                case ByamlNodeType.UInt32:
                    EnforceMinimumVersion(type, ByamlVersion.Two);

                    writer.Write(value);
                    return null;
                case ByamlNodeType.Int64:
                case ByamlNodeType.UInt64:
                case ByamlNodeType.Double:
                    EnforceMinimumVersion(type, ByamlVersion.Three);
                    return writer.ReserveOffset();
                case ByamlNodeType.Null:
                    writer.Write(0x0);
                    return null;
                default:
                    throw new ByamlException($"{type} not supported as value node.");
            }
        }

        private void WriteValueContents(BinaryDataWriter writer, Offset offset, ByamlNodeType type, dynamic value)
        {
            // Satisfy the offset to the complex node value which must be 4-byte aligned.
            writer.Align(4);
            offset.Satisfy();

            // Write the value contents.
            switch (type)
            {
                case ByamlNodeType.Dictionary:
                    WriteDictionaryNode(writer, value);
                    break;
                case ByamlNodeType.StringArray:
                    WriteStringArrayNode(writer, value);
                    break;
                case ByamlNodeType.BinaryDataArray:
                    WriteBinaryDataArrayNode(writer, value);
                    break;
                case ByamlNodeType.Array:
                    WriteArrayNode(writer, value);
                    break;
                case ByamlNodeType.Int64:
                case ByamlNodeType.UInt64:
                case ByamlNodeType.Double:
                    writer.Write(value);
                    break;
                default:
                    throw new ByamlException($"{type} not supported as complex node.");
            }
        }

        private void WriteTypeAndLength(BinaryDataWriter writer, ByamlNodeType type, dynamic node)
        {
            uint value;
            if (_settings.ByteOrder == ByteOrder.BigEndian)
            {
                value = (uint)type << 24 | (uint)Enumerable.Count(node);
            }
            else
            {
                value = (uint)type | (uint)Enumerable.Count(node) << 8;
            }
            writer.Write(value);
        }

        private void WriteStringIndexNode(BinaryDataWriter writer, string node)
        {
            writer.Write(_stringArray.IndexOf(node));
        }

        private void WriteBinaryDataIndexNode(BinaryDataWriter writer, byte[] node)
        {
            writer.Write(_binaryDataArray.IndexOf(node));
        }

        private void WriteArrayNode(BinaryDataWriter writer, IEnumerable node)
        {
            WriteTypeAndLength(writer, ByamlNodeType.Array, node);

            // Write the element types.
            foreach (dynamic element in node)
            {
                writer.Write((byte)GetNodeType(element));
            }

            // Write the elements, which begin after a padding to the next 4 bytes.
            writer.Align(4);
            List<Offset> offsets = new List<Offset>();
            foreach (dynamic element in node)
            {
                offsets.Add(WriteValue(writer, element));
            }

            // Write the contents of complex nodes and satisfy the offsets.
            int index = 0;
            foreach (dynamic element in node)
            {
                Offset offset = offsets[index];
                if (offset != null)
                {
                    WriteValueContents(writer, offset, GetNodeType(element), element);
                }
                index++;
            }
        }

        private void WriteDictionaryNode(BinaryDataWriter writer, IDictionary<string, dynamic> node)
        {
            WriteTypeAndLength(writer, ByamlNodeType.Dictionary, node);

            // Dictionaries need to be sorted by key.
            var sortedDict = node.Values.Zip(node.Keys, (Value, Key) => new { Key, Value })
                .OrderBy(x => x.Key, StringComparer.Ordinal).ToList();

            // Write the key-value pairs.
            List<Offset> offsets = new List<Offset>(node.Count);
            foreach (var keyValuePair in sortedDict)
            {
                // Get the index of the key string in the file's name array and write it together with the type.
                uint keyIndex = (uint)_nameArray.IndexOf(keyValuePair.Key);
                if (_settings.ByteOrder == ByteOrder.BigEndian)
                {
                    writer.Write(keyIndex << 8 | (uint)GetNodeType(keyValuePair.Value));
                }
                else
                {
                    writer.Write(keyIndex | (uint)GetNodeType(keyValuePair.Value) << 24);
                }

                // Write the elements.
                offsets.Add(WriteValue(writer, keyValuePair.Value));
            }

            // Write the value contents.
            for (int i = 0; i < offsets.Count; i++)
            {
                Offset offset = offsets[i];
                if (offset != null)
                {
                    dynamic value = sortedDict[i].Value;
                    WriteValueContents(writer, offset, GetNodeType(value), value);
                }
            }
        }

        private void WriteStringArrayNode(BinaryDataWriter writer, IEnumerable<string> node)
        {
            writer.Align(4);
            WriteTypeAndLength(writer, ByamlNodeType.StringArray, node);

            // Write the offsets to the strings, where the last one points to the end of the last string.
            long offset = 4 + 4 * (node.Count() + 1); // Relative to node start + all uint32 offsets.
            foreach (string str in node)
            {
                writer.Write((uint)offset);
                offset += str.Length + 1;
            }
            writer.Write((uint)offset);

            // Write the 0-terminated strings.
            foreach (string str in node)
            {
                writer.Write(str, StringDataFormat.ZeroTerminated);
            }
        }

        private void WriteBinaryDataArrayNode(BinaryDataWriter writer, IEnumerable<byte[]> node)
        {
            writer.Align(4);
            WriteTypeAndLength(writer, ByamlNodeType.BinaryDataArray, node);

            // Write the offsets to the data, where the last one points to the end of the last data.
            long offset = 4 + 4 * (node.Count() + 1); // Relative to node start + all uint32 offsets.
            foreach (byte[] data in node)
            {
                writer.Write((uint)offset);
                offset += data.Length;
            }
            writer.Write((uint)offset);

            // Write the data.
            foreach (byte[] data in node)
            {
                writer.Write(data);
            }
        }

        // ---- Helper methods ----

        private ByamlNodeType GetNodeType(dynamic node, bool isInternalNode = false)
        {
            if (isInternalNode)
            {
                if (node is IEnumerable<string>) return ByamlNodeType.StringArray;
                else if (node is IEnumerable<byte[]>) return ByamlNodeType.BinaryDataArray;
                else throw new ByamlException($"Type '{node.GetType()}' is not supported as a main BYAML node.");
            }
            else
            {
                if (node is string) return ByamlNodeType.StringIndex;
                else if (node is byte[]) return ByamlNodeType.BinaryData;
                else if (node is IDictionary<string, dynamic>) return ByamlNodeType.Dictionary;
                else if (node is IEnumerable) return ByamlNodeType.Array;
                else if (node is bool) return ByamlNodeType.Boolean;
                else if (node is int) return ByamlNodeType.Int32;
                else if (node is float) return ByamlNodeType.Float;
                else if (node == null) return ByamlNodeType.Null;
                else throw new ByamlException($"Type '{node.GetType()}' is not supported as a BYAML node.");
            }
        }
        
        private void EnforceMinimumVersion(ByamlNodeType nodeType, ByamlVersion minimumVersion)
        {
            if ((ushort)minimumVersion > (ushort)_currentReadVersion)
            {
                throw new ByamlException($"Unexpected node type '{nodeType}' for current BYAML version."); 
            }
        }

        private uint Get1MsbByte(uint value)
        {
            if (_settings.ByteOrder == ByteOrder.BigEndian)
            {
                return value & 0x000000FF;
            }
            else
            {
                return value >> 24;
            }
        }

        private uint Get3LsbBytes(uint value)
        {
            if (_settings.ByteOrder == ByteOrder.BigEndian)
            {
                return value & 0x00FFFFFF;
            }
            else
            {
                return value >> 8;
            }
        }

        private uint Get3MsbBytes(uint value)
        {
            if (_settings.ByteOrder == ByteOrder.BigEndian)
            {
                return value >> 8;
            }
            else
            {
                return value & 0x00FFFFFF;
            }
        }
    }
}
