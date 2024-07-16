using System.Runtime.CompilerServices;

namespace SpritesheetExtender
{
    public sealed class ModConfig
    {
        public int[][] OffsetsFemale { get; set; }
        public int[][] OffsetsMale { get; set; }
        public int NumSpritesAddedFemale { get; set; }
        public int NumSpritesAddedMale { get; set; }
    }
}
