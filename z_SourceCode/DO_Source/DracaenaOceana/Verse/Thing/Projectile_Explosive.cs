using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RimWorld;

namespace Verse{
public class Projectile_Explosive : Projectile
{
	private int ticksToDetonation = 0;

	public override void ExposeData()
	{
		base.ExposeData();

		Scribe_Values.Look(ref ticksToDetonation, "ticksToDetonation");
	}


	public override void Tick()
	{
		base.Tick();
		
		if( ticksToDetonation > 0 )
		{
			ticksToDetonation--;
			
			if( ticksToDetonation <= 0 )
				Explode();
		}
	}
	
	protected override void Impact(Thing hitThing, bool blockedByShield = false)
	{
        if( blockedByShield || def.projectile.explosionDelay == 0 )
		{
			Explode();
			return;
		}
		else
		{
			landed = true;
			ticksToDetonation = def.projectile.explosionDelay;
			GenExplosion.NotifyNearbyPawnsOfDangerousExplosive(this, def.projectile.damageDef, launcher.Faction, launcher);
		}
	}
	
	protected virtual void Explode()
	{
		var map = Map; // before Destroy()!

		Destroy();

		if( def.projectile.explosionEffect != null )
		{
			var effect = def.projectile.explosionEffect.Spawn();
                
            if (def.projectile.explosionEffectLifetimeTicks != 0)
            {
                map.effecterMaintainer.AddEffecterToMaintain(effect, Position.ToVector3().ToIntVec3(), def.projectile.explosionEffectLifetimeTicks);
            }
            else
            {   
			    effect.Trigger(new TargetInfo(Position, map), new TargetInfo(Position, map));
			    effect.Cleanup();
            }
		}

		GenExplosion.DoExplosion(Position, map, def.projectile.explosionRadius, def.projectile.damageDef, launcher,
			damAmount: DamageAmount,
			armorPenetration: ArmorPenetration,
			explosionSound: def.projectile.soundExplode,
			weapon: equipmentDef,
			projectile: def,
			intendedTarget: intendedTarget.Thing,
			postExplosionSpawnThingDef: def.projectile.postExplosionSpawnThingDef ?? def.projectile.filth,
            postExplosionSpawnThingDefWater: def.projectile.postExplosionSpawnThingDefWater,
			postExplosionSpawnChance: def.projectile.postExplosionSpawnChance,
			postExplosionSpawnThingCount: def.projectile.postExplosionSpawnThingCount,
            postExplosionGasType: def.projectile.postExplosionGasType,
			preExplosionSpawnThingDef: def.projectile.preExplosionSpawnThingDef,
			preExplosionSpawnChance: def.projectile.preExplosionSpawnChance,
			preExplosionSpawnThingCount: def.projectile.preExplosionSpawnThingCount,
			applyDamageToExplosionCellsNeighbors: def.projectile.applyDamageToExplosionCellsNeighbors,
			chanceToStartFire: def.projectile.explosionChanceToStartFire,
			damageFalloff: def.projectile.explosionDamageFalloff,
			direction: origin.AngleToFlat(destination),
            propagationSpeed: def.projectile.damageDef.expolosionPropagationSpeed,
            screenShakeFactor: def.projectile.screenShakeFactor,
            doVisualEffects: def.projectile.doExplosionVFX);
	}
}
}