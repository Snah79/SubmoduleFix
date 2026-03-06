#if SERVER
using PersistentEmpiresServer.ServerMissions;
#endif
using PersistentEmpiresLib.Database.DBEntities;
using PersistentEmpiresLib.Factions;
using PersistentEmpiresLib.SceneScripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using System.Xml.Linq;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using PersistentEmpiresLib.Data;
using PersistentEmpiresLib.Helpers;

namespace PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors
{
    public class SaveSystemBehavior : MissionNetwork
    {
        private static bool _inDebug = System.Diagnostics.Debugger.IsAttached;
        public long LastSaveAt = DateTimeOffset.Now.ToUnixTimeSeconds();
        public int SaveDuration = 600;

        /* Events for handles */
        public delegate void StartMigration();
        /* Players */
        public delegate void CreateOrSavePlayers(List<NetworkCommunicator> peers);
        public delegate DBPlayer CreateOrSavePlayer(NetworkCommunicator peer);
        public delegate DBPlayer GetOrCreatePlayer(NetworkCommunicator peer, out bool created);
        public delegate DBPlayer GetPlayer(string playerId);
        public delegate bool UpdateCustomName(NetworkCommunicator peer, string customName);
        public delegate void UpdateWoundedUntil(NetworkCommunicator peer, long woundedUntil);
        public delegate void SavePlayerEquipmentOnDeath(string playerId, Equipment equipment);
        public delegate void SaveDefaultsForNewPlayer(NetworkCommunicator peer, Equipment equipment);
        public delegate long? GetWoundedUntil(NetworkCommunicator peer);

        /* Inventories */
        public delegate IEnumerable<DBInventory> GetAllInventories();
        public delegate DBInventory GetOrCreatePlayerInventory(NetworkCommunicator networkCommunicator, out bool created);
        public delegate DBInventory CreateOrSavePlayerInventory(NetworkCommunicator networkCommunicator);
        public delegate DBInventory CreateOrSavePlayerInventory2(NetworkCommunicator networkCommunicator, PersistentEmpireRepresentative persistentEmpireRepresentative);
        public delegate void CreateOrSavePlayerInventories(List<NetworkCommunicator> networkCommunicators);
        public delegate DBInventory GetOrCreateInventory(string inventoryId);
        public delegate DBInventory CreateOrSaveInventory(string inventoryId);
        /* Factions */
        public delegate IEnumerable<DBFactions> GetFactions();
        public delegate DBFactions GetFaction(int factionIndex);
        public delegate DBFactions CreateOrSaveFaction(Faction faction, int factionIndex);
        /* Upgradeable Buildings */
        public delegate IEnumerable<DBUpgradeableBuilding> GetAllUpgradeableBuildings();
        public delegate DBUpgradeableBuilding GetUpgradeableBuilding(PE_UpgradeableBuildings upgradeableBuildings);
        public delegate DBUpgradeableBuilding CreateOrSaveUpgradebleBuilding(PE_UpgradeableBuildings upgradeableBuildings);
        /* Stockpile Markets */
        public delegate IEnumerable<DBStockpileMarket> GetAllStockpileMarkets();
        public delegate DBStockpileMarket GetStockpileMarket(PE_StockpileMarket stockpileMarket);
        public delegate DBStockpileMarket CreateOrSaveStockpileMarket(PE_StockpileMarket stockpileMarket);
        public delegate void UpsertStockpileMarkets(List<PE_StockpileMarket> markets);
        /* Horse Markets */
        public delegate IEnumerable<DBHorseMarket> GetAllHorseMarkets();
        public delegate DBHorseMarket GetHorseMarket(PE_HorseMarket horseMarket);
        public delegate DBHorseMarket CreateOrSaveHorseMarket(PE_HorseMarket horseMarket);

        public delegate void CreatePlayerNameIfNotExists(NetworkCommunicator player);
        public delegate void DiscordRegister(NetworkCommunicator player, string id);

        public delegate bool IsPlayerWhitelisted(string player);


        /*Castles*/
        public delegate IEnumerable<DBCastle> GetCastles();
        public delegate DBCastle GetCastle(int factionIndex);
        public delegate DBCastle CreateOrSaveCastle(int castleIndex, int factionIndex);

        public static event IsPlayerWhitelisted OnIsPlayerWhitelisted;

        public static event CreatePlayerNameIfNotExists OnCreatePlayerNameIfNotExists;

        public static event DiscordRegister OnDiscordRegister;
        public static event StartMigration OnStartMigration;
        /* Players */
        public static event CreateOrSavePlayers OnCreateOrSavePlayers;
        public static event CreateOrSavePlayer OnCreateOrSavePlayer;
        public static event GetOrCreatePlayer OnGetOrCreatePlayer;
        public static event GetPlayer OnGetPlayer;
        public static event UpdateCustomName OnPlayerUpdateCustomName;
        public static event UpdateWoundedUntil OnPlayerUpdateWoundedUntil;
        //public static event SavePlayerEquipmentOnDeath OnSavePlayerEquipmentOnDeath;
        public static event SaveDefaultsForNewPlayer OnSaveDefaultsForNewPlayer;
        public static event GetWoundedUntil OnGetWoundedUntil;
        
        /* Inventories */
        public static event GetOrCreatePlayerInventory OnGetOrCreatePlayerInventory;
        public static event CreateOrSavePlayerInventories OnCreateOrSavePlayerInventories;
        public static event CreateOrSavePlayerInventory OnCreateOrSavePlayerInventory;
        public static event CreateOrSavePlayerInventory2 OnCreateOrSavePlayerInventory2;
        public static event GetOrCreateInventory OnGetOrCreateInventory;
        public static event CreateOrSaveInventory OnCreateOrSaveInventory;
        public static event GetAllInventories OnGetAllInventories;
        /* Factions */
        public static event GetFactions OnGetFactions;
        public static event GetFaction OnGetFaction;
        public static event CreateOrSaveFaction OnCreateOrSaveFaction;
        /* Castles */
        public static event GetCastles OnGetCastles;
        public static event CreateOrSaveCastle OnCreateOrSaveCastle;


        /* Upgradeable Buildings */
        public static event GetAllUpgradeableBuildings OnGetAllUpgradeableBuildings;
        public static event GetUpgradeableBuilding OnGetUpgradeableBuilding;
        public static event CreateOrSaveUpgradebleBuilding OnCreateOrSaveUpgradebleBuilding;
        /* Stockpile Markets */
        public static event GetAllStockpileMarkets OnGetAllStockpileMarkets;
        public static event GetStockpileMarket OnGetStockpileMarket;
        public static event UpsertStockpileMarkets OnUpsertStockpileMarkets;
        
        /* Horse Markets */
        public static event GetAllHorseMarkets OnGetAllHorseMarkets;
        public static event GetHorseMarket OnGetHorseMarket;
        public static event CreateOrSaveHorseMarket OnCreateOrSaveHorseMarket;

        public static void AutoSaveJob(List<NetworkCommunicator> peers)
        {
            Debug.Print($"** Persistent Empires Auto Save ** Saving {peers.Count()} players", 0, Debug.DebugColor.Blue);
            // Run it on own thread so we dont block onTick.
            HandleCreateOrSavePlayers(peers);
            if (!ConfigManager.GetBoolConfig("SaveOnChange", false))
            {
                HandleCreateOrSavePlayerInventories(peers);
            }
        }

        private static void LogQuery(string query)
        {
            File.AppendAllText("save-logs.txt", query + "\n");
        }

        public static bool HandleIsPlayerWhitelisted(NetworkCommunicator player)
        {
            if (OnIsPlayerWhitelisted != null)
            {
                if (_inDebug)
                {
                    var rightNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    var result2 = OnIsPlayerWhitelisted(player.VirtualPlayer.Id.ToString());
                    
                    LogQuery(String.Format("OnIsPlayerWhitelisted Took {0} ms, {1}", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - rightNow, player?.VirtualPlayer?.ToPlayerId()));
                    
                    return result2;
                }
                var result = OnIsPlayerWhitelisted(player.VirtualPlayer.Id.ToString());

                return result;
            }
            return false;
        }

        public static void HandleDiscordRegister(NetworkCommunicator player, string id)
        {
            if (OnDiscordRegister != null)
            {
                if (_inDebug)
                {
                    var rightNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    
                    OnDiscordRegister(player, id);
                    LogQuery(String.Format("OnDiscordRegister Took {0} ms, {1}, {2}", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - rightNow, player?.VirtualPlayer?.ToPlayerId(), id));

                    return;
                }
                OnDiscordRegister(player, id);
            }
        }

        public static void HandleStartMigration()
        {
            TaleWorlds.Library.Debug.Print("[Save System] Is OnStartMigration null ? " + (OnStartMigration == null).ToString());
            
            if (OnStartMigration != null)
            {
                if (_inDebug)
                {
                    var rightNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                    LogQuery(String.Format("OnStartMigration Took {0} ms", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - rightNow));
                    OnStartMigration();
                    
                    return;
                }

                OnStartMigration();
            }
        }

        public static void HandleCreatePlayerNameIfNotExists(NetworkCommunicator player)
        {
            if (OnCreatePlayerNameIfNotExists != null)
            {
                if (_inDebug)
                {
                    var rightNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    
                    OnCreatePlayerNameIfNotExists(player);
                    LogQuery(String.Format("OnCreatePlayerNameIfNotExists Took {0} ms, {1}", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - rightNow, player?.VirtualPlayer?.ToPlayerId()));

                    return;
                }
                OnCreatePlayerNameIfNotExists(player);
            }
        }

        public static IEnumerable<DBHorseMarket> HandleGetAllHorseMarkets()
        {
            if (OnGetAllStockpileMarkets != null)
            {
                if (_inDebug)
                {
                    var rightNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    var result2 = OnGetAllHorseMarkets();

                    LogQuery(String.Format("OnGetAllHorseMarkets Took {0} ms", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - rightNow));

                    return result2;
                }
                var result = OnGetAllHorseMarkets();
                
                return result;
            }
            return null;
        }

        public static DBHorseMarket HandleGetHorseMarket(PE_HorseMarket market)
        {
            if (OnGetHorseMarket != null)
            {
                if (_inDebug)
                {
                    var rightNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    var result2 = OnGetHorseMarket(market);

                    LogQuery(String.Format("OnGetHorseMarket Took {0} ms, {1}", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - rightNow, market.HorseId));

                    return result2;
                }
                var result = OnGetHorseMarket(market);
                
                return result;
            }

            return null;
        }

        public static DBHorseMarket HandleCreateOrSaveHorseMarket(PE_HorseMarket market)
        {
            if (OnCreateOrSaveHorseMarket != null)
            {
                if (_inDebug)
                {
                    var rightNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    var result2 = OnCreateOrSaveHorseMarket(market);

                    LogQuery(String.Format("OnCreateOrSaveHorseMarket Took {0} ms, {1}", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - rightNow, market.HorseId));

                    return result2;
                }
                var result = OnCreateOrSaveHorseMarket(market);
                
                return result;
            }

            return null;
        }

        public static IEnumerable<DBStockpileMarket> HandleGetAllStockpileMarkets()
        {
            TaleWorlds.Library.Debug.Print("[Save System] Is OnGetAllStockpileMarkets null ? " + (OnGetAllStockpileMarkets == null).ToString());
            
            if (OnGetAllStockpileMarkets != null)
            {
                if (_inDebug)
                {
                    var rightNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    var result2 = OnGetAllStockpileMarkets();

                    LogQuery(String.Format("OnGetAllStockpileMarkets Took {0} ms", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - rightNow));

                    return result2;
                }
                var result = OnGetAllStockpileMarkets();
                
                return result;
            }

            return null;
        }
        public static DBStockpileMarket HandleGetStockpileMarket(PE_StockpileMarket market)
        {
            Debug.Print("[Save System] Is OnGetStockpileMarket null ? " + (OnGetStockpileMarket == null).ToString());

            if (OnGetStockpileMarket != null)
            {
                if (_inDebug)
                {
                    var rightNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    var result2 = OnGetStockpileMarket(market);

                    LogQuery(String.Format("OnGetStockpileMarket Took {0} ms", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - rightNow));

                    return result2;
                }
                var result = OnGetStockpileMarket(market);
                
                return result;
            }
            return null;
        }

        public static void HandleCreateOrSaveStockpileMarkets(List<PE_StockpileMarket> markets)
        {
            Debug.Print("[Save System] HandleCreateOrSaveStockpileMarkets)");
            if (OnUpsertStockpileMarkets != null)
            {
                OnUpsertStockpileMarkets(markets);
            }
        }
        
        public static DBUpgradeableBuilding HandleCreateOrSaveUpgradebleBuilding(PE_UpgradeableBuildings building)
        {
            Debug.Print("[Save System] Is OnCreateOrSaveUpgradebleBuilding null ? " + (OnCreateOrSaveUpgradebleBuilding == null).ToString());
            
            if (OnCreateOrSaveUpgradebleBuilding != null)
            {
                if (_inDebug)
                {
                    var rightNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    var result2 = OnCreateOrSaveUpgradebleBuilding(building);

                    LogQuery(String.Format("OnCreateOrSaveUpgradebleBuilding Took {0} ms, {1}", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - rightNow, building.BuildingName));

                    return result2;
                }
                var result = OnCreateOrSaveUpgradebleBuilding(building);
                
                return result;
            }

            return null;
        }

        public static IEnumerable<DBUpgradeableBuilding> HandleGetAllUpgradeableBuildings()
        {
            Debug.Print("[Save System] Is OnGetAllUpgradeableBuildings null ? " + (OnGetAllUpgradeableBuildings == null).ToString());
            
            if (OnGetAllUpgradeableBuildings != null)
            {
                if (_inDebug)
                {
                    var rightNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    var result2 = OnGetAllUpgradeableBuildings();

                    LogQuery(String.Format("OnGetAllUpgradeableBuildings Took {0} ms", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - rightNow));

                    return result2;
                }
                var result = OnGetAllUpgradeableBuildings();
                
                return result;
            }
            return null;
        }

        public static DBUpgradeableBuilding HandleGetUpgradeableBuilding(PE_UpgradeableBuildings building)
        {
            Debug.Print("[Save System] Is OnGetUpgradeableBuilding null ? " + (OnGetUpgradeableBuilding == null).ToString());
            
            if (OnGetUpgradeableBuilding != null)
            {
                if (_inDebug)
                {
                    var rightNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    var result2 = OnGetUpgradeableBuilding(building);

                    LogQuery(String.Format("OnGetUpgradeableBuilding Took {0} ms, {1}", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - rightNow, building.BuildingName));

                    return result2;
                }
                var result = OnGetUpgradeableBuilding(building);
                
                return result;
            }
            return null;
        }

        public static DBPlayer HandleCreateOrSavePlayer(NetworkCommunicator peer)
        {
            Debug.Print("[Save System] Is OnCreateOrSavePlayer null ? " + (OnCreateOrSavePlayer == null).ToString());
            
            if (OnCreateOrSavePlayer != null)
            {
                if (_inDebug)
                {
                    var rightNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    var result2 = OnCreateOrSavePlayer(peer);

                    LogQuery(String.Format("OnCreateOrSavePlayer Took {0} ms, {1}", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - rightNow, peer?.VirtualPlayer?.ToPlayerId()));

                    return result2;
                }
                var result = OnCreateOrSavePlayer(peer);
                
                return result;
            }
            return null;
        }

        //public static void HandleSavePlayerEquipmentOnDeath(string playerId, Equipment equipment)
        //{
        //    Debug.Print("[Save System] Is OnSavePlayerEquipmentOnDeath null ? " + (OnSavePlayerEquipmentOnDeath == null).ToString());
        //    long rightNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        //    if (OnSavePlayerEquipmentOnDeath != null)
        //    {
        //        OnSavePlayerEquipmentOnDeath(playerId, equipment);
        //    }
        //}
        
        public static void HandleSaveDefaultsForNewPlayer(NetworkCommunicator networkCommunicator, Equipment equipment)
        {
            Debug.Print("[Save System] Is OnSaveDefaultsForNewPlayerr null ? " + (OnSaveDefaultsForNewPlayer == null).ToString());
            if (OnSaveDefaultsForNewPlayer != null)
            {
                if (_inDebug)
                {
                    var rightNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    
                    OnSaveDefaultsForNewPlayer(networkCommunicator, equipment);
                    LogQuery(String.Format("OnSaveDefaultsForNewPlayerr Took {0} ms, {1}, {2}", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - rightNow, networkCommunicator?.VirtualPlayer?.ToPlayerId(), equipment.CalculateEquipmentCode()));

                    return;
                }
                OnSaveDefaultsForNewPlayer(networkCommunicator, equipment);
            }
        }

        public static void HandleCreateOrSavePlayers(List<NetworkCommunicator> peers)
        {
            Debug.Print("[Save System] Is OnCreateOrSavePlayers null ? " + (OnCreateOrSavePlayers == null).ToString());
            if (OnCreateOrSavePlayers != null)
            {
                if (_inDebug)
                {
                    var rightNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    
                    OnCreateOrSavePlayers(peers);
                    LogQuery(String.Format("OnCreateOrSavePlayers Took {0} ms", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - rightNow));

                    return;
                }
                OnCreateOrSavePlayers(peers);
            }
        }

        public static DBPlayer HandleGetOrCreatePlayer(NetworkCommunicator peer, out bool created)
        {
            Debug.Print("[Save System] Is OnGetOrCreatePlayer null ? " + (OnGetOrCreatePlayer == null).ToString());
            created = false;

            if (OnGetOrCreatePlayer != null)
            {
                if (_inDebug)
                {
                    var rightNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    var result2 = OnGetOrCreatePlayer(peer, out created);

                    LogQuery(String.Format("OnGetOrCreatePlayer Took {0} ms, {1}, {2}", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - rightNow, peer?.VirtualPlayer?.ToPlayerId(), result2));

                    return result2;
                }
                var result = OnGetOrCreatePlayer(peer, out created);
                
                return result;
            }
            return null;
        }
        public static IEnumerable<DBInventory> HandleGetAllInventories()
        {
            Debug.Print("[Save System] Is OnGetAllInventories null ? " + (OnGetAllInventories == null).ToString());
            
            if (OnGetAllInventories != null)
            {
                if (_inDebug)
                {
                    var rightNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    var result2 = OnGetAllInventories();

                    LogQuery(String.Format("OnGetAllInventories Took {0} ms", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - rightNow));

                    return result2;
                }
                var result = OnGetAllInventories();
                
                return result;
            }
            return null;
        }
        public static DBInventory HandleGetOrCreatePlayerInventory(NetworkCommunicator networkCommunicator, out bool created)
        {
            Debug.Print("[Save System] Is OnGetOrCreatePlayerInventory null ? " + (OnGetOrCreatePlayerInventory == null).ToString());
            created = false;
            
            if (OnGetOrCreatePlayerInventory != null)
            {
                if (_inDebug)
                {
                    var rightNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    var result2 = OnGetOrCreatePlayerInventory(networkCommunicator, out created);

                    LogQuery(String.Format("OnGetOrCreatePlayerInventory Took {0} ms, {1}, {2}", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - rightNow, networkCommunicator?.VirtualPlayer?.ToPlayerId(), result2));

                    return result2;
                }
                var result = OnGetOrCreatePlayerInventory(networkCommunicator, out created);
                
                return result;
            }
            return null;
        }

        public static DBInventory HandleCreateOrSavePlayerInventory(NetworkCommunicator networkCommunicator)
        {
            Debug.Print("[Save System] Is OnCreateOrSavePlayerInventory null ? " + (OnCreateOrSavePlayerInventory == null).ToString());
            
            if (OnCreateOrSavePlayerInventory != null)
            {
                if (_inDebug)
                {
                    var rightNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    var result2 = OnCreateOrSavePlayerInventory(networkCommunicator);

                    LogQuery(String.Format("OnCreateOrSavePlayerInventory Took {0} ms, {1}", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - rightNow, networkCommunicator?.VirtualPlayer?.ToPlayerId()));

                    return result2;
                }
                var result = OnCreateOrSavePlayerInventory(networkCommunicator);
                
                return result;
            }
            return null;
        }

        public static DBInventory HandleCreateOrSavePlayerInventory(NetworkCommunicator networkCommunicator, PersistentEmpireRepresentative persistentEmpireRepresentative)
        {
            Debug.Print("[Save System] Is OnCreateOrSavePlayerInventory null ? " + (OnCreateOrSavePlayerInventory == null).ToString());

            if (OnCreateOrSavePlayerInventory2 != null)
            {
                if (_inDebug)
                {
                    var rightNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    var result2 = OnCreateOrSavePlayerInventory2(networkCommunicator, persistentEmpireRepresentative);

                    LogQuery(String.Format("OnCreateOrSavePlayerInventory2 Took {0} ms, {1}", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - rightNow, networkCommunicator?.VirtualPlayer?.ToPlayerId()));

                    return result2;
                }
                var result = OnCreateOrSavePlayerInventory2(networkCommunicator, persistentEmpireRepresentative);

                return result;
            }
            return null;
        }

        public static void HandleCreateOrSavePlayerInventories(List<NetworkCommunicator> networkCommunicators)
        {
            Debug.Print("[Save System] Is OnCreateOrSavePlayerInventories null ? " + (OnCreateOrSavePlayerInventories == null).ToString());
            if (OnCreateOrSavePlayerInventories != null)
            {
                if (_inDebug)
                {
                    var rightNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                    OnCreateOrSavePlayerInventories(networkCommunicators);
                    LogQuery(String.Format("OnCreateOrSavePlayerInventories Took {0} ms", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - rightNow));

                    return;
                }
                OnCreateOrSavePlayerInventories(networkCommunicators);
            }
        }

        public static DBInventory HandleGetOrCreateInventory(string inventoryId)
        {
            Debug.Print("[Save System] Is OnGetOrCreateInventory null ? " + (OnGetOrCreateInventory == null).ToString());
            
            if (OnGetOrCreateInventory != null)
            {
                if (_inDebug)
                {
                    var rightNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    var result2 = OnGetOrCreateInventory(inventoryId);

                    LogQuery(String.Format("OnGetOrCreateInventory Took {0} ms, {1}", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - rightNow, inventoryId));

                    return result2;
                }
                var result = OnGetOrCreateInventory(inventoryId);
                
                return result;
            }
            return null;
        }

        public static DBInventory HandleCreateOrSaveInventory(string inventoryId)
        {
            Debug.Print("[Save System] Is OnCreateOrSaveInventory null ? " + (OnCreateOrSaveInventory == null).ToString());
            
            if (OnCreateOrSaveInventory != null)
            {
                if (_inDebug)
                {
                    var rightNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    var result2 = OnCreateOrSaveInventory(inventoryId);

                    LogQuery(String.Format("OnCreateOrSaveInventory Took {0} ms, {1}", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - rightNow, inventoryId));

                    return result2;
                }
                var result = OnCreateOrSaveInventory(inventoryId);
                
                return result;
            }
            return null;
        }

        public static IEnumerable<DBCastle> HandleGetCastles()
        {
            if (OnGetCastles != null)
            {
                if (_inDebug)
                {
                    var rightNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    var result2 = OnGetCastles();

                    LogQuery(String.Format("OnGetCastles Took {0} ms", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - rightNow));

                    return result2;
                }
                var result = OnGetCastles();
                
                return result;
            }
            return null;
        }

        public static DBCastle HandleCreateOrSaveCastle(int castleIndex, int factionIndex)
        {
            if (OnCreateOrSaveCastle != null)
            {
                if (_inDebug)
                {
                    var rightNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    var result2 = OnCreateOrSaveCastle(castleIndex, factionIndex);

                    LogQuery(String.Format("OnCreateOrSaveCastle Took {0} ms, {1}, {2}", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - rightNow, castleIndex, factionIndex));

                    return result2;
                }
                var result = OnCreateOrSaveCastle(castleIndex, factionIndex);
                
                return result;
            }
            return null;
        }

        public static IEnumerable<DBFactions> HandleGetFactions()
        {
            Debug.Print("[Save System] Is OnGetFactions null ? " + (OnGetFactions == null).ToString());
            if (OnGetFactions != null)
            {
                if (_inDebug)
                {
                    var rightNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    var result2 = OnGetFactions();

                    LogQuery(String.Format("OnGetFactions Took {0} ms", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - rightNow));

                    return result2;
                }
                var result = OnGetFactions();
                
                return result;
            }
            return null;
        }

        public static DBFactions HandleGetFaction(int factionIndex)
        {
            Debug.Print("[Save System] Is OnGetFaction null ? " + (OnGetFaction == null).ToString());
            if (OnGetFaction != null)
            {
                if (_inDebug)
                {
                    var rightNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    var result2 = OnGetFaction(factionIndex);

                    LogQuery(String.Format("OnGetFaction Took {0} ms, {1}", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - rightNow, factionIndex));

                    return result2;
                }
                var result = OnGetFaction(factionIndex);
                
                return result;
            }
            return null;
        }

        public static DBFactions HandleCreateOrSaveFaction(Faction faction, int factionIndex)
        {
            Debug.Print("[Save System] Is OnCreateOrSaveFaction null ? " + (OnCreateOrSaveFaction == null).ToString());
            
            if (OnCreateOrSaveFaction != null)
            {
                if (_inDebug)
                {
                    var rightNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    var result2 = OnCreateOrSaveFaction(faction, factionIndex);

                    LogQuery(String.Format("OnCreateOrSaveFaction Took {0} ms, {1}", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - rightNow, factionIndex));

                    return result2;
                }
                var result = OnCreateOrSaveFaction(faction, factionIndex);
                
                return result;
            }
            return null;
        }

        public static bool HandlePlayerUpdateCustomName(NetworkCommunicator peer, string customName)
        {
            if (OnPlayerUpdateCustomName != null)
            {
                if (_inDebug)
                {
                    var rightNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    var result2 = OnPlayerUpdateCustomName(peer, customName);
                    if (result2)
                    {
                        peer.VirtualPlayer.GetType().GetProperty("UserName").SetValue(peer.VirtualPlayer, customName);

                        var playerComponent = peer.GetComponent<PersistentEmpireRepresentative>();
                        var inventory = playerComponent.GetInventory();
                        inventory.InventoryId = $"{peer.VirtualPlayer.Id.ToString()}_{customName}";
                        playerComponent.SetInventory(inventory);
                    }
                    LogQuery(String.Format("OnPlayerUpdateCustomName Took {0} ms, {1}, {2}", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - rightNow, peer?.VirtualPlayer?.ToPlayerId(), customName));

                    return result2;
                }

                var result = OnPlayerUpdateCustomName(peer, customName);
                if(result)
                {
                    peer.VirtualPlayer.GetType().GetProperty("UserName").SetValue(peer.VirtualPlayer, customName);
                    
                    var playerComponent = peer.GetComponent<PersistentEmpireRepresentative>();
                    var inventory = playerComponent.GetInventory();
                    inventory.InventoryId = $"{peer.VirtualPlayer.Id.ToString()}_{customName}";
                    playerComponent.SetInventory(inventory);
                }
                
                return result;
            }
            return false;
        }

        internal static void HandleUpdateWoundedUntil(NetworkCommunicator communicator, long woundTime)
        {
            if (OnPlayerUpdateWoundedUntil != null)
            {
                if (_inDebug)
                {
                    var rightNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                    OnPlayerUpdateWoundedUntil(communicator, woundTime);
                    LogQuery(String.Format("OnPlayerUpdateWoundedUntil Took {0} ms, {1}, {2}", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - rightNow, communicator?.VirtualPlayer?.ToPlayerId(), woundTime));

                    return;
                }
                OnPlayerUpdateWoundedUntil(communicator, woundTime);
            }
        }

        internal static long? HandleGetWoundedUntil(NetworkCommunicator communicator)
        {
            long? woundedUntill = null;
            if (OnGetWoundedUntil != null)
            {
                if (_inDebug)
                {
                    var rightNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                    woundedUntill = OnGetWoundedUntil(communicator);
                    LogQuery(String.Format("OnGetWoundedUntil Took {0} ms, {1}", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - rightNow, communicator?.VirtualPlayer?.ToPlayerId()));

                    return woundedUntill;
                }
                woundedUntill = OnGetWoundedUntil(communicator);
            }

            return woundedUntill;
        }

        public override void OnMissionResultReady(MissionResult missionResult)
        {
            base.OnMissionResultReady(missionResult);
            foreach (NetworkCommunicator peer in GameNetwork.NetworkPeers)
            {
                var persistentEmpireRepresentative = peer.GetComponent<PersistentEmpireRepresentative>();

                if (peer.ControlledAgent != null && persistentEmpireRepresentative.IsFirstAgentSpawned)
                {
                    HandleCreateOrSavePlayer(peer);
                    HandleCreateOrSavePlayerInventory(peer, persistentEmpireRepresentative);
                }
            }
        }

        private void HandleExceptionalExit(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            Debug.Print("UNHANDLED EXCEPTION THROWN : ");
            Debug.Print(e.Message);
            Debug.Print(e.StackTrace);
            Debug.Print(e.ToString());
            if (e.InnerException != null)
            {
                Debug.Print("  INNER EXCEPTION: ");
                Debug.Print(e.InnerException.Message);
                Debug.Print(e.InnerException.StackTrace);
                Debug.Print(e.InnerException.ToString());
            }
#if SERVER
            DiscordBehavior.NotifyException(e);
#endif
            HandleApplicationExit(sender, args);
        }

        private void HandleApplicationExit(object sender, EventArgs e)
        {
#if SERVER
            DiscordBehavior.NotifyServerStatus("SERVER CRASH DETECTED", DiscordBehavior.ColorRed);
#endif
            Debug.Print("! SERVER CRASH DETECTED. SAVING PLAYER DATA ONLY !!!");
            foreach (NetworkCommunicator peer in GameNetwork.NetworkPeers)
            {
                var persistentEmpireRepresentative = peer.GetComponent<PersistentEmpireRepresentative>();

                if (peer.ControlledAgent != null && persistentEmpireRepresentative.IsFirstAgentSpawned)
                {
                    HandleCreateOrSavePlayer(peer);
                    HandleCreateOrSavePlayerInventory(peer, persistentEmpireRepresentative);
                }
            }
        }

        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
#if SERVER
            AppDomain.CurrentDomain.ProcessExit += HandleApplicationExit;
            AppDomain.CurrentDomain.UnhandledException += HandleExceptionalExit;

            SaveDuration = ConfigManager.GetIntConfig("AutosaveDuration", 600);
            HandleStartMigration();
#endif
        }

        public static void OnAddNewPlayerOnServer(ref PlayerConnectionInfo playerConnectionInfo, bool serverPeer, bool isAdmin)
        {
            if (OnGetPlayer != null && playerConnectionInfo != null)
            {
                DBPlayer player = OnGetPlayer($"{playerConnectionInfo.PlayerID.ToString()}_{playerConnectionInfo.Name}");
                if(player == null)
                {
                    player = OnGetPlayer($"{playerConnectionInfo.PlayerID.ToString()}%");

                    if (player != null)
                    {
                        if (string.IsNullOrEmpty(player.CustomName))
                        {
                            player.CustomName = player.Name;
                        }
                    }
                }
                if (player != null && player.CustomName != null && player.CustomName != "" && player.CustomName.IsEmpty() == false)
                {
                    playerConnectionInfo.Name = player.CustomName;
                    if (GameNetwork.VirtualPlayers != null)
                    {
                        foreach (NetworkCommunicator communicator in GameNetwork.DisconnectedNetworkPeers)
                        {
                            if (communicator == null) continue;
                            if (communicator.VirtualPlayer.Id == playerConnectionInfo.PlayerID)
                            {
                                communicator.VirtualPlayer.GetType().GetProperty("UserName").SetValue(communicator.VirtualPlayer, player.CustomName);
                            }
                        }
                    }
                }
            }
        }

        public static void RglExceptionThrown(System.Diagnostics.StackTrace stackTrace, Exception exception)
        {
            // Define your error logging logic
            try
            {
                var message = ToLogString(exception, stackTrace);
                
                Debug.Print($"* Exception in Persistent Empires Module!{message}", color: Debug.DebugColor.Red);

                var path = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"..\..\"));

                path += $"Error_{DateTime.Now.ToString("yyyyMMdd_hhmmss_fff")}.txt";
                using (FileStream fs = File.Create(path))
                {
                    using (var sw = new StreamWriter(fs))
                    {
                        sw.Write(message);
                    }
                }
#if SERVER
                DiscordBehavior.NotifyException(exception);
#endif
            }
            catch(Exception ex)
            {
                Debug.Print($"* Exception in RglExceptionThrown handler! {ex.Message}", color: Debug.DebugColor.Red);
            }
        }

        private static string ToLogString(Exception ex, System.Diagnostics.StackTrace stackTrace)
        {
            var message = $"Message: {ex.Message}{Environment.NewLine}" +
                $"Stack: {ex.StackTrace}{Environment.NewLine}" +
                $"InnerException: {ex.InnerException}{Environment.NewLine}" +
                $"Data: {ex.Data}{Environment.NewLine}" +
                $"Source: {ex.Source}{Environment.NewLine}" +
                $"Help info: {ex.HelpLink}{Environment.NewLine}" +
                $"CustomStackTrace: {stackTrace}";

            return message;
        }

        public static bool IsRunning { get { return _running; } }
        private static bool _running = false;
        public override void OnMissionTick(float dt)
        {
            // Auto save
            base.OnMissionTick(dt);

            if (!_running && DateTimeOffset.Now.ToUnixTimeSeconds() > LastSaveAt + SaveDuration)
            {
                _running = true;
                //Task.Run(() =>
                //{
                    // Create a Job
                    try
                    {
                        InformationComponent.Instance.BroadcastMessage("* Autosave player inventories triggered. It might lag a bit", Colors.Blue.ToUnsignedInteger());
                        AutoSaveJob(GameNetwork.NetworkPeers.Where(x=> x.ControlledAgent != null && x.ControlledAgent.IsActive()).ToList());
                        LastSaveAt = DateTimeOffset.Now.ToUnixTimeSeconds();
                    }
                    catch(Exception ex)
                    {
                        Debug.Print($"* Exception in Autosave player inventories! {ex.Message}", color: Debug.DebugColor.Red);
                        throw ex;
                    }
                    finally
                    {
                        _running = false;
                    }
                //});
            }
        }        
    }
}