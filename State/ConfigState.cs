using Ennui.Api;
using Ennui.Api.Builder;
using Ennui.Api.Gui;
using Ennui.Api.Meta;
using Ennui.Api.Script;
using System;
using System.Collections.Generic;

namespace S1mpleESP
{
    public class ConfigState : StateScript
    {
        private IPanel primaryPanel;

        private ILabel tierLabel;

        private ICheckBox _checkESPResources;
        private ICheckBox _checkESPPlayers;

        private IInputField WoodInput;
        private ICheckBox WoodCheckBox;
        private IInputField OreInput;
        private ICheckBox OreCheckBox;
        private IInputField FiberInput;
        private ICheckBox FiberCheckBox;
        private IInputField HideInput;
        private ICheckBox HideCheckBox;
        private IInputField StoneInput;
        private ICheckBox StoneCheckBox;

        private ICheckBox _checkESPBlack;

        private ICheckBox _checkESPFriendly;
        private ICheckBox _checkESPHostile;
        private ICheckBox _checkESPIgnorePG;

        private ICheckBox _checkESPDebug;


        private IButton runButton;

        private Configuration config;
        private Context context;

        public ConfigState(Configuration config, Context context)
        {
            this.config = config;
            this.context = context;
        }

        public void UpdateForConfig()
        {
            var sets = new Dictionary<ResourceType, List<string>>();
            sets.Add(ResourceType.Fiber, new List<string>());
            sets.Add(ResourceType.Hide, new List<string>());
            sets.Add(ResourceType.Ore, new List<string>());
            sets.Add(ResourceType.Rock, new List<string>());
            sets.Add(ResourceType.Wood, new List<string>());

            foreach (var ts in config.TypeSetsToUse)
            {
                if (sets.ContainsKey(ts.Type))
                {
                    Logging.Log("Adding typeset " + (ts.MaxTier + ts.MaxRarity > 0 ? ("." + ts.MaxRarity) : ""));
                    var input = ts.MaxTier + "." + ts.MaxRarity;

                    sets[ts.Type].Add(ts.MaxTier + ((ts.MinRarity > 0) ? ("." + ts.MaxRarity) : ""));
                }
            }

            if (sets[ResourceType.Fiber].Count > 0)
                FiberInput.SetText(string.Join(",", sets[ResourceType.Fiber].ToArray()));

            if (sets[ResourceType.Hide].Count > 0)
                HideInput.SetText(string.Join(",", sets[ResourceType.Hide].ToArray()));

            if (sets[ResourceType.Ore].Count > 0)
                OreInput.SetText(string.Join(",", sets[ResourceType.Ore].ToArray()));

            if (sets[ResourceType.Rock].Count > 0)
                StoneInput.SetText(string.Join(",", sets[ResourceType.Rock].ToArray()));

            if (sets[ResourceType.Wood].Count > 0)
                WoodInput.SetText(string.Join(",", sets[ResourceType.Wood].ToArray()));

            _checkESPResources.SetSelected(config.ESPResources);
            _checkESPPlayers.SetSelected(config.ESPPlayers);

            WoodCheckBox.SetSelected(config.ESPWood);
            OreCheckBox.SetSelected(config.ESPOre);
            FiberCheckBox.SetSelected(config.ESPFiber);
            HideCheckBox.SetSelected(config.ESPHide);
            StoneCheckBox.SetSelected(config.ESPStone);

            _checkESPFriendly.SetSelected(config.ESPFriendly);
            _checkESPHostile.SetSelected(config.ESPHostile);
            _checkESPIgnorePG.SetSelected(config.ESPIgnorePG);
            _checkESPBlack.SetSelected(config.ESPBlack);
            _checkESPDebug.SetSelected(config.ESPDebug);

            config.TypeSetsToUse.Clear();
        }

        private void AddTiers(ResourceType type, string input)
        {
            if (input.Length == 0)
            {
                return;
            }

            try
            {
                var tierGroups = input.Replace(" ", "").Split(',');
                foreach (var tierGroup in tierGroups)
                {
                    var filtered = tierGroup.Trim(' ', ',');
                    if (filtered.Length == 0)
                    {
                        continue;
                    }

                    var targetInfo = filtered.Split('.');

                    var tier = 0;
                    if (targetInfo.Length >= 1)
                    {
                        if (!int.TryParse(targetInfo[0], out tier))
                        {
                            Logging.Log("Failed to parse tier " + input);
                        }
                    }

                    var rarity = 0;
                    if (targetInfo.Length >= 2)
                    {
                        if (!int.TryParse(targetInfo[1], out rarity))
                        {
                            Logging.Log("Failed to parse rarity " + input);
                        }
                    }

                    config.TypeSetsToUse.Add(new SafeTypeSet(tier, tier, type, rarity, rarity));
                }
            }
            catch (Exception e)
            {
                context.State = "Failed to parise tiers " + input;
                context.State = "Failed to parse tiers " + input;
            }
        }

        private void SaveConfig()
        {
            config.ESPResources = _checkESPResources.IsSelected();
            config.ESPPlayers = _checkESPPlayers.IsSelected();

            config.ESPFriendly = _checkESPFriendly.IsSelected();
            config.ESPHostile = _checkESPHostile.IsSelected();
            config.ESPIgnorePG = _checkESPIgnorePG.IsSelected();
            config.ESPBlack = _checkESPBlack.IsSelected();

            config.ESPWood = WoodCheckBox.IsSelected();
            config.ESPOre = OreCheckBox.IsSelected();
            config.ESPFiber = FiberCheckBox.IsSelected();
            config.ESPHide = HideCheckBox.IsSelected();
            config.ESPStone = StoneCheckBox.IsSelected();

            config.ESPDebug = _checkESPDebug.IsSelected();
            try
            {
                if (Files.Exists("s1mpleESP.json"))
                {
                    Files.Delete("s1mpleESP.json");
                }
                Files.WriteText("s1mpleESP.json", Codecs.ToJson(config));
            }
            catch (Exception e)
            {
                Logging.Log("Failed to save config " + e, LogLevel.Error);
            }
        }

        private void SelectedStart()
        {
            if (WoodCheckBox.IsSelected())
            {
                AddTiers(ResourceType.Wood, WoodInput.GetText());
                config.ESPWood = true;
            }

            if (OreCheckBox.IsSelected())
            {
                AddTiers(ResourceType.Ore, OreInput.GetText());
                config.ESPOre = true;
            }

            if (FiberCheckBox.IsSelected())
            {
                AddTiers(ResourceType.Fiber, FiberInput.GetText());
                config.ESPFiber = true;
            }

            if (HideCheckBox.IsSelected())
            {
                AddTiers(ResourceType.Hide, HideInput.GetText());
                config.ESPHide = true;
            }

            if (StoneCheckBox.IsSelected())
            {
                AddTiers(ResourceType.Rock, StoneInput.GetText());
                config.ESPStone = true;
            }

            SaveConfig();

            primaryPanel.Destroy();
            parent.EnterState("work");
        }

        public override bool OnStart(IScriptEngine se)
        {
            context.State = "Configuring...";

            Game.Sync(() =>
            {
                var screenSize = Game.ScreenSize;

                primaryPanel = Factories.CreateGuiPanel();
                GuiScene.Add(primaryPanel);
                primaryPanel.SetSize(400, 450);
                primaryPanel.SetPosition(155, (screenSize.Y / 2), 0);
                primaryPanel.SetAnchor(new Vector2f(0.0f, 0.0f), new Vector2f(0.0f, 0.0f));
                primaryPanel.SetPivot(new Vector2f(0.5f, 0.5f));

                _checkESPResources = Factories.CreateGuiCheckBox();
                primaryPanel.Add(_checkESPResources);
                _checkESPResources.SetPosition(0, 200, 0);
                _checkESPResources.SetSize(300, 25);
                _checkESPResources.SetText("ESP Resources");
                _checkESPResources.SetSelected(true);

                tierLabel = Factories.CreateGuiLabel();
                primaryPanel.Add(tierLabel);
                tierLabel.SetPosition(0, 175, 0);
                tierLabel.SetSize(300, 25);
                tierLabel.SetText("Tier: 2,3,4,4.1,4.2,4.3");

                WoodInput = Factories.CreateGuiInputField();
                primaryPanel.Add(WoodInput);
                WoodInput.SetPosition(-70, 150, 0);
                WoodInput.SetSize(120, 25);

                WoodCheckBox = Factories.CreateGuiCheckBox();
                primaryPanel.Add(WoodCheckBox);
                WoodCheckBox.SetPosition(60, 150, 0);
                WoodCheckBox.SetSize(100, 25);
                WoodCheckBox.SetText("ESP Wood");
                WoodCheckBox.SetSelected(true);

                OreInput = Factories.CreateGuiInputField();
                primaryPanel.Add(OreInput);
                OreInput.SetPosition(-70, 120, 0);
                OreInput.SetSize(120, 25);

                OreCheckBox = Factories.CreateGuiCheckBox();
                primaryPanel.Add(OreCheckBox);
                OreCheckBox.SetPosition(60, 120, 0);
                OreCheckBox.SetSize(100, 25);
                OreCheckBox.SetText("ESP Ore");
                OreCheckBox.SetSelected(true);

                FiberInput = Factories.CreateGuiInputField();
                primaryPanel.Add(FiberInput);
                FiberInput.SetPosition(-70, 90, 0);
                FiberInput.SetSize(120, 25);

                FiberCheckBox = Factories.CreateGuiCheckBox();
                primaryPanel.Add(FiberCheckBox);
                FiberCheckBox.SetPosition(60, 90, 0);
                FiberCheckBox.SetSize(100, 25);
                FiberCheckBox.SetText("ESP Fiber");
                FiberCheckBox.SetSelected(true);

                HideInput = Factories.CreateGuiInputField();
                primaryPanel.Add(HideInput);
                HideInput.SetPosition(-70, 60, 0);
                HideInput.SetSize(120, 25);

                HideCheckBox = Factories.CreateGuiCheckBox();
                primaryPanel.Add(HideCheckBox);
                HideCheckBox.SetPosition(60, 60, 0);
                HideCheckBox.SetSize(100, 25);
                HideCheckBox.SetText("ESP Hide");
                HideCheckBox.SetSelected(true);

                StoneInput = Factories.CreateGuiInputField();
                primaryPanel.Add(StoneInput);
                StoneInput.SetPosition(-70, 30, 0);
                StoneInput.SetSize(120, 25);

                StoneCheckBox = Factories.CreateGuiCheckBox();
                primaryPanel.Add(StoneCheckBox);
                StoneCheckBox.SetPosition(60, 30, 0);
                StoneCheckBox.SetSize(100, 25);
                StoneCheckBox.SetText("ESP Stone");
                StoneCheckBox.SetSelected(true);

                _checkESPPlayers = Factories.CreateGuiCheckBox();
                primaryPanel.Add(_checkESPPlayers);
                _checkESPPlayers.SetPosition(0, 0, 0);
                _checkESPPlayers.SetSize(300, 25);
                _checkESPPlayers.SetText("ESP Players (combine yourself)");

                _checkESPFriendly = Factories.CreateGuiCheckBox();
                primaryPanel.Add(_checkESPFriendly);
                _checkESPFriendly.SetPosition(-70, -30, 0);
                _checkESPFriendly.SetSize(100, 25);
                _checkESPFriendly.SetText("Friendly");

                _checkESPHostile = Factories.CreateGuiCheckBox();
                primaryPanel.Add(_checkESPHostile);
                _checkESPHostile.SetPosition(-70, -60, 0);
                _checkESPHostile.SetSize(100, 25);
                _checkESPHostile.SetText("Hostile");

                _checkESPIgnorePG = Factories.CreateGuiCheckBox();
                primaryPanel.Add(_checkESPIgnorePG);
                _checkESPIgnorePG.SetPosition(70, -30, 0);
                _checkESPIgnorePG.SetSize(-10, 25);
                _checkESPIgnorePG.SetText("Ignore Party/Guild");

                _checkESPBlack = Factories.CreateGuiCheckBox();
                primaryPanel.Add(_checkESPBlack);
                _checkESPBlack.SetPosition(70, -60, 0);
                _checkESPBlack.SetSize(-10, 25);
                _checkESPBlack.SetText("Black Zone Mode");
                
                _checkESPDebug = Factories.CreateGuiCheckBox();
                primaryPanel.Add(_checkESPDebug);
                _checkESPDebug.SetPosition(0, -90, 0);
                _checkESPDebug.SetSize(100, 25);
                _checkESPDebug.SetText("Debug");

                runButton = Factories.CreateGuiButton();
                primaryPanel.Add(runButton);
                runButton.SetPosition(0, -175, 0);
                runButton.SetSize(125, 25);
                runButton.SetText("Run");
                runButton.AddActionListener((e) =>
                {
                    SelectedStart();
                });

                UpdateForConfig();
            });

            Logging.Log("Menu loaded", LogLevel.Info);

            return base.OnStart(se);
        }

        public override int OnLoop(IScriptEngine se)
        {
            return 100;
        }
    }
}

