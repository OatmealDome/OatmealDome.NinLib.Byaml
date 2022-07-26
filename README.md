# OatmealDome's NintenTools.Byaml Fork

This is a fork of Syroot.NintenTools.Byaml on version 2.0.2.

While I have another fork of this project, it was not properly tracked by source control, and it implements deserialization of BYAML version 3 nodes incorrectly. Therefore, I decided to re-fork this project, move it under my own namespace (which also brings along the benefits I talk about in my BinaryData fork), clean it up, and re-implement support for newer BYAML versions.

Please note that while reading has been well tested, writing has not. There may be bugs.

Here's what's different:

* BYAML versions 1, 2, 3, and 4 are all fully supported. It is possible to deserialize BYAML versions 5, but it is not possible to serialize it. Experimental deserialization is available for BYAML versions 6 and 7. (New node types may not be available.)
* ``ByamlFile.Load()`` now automatically detects BYAML version, whether binary data is present with BYAML version 1, and what endianness the BYAML is stored in.
* ``ByamlFile.Save()`` now uses ``ByamlSerializerSettings`` to hold serialization settings.
* "Paths" has been changed to generic binary data (``byte[]``). Automatic deserialization of Mario Kart 8 path points (``ByamlPathPoint``) is no longer supported.
* There is no longer a dependency on Syroot.Maths.
* The classes that were part of the Serialization namespace were removed. The only serialization and deserialization type supported is Dynamic.

# Original Readme

This is a .NET library to handle the BYAML Nintendo data file format, allowing you to read and store the files as either dynamic objects or strongly-typed with serialized classes similar to what the .NET `XmlReader` and `XmlWriter` provide.

The library is available as a [NuGet package](https://www.nuget.org/packages/Syroot.NintenTools.Byaml).

## Status

It currently only supports version 1 without reference nodes, and at this time there are no plans to add support for these, and no ETA for the originally scheduled unification of the parsing logic.

## Support

You can ask questions and suggest features on Discord aswell. Feel free to [join the NintenTools channel on the Syroot server](https://discord.gg/asB4uaf)!
