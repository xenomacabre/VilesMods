using UnityEngine;
using Verse;

namespace DoorsExpanded
{
    [StaticConstructorOnStartup]
    internal class TexButton
    {
        public static readonly Texture2D ConnectToButton = ContentFinder<Texture2D>.Get("UI/Buttons/ConnectToButton", true);
        public static readonly Texture2D DisconnectButton = ContentFinder<Texture2D>.Get("UI/Buttons/DisconnectButton", true);
        public static readonly Texture2D SecuredRemotely = ContentFinder<Texture2D>.Get("UI/Buttons/SecuredRemotely", true);
        public static readonly Texture2D UseButtonOrLever = ContentFinder<Texture2D>.Get("UI/Buttons/UseButtonOrLever", true);
    }
}
