using Ennui.Api.Meta;
using System.Collections.Generic;

namespace S1mpleESP
{
    public class Configuration
    {
        public bool ESPResources = false;
        public bool ESPPlayers = false;

        public bool ESPFriendly = false;
        public bool ESPHostile = false;
        public bool ESPIgnorePG = false;
        public bool ESPBlack = false;

        public bool ESPWood = false;
        public bool ESPOre = false;
        public bool ESPFiber = false;
        public bool ESPHide = false;
        public bool ESPStone = false;

        public List<SafeTypeSet> TypeSetsToUse = new List<SafeTypeSet>();

    }
}