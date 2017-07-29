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
        private ILabel h1Label;
        private ILabel h2Label;
        private ILabel h3Label;
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

        private ICheckBox _checkESPFriendly;
        private ICheckBox _checkESPHostile;
        private ICheckBox _checkESPIgnorePG;
        private ICheckBox _checkESPActivatedResources;


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

            WoodCheckBox.SetSelected(config.ESPWood);
            OreCheckBox.SetSelected(config.ESPOre);
            FiberCheckBox.SetSelected(config.ESPFiber);
            HideCheckBox.SetSelected(config.ESPHide);
            StoneCheckBox.SetSelected(config.ESPStone);

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
            }

            if (OreCheckBox.IsSelected())
            {
                AddTiers(ResourceType.Ore, OreInput.GetText());
            }

            if (FiberCheckBox.IsSelected())
            {
                AddTiers(ResourceType.Fiber, FiberInput.GetText());
            }

            if (HideCheckBox.IsSelected())
            {
                AddTiers(ResourceType.Hide, HideInput.GetText());
            }

            if (StoneCheckBox.IsSelected())
            {
                AddTiers(ResourceType.Rock, StoneInput.GetText());
            }

            if (config.TypeSetsToUse.Count == 0)
            {
                context.State = "No type sets to ESP!";
                return;
            }

            SaveConfig();

            primaryPanel.Destroy();
            parent.EnterState("resolve");
        }

        public override bool OnStart(IScriptEngine se)
        {
            context.State = "Configuring...";

            Game.Sync(() =>
            {
                var screenSize = Game.ScreenSize;

                primaryPanel = Factories.CreateGuiPanel();
                GuiScene.Add(primaryPanel);
                primaryPanel.SetSize(300, 320);
                primaryPanel.SetPosition(155, (screenSize.Y / 2), 0);
                primaryPanel.SetAnchor(new Vector2f(0.0f, 0.0f), new Vector2f(0.0f, 0.0f));
                primaryPanel.SetPivot(new Vector2f(0.5f, 0.5f));

                tierLabel = Factories.CreateGuiLabel();
                primaryPanel.Add(tierLabel);
                tierLabel.SetPosition(-60, 175, 0);
                tierLabel.SetSize(100, 25);
                tierLabel.SetText("Tier: 2,3,4,4.1,4.2,4.3");

                WoodInput = Factories.CreateGuiInputField();
                primaryPanel.Add(WoodInput);
                WoodInput.SetPosition(-70, 150, 0);
                WoodInput.SetSize(120, 25);

                WoodCheckBox = Factories.CreateGuiCheckBox();
                primaryPanel.Add(WoodCheckBox);
                WoodCheckBox.SetPosition(60, 150, 0);
                WoodCheckBox.SetSize(100, 25);
                WoodCheckBox.SetText(" Wood");
                WoodCheckBox.SetSelected(true);

                OreInput = Factories.CreateGuiInputField();
                primaryPanel.Add(OreInput);
                OreInput.SetPosition(-70, 120, 0);
                OreInput.SetSize(120, 25);

                OreCheckBox = Factories.CreateGuiCheckBox();
                primaryPanel.Add(OreCheckBox);
                OreCheckBox.SetPosition(60, 120, 0);
                OreCheckBox.SetSize(100, 25);
                OreCheckBox.SetText(" Ore");
                OreCheckBox.SetSelected(true);

                FiberInput = Factories.CreateGuiInputField();
                primaryPanel.Add(FiberInput);
                FiberInput.SetPosition(-70, 90, 0);
                FiberInput.SetSize(120, 25);

                FiberCheckBox = Factories.CreateGuiCheckBox();
                primaryPanel.Add(FiberCheckBox);
                FiberCheckBox.SetPosition(60, 90, 0);
                FiberCheckBox.SetSize(100, 25);
                FiberCheckBox.SetText(" Fiber");
                FiberCheckBox.SetSelected(true);

                HideInput = Factories.CreateGuiInputField();
                primaryPanel.Add(HideInput);
                HideInput.SetPosition(-70, 60, 0);
                HideInput.SetSize(120, 25);

                HideCheckBox = Factories.CreateGuiCheckBox();
                primaryPanel.Add(HideCheckBox);
                HideCheckBox.SetPosition(60, 60, 0);
                HideCheckBox.SetSize(100, 25);
                HideCheckBox.SetText(" Hide");
                HideCheckBox.SetSelected(true);

                StoneInput = Factories.CreateGuiInputField();
                primaryPanel.Add(StoneInput);
                StoneInput.SetPosition(-70, 30, 0);
                StoneInput.SetSize(120, 25);

                StoneCheckBox = Factories.CreateGuiCheckBox();
                primaryPanel.Add(StoneCheckBox);
                StoneCheckBox.SetPosition(60, 30, 0);
                StoneCheckBox.SetSize(100, 25);
                StoneCheckBox.SetText(" Stone");
                StoneCheckBox.SetSelected(true);

                h2Label = Factories.CreateGuiLabel();
                primaryPanel.Add(h2Label);
                h2Label.SetPosition(60, 0, 0);
                h2Label.SetSize(100, 25);
                h2Label.SetText("ESP Players (combine yourself)");

                _checkESPFriendly = Factories.CreateGuiCheckBox();
                primaryPanel.Add(_checkESPFriendly);
                _checkESPFriendly.SetPosition(-70, -30, 0);
                _checkESPFriendly.SetSize(100, 25);
                _checkESPFriendly.SetText("Friendly");
                _checkESPFriendly.SetSelected(true);

                _checkESPHostile = Factories.CreateGuiCheckBox();
                primaryPanel.Add(_checkESPHostile);
                _checkESPHostile.SetPosition(-70, -60, 0);
                _checkESPHostile.SetSize(100, 25);
                _checkESPHostile.SetText("Hostile");
                _checkESPHostile.SetSelected(true);

                _checkESPIgnorePG = Factories.CreateGuiCheckBox();
                primaryPanel.Add(_checkESPIgnorePG);
                _checkESPIgnorePG.SetPosition(70, -30, 0);
                _checkESPIgnorePG.SetSize(-10, 25);
                _checkESPIgnorePG.SetText("Ignore Party/Guild");
                _checkESPIgnorePG.SetSelected(true);

                _checkESPActivatedResources = Factories.CreateGuiCheckBox();
                primaryPanel.Add(_checkESPActivatedResources);
                _checkESPActivatedResources.SetPosition(-70, -90, 0);
                _checkESPActivatedResources.SetSize(100, 25);
                _checkESPActivatedResources.SetText("ESP Resources");
                _checkESPActivatedResources.SetSelected(true);

                runButton = Factories.CreateGuiButton();
                primaryPanel.Add(runButton);
                runButton.SetPosition(0, -175, 0);
                runButton.SetSize(125, 25);
                runButton.SetText("Run");
                runButton.AddActionListener((e) =>
                {
                    SelectedStart();
                });
            });

            Logging.Log("Menu loaded", LogLevel.Info);

            return base.OnStart(se);
        }

        public override int OnLoop(IScriptEngine se)
        {
            var lpo = Players.LocalPlayer;
            return 100;
        }
    }
}

