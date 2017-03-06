using System;
using System.Linq;
using System.Reflection;
using ColossalFramework;
using ColossalFramework.IO;
using SaveOurSaves.Redirection;
using UnityEngine;

namespace SaveOurSaves.Detours
{
    [TargetType(typeof(LoadingProfiler))]
    public class LoadingProfilerDetour : LoadingProfiler
    {
        private static readonly string[] managers = {
            "SimulationManager",
            "GameAreaManager",
            "TerrainManager",
            "WaterSimulation",
            "InstanceManager",
            "PathManager",
            "NetManager",
            "BuildingManager",
            "CitizenManager",
            "TransferManager",
            "NaturalResourceManager",
            "ImmaterialResourceManager",
            "ElectricityManager",
            "WaterManager",
            "ZoneManager",
            "PropManager",
            "TreeManager",
            "VehicleManager",
            "TransportManager",
            "EconomyManager",
            "DistrictManager",
            "InfoManager",
            "StatisticsManager",
            "UnlockManager",
            "MessageManager",
            "WeatherManager",
            "GuideManager"
        };

        public static int counter = 0;
        public static bool fixesApplied = false;


        public static void Initialize()
        {
            fixesApplied = false;
            counter = 0;
        }

        public static void Revert()
        {
            fixesApplied = false;
            counter = 0;
        }

        [RedirectMethod]
        public void EndAfterDeserialize(DataSerializer s, string name)
        {
            try
            {
                if (managers.Contains(name))
                {
                    counter++;
                }
                if (!fixesApplied && counter == managers.Length - 1)
                {
                    try
                    {
                        RepairSave();
                    }
                    catch
                    {
                        // ignored
                    }
                    fixesApplied = true;
                }
            }
            finally
            {
                var events =
                (FastList<LoadingProfiler.Event>)
                typeof(LoadingProfiler).GetField("m_events", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this);
                events.Add(new LoadingProfiler.Event(LoadingProfiler.Type.EndAfterDeserialize, (string)null, 0));
            }
        }

        private static void RepairSave()
        {
            FixBrokenSegments();
            FixBrokenTransfers();
            FixBrokenVehicles();
            FixBrokenCitizens();
            FixBrokenBuildings();

            FixBrokenTrees();
            FixBrokenProps();
            FixBrokenLines();
        }

        private static void FixBrokenLines()
        {
            var brokenCount = 0;
            for (ushort lineId = 0; lineId < TransportManager.instance.m_lines.m_buffer.Length; lineId++)
            {
                var transportLine = TransportManager.instance.m_lines.m_buffer[lineId];
                if (transportLine.m_flags == TransportLine.Flags.None)
                {
                    continue;
                }
                try
                {
                    if (transportLine.CountStops(lineId) >= 32768)
                    {
                        TransportManager.instance.ReleaseLine(lineId);
                        brokenCount++;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            if (brokenCount > 0) UnityEngine.Debug.Log("Removed " + brokenCount + " broken transport line instances.");
        }

        private static void FixBrokenBuildings()
        {
            var brokenCount = 0;

            // Fix broken buildings
            Array16<Building> buildings = Singleton<BuildingManager>.instance.m_buildings;
            for (int i = 0; i < buildings.m_size; i++)
            {
                if (buildings.m_buffer[i].m_flags == Building.Flags.None)
                {
                    continue;
                }
                try
                {
                    if (buildings.m_buffer[i].Info == null)
                    {
                        Singleton<BuildingManager>.instance.ReleaseBuilding((ushort)i);
                        brokenCount++;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            if (brokenCount > 0) UnityEngine.Debug.Log("Removed " + brokenCount + " broken building instances.");
        }

        private static void FixBrokenVehicles()
        {
            uint brokenCount = 0;
            uint confusedCount = 0;

            // Fix broken vehicles
            Array16<Vehicle> vehicles = Singleton<VehicleManager>.instance.m_vehicles;
            for (int i = 0; i < vehicles.m_size; i++)
            {
                if (vehicles.m_buffer[i].m_flags != Vehicle.Flags.None)
                {
                    try
                    {
                        bool exists = (vehicles.m_buffer[i].m_flags & Vehicle.Flags.Spawned) != Vehicle.Flags.None;

                        // Vehicle validity
                        InstanceID target;
                        bool isInfoNull = vehicles.m_buffer[i].Info == null;
                        bool isLeading = vehicles.m_buffer[i].m_leadingVehicle == 0;
                        bool isWaiting = !exists &&
                                         (vehicles.m_buffer[i].m_flags & Vehicle.Flags.WaitingSpace) != Vehicle.Flags.None;
                        bool isConfused = exists && isLeading && !isInfoNull &&
                                          vehicles.m_buffer[i].Info.m_vehicleAI.GetLocalizedStatus((ushort)i,
                                              ref vehicles.m_buffer[i], out target) ==
                                          ColossalFramework.Globalization.Locale.Get("VEHICLE_STATUS_CONFUSED");
                        bool isSingleTrailer = false;

                        bool isWrongTarget =
                            BuildingManager.instance.m_buildings.m_buffer[vehicles.m_buffer[i].m_targetBuilding].Info ==
                            null;
                        if (isWrongTarget)
                        {
                            vehicles.m_buffer[i].m_targetBuilding = 0;
                        }
                        bool isWrongSource =
                            BuildingManager.instance.m_buildings.m_buffer[vehicles.m_buffer[i].m_sourceBuilding].Info == null;
                        if (isWrongSource)
                        {
                            vehicles.m_buffer[i].m_sourceBuilding = 0;
                        }
                        if (isInfoNull || isSingleTrailer || isWaiting || isConfused)
                        {

                            Singleton<VehicleManager>.instance.ReleaseVehicle((ushort)i);
                            if (isInfoNull) brokenCount++;
                            if (isConfused) confusedCount++;

                        }
                    }
                    catch (Exception e)
                    {
                        UnityEngine.Debug.LogException(e);
                    }
                }
            }

            if (confusedCount > 0) UnityEngine.Debug.Log("Removed " + confusedCount + " confused vehicle instances.");

            Array16<VehicleParked> vehiclesParked = Singleton<VehicleManager>.instance.m_parkedVehicles;
            for (int i = 0; i < vehiclesParked.m_size; i++)
            {
                if (vehiclesParked.m_buffer[i].Info == null)
                {
                    try
                    {
                        Singleton<VehicleManager>.instance.ReleaseParkedVehicle((ushort)i);
                        brokenCount++;
                    }
                    catch (Exception e)
                    {
                        UnityEngine.Debug.LogError(e);
                    }
                }
            }

            if (brokenCount > 0) UnityEngine.Debug.Log("Removed " + brokenCount + " broken vehicle instances.");
        }

        private static void FixBrokenTransfers()
        {
            var brokenCount = 0;
            // Fix broken offers
            TransferManager.TransferOffer[] incomingOffers = typeof(TransferManager).GetField("m_incomingOffers", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(TransferManager.instance) as TransferManager.TransferOffer[];
            TransferManager.TransferOffer[] outgoingOffers = typeof(TransferManager).GetField("m_outgoingOffers", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(TransferManager.instance) as TransferManager.TransferOffer[];

            ushort[] incomingCount = typeof(TransferManager).GetField("m_incomingCount", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(TransferManager.instance) as ushort[];
            ushort[] outgoingCount = typeof(TransferManager).GetField("m_outgoingCount", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(TransferManager.instance) as ushort[];

            int[] incomingAmount = typeof(TransferManager).GetField("m_incomingAmount", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(TransferManager.instance) as int[];
            int[] outgoingAmount = typeof(TransferManager).GetField("m_outgoingAmount", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(TransferManager.instance) as int[];

            // Based on TransferManager.RemoveAllOffers
            for (int i = 0; i < 64; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    int num = i * 8 + j;
                    int num2 = (int)incomingCount[num];
                    for (int k = num2 - 1; k >= 0; k--)
                    {
                        int num3 = num * 256 + k;
                        if (IsInfoNull(incomingOffers[num3]))
                        {
                            incomingAmount[i] -= incomingOffers[num3].Amount;
                            incomingOffers[num3] = incomingOffers[--num2];
                            brokenCount++;
                        }
                    }
                    incomingCount[num] = (ushort)num2;
                    int num4 = (int)outgoingCount[num];
                    for (int l = num4 - 1; l >= 0; l--)
                    {
                        int num5 = num * 256 + l;
                        if (IsInfoNull(outgoingOffers[num5]))
                        {
                            outgoingAmount[i] -= outgoingOffers[num5].Amount;
                            outgoingOffers[num5] = outgoingOffers[--num4];
                            brokenCount++;
                        }
                    }
                    outgoingCount[num] = (ushort)num4;
                }
            }

            typeof(TransferManager).GetField("m_incomingOffers", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(TransferManager.instance, incomingOffers);
            typeof(TransferManager).GetField("m_outgoingOffers", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(TransferManager.instance, outgoingOffers);

            typeof(TransferManager).GetField("m_incomingCount", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(TransferManager.instance, incomingCount);
            typeof(TransferManager).GetField("m_outgoingCount", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(TransferManager.instance, outgoingCount);

            typeof(TransferManager).GetField("m_incomingAmount", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(TransferManager.instance, incomingAmount);
            typeof(TransferManager).GetField("m_outgoingAmount", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(TransferManager.instance, outgoingAmount);


            if (brokenCount > 0) UnityEngine.Debug.Log("Removed " + brokenCount + " broken transfer offers.");
        }


        private static void FixBrokenCitizens()
        {
            var brokenCount = 0;

            // Fix broken citizens
            Array16<CitizenInstance> instances = Singleton<CitizenManager>.instance.m_instances;
            for (int i = 0; i < instances.m_size; i++)
            {

                if (instances.m_buffer[i].m_targetBuilding >= 0 && instances.m_buffer[i].m_targetBuilding < BuildingManager.MAX_BUILDING_COUNT)

                    if (BuildingManager.instance.m_buildings.m_buffer[instances.m_buffer[i].m_targetBuilding].Info ==
                        null)
                    {
                        try
                        {
                            instances.m_buffer[i].m_targetBuilding = 0;
                            brokenCount++;
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogException(e);
                        }
                    }
                if (BuildingManager.instance.m_buildings.m_buffer[instances.m_buffer[i].m_sourceBuilding].Info == null)
                {
                    try
                    {
                        instances.m_buffer[i].m_sourceBuilding = 0;
                        brokenCount++;
                    }
                    catch (Exception e)
                    {
                        UnityEngine.Debug.LogException(e);
                    }
                }
            }


            if (brokenCount > 0) UnityEngine.Debug.Log("Removed " + brokenCount + " broken citizen instances.");
        }

        private static void FixBrokenTrees()
        {

            var brokenCount = 0;

            // Fix broken trees
            Array32<TreeInstance> trees = TreeManager.instance.m_trees;
            for (int i = 0; i < trees.m_size; i++)
            {
                if (trees.m_buffer[i].Info == null)
                {
                    try
                    {
                        TreeManager.instance.ReleaseTree((ushort)i);
                        brokenCount++;
                    }
                    catch (Exception e)
                    {
                        UnityEngine.Debug.LogException(e);
                    }
                }
            }

            if (brokenCount > 0) Debug.Log("Removed " + brokenCount + " broken tree instances.");
        }

        private static void FixBrokenProps()
        {
            var brokenCount = 0;

            // Fix broken props
            Array16<PropInstance> props = PropManager.instance.m_props;
            for (int i = 0; i < props.m_size; i++)
            {
                if (props.m_buffer[i].Info == null)
                {
                    try
                    {
                        PropManager.instance.ReleaseProp((ushort)i);
                        brokenCount++;
                    }
                    catch (Exception e)
                    {
                        UnityEngine.Debug.LogException(e);
                    }
                }
            }

            if (brokenCount > 0) Debug.Log("Removed " + brokenCount + " broken prop instances.");
        }

        private static void FixBrokenSegments()
        {
            var brokenCount = 0;

            // Fix broken props
            Array16<NetSegment> segments = NetManager.instance.m_segments;
            for (int i = 0; i < segments.m_size; i++)
            {
                if (segments.m_buffer[i].Info == null)
                {
                    try
                    {
                        NetManager.instance.ReleaseSegment((ushort)i, false);
                        brokenCount++;
                    }
                    catch (Exception e)
                    {
                        UnityEngine.Debug.LogException(e);
                    }
                }
            }

            if (brokenCount > 0) Debug.Log("Removed " + brokenCount + " broken segment instances.");
        }




        private static bool IsInfoNull(TransferManager.TransferOffer offer)
        {
            if (!offer.Active) return false;

            if (offer.Vehicle != 0)
                return VehicleManager.instance.m_vehicles.m_buffer[offer.Vehicle].Info == null;

            if (offer.Building != 0)
                return BuildingManager.instance.m_buildings.m_buffer[offer.Building].Info == null;

            return false;
        }
    }
}