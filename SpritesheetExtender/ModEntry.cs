using GenericModConfigMenu;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace SpritesheetExtender
{
    internal class ModEntry : Mod
    {
        private ModConfig Config;
        private IContentPack Target;

        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.ConsoleCommands.Add("setFrameOffset", "Sets the x and y offset of the farmer sprite at indicated index.\n \nUsage: setFrameOffset <frame-index> <x-offset> <y-offset>\n" +
                "<frame-index> The index of the targeted sprite on the farmer's sprite sheet\n<x-offset> The number of pixels to horizontally offset the farmer's hair and clothes (positive right, negative left)\n" +
                "<y-offset> The number of pixels to vertically offset the farmer's hair and clothes (positive down, negative up)", SetFrameOffset);
            helper.ConsoleCommands.Add("getFrameOffset", "Gets the x and y offset of the farmer sprite at indicated index.\n \nUsage: getFrameOffset <frame-index>\n" +
                "<frame-index> The index of the targeted sprite on the farmer's sprite sheet", GetFrameOffset);

            SetLoadTarget();
            if (Target == null)
            {
                Monitor.Log("No available content packs found.", LogLevel.Debug);
                Config = new ModConfig()
                {
                    NumSpritesAddedMale = 0,
                    NumSpritesAddedFemale = 0
                };
            }
            else
            {
                SetConfig();
            }
        }

        private void GameLoop_GameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            configMenu.Register(
                mod: ModManifest,
                reset: () => Config = new ModConfig(),
                save: () => Helper.WriteConfig(Config)
                );

            configMenu.SetTitleScreenOnlyForNextOptions(
                mod: ModManifest,
                titleScreenOnly: true);

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Male Sprites to Be Added",
                tooltip: () => "Number of sprites to be added to male farmer sprite sheet",
                getValue: () => Config.NumSpritesAddedMale,
                setValue: value => Config.NumSpritesAddedMale = value,
                min: 0,
                interval: 1,
                formatValue: null,
                fieldId: null
                );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Female Sprites to Be Added",
                tooltip: () => "Number of sprites to be added to female farmer sprite sheet",
                getValue: () => Config.NumSpritesAddedFemale,
                setValue: value => Config.NumSpritesAddedFemale = value,
                min: 0,
                interval: 1,
                formatValue: null,
                fieldId: null
                );
        }

        private void GameLoop_SaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            Config.OffsetsMale ??= Array.Empty<int[]>();
            Config.OffsetsFemale ??= Array.Empty<int[]>();
            if (Config.OffsetsMale.Length == 0 && Config.NumSpritesAddedMale > 0)
            {
                List<int[]> newOffsets = new();
                for (int i = 0; i < Config.NumSpritesAddedMale; i++)
                {
                    newOffsets.Add(new int[] { 0, 0 });
                }
                Config.OffsetsMale = newOffsets.ToArray();
                Target?.WriteJsonFile("SSXConfig.json", Config);
                Monitor.Log($"No male offset pairs found. Added {Config.NumSpritesAddedMale} offset pairs to config file. Set offset in-game using setframeoffset command.", LogLevel.Debug);

            }
            if (Config.OffsetsFemale.Length == 0 && Config.NumSpritesAddedFemale > 0)
            {
                List<int[]> newOffsets = new();
                for (int i = 0; i < Config.NumSpritesAddedFemale; i++)
                {
                    newOffsets.Add(new int[] { 0, 0 });
                }
                Config.OffsetsFemale = newOffsets.ToArray();
                Target?.WriteJsonFile("SSXConfig.json", Config);
                Monitor.Log($"No female offset pairs found. Added {Config.NumSpritesAddedFemale} offset pairs to config file. Set offset in-game using setframeoffset command.", LogLevel.Debug);

            }
            if (Config.NumSpritesAddedMale > 0 || Config.NumSpritesAddedFemale > 0) {
                AddSpriteIndex();
            }
        }

        private void AddSpriteIndex()
        {
            List<int> origXOffset = FarmerRenderer.featureXOffsetPerFrame.ToList();
            List<int> origYOffset = FarmerRenderer.featureYOffsetPerFrame.ToList();
            int[][] offsets = Game1.player.Gender == Gender.Male ? Config.OffsetsMale : Config.OffsetsFemale;
            foreach (int[] offset in offsets)
            {
                origXOffset.Add(offset[0]);
                origYOffset.Add(offset[1]);
            }
            FarmerRenderer.featureXOffsetPerFrame = origXOffset.ToArray();
            FarmerRenderer.featureYOffsetPerFrame = origYOffset.ToArray();

            Monitor.Log($"{offsets.Length} sprites added to farmer sprite sheet.", LogLevel.Info);
        }

        private void SetFrameOffset(string command, string[] args)
        {
            try
            {
                if (args.Length == 3)
                {
                    if (int.TryParse(args[0], out int frameIndex) && int.TryParse(args[1], out int xOffset) && int.TryParse(args[2], out int yOffset))
                    {
                        List<int> origXOffset = FarmerRenderer.featureXOffsetPerFrame.ToList();
                        List<int> origYOffset = FarmerRenderer.featureYOffsetPerFrame.ToList();
                        if (origXOffset.Count < frameIndex)
                        {
                            throw new ArgumentOutOfRangeException($"Frame index {frameIndex} is not currently added to values. Check input or increase Number of Sprites to Add in config.");
                        }
                        else if (frameIndex > 125)
                        {
                            origXOffset[frameIndex] = xOffset;
                            origYOffset[frameIndex] = yOffset;
                            FarmerRenderer.featureXOffsetPerFrame = origXOffset.ToArray();
                            FarmerRenderer.featureYOffsetPerFrame = origYOffset.ToArray();
                            List<int[]> offsets = Game1.player.Gender == Gender.Male ? Config.OffsetsMale.ToList<int[]>() : Config.OffsetsFemale.ToList<int[]>();

                            int[] offsetPair = { xOffset, yOffset };

                            offsets[frameIndex - 126] = offsetPair;
                            if (Game1.player.Gender == Gender.Male)
                            {
                                Config.OffsetsMale = offsets.ToArray();
                                Target?.WriteJsonFile("SSXConfig.json", Config);
                            }
                            else
                            {
                                Config.OffsetsFemale = offsets.ToArray();
                                Target?.WriteJsonFile("SSXConfig.json", Config);
                            }

                            Monitor.Log($"Sprite at index {frameIndex} has been set to offset [{xOffset}, {yOffset}].", LogLevel.Info);
                        }
                        else
                        {
                            throw new ArgumentOutOfRangeException("Cannot edit vanilla sprites. Frame index must be greater than 125.");
                        }
                    }
                    else
                    {
                        throw new FormatException($"Arguments must be integers.");
                    }
                }
                else
                {
                    throw new ArgumentOutOfRangeException($"Arguments require 3 indexes. {args.Length} indexes found.");
                }
            }
            catch (Exception e)
            {
                Monitor.Log(e.Message, LogLevel.Error);
            }
        }

        private void GetFrameOffset(string command, string[] args)
        {
            if (int.TryParse(args[0], out int frameIndex))
            {
                frameIndex -= 126;
                int[][] offsets = Game1.player.Gender == Gender.Male ? Config.OffsetsMale : Config.OffsetsFemale;
                if (offsets.Length >= frameIndex)
                {
                    Monitor.Log($"Current offset for frame {frameIndex}:\nx = {offsets[frameIndex][0]}\ny = {offsets[frameIndex][1]}", LogLevel.Debug);
                }
                else
                {
                    Monitor.Log($"No frame at index {frameIndex}.", LogLevel.Warn);
                }
            }
            else
            {
                Monitor.Log("Invalid input.", LogLevel.Error);
            }
        }

        private void SetLoadTarget()
        {
            foreach (IModInfo mod in Helper.ModRegistry.GetAll())
            {
                if (mod.IsContentPack && mod.Manifest.Dependencies.Any((d) => d.UniqueID == "mooseybrutale.spriteextender") && Helper.ModRegistry.IsLoaded(mod.Manifest.UniqueID))
                {

                    Target = (IContentPack)mod.GetType().GetProperty("ContentPack").GetValue((object)mod);
                }
            }
        }

        private void SetConfig()
        {
            Config = Target?.ReadJsonFile<ModConfig>("SSXConfig.json");
            if (Config == null)
            {
                Config = new ModConfig();
                Target?.WriteJsonFile("SSXConfig.json", Config);
                Monitor.Log($"No config file found for {Target.Manifest.Name}. Creating SSXConfig.json.", LogLevel.Debug);
            }
            else
            {
                Monitor.Log($"Loaded config file from {Target.Manifest.Name}.", LogLevel.Debug);
            }
        }
    }
}
