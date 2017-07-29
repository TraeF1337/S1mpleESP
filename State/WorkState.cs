﻿using Ennui.Api;
using Ennui.Api.Direct.Object;
using Ennui.Api.Script;
using System;
using System.Linq;
using System.Collections.Generic;

namespace S1mpleESP
{
    [LocalScript]
    public class WorkState : StateScript
    {
        private Configuration config;
        private Context context;
        private HashSet<long> _players;
        private HashSet<long> _harvestable;
        private HashSet<long> _mobs;

        public WorkState(Configuration configuration)
        {
            _players = new HashSet<long>();
            _harvestable = new HashSet<long>();
            _mobs = new HashSet<long>();
            config = configuration;
        }

        public override int OnLoop(IScriptEngine se)
        {
            if (Players.LocalPlayer == null)
                return 1000;

            var currentPlayers = new HashSet<long>();
            var currentHarvestables = new HashSet<long>();
            var currentMobs = new HashSet<long>();

            var localLocation = Players.LocalPlayer.ThreadSafeLocation;

            if (_harvestable.LongCount<long>() > 100)
            {
                _harvestable.Clear();
            }

            if (_mobs.LongCount<long>() > 100)
            {
                _harvestable.Clear();
            }

            if (_players.LongCount<long>() > 100)
            {
                _harvestable.Clear();
            }

            if (true)
            {
                var harvestableChain = Objects
                    .HarvestableChain
                    .FilterDepleted()
                    .FilterByTypeSet(SafeTypeSet.BatchConvert(config.TypeSetsToUse));

                IOrderedEnumerable<IHarvestableObject> harvestables;

                harvestables = harvestableChain
                .AsList
                .OrderBy(x => x.ThreadSafeLocation.SimpleDistance(localLocation));

                var i = 0;
                foreach (var harvestable in harvestables)
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
                        .FilterByType(config.Resources)
                        .FilterByTypeSet(SafeTypeSet.BatchConvert(config.TypeSetsToUse))
                        .AsList;

                    if (mobDrop.Count > 0)
                    {
                        currentMobs.Add(mob.Id);
                        i++;
                    }
                }
            }

            _players = currentPlayers;
            _harvestable = currentHarvestables;
            _mobs = currentMobs;
            return 100;
        }

        private void DrawDistance(Vector2<float> localPlayerPos, Vector2<float> targetPos, GraphicContext g, string tr)
        {
            g.DrawLine(Convert.ToInt32(localPlayerPos.X),
                           Convert.ToInt32(localPlayerPos.Y),
                           Convert.ToInt32(targetPos.X),
                           Convert.ToInt32(targetPos.Y));

            // Calculate distance
            var distance = localPlayerPos.SimpleDistance(localPlayerPos);
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
            var localPlayerPos = Players.LocalPlayer.ScreenLocation;

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
                        if (config.ESPHostile && otherPlayer.IsPvpEnabled)
                        {
                            if (config.ESPIgnorePG)
                            {
                                if (otherPlayer.IsInLocalPlayerParty == true || (otherPlayer.Guild == Players.LocalPlayer.Guild))
                                {

                                }
                                else
                                {
                                    g.SetColor(Color.Red);
                                    tr = "Hostile Player!!!";
                                }
                            }
                            else
                            {
                                g.SetColor(Color.Red);
                                tr = "Hostile Player!!!";
                            }
                        }
                        if (config.ESPFriendly && !otherPlayer.IsPvpEnabled)
                        {
                            if (config.ESPIgnorePG)
                            {
                                if (otherPlayer.IsInLocalPlayerParty == true || (otherPlayer.Guild == Players.LocalPlayer.Guild))
                                {

                                }
                                else
                                {
                                    g.SetColor(Color.Green);
                                    tr = "Friendly Player";
                                }
                            }
                            else
                            {
                                g.SetColor(Color.Green);
                                tr = "Friendly Player";
                            }
                        }

                        DrawDistance(localPlayerPos, screenPosition, g, tr + " - " + otherPlayer.Name);
                    }
                }
            }

            if (_harvestable.Count > 0)
            {
                ///////////////////////////////////////////////////////////////////////////////
                var harvestableList = Objects
                    .HarvestableChain.Filter(new OnlyThisIds<IHarvestableObject>(_harvestable.ToArray()))
                    .AsList;
                ///////////////////////////////////////////////////////////////////////////
                foreach (var harvestable in harvestableList)
                {
                    var screenPosition = harvestable.ScreenLocation;
                    string text = harvestable.Type + " T-" + harvestable.Tier.ToString() + "." + harvestable.RareState.ToString();
                    if (screenPosition != null)
                    {
                        switch (harvestable.Type)
                        {
                            case Ennui.Api.Meta.ResourceType.Fiber:
                                g.SetColor(Color.Black);
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

            if (_mobs.Count > 0)
            {
                var harvestableList = Entities.MobChain.Filter(new OnlyThisIds<IMobObject>(_mobs.ToArray())).AsList;

                foreach (var harvestable in harvestableList)
                {
                    var screenPosition = harvestable.ScreenLocation;
                    string text = harvestable.Name;
                    if (screenPosition != null)
                    {
                        g.SetColor(Color.SkyBlue);
                        DrawDistance(localPlayerPos, screenPosition, g, text);
                    }
                }
            }
        }
    }
}