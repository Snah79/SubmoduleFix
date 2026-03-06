using Dapper;
using PersistentEmpiresLib;
using PersistentEmpiresLib.Database.DBEntities;
using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresServer.ServerMissions;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.PlayerServices;

namespace PersistentEmpiresSave.Database.Repositories
{
    public class DBInventoryRepository
    {
        private static bool _inDebug = System.Diagnostics.Debugger.IsAttached;

        public static void Initialize()
        {
            SaveSystemBehavior.OnGetAllInventories += GetAllInventories;
            SaveSystemBehavior.OnGetOrCreateInventory += GetOrCreateInventory;
            
            SaveSystemBehavior.OnGetOrCreatePlayerInventory += GetOrCreatePlayerInventory;
            SaveSystemBehavior.OnCreateOrSaveInventory += CreateOrSaveInventory;
            SaveSystemBehavior.OnCreateOrSavePlayerInventories += UpsertPlayerInventories;
            SaveSystemBehavior.OnCreateOrSavePlayerInventory += CreateOrSavePlayerInventory;
            SaveSystemBehavior.OnCreateOrSavePlayerInventory2 += CreateOrSavePlayerInventory2;
        }

        private static void LogQuery(string query)
        {
            System.IO.File.AppendAllText("save-logs.txt", query + "\n");
        }

        private static DBInventory CreateDBInventoryFromPlayer(NetworkCommunicator networkCommunicator)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = networkCommunicator.GetComponent<PersistentEmpireRepresentative>();
            if (persistentEmpireRepresentative == null) return null; // Shouldn't be the case
            Debug.Print("[Save Module] CREATING DBInventory FOR PLAYER " + (networkCommunicator != null ? networkCommunicator.UserName : "NETWORK COMMUNICATOR IS NULL !!!!"));

            string playerId = networkCommunicator.VirtualPlayer.ToPlayerId();

            return new DBInventory
            {
                InventoryId = playerId,
                InventorySerialized = persistentEmpireRepresentative == null ? "||||" : persistentEmpireRepresentative.GetInventory().Serialize(),
                IsPlayerInventory = true
            };
        }

        private static DBInventory CreateDBInventoryFromPlayer(PersistentEmpireRepresentative persistentEmpireRepresentative, string playerId)
        {
            if (persistentEmpireRepresentative == null) return null; // Shouldn't be the case
            Debug.Print("[Save Module] CREATING DBInventory FOR PLAYER " + (!string.IsNullOrEmpty(playerId) ? playerId : "NETWORK COMMUNICATOR IS NULL !!!!"));

            return new DBInventory
            {
                InventoryId = playerId,
                InventorySerialized = persistentEmpireRepresentative == null ? "||||" : persistentEmpireRepresentative.GetInventory().Serialize(),
                IsPlayerInventory = true
            };
        }

        private static DBInventory CreateDBInventoryFromId(string inventoryId)
        {
            PlayerInventoryComponent playerInventoryComponent = Mission.Current.GetMissionBehavior<PlayerInventoryComponent>();
            Debug.Print("[Save Module] CREATING DBInventory FOR INVENTORY " + inventoryId + " IS REGISTERED ? " + playerInventoryComponent.CustomInventories.ContainsKey(inventoryId));
            return new DBInventory
            {
                InventoryId = inventoryId,
                InventorySerialized = playerInventoryComponent.CustomInventories[inventoryId].Serialize(),
                IsPlayerInventory = false
            };
        }

        public static IEnumerable<DBInventory> GetAllInventories()
        {
            try
            {
                Debug.Print("[Save Module] LOADING ALL INVENTORIES FROM DB");
                return DBConnection.Connection.Query<DBInventory>("SELECT InventoryId, InventorySerialized FROM Inventories WHERE IsPlayerInventory = 0");
            }
            catch (Exception ex)
            {
                DiscordBehavior.NotifyException(ex);
                return null;
            }
        }

        public static DBInventory GetInventory(string inventoryId)
        {
            try
            {
                Debug.Print("[Save Module] LOADING INVENTORY " + inventoryId + " FROM DB");
                IEnumerable<DBInventory> results = DBConnection.Connection.Query<DBInventory>("SELECT * FROM Inventories WHERE InventoryId = @InventoryId", new { InventoryId = inventoryId });
                Debug.Print("[Save Module] LOADING INVENTORY " + inventoryId + " RESULT COUNT IS " + results.Count());
                if (results.Count() == 0) return null;
                return results.First();
            }
            catch (Exception ex)
            {
                DiscordBehavior.NotifyException(ex);
                return null;
            }
        }

        public static DBInventory GetPlayerInventory(NetworkCommunicator networkCommunicator)
        {
            try
            {
                string playerId = networkCommunicator.VirtualPlayer.ToPlayerId();

                Debug.Print("[Save Module] LOADING INVENTORY FOR PLAYER " + (networkCommunicator != null ? networkCommunicator.UserName : "NETWORK COMMUNICATOR IS NULL !!!!") + " FROM DB");
                IEnumerable<DBInventory> results = DBConnection.Connection.Query<DBInventory>("SELECT * FROM Inventories WHERE InventoryId = @InventoryId and IsPlayerInventory = 1", new { InventoryId = playerId });
                Debug.Print("[Save Module] LOADING INVENTORY FOR PLAYER " + (networkCommunicator != null ? networkCommunicator.UserName : "NETWORK COMMUNICATOR IS NULL !!!!") + " RESULT COUNT IS " + results.Count());
                if (results.Count() == 0) return null;
                return results.First();
            }
            catch (Exception ex)
            {
                DiscordBehavior.NotifyException(ex);

                return null;
            }
        }

        public static DBInventory GetOrCreatePlayerInventory(NetworkCommunicator networkCommunicator, out bool created)
        {
            created = false;
            DBInventory dbInventory = GetPlayerInventory(networkCommunicator);
            if (dbInventory == null)
            {
                created = true;
                dbInventory = CreatePlayerInventory(networkCommunicator);
            }
            return dbInventory;
        }

        public static DBInventory CreatePlayerInventory(NetworkCommunicator networkCommunicator)
        {
            try
            {
                DBInventory dbInventory = CreateDBInventoryFromPlayer(networkCommunicator);
                if (dbInventory == null) return dbInventory;
                Debug.Print("[Save Module] CREATING INVENTORY FOR PLAYER " + (networkCommunicator != null ? networkCommunicator.UserName : "NETWORK COMMUNICATOR IS NULL !!!!"));
                string insertQuery = "INSERT INTO Inventories (InventoryId, IsPlayerInventory, InventorySerialized) VALUES (@InventoryId, @IsPlayerInventory, @InventorySerialized)";
                DBConnection.Connection.Execute(insertQuery, dbInventory);
                Debug.Print("[Save Module] CREATED INVENTORY FOR PLAYER " + (networkCommunicator != null ? networkCommunicator.UserName : "NETWORK COMMUNICATOR IS NULL !!!!"));
                return dbInventory;
            }
            catch (Exception ex)
            {
                DiscordBehavior.NotifyException(ex);

                return null;
            }
        }

        public static DBInventory SavePlayerInventory(NetworkCommunicator networkCommunicator)
        {
            try
            {
                DBInventory dbInventory = CreateDBInventoryFromPlayer(networkCommunicator);
                if (dbInventory == null) return dbInventory;

                Debug.Print("[Save Module] UPDATING INVENTORY FOR PLAYER " + (networkCommunicator != null ? networkCommunicator.UserName : "NETWORK COMMUNICATOR IS NULL !!!!"));
                var updateQuery = "UPDATE Inventories SET InventorySerialized = @InventorySerialized WHERE InventoryId = @InventoryId";
                DBConnection.Connection.Execute(updateQuery, new { InventoryId = dbInventory.InventoryId, InventorySerialized = dbInventory.InventorySerialized });
                Debug.Print("[Save Module] UPDATED INVENTORY FOR PLAYER " + (networkCommunicator != null ? networkCommunicator.UserName : "NETWORK COMMUNICATOR IS NULL !!!!"));
                LoggerHelper.LogAnActionNoDiscord(networkCommunicator, LogAction.UpsertPlayerInventory, null, new object[] { "InventoryId: " + dbInventory.InventoryId + ", Inventory: " + dbInventory.InventorySerialized });

                return dbInventory;
            }
            catch (Exception ex)
            {
                DiscordBehavior.NotifyException(ex);

                return null;
            }
        }

        public static DBInventory SavePlayerInventory(NetworkCommunicator networkCommunicator, PersistentEmpireRepresentative persistentEmpireRepresentative)
        {
            try
            {
                var playerId = networkCommunicator.VirtualPlayer?.ToPlayerId();
                if (_inDebug)
                {
                    var rightNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    var dbInventory2 = CreateDBInventoryFromPlayer(persistentEmpireRepresentative, playerId);
                    LogQuery(String.Format("CreateDBInventoryFromPlayer Took {0} ms, {1}", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - rightNow, networkCommunicator?.VirtualPlayer?.ToPlayerId()));
                    
                    if (dbInventory2 == null) return null;

                    rightNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    Debug.Print("[Save Module] UPDATING INVENTORY FOR PLAYER " + (!string.IsNullOrEmpty(playerId) ? playerId : "NETWORK COMMUNICATOR IS NULL !!!!"));
                    string updateQuery2 = "UPDATE Inventories SET InventorySerialized = @InventorySerialized WHERE InventoryId = @InventoryId";
                    DBConnection.Connection.Execute(updateQuery2, new { InventoryId = dbInventory2.InventoryId, InventorySerialized = dbInventory2.InventorySerialized });
                    Debug.Print("[Save Module] UPDATED INVENTORY FOR PLAYER " + (!string.IsNullOrEmpty(playerId) ? playerId : "NETWORK COMMUNICATOR IS NULL !!!!"));
                    LoggerHelper.LogAnActionNoDiscord(networkCommunicator, LogAction.UpsertPlayerInventory, null, new object[] { "InventoryId: " + dbInventory2.InventoryId + ", Inventory: " + dbInventory2.InventorySerialized });
                    LogQuery(String.Format("DB SAVE Took {0} ms, {1}", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - rightNow, networkCommunicator?.VirtualPlayer?.ToPlayerId()));

                    return dbInventory2;
                }
                var dbInventory = CreateDBInventoryFromPlayer(persistentEmpireRepresentative, playerId);
                
                if (dbInventory == null) return null;
                
                Debug.Print("[Save Module] UPDATING INVENTORY FOR PLAYER " + (!string.IsNullOrEmpty(playerId) ? playerId : "NETWORK COMMUNICATOR IS NULL !!!!"));
                string updateQuery = "UPDATE Inventories SET InventorySerialized = @InventorySerialized WHERE InventoryId = @InventoryId";
                DBConnection.Connection.Execute(updateQuery, new { InventoryId = dbInventory.InventoryId, InventorySerialized = dbInventory.InventorySerialized });
                Debug.Print("[Save Module] UPDATED INVENTORY FOR PLAYER " + (!string.IsNullOrEmpty(playerId) ? playerId : "NETWORK COMMUNICATOR IS NULL !!!!"));
                LoggerHelper.LogAnActionNoDiscord(networkCommunicator, LogAction.UpsertPlayerInventory, null, new object[] { "InventoryId: " + dbInventory.InventoryId + ", Inventory: " + dbInventory.InventorySerialized });
             
                return dbInventory;
            }
            catch (Exception ex)
            {
                DiscordBehavior.NotifyException(ex);

                return null;
            }
        }

        public static void UpsertPlayerInventories(List<NetworkCommunicator> players)
        {
            try
            {
                Debug.Print($"[Save Module] INSERT/UPDATE FOR {players.Count()} PLAYER INVENTORIES TO DB");
                if (players.Any())
                {
                    string query = @"
            INSERT INTO Inventories (InventoryId, IsPlayerInventory, InventorySerialized)
            VALUES "
                    ;

                    foreach (var player in players)
                    {
                        var dbInventory = CreateDBInventoryFromPlayer(player);

                        query += $"('{dbInventory.InventoryId}', 1, '{dbInventory.InventorySerialized}'),";
                    }
                    // remove last ","
                    query = query.TrimEnd(',');
                    query += @" 
                    ON DUPLICATE KEY UPDATE
                    InventorySerialized = VALUES(InventorySerialized)";

                    DBConnection.Connection.Execute(query);
                }
            }
            catch (Exception ex)
            {
                DiscordBehavior.NotifyException(ex);
            }
        }

        public static DBInventory CreateOrSavePlayerInventory2(NetworkCommunicator networkCommunicator, PersistentEmpireRepresentative persistentEmpireRepresentative)
        {
            if (_inDebug)
            {
                var rightNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                if (GetPlayerInventory(networkCommunicator) == null)
                {
                    return CreatePlayerInventory(networkCommunicator);
                }

                LogQuery(String.Format("GetPlayerInventory Took {0} ms, {1}", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - rightNow, networkCommunicator?.VirtualPlayer?.ToPlayerId()));

                rightNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                var tmp2 = SavePlayerInventory(networkCommunicator, persistentEmpireRepresentative);

                LogQuery(String.Format("SavePlayerInventory Took {0} ms, {1}", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - rightNow, networkCommunicator?.VirtualPlayer?.ToPlayerId()));

                return tmp2;
            }
            
            if (GetPlayerInventory(networkCommunicator) == null)
            {
                return CreatePlayerInventory(networkCommunicator);
            }

            var tmp = SavePlayerInventory(networkCommunicator, persistentEmpireRepresentative);
            
            return tmp;
        }

        public static DBInventory CreateOrSavePlayerInventory(NetworkCommunicator networkCommunicator)
        {
            if (GetPlayerInventory(networkCommunicator) == null)
            {
                return CreatePlayerInventory(networkCommunicator);
            }
            return SavePlayerInventory(networkCommunicator);
        }

        public static DBInventory GetOrCreateInventory(string inventoryId)
        {
            DBInventory dBInventory = GetInventory(inventoryId);
            if (dBInventory == null)
            {
                dBInventory = CreateInventory(inventoryId);
            }
            return dBInventory;
        }


        public static DBInventory CreateOrSaveInventory(string inventoryId)
        {
            DBInventory dbInventory = GetInventory(inventoryId);
            if (dbInventory == null)
            {
                return CreateInventory(inventoryId);
            }
            return SaveInventory(inventoryId);
        }

        public static DBInventory CreateInventory(string inventoryId)
        {
            try
            {
                DBInventory dbInventory = CreateDBInventoryFromId(inventoryId);
                PlayerInventoryComponent playerInventoryComponent = Mission.Current.GetMissionBehavior<PlayerInventoryComponent>();
                Debug.Print("[Save Module] CREATING RECORD FOR INVENTORY " + inventoryId + " IS REGISTERED ? " + playerInventoryComponent.CustomInventories.ContainsKey(inventoryId));
                string insertQuery = "INSERT INTO Inventories (InventoryId, IsPlayerInventory, InventorySerialized) VALUES (@InventoryId, 0, @InventorySerialized)";
                DBConnection.Connection.Execute(insertQuery, dbInventory);
                Debug.Print("[Save Module] CREATED RECORD FOR INVENTORY " + inventoryId + " IS REGISTERED ? " + playerInventoryComponent.CustomInventories.ContainsKey(inventoryId));
                return dbInventory;
            }
            catch (Exception ex)
            {
                DiscordBehavior.NotifyException(ex);

                return null;
            }
        }

        public static DBInventory SaveInventory(string inventoryId)
        {
            try
            {
                var dbInventory = CreateDBInventoryFromId(inventoryId);
                var playerInventoryComponent = Mission.Current.GetMissionBehavior<PlayerInventoryComponent>();

                Debug.Print("[Save Module] UPDATING RECORD FOR INVENTORY " + inventoryId + " IS REGISTERED ? " + playerInventoryComponent.CustomInventories.ContainsKey(inventoryId));
                var updateQuery = "UPDATE Inventories SET InventorySerialized = @InventorySerialized WHERE InventoryId = @InventoryId";
                DBConnection.Connection.Execute(updateQuery, new { InventoryId = inventoryId, InventorySerialized = dbInventory.InventorySerialized });
                Debug.Print("[Save Module] UPDATED RECORD FOR INVENTORY " + inventoryId + " IS REGISTERED ? " + playerInventoryComponent.CustomInventories.ContainsKey(inventoryId));
                
                return dbInventory;
            }
            catch (Exception ex)
            {
                DiscordBehavior.NotifyException(ex);

                return null;
            }
        }

        public static void UpdateInventoryId(string oldInventoryId, string newInventoryId)
        {
            try
            {
                string updateQuery = "UPDATE Inventories SET InventoryId = @InventoryId WHERE InventoryId = @OldInventoryId ";
                DBConnection.Connection.Execute(updateQuery,
                    new
                    {
                        OldInventoryId = oldInventoryId,
                        InventoryId = newInventoryId
                    });
            }
            catch (Exception ex)
            {
                DiscordBehavior.NotifyException(ex);
            }
        }

        public static void DeleteInventoryId(string inventoryId)
        {
            try
            {
                string updateQuery = "DELETE FROM Inventories WHERE InventoryId = @InventoryId ";
                DBConnection.Connection.Execute(updateQuery,
                    new
                    {
                        InventoryId = inventoryId,
                    });
            }
            catch (Exception ex)
            {
                DiscordBehavior.NotifyException(ex);
            }
        }
    }
}