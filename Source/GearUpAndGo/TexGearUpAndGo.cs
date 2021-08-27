using UnityEngine;
using Verse;

namespace GearUpAndGo
{
    [StaticConstructorOnStartup]
    public class TexGearUpAndGo
    {
        public static readonly Texture2D guagIcon = ContentFinder<Texture2D>.Get("CommandGearUpAndGo");
        public static readonly Texture2D guagIconActive = ContentFinder<Texture2D>.Get("CommandGearUpAndGoActive");
    }
}