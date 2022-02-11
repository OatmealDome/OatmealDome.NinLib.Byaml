﻿using System.Collections.Generic;
using OatmealDome.BinaryData;

namespace OatmealDome.NinLib.Byaml
{
    /// <summary>
    /// Represents options to control the serialization process of a <see cref="ByamlSerializer"/>.
    /// </summary>
    public class ByamlSerializerSettings
    {
        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="ByamlSerializerSettings"/> class with default settings (big
        /// endian, no path arrays, version 1).
        /// </summary>
        public ByamlSerializerSettings()
        {
            ByteOrder = ByteOrder.BigEndian;
            SupportsBinaryData = false;
            Version = ByamlVersion.One;
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------
        
        /// <summary>
        /// Gets or sets the <see cref="ByteOrder"/> the data will be read and stored with.
        /// </summary>
        public ByteOrder ByteOrder { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether binary data will be supported and expected in a version 1 BYAML.
        /// </summary>
        public bool SupportsBinaryData { get; set; }

        /// <summary>
        /// Gets or sets the version of the BYAML file to write or expect.
        /// It is not necessary to specify this when deserializing a BYAML. However, if it is set, the deserializer
        /// will check to ensure that the version specified matches the version of the BYAML.
        /// </summary>
        public ByamlVersion? Version { get; set; }

    }
}
