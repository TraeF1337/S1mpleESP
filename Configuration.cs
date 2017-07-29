using Ennui.Api.Meta;
using System.Collections.Generic;

namespace SimpleESP
{
    public class Configuration
    {        
        public bool ESPActivatedResources { get; set; }
        public bool ESPActivatedPlayers { get; set; }
        public bool ESPActivatedPlayersOnlyHostile { get; set; }
        public bool StoneOnlyT5 { get; set; }
        public bool StoneOnlyT6 { get; set; }
        public bool StoneOnlyT7 { get; set; }
        public bool StoneOnlyT8 { get; set; }
        public ResourceType[] Resources { get; set; }   
        public List<string> TierAndRarity { get; set; }        
        public bool OnlyResourcesWithMoreThan1 { get; set; }

        public Configuration()
        {
            TierAndRarity = new List<string>();
        }
    }
}
