using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_gameplay_set : MonoBehaviour
{
    public SC_data SC_data;
    public SC_fun SC_fun;
    public SC_bullet SC_bullet;
    public SC_adecodron Adecodron;
    public SC_adecodron Octogone;
    public SC_wind stone_geyzer;
    public SC_wind magnetic_geyzer;
    public SC_wind lava_geyzer;
    public SC_fobs small_amethyst;
    public SC_fobs medium_amethyst;
    public SC_fobs magnetic_alien;
    public SC_fobs treasure;
    public SC_fobs dark_treasure;
    public SC_fobs metal_treasure;
    public SC_fobs soft_treasure;
    public SC_fobs hard_treasure;
    public SC_control SC_control;

    public void GameplayAwakeSet()
    {
        SC_fun.boss_damages[4] = SC_data.GplGet("boss_bullet_electron_damage");
        SC_fun.boss_damages[5] = SC_data.GplGet("boss_bullet_fire_damage");
        SC_fun.boss_damages[6] = SC_data.GplGet("boss_bullet_spike_damage");
        SC_fun.boss_damages[7] = SC_data.GplGet("boss_bullet_brainwave_damage");
        SC_fun.boss_damages[9] = SC_data.GplGet("boss_bullet_rocket_damage");
        SC_fun.boss_damages[10] = SC_data.GplGet("boss_bullet_spikeball_damage");
        SC_fun.boss_damages[11] = SC_data.GplGet("boss_bullet_copper_damage");
        SC_fun.boss_damages[12] = SC_data.GplGet("boss_bullet_red_damage");
        SC_fun.boss_damages[13] = SC_data.GplGet("boss_bullet_unstable_damage");
        SC_fun.boss_damages[16] = SC_data.GplGet("boss_bullet_graviton_damage");
        SC_fun.boss_damages[17] = SC_data.GplGet("boss_bullet_neutronium_damage");

        Adecodron.enter_damage = SC_data.GplGet("boss_adecodron_sphere_damage");
        Octogone.enter_damage = SC_data.GplGet("boss_octogone_sphere_damage");

        SC_fun.bullet_effector[5] = (int)SC_data.GplGet("cyclic_fire_time");
        SC_fun.bullet_effector[15] = (int)SC_data.GplGet("cyclic_fire_time");
        SC_fun.bullet_effector[6] = (int)SC_data.GplGet("cyclic_spike_time");
        SC_fun.bullet_effector[10] = (int)SC_data.GplGet("cyclic_spikeball_time");
        SC_fun.bullet_effector[8] = (int)SC_data.GplGet("cyclic_stickybulb_time");

        SC_fun.boss_damages_cyclic[5] = SC_data.GplGet("cyclic_fire_damage");
        SC_fun.boss_damages_cyclic[6] = SC_data.GplGet("cyclic_poison_damage");

        SC_bullet.BulletSpeeds[1] = SC_data.GplGet("copper_bullet_speed");
        SC_bullet.BulletSpeeds[2] = SC_data.GplGet("red_bullet_speed");
        SC_bullet.BulletSpeeds[3] = SC_data.GplGet("unstable_bullet_speed");
        SC_bullet.BulletSpeeds[14] = SC_data.GplGet("wind_bullet_speed");
        SC_bullet.BulletSpeeds[15] = SC_data.GplGet("fire_bullet_speed");

        stone_geyzer.force = 50f * SC_data.GplGet("stone_geyzer_force_multiplier");
        magnetic_geyzer.force = 50f * SC_data.GplGet("magnetic_geyzer_force_multiplier");
        lava_geyzer.force = 50f * SC_data.GplGet("lava_geyzer_force_multiplier");

        magnetic_alien.GrowTimeMin = 50 * (int)SC_data.GplGet("magnetic_alien_grow_time");
        if(magnetic_alien.GrowTimeMin<=0) magnetic_alien.GrowTimeMin = 50;
        magnetic_alien.GrowTimeMax = magnetic_alien.GrowTimeMin;
        small_amethyst.GrowTimeMin = 50 * (int)SC_data.GplGet("amethyst_grow_time_min");
        small_amethyst.GrowTimeMax = 50 * (int)SC_data.GplGet("amethyst_grow_time_max");
        if(small_amethyst.GrowTimeMin<=0) small_amethyst.GrowTimeMin = 50;
        if(small_amethyst.GrowTimeMax<=0) small_amethyst.GrowTimeMax = 50;
        if(small_amethyst.GrowTimeMax < small_amethyst.GrowTimeMin)
        {
            int pom = small_amethyst.GrowTimeMax;
            small_amethyst.GrowTimeMax = small_amethyst.GrowTimeMin;
            small_amethyst.GrowTimeMin = pom;
        }
        medium_amethyst.GrowTimeMin = small_amethyst.GrowTimeMin;
        medium_amethyst.GrowTimeMax = small_amethyst.GrowTimeMax;

        treasure.lootSE3 = SC_data.Gameplay[105];
        dark_treasure.lootSE3 = SC_data.Gameplay[106];
        metal_treasure.lootSE3 = SC_data.Gameplay[125];
        soft_treasure.lootSE3 = SC_data.Gameplay[126];
        hard_treasure.lootSE3 = SC_data.Gameplay[127];

        SC_control.at_unstable_regen1 = -Parsing.FloatE(SC_data.Gameplay[108]);
        SC_control.at_unstable_regen2 = -Parsing.FloatE(SC_data.Gameplay[109]);
        SC_control.at_unstable_regen3 = Parsing.FloatE(SC_data.Gameplay[110]);
        SC_control.unstabling_max_deviation = Parsing.FloatE(SC_data.Gameplay[111]);
    }
}
