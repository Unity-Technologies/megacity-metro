using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Burst;
using Unity.Entities;
using Unity.MegacityMetro.Gameplay;
using Unity.NetCode.Extensions;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public partial struct RandomShipVisualsSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        uint nowMiliseconds = (uint)DateTime.Now.Millisecond;
        ComponentLookup<PlayerName> playerNameLookupRO = SystemAPI.GetComponentLookup<PlayerName>(true);
        EntityCommandBuffer ecb = SystemAPI.GetSingletonRW<BeginSimulationEntityCommandBufferSystem.Singleton>().ValueRW.CreateCommandBuffer(state.WorldUnmanaged);
        foreach (var (legEntity, prefabs, entity) in SystemAPI.Query<RootEntity, DynamicBuffer<ShipRandomVisuals>>().WithEntityAccess())
        {
            Random random;
            if (playerNameLookupRO.TryGetComponent(legEntity.Entity, out PlayerName playerName))
            {
                if (playerName.Name == "") continue;
                random = new Random((uint)playerName.Name.GetHashCode());
            }
            else
            {
                random = Random.CreateFromIndex(nowMiliseconds);
            }

            Entity randomPrefab = prefabs[random.NextInt(0, prefabs.Length)].ShipVisual;
            Entity instance = ecb.Instantiate(randomPrefab);

            ecb.AddComponent(instance, new Parent { Value = entity });
            ecb.AddComponent(instance, new RootEntity { Entity = legEntity.Entity });
            
            ecb.AppendToBuffer(legEntity.Entity, new LinkedEntityGroup { Value = instance});
            
            ecb.RemoveComponent<ShipRandomVisuals>(entity);
        }
        
        foreach (var (shipVisuals, rootEntity, entity) in SystemAPI.Query<ShipVisuals, RootEntity>().WithEntityAccess())
        {
            VehicleLaser vehicleLaser = SystemAPI.GetComponent<VehicleLaser>(rootEntity.Entity);
            vehicleLaser.LocalLaserStartPoint = shipVisuals.LocalLaserStartPoint;
            SystemAPI.SetComponent(rootEntity.Entity, vehicleLaser);
            
            PlayerVehicleSettings vehicleSettings = SystemAPI.GetComponent<PlayerVehicleSettings>(rootEntity.Entity);
            vehicleSettings.VehicleFX = shipVisuals.FXEntity;
            SystemAPI.SetComponent(rootEntity.Entity, vehicleSettings);

            ecb.RemoveComponent<ShipVisuals>(entity);
            ecb.RemoveComponent<RootEntity>(entity);
        }
    }
}
