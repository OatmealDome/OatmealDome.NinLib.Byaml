namespace OatmealDome.NinLib.Byaml
{
    /// <summary>
    /// Represents the version of a BYAML.
    /// </summary>
    public enum ByamlVersion : ushort
    {
        /// <summary>
        /// Format version 1, as found in most Wii U games.
        /// </summary>
        One = 1,
        
        /// <summary>
        /// Format version 2, as found in Breath of the Wild.
        /// </summary>
        Two = 2,
        
        /// <summary>
        /// Format version 3, as found in many early Switch titles like Super Mario Odyssey and Splatoon 2.
        /// </summary>
        Three = 3,
        
        /// <summary>
        /// Format version 4, as found in Animal Crossing: New Horizons.
        /// </summary>
        Four = 4,
        
        /// <summary>
        /// Format version 5, as found in an unknown Switch game. Listed on Kinnay's Nintendo File Formats wiki.
        /// </summary>
        Five = 5,
    }
}
