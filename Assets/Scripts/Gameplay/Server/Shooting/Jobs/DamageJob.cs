using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode.Extensions;
using Unity.Transforms;

namespace Unity.MegacityMetro.Gameplay
{
    [BurstCompile]
    [WithAll(typeof(Simulate))]
    [WithAll(typeof(VehicleLaser))]
    public partial struct DamageJob : IJobEntity
    {
        [ReadOnly]
        public float DeltaTime;
        [ReadOnly]
        public ComponentLookup<Immunity> ImmunityLookup;
        [ReadOnly]
        public ComponentLookup<PlayerName> PlayerNameLookup;
        [ReadOnly] 
        public ComponentLookup<LocalToWorld> LocalToWorldLookup;
        public ComponentLookup<VehicleHealth> HealthLookup;
        public ComponentLookup<PlayerScore> PlayerScoreLookup;
        public ComponentLookup<VehicleLaser> LaserLookup;

        [BurstCompile]
        private void Execute(in Entity entity)
        {
            var laser = LaserLookup[entity];
            
            if (laser.IsShooting &&
                laser.DetectedTarget != Entity.Null &&
                laser.DetectedTarget != entity &&
                HealthLookup.TryGetComponent(laser.DetectedTarget, out VehicleHealth targetHealth) &&
                ImmunityLookup.TryGetComponent(laser.DetectedTarget, out Immunity targetImmunity) &&
                LaserLookup.TryGetComponent(laser.DetectedTarget, out VehicleLaser targetLaser) &&
                PlayerScoreLookup.TryGetComponent(entity, out PlayerScore ownerScore) &&
                PlayerNameLookup.TryGetComponent(entity, out PlayerName ownerName) &&
                LocalToWorldLookup.TryGetComponent(entity, out LocalToWorld localToWorld) &&
                LocalToWorldLookup.TryGetComponent(laser.DetectedTarget, out LocalToWorld targetLocalToWorld) )
            {
                float damage = DeltaTime * laser.DamagePerSecond;

                if (targetImmunity.Counter > 0f)
                    return;

                targetHealth.LookAtEnemyDegrees = 0;
                if (targetHealth.Value > 0)
                {
                    targetHealth.Value -= damage;
                    ownerScore.Value += damage;

                    //victim gets the angle to look at the attacker
                    {
                        var targetForward = targetLocalToWorld.Forward;
                        var attackerDirection = targetLocalToWorld.Position - localToWorld.Position;
                        targetForward.y = 0f;
                        attackerDirection.y = 0f;
                        
                        float angle = math.degrees(math.atan2(attackerDirection.x, attackerDirection.z) - math.atan2(targetForward.x, targetForward.z));
                        // adjust the angle to the range between [0, 360]
                        angle = (angle + 180) % 360;
                        targetHealth.LookAtEnemyDegrees = math.radians(angle);
                    }
                    
                    // Saving the name of the last player applying damage and the last player attacked
                    if (PlayerScoreLookup.TryGetComponent(laser.DetectedTarget, out PlayerScore targetScore) &&
                        PlayerNameLookup.TryGetComponent(laser.DetectedTarget, out PlayerName targetName))
                    {
                        targetScore.KillerName = ownerName.Name;
                        ownerScore.KilledPlayer = targetName.Name;
                        PlayerScoreLookup[laser.DetectedTarget] = targetScore; 
                    }
                    
                    // Detect kill
                    if (math.abs(targetHealth.Value) < 0.01f)
                    {      
                        ownerScore.Kills += 1;
                        targetHealth.Value = 0;
                        targetLaser.IsShooting = false;
                    }

                    LaserLookup[laser.DetectedTarget] = targetLaser;
                    HealthLookup[laser.DetectedTarget] = targetHealth;
                    PlayerScoreLookup[entity] = ownerScore;
                }
            }
        }
    }
}
