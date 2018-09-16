using System;
using System.Diagnostics;
using System.Timers;
using PoeHUD.Controllers;
using PoeHUD.Hud.Settings;
using PoeHUD.Plugins;
using Rich_Presence.Discord_API;
using Rich_Presence.Libs;

namespace Rich_Presence
{
    public class Core : BaseSettingsPlugin<Settings>
    {
        private DiscordRpc.EventHandlers handlers;
        public string PluginDirectory = "";
        private DiscordRpc.RichPresence presence;
        private readonly Stopwatch UpdateTick = Stopwatch.StartNew();

        public Core()
        {
            PluginName = "Rich Presence";
        }

        public override void Initialise()
        {
            Initialize("490185567133237248");
            RunCallbacks();

            presence.startTimestamp = DateTimeToTimestamp(DateTime.UtcNow);
            GameController.Area.OnAreaChange += OnAreaChange;
        }

        public override void Render()
        {
            base.Render();
            try
            {
                if (UpdateTick.ElapsedMilliseconds <= 2500) return;

                if (!CanTick())
                    return;

                UpdateTick.Restart();

                UpdatePresence();
            }
            catch (Exception e)
            {
                LogMessage(e, 10);
            }
        }

        public bool CanTick()
        {
            if (GameController.IsLoading)
                return false;
            if (!GameController.Game.IngameState.ServerData.IsInGame)
                return false;
            if (GameController.Player == null || GameController.Player.Address == 0 || !GameController.Player.IsValid)
                return false;
            if (!GameController.Window.IsForeground())
                return false;
            //else if (Core.Cache.InTown)
            //{
            //    //TreeRoutine.LogMessage("Player is in town.", 0.2f);
            //    return false;
            //}
            return true;
        }

        /*
		=============================================
		Private
		=============================================
		*/


        /// <summary>
        ///     Updates presence on area change
        /// </summary>
        private void OnAreaChange(AreaController area)
        {
            presence.startTimestamp = DateTimeToTimestamp(DateTime.UtcNow);
        }

        private void getClassImage()
        {
            presence.largeImageKey = $"{GameController.Game.IngameState.ServerData.PlayerClass.ToString().ToLower()}";
            presence.largeImageText = $"{GameController.Game.IngameState.ServerData.PlayerClass}";
        }
        private void getSmallImage()
        {
            if (LocalPlayer.Area.IsTown)
            {
                presence.smallImageKey = $"town";
                presence.smallImageText = "Town";
            }
            else if (LocalPlayer.Area.IsHideout)
            {
                presence.smallImageKey = $"town";
                presence.smallImageText = "Hideout";
            }
            else if (GameController.Game.IngameState.Data.CurrentWorldArea.IsCorruptedArea)
            {
                presence.smallImageKey = $"redwaypoint";
                presence.smallImageText = "Vaal Area";
            }
            else if(GameController.Game.IngameState.Data.CurrentWorldArea.IsLabyrinthArea)
            {
                presence.smallImageKey = $"lab";
                presence.smallImageText = "Labyrinth";
            }
            else if(GameController.Game.IngameState.Data.CurrentWorldArea.IsDailyArea)
            {
                presence.smallImageKey = $"yellowwaypoint";
                presence.smallImageText = "Master Mission";
            }
            else if(GameController.Game.IngameState.Data.CurrentWorldArea.Name == "Xoph's Domain")
            {
                presence.smallImageKey = $"xophs_domain";
                presence.smallImageText = GameController.Game.IngameState.Data.CurrentWorldArea.Name;
            }
            else if(GameController.Game.IngameState.Data.CurrentWorldArea.Name == "Tul's Domain")
            {
                presence.smallImageKey = $"tuls_domain";
                presence.smallImageText = GameController.Game.IngameState.Data.CurrentWorldArea.Name;
            }
            else if(GameController.Game.IngameState.Data.CurrentWorldArea.Name == "Chayula's Domain")
            {
                presence.smallImageKey = $"chayulas_domain";
                presence.smallImageText = GameController.Game.IngameState.Data.CurrentWorldArea.Name;
            }
            else if(GameController.Game.IngameState.Data.CurrentWorldArea.Name == "Uul-Netol's Domain")
            {
                presence.smallImageKey = $"uul-netols_domain";
                presence.smallImageText = GameController.Game.IngameState.Data.CurrentWorldArea.Name;
            }
            else if(GameController.Game.IngameState.Data.CurrentWorldArea.Name == "Esh's Domain")
            {
                presence.smallImageKey = $"eshs_domain";
                presence.smallImageText = GameController.Game.IngameState.Data.CurrentWorldArea.Name;
            }
            else if (GameController.Game.IngameState.Data.CurrentWorldArea.IsMapWorlds)
            {
                var tier = 0;
                tier = LocalPlayer.Area.RealLevel - 67;
                if (tier < 6)
                {
                    presence.smallImageKey = $"white";
                    presence.smallImageText = GameController.Game.IngameState.Data.CurrentWorldArea.Name;
                }
                else if (tier < 11)
                {
                    presence.smallImageKey = $"yellow";
                    presence.smallImageText = GameController.Game.IngameState.Data.CurrentWorldArea.Name;
                }
                else
                {
                    presence.smallImageKey = $"red";
                    presence.smallImageText = GameController.Game.IngameState.Data.CurrentWorldArea.Name;
                }
            }
            else
            {
                presence.smallImageKey = $"waypoint";
                presence.smallImageText = "Somewhere";
            }
        }

        private string getPlayerLevelPerc()
        {
            double perc = ((double) LocalPlayer.Experience - PlayerExperience.TotalExperience[LocalPlayer.Level]) / PlayerExperience.NextExperience[LocalPlayer.Level] * 100.0;
            if (perc < 1.0)
                return LocalPlayer.Level != 100 ? $"(<1%)" : "";
            return LocalPlayer.Level != 100 ? $"({Math.Floor(perc)}%)" : "";
        }

        private string getLevelString()
        {
            var perc = getPlayerLevelPerc();

            return $"Level {LocalPlayer.Level} {perc}";
        }

        /// <summary>
        ///     Initialize the RPC.
        /// </summary>
        /// <param name="clientId"></param>
        private void Initialize(string clientId)
        {
            handlers = new DiscordRpc.EventHandlers();

            handlers.readyCallback = ReadyCallback;
            handlers.disconnectedCallback += DisconnectedCallback;
            handlers.errorCallback += ErrorCallback;

            DiscordRpc.Initialize(clientId, ref handlers, true, null);

            WriteConsole("Initialized.");
        }

        /// <summary>
        ///     Update the presence status from what's in the UI fields.
        /// </summary>
        private void UpdatePresence()
        {
            if (GameController.Game.IngameState.Data.CurrentWorldArea.IsMapWorlds)
            {
                var tier = 0;
                tier = LocalPlayer.Area.RealLevel - 67;

                presence.details = $"Tier: {tier.ToString()}";
            }
            else
            {
                presence.details = GameController.Game.IngameState.Data.CurrentWorldArea.Name;
            }
            presence.state = getLevelString();


            getClassImage();
            getSmallImage();

            DiscordRpc.UpdatePresence(ref presence);

            //WriteConsole("Presence updated.");
        }

        /// <summary>
        ///     Calls ReadyCallback(), DisconnectedCallback(), ErrorCallback().
        /// </summary>
        private void RunCallbacks()
        {
            DiscordRpc.RunCallbacks();

            WriteConsole("Rallbacks run.");
        }

        /// <summary>
        ///     Send a message to PoeHUD's window.
        /// </summary>
        private void WriteConsole(string msg)
        {
            LogMessage(msg, 10);
        }

        /// <summary>
        ///     Stop RPC.
        /// </summary>
        private void Shutdown()
        {
            DiscordRpc.Shutdown();

            WriteConsole("Shuted down.");
        }

        /// <summary>
        ///     Called after RunCallbacks() when ready.
        /// </summary>
        private void ReadyCallback()
        {
            //WriteConsole("Ready.");
        }

        /// <summary>
        ///     Called after RunCallbacks() in cause of disconnection.
        /// </summary>
        /// <param name="errorCode"></param>
        /// <param name="message"></param>
        private void DisconnectedCallback(int errorCode, string message)
        {
            WriteConsole(string.Format("Disconnect {0}: {1}", errorCode, message));
        }

        /// <summary>
        ///     Called after RunCallbacks() in cause of error.
        /// </summary>
        /// <param name="errorCode"></param>
        /// <param name="message"></param>
        private void ErrorCallback(int errorCode, string message)
        {
            WriteConsole(string.Format("Error {0}: {1}", errorCode, message));
        }

        /// <summary>
        ///     Convert a DateTime object into a timestamp.
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        private long DateTimeToTimestamp(DateTime dt)
        {
            return (dt.Ticks - 621355968000000000) / 10000000;
        }

        /*
		=============================================
		Event
		=============================================
		*/
    }


    public class Settings : SettingsBase
    {
    }
}