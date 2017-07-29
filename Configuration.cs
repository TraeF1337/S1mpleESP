using Ennui.Api.Meta;
using System.Collections.Generic;

namespace S1mpleESP
{
    public class Configuration
    {
        public bool ESPResources { get; set; }
        public bool ESPFriendly { get; set; }
        public bool ESPHostile { get; set; }
        public bool ESPIgnorePG { get; set; }
        public bool ESPActivatedPlayersOnlyHostile { get; set; }
        public ResourceType[] Resources { get; set; }

        public bool ESPWood = false;
        public bool ESPOre = false;
        public bool ESPFiber = false;
        public bool ESPHide = false;
        public bool ESPStone = false;

        public List<SafeTypeSet> TypeSetsToUse = new List<SafeTypeSet>();

    }
}