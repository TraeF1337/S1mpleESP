﻿using Ennui.Api;
using Ennui.Api.Direct.Object;
using Ennui.Api.Script;
using System;
using System.Linq;
using System.Collections.Generic;

namespace S1mpleESP
{
    public class NoobState : StateScript
    {
        private Configuration config;
        private Context context;
        private HashSet<long> _players;
        private HashSet<long> _harvestable;
        private HashSet<long> _mobs;
        private string db1;
        private string db2;
        private string db3;

        public NoobState(Configuration config, Context context)
        {
            _players = new HashSet<long>();
            _harvestable = new HashSet<long>();
            _mobs = new HashSet<long>();

            this.config = config;
            this.context = context;
        }

        public override int OnLoop(IScriptEngine se)
        {
            if (Players.LocalPlayer == null)
                return 1000;
            var localLocation = Players.LocalPlayer.ThreadSafeLocation;

            if (config.ESPPlayers)
            {
                playersESP(localLocation);
            }
            if (config.ESPResources)
            {
                ressourcesESP(localLocation);
            }

            return 100;
        }

        private void playersESP(Vector3<float> loc)
        {
            var currentPlayers = new HashSet<long>();
            var otherPlayers = Players.RemotePlayers.OrderBy(x => x.ThreadSafeLocation.SimpleDistance(loc));
            foreach (var otherPlayer in otherPlayers)
            {
                currentPlayers.Add(otherPlayer.Id);
            }

            _players = currentPlayers;
        }

        private void ressourcesESP(Vector3<float> loc)
        {
            var localLocation = Players.LocalPlayer.ThreadSafeLocation;

            var currentHarvestables = new HashSet<long>();
            var currentMobs = new HashSet<long>();

            var harvestableChain = Objects
                .HarvestableChain
                .FilterByTypeSet(SafeTypeSet.BatchConvert(config.TypeSetsToUse))
                .AsList
                .OrderBy(x => x.ThreadSafeLocation.SimpleDistance(loc));

            var i = 0;

            foreach (var harvestable in harvestableChain)
            {
                if (i == 10)
                    break;

                currentHarvestables.Add(harvestable.Id);
                i++;
            }

            foreach (var mob in Entities.Mobs)
            {
                if (i == 10)
                    break;

                var mobDrop = mob.HarvestableDropChain
                    .FilterByTypeSet(SafeTypeSet.BatchConvert(config.TypeSetsToUse))
                    .AsList;

                currentMobs.Add(mob.Id);
                i++;

            }

            _harvestable = currentHarvestables;
            _mobs = currentMobs;
        }
        private void DrawDistance(Vector2<float> localPlayerPos, Vector2<float> targetPos, GraphicContext g, string tr)
        {
            g.DrawLine(Convert.ToInt32(localPlayerPos.X),
                           Convert.ToInt32(localPlayerPos.Y),
                           Convert.ToInt32(targetPos.X),
                           Convert.ToInt32(targetPos.Y));

            // Calculate distance
            var distance = localPlayerPos.SimpleDistance(targetPos);
            tr = "dist: " + Convert.ToInt16(distance / 100) + "m " + tr;
            if (distance <= 5000)
            {

                if (Math.Abs(targetPos.X) >= Math.Abs(targetPos.Y))
                {
                    if (localPlayerPos.X < targetPos.X)
                    {
                        var y = ((targetPos.Y - localPlayerPos.Y) / (targetPos.X - localPlayerPos.X)) * 100;
                        g.DrawString(tr, Convert.ToInt32(localPlayerPos.X + 100), Convert.ToInt32(localPlayerPos.Y + y));
                    }
                    else if (localPlayerPos.X > targetPos.X)
                    {
                        var y = ((targetPos.Y - localPlayerPos.Y) / (targetPos.X - localPlayerPos.X)) * -100;
                        g.DrawString(tr, Convert.ToInt32(localPlayerPos.X - 100), Convert.ToInt32(localPlayerPos.Y + y));
                    }
                }
                else
                {
                    if (localPlayerPos.Y < targetPos.Y)
                    {
                        var x = (targetPos.X - localPlayerPos.X) * 100 / (targetPos.Y - localPlayerPos.Y);
                        g.DrawString(tr, Convert.ToInt32(localPlayerPos.X + x), Convert.ToInt32(localPlayerPos.Y + 100));
                    }
                    else if (localPlayerPos.Y > targetPos.Y)
                    {
                        var x = (targetPos.X - localPlayerPos.X) * -100 / (targetPos.Y - localPlayerPos.Y);
                        g.DrawString(tr, Convert.ToInt32(localPlayerPos.X + x), Convert.ToInt32(localPlayerPos.Y - 100));
                    }
                }
            }
        }
        public override void OnPaint(IScriptEngine se, GraphicContext g)
        {
            g.SetColor(new Color(0.3f, 0.3f, 0.3f, 1.0f));
            g.FillRect(15, 100, 200, 150);
            g.SetColor(new Color(1.0f, 1.0f, 1.0f, 1.0f));
            g.DrawString("S1mpleESP", 20, 100);
            g.DrawString(string.Format("State: {0}", context.State), 20, 130);
            g.DrawString(string.Format("DB1: {0}", db1), 20, 160);
            g.DrawString(string.Format("DB2: {0}", db2), 20, 190);
            g.DrawString(string.Format("DB3: {0}", db3), 20, 220);

            context.State = "PL: " + _players.Count + " RES: " + _harvestable.Count;

            var localPlayerPos = Players.LocalPlayer.ScreenLocation;
            if (config.ESPPlayers)
            {
                if (_players.Count > 0)
                {
                    var playerList = Players.RemotePlayerChain
                        .Filter(new OnlyThisIds<IRemotePlayerObject>(_players.ToArray()))
                        .AsList;

                    foreach (var otherPlayer in playerList)
                    {
                        var screenPosition = otherPlayer.ScreenLocation;
                        string tr = " ";
                        if (screenPosition != null)
                        {
                            if (!config.ESPBlack)
                            {
                                if (config.ESPHostile)
                                {
                                    if (config.ESPIgnorePG)
                                    {
                                        if (otherPlayer.IsPvpEnabled && !otherPlayer.IsInLocalPlayerParty && otherPlayer.Guild != Players.LocalPlayer.Guild && otherPlayer.GuildTag != Players.LocalPlayer.GuildTag)
                                        {
                                            g.SetColor(Color.Red);
                                            tr = "Hostile Player!!!";
                                            DrawDistance(localPlayerPos, screenPosition, g, tr + " - " + otherPlayer.Name);
                                        }
                                    }
                                    else if (!config.ESPIgnorePG)
                                    {
                                        if (otherPlayer.IsPvpEnabled)
                                        {
                                            g.SetColor(Color.Red);
                                            tr = "Hostile Player!!!";
                                            DrawDistance(localPlayerPos, screenPosition, g, tr + " - " + otherPlayer.Name);
                                        }
                                    }
                                }

                                if (config.ESPFriendly)
                                {
                                    if (config.ESPIgnorePG)
                                    {
                                        if (!otherPlayer.IsPvpEnabled && !otherPlayer.IsInLocalPlayerParty && otherPlayer.Guild != Players.LocalPlayer.Guild && otherPlayer.GuildTag != Players.LocalPlayer.GuildTag)
                                        {
                                            g.SetColor(Color.Green);
                                            tr = "Friendly Player";
                                            DrawDistance(localPlayerPos, screenPosition, g, tr + " - " + otherPlayer.Name);
                                        }
                                    }
                                    else if (!config.ESPIgnorePG)
                                    {
                                        if (!otherPlayer.IsPvpEnabled)
                                        {
                                            g.SetColor(Color.Green);
                                            tr = "Friendly Player";
                                            DrawDistance(localPlayerPos, screenPosition, g, tr + " - " + otherPlayer.Name);
                                        }
                                    }
                                }
                            }

                            else if (!otherPlayer.IsInLocalPlayerParty && otherPlayer.Guild != Players.LocalPlayer.Guild && otherPlayer.GuildTag != Players.LocalPlayer.GuildTag)
                            {
                                g.SetColor(Color.Red);
                                tr = "Hostile Player!!!";
                                DrawDistance(localPlayerPos, screenPosition, g, tr + " - " + otherPlayer.Name + " [" + otherPlayer.GuildTag + "] " + otherPlayer.Guild);
                            }
                        }
                    }
                }
            }

            if (config.ESPResources)
            {
                if (_harvestable.Count > 0)
                {
                    //////////////////////////////////////////////////////////////////////////////
                    var harvestableList = Objects
                        .HarvestableChain.Filter(new OnlyThisIds<IHarvestableObject>(_harvestable.ToArray()))
                        .AsList;
                    //////////////////////////////////////////////////////////////////////////
                    foreach (var harvestable in harvestableList)
                    {
                        if (harvestable.IsValid && harvestable.Charges > 0)
                        {
                            var screenPosition = harvestable.ScreenLocation;
                            string text = harvestable.Type + " T-" + harvestable.Tier + "." + harvestable.RareState + " " + harvestableList.Count;
                            if (screenPosition != null)
                            {
                                switch (harvestable.Type)
                                {
                                    case Ennui.Api.Meta.ResourceType.Fiber:
                                        g.SetColor(Color.White);
                                        break;
                                    case Ennui.Api.Meta.ResourceType.Ore:
                                        g.SetColor(Color.Yellow);
                                        break;
                                    case Ennui.Api.Meta.ResourceType.Hide:
                                        g.SetColor(Color.Blue);
                                        break;
                                    case Ennui.Api.Meta.ResourceType.Wood:
                                        g.SetColor(Color.Orange);
                                        break;
                                    case Ennui.Api.Meta.ResourceType.Rock:
                                        g.SetColor(Color.Cyan);
                                        break;
                                    case Ennui.Api.Meta.ResourceType.Coins:
                                        g.SetColor(Color.White);
                                        break;
                                    default:
                                        g.SetColor(Color.Blue);
                                        break;
                                }
                                switch (harvestable.RareState)
                                {
                                    case 1:
                                        g.SetColor(Color.Green);
                                        break;
                                    case 2:
                                        g.SetColor(Color.SkyBlue);
                                        break;
                                    case 3:
                                        g.SetColor(Color.Purple);
                                        break;
                                }
                                DrawDistance(localPlayerPos, screenPosition, g, text);
                            }
                        }
                    }
                }

                if (_mobs.Count > 0)
                {
                    var harvestableList = Entities.MobChain.Filter(new OnlyThisIds<IMobObject>(_mobs.ToArray())).AsList;

                    foreach (var harvestable in harvestableList)
                    {

                        var mobDrop = harvestable.HarvestableDropChain
                            .FilterByTypeSet(SafeTypeSet.BatchConvert(config.TypeSetsToUse))
                            .AsList;
                        var screenPosition = harvestable.ScreenLocation;
                        string text = mobDrop[0].Type + " Mob T-" + mobDrop[0].Tier + "." + mobDrop[0].Rarity + " " + mobDrop.Count;
                        if (screenPosition != null)
                        {
                            g.SetColor(Color.SkyBlue);
                            if (harvestable.IsValid && harvestable.CurrentHealth > 0)
                                DrawDistance(localPlayerPos, screenPosition, g, text);
                        }
                    }
                }
            }
        }
    }// End of class
}