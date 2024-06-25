//String functions
String.prototype.replaceAll = function replaceAll(search, replace) {
    return this.split(search).join(replace);
};

class CParsing
{
    /*
        C - checker
        U - 0 maker
        E - thrower
    */

    //Validation methods
    IntC(s)
    {
        try{ this.IntE(s); }
        catch { return false; }
        return true;
    }
    FloatC(s)
    {
        try{ this.FloatE(s); }
        catch { return false; }
        return true;
    }

    //Safe 0 methods
    IntU(s)
    {
        let ret = 0;
        try{ ret = this.IntE(s); } catch {}
        return ret;
    }
    FloatU(s)
    {
        let ret = 0;
        try{ ret = this.FloatE(s); } catch {}
        return ret;
    }

    //Core error methods
    IntE(s)
    {
        s=s+"";
        try{
            if(s=="") throw "error";
            if(s.length > 256) throw "error";
            let i=0, lngt = s.length, dl = ('0').charCodeAt(0), cn;
            let sum = 0;
            let minus = s[0]=='-';
            if(minus) i++;
            for(;i<lngt;i++)
            {
                cn = s[i].charCodeAt(0) - dl;
                if(cn<0 || cn>9) throw "error";
                sum *= 10; sum -= cn;
            }
            if(!minus) sum *= -1;
            if(sum<-2147483648 || sum>2147483647) throw "error";
            return sum;
        }
        catch {
            throw "Could not parse string to int: "+s;
        }
    }
    FloatE(s)
    {
        s=s+"";
        try{
            if(s=="") throw "error";
            if(s.length > 256) throw "error";
            let i=0, lngt = s.length, dl = ('0').charCodeAt(0), cn;
            let sum = 0, mn = 1;
            let minus = s[0]=='-';
            if(minus) i++;

            let mode = 0; // 0 - before sep, 1 - after sep, 2 - exponent
            let exp_str = "";
            for(;i<lngt;i++)
            {
                if(mode<=1)
                {
                    if(mode==0 && (s[i]=='.' || s[i]==','))
                    {
                        mode = 1;
                        continue;
                    }
                    if(s[i]=='e' || s[i]=='E')
                    {
                        mode = 2;
                        continue;
                    }

                    cn = (s[i]).charCodeAt(0) - dl;
                    if(cn<0 || cn>9) throw "error";
                    if(mode==0) sum *= 10; else mn /= 10;
                    sum += mn*cn;
                }
                else exp_str += s[i];
            }

            if(mode==2)
            {
                let exp = this.IntE(exp_str.substring(1));
                if(exp_str[0]=='-') exp *= -1;
                else if(exp_str[0]!='+') throw "error";
                sum *= Math.pow(10,exp);
            }

            if(sum > Math.pow(10,32)) sum = Math.pow(10,32);
            if(sum < Math.pow(10,-32)) return 0;

            if(minus) sum *= -1;
            return sum;
        }
        catch {
            throw "Could not parse string to float: "+s;
        }
    }
}
let Parsing = new CParsing();

//Classes
class cfun
{
    constructor() {
        this.lps = new CLinearPreset();
        this.gpl_info = [
            
            //[0] - variable name
            //[1] - variable filter (*)all or (+)positive

            "turbo_regenerate_multiplier:+", //0
            "turbo_use_multiplier:+", //1
            "drill_level_add:*", //2
            "copper_bullet_damage:+", //3
            "health_regenerate_cooldown:+", //4
            "health_regenerate_multiplier:+", //5
            "crash_minimum_energy:+", //6
            "crash_damage_multiplier:+", //7
            "spike_damage:+", //8
            "player_normal_speed:*", //9
            "player_brake_speed:*", //10
            "player_turbo_speed:*", //11
            "drill_normal_speed:*", //12
            "drill_brake_speed:*", //13
            "vacuum_drag_multiplier:*", //14
            "all_speed_multiplier:*", //15
            "at_protection_health_level_add:*", //16
            "at_protection_health_regenerate_multiplier:+", //17
            "at_impulse_power_regenerate_multiplier:+", //18
            "at_impulse_time:+", //19
            "at_impulse_speed:*", //20
            "at_illusion_power_regenerate_multiplier:+", //21
            "at_illusion_power_use_multiplier:+", //22
            "at_unstable_normal_avarage_time:+", //23
            "at_unstable_special_avarage_time:+", //24
            "at_unstable_force:*", //25
            "health_level_add:*", //26
            "red_bullet_damage:+", //27
            "unstable_matter_damage:+", //28
            "at_impulse_damage:+", //29
            "bullet_owner_push:*", //30
            "healing_potion_hp:+", //31
            "boss_damage_multiplier:+", //32
            "wind_bullet_damage:+", //33
            "fire_bullet_damage:+", //34
            "killing_potion_hp:+", //35
            "cyclic_damage_multiplier:+", //36
            "boss_fire_cycles:+", //37
            "boss_fire_damage:+", //38
            "blank_potion_hp:+", //39
            "stone_geyzer_force_multiplier:*", //40
            "magnetic_geyzer_force_multiplier:*", //41
            "amethyst_grow_time_min:+", //42
            "amethyst_grow_time_max:+", //43
            "magnetic_alien_grow_time:+", //44
            "copper_bullet_speed:*", //45
            "red_bullet_speed:*", //46
            "unstable_bullet_speed:*", //47
            "fire_bullet_speed:*", //48
            "wind_bullet_speed:*", //49
            "boss_bullet_speed:+", //50
            "boss_seeker_speed:+", //51
            "cyclic_fire_damage:+", //52
            "cyclic_poison_damage:+", //53
            "cyclic_remote_damage:+", //54
            "cyclic_fire_time:+", //55
            "cyclic_spike_time:+", //56
            "cyclic_spikeball_time:+", //57
            "cyclic_stickybulb_time:+", //58
            "star_collider_damage:+", //59
            "boss_starandus_geyzer_damage:+", //60
            "boss_adecodron_sphere_damage:+", //61
            "boss_octogone_sphere_damage:+", //62
            "boss_bullet_electron_damage:+", //63
            "boss_bullet_fire_damage:+", //64
            "boss_bullet_spike_damage:+", //65
            "boss_bullet_brainwave_damage:+", //66
            "boss_bullet_rocket_damage:+", //67
            "boss_bullet_spikeball_damage:+", //68
            "boss_bullet_copper_damage:+", //69
            "boss_bullet_red_damage:+", //70
            "boss_bullet_unstable_damage:+", //71
            "boss_bullet_graviton_damage:+", //72
            "boss_bullet_neutronium_damage:+", //73
            "boss_battle_time:+", //74
            "boss_hp_protector_1:+", //75
            "boss_hp_protector_2:+", //76
            "boss_hp_protector_3:+", //77
            "boss_hp_adecodron_1:+", //78
            "boss_hp_adecodron_2:+", //79
            "boss_hp_adecodron_3:+", //80
            "boss_hp_octogone_1:+", //81
            "boss_hp_octogone_2:+", //82
            "boss_hp_octogone_3:+", //83
            "boss_hp_starandus_1:+", //84
            "boss_hp_starandus_2:+", //85
            "boss_hp_starandus_3:+", //86
            "boss_hp_degenerator_1:+", //87
            "boss_hp_degenerator_2:+", //88
            "boss_hp_degenerator_3:+", //89
            "cyclic_star_time:+", //90
            "cyclic_starandus_geyzer_time:+", //91
            "copper_bullet_defrange:+", //92
            "red_bullet_defrange:+", //93
            "wind_bullet_defrange:+", //94
            "fire_bullet_defrange:+", //95
            "unstable_bullet_defrange:+", //96
            "copper_bullet_cooldown:+", //97
            "red_bullet_cooldown:+", //98
            "wind_bullet_cooldown:+", //99
            "fire_bullet_cooldown:+", //100
            "unstable_bullet_cooldown:+", //101
            "impulse_cooldown:+", //102
            "lava_geyzer_force_multiplier:*", //103
            "lava_geyzer_damage:+", //104
            "treasure_loot:s", //105
            "dark_treasure_loot:s", //106
            "at_unstable_power_regenerate_multiplier:+", //107
	        "at_unstable_power_normal_eat:+", //108
	        "at_unstable_power_special_eat:+", //109
	        "at_unstable_power_killpot_give:+", //110
	        "at_unstable_max_unstabling_deviation:+", //111
            "upg_1_item:+", //112
            "upg_1_cost:+", //113
            "upg_2_item:+", //114
            "upg_2_cost:+", //115
            "upg_3_item:+", //116
            "upg_3_cost:+", //117
            "upg_4_item:+", //118
            "upg_4_cost:+", //119
            "upg_5_item:+", //120
            "upg_5_cost:+", //121
            "wind_owner_push:*", //122
	        "wind_boss_push:*", //123
            "wind_victim_push:*", //124
            "metal_treasure_loot:s", //125
            "soft_treasure_loot:s", //126
            "hard_treasure_loot:s", //127
            "at_illusion_speed_multiplier:*", //128
            "shield_potion_duration:+", //129
        ];
    }

    parseFloatE(str)
    { //Parsing generating error on failied
        str = this.parseFloatP(str+"");
        if (!isNaN(str)) return str;
        else return nev;
    }
    parseIntE(str)
    {
        str = this.parseIntP(str);
        if (!isNaN(str)) return str;
        else return nev;
    }
    parseIntP(str)
    { //Parsing returning isNaN on failied
        return parseInt(str);
    }
    parseFloatP(str)
    {
        if(typeof str === 'string' || str instanceof String) return parseFloat(str.replaceAll(",", "."));
        else return parseFloat(str);
    }
    parseIntU(str)
    { //Parsing returning 0 on failied
        str = this.parseIntP(str);
        if (!isNaN(str)) return str;
        else return 0;
    }
    parseFloatU(str)
    {
        str = this.parseFloatP(str);
        if (!isNaN(str)) return str;
        else return 0;
    }

    FloatToScrd(src) {
        return Math.floor(src*124);
    }
    ScrdToFloat(src) {
        return src/124;
    }

    randomInteger(min, max) {
        min = Math.ceil(min);
        max = Math.floor(max);
        return Math.floor(Math.random() * (max - min + 1)) + min;
    }

    AngleBetweenVectorAndOX(dx, dy)
    {
        var angle = Math.atan2(dy, dx) * (180 / Math.PI);
        if (angle < 0) angle += 360;
        return angle;
    }

    RotatePoint(crd,rot,convert_to_radians=true)
    {
        var x = crd[0];
        var y = crd[1];
        var alpha;
        if(convert_to_radians) alpha = rot * Math.PI / 180;
        else alpha = rot;
        return [
            ( x*Math.cos(alpha) + y*Math.cos(alpha + Math.PI / 2) ),
            ( x*Math.sin(alpha) + y*Math.sin(alpha + Math.PI / 2) )
        ];
    }

    GetBounceCoords(x1,y1,x2,y2,bounce_radius)
    {
        if(x1==0 && x2==0 && y1==0 && y2==0) return [0,0,0,0];

        var xa,ya,xb,yb,xc=0,yc=0,cnt=0;
        var x = x2-x1;
        var y = y2-y1;
        if(x==0) {
            xa = x1;
            ya = Math.sqrt(bounce_radius**2 - xa**2);
            xb = x1;
            yb = -Math.sqrt(bounce_radius**2 - xb**2);

            if(Math.sign(y2-ya)==Math.sign(ya-y1)) {xc=xa; yc=ya; cnt++;}
            if(Math.sign(y2-yb)==Math.sign(yb-y1)) {xc=xb; yc=yb; cnt++;}
        }
        else {
            var a = y/x;
            var b = y1 - a*x1;
            xa = (-a*b + Math.sqrt((a**2)*(b**2)-(b**2-bounce_radius**2)*(a**2+1)))/(a**2+1);
            ya = a*xa+b;
            xb = (-a*b - Math.sqrt((a**2)*(b**2)-(b**2-bounce_radius**2)*(a**2+1)))/(a**2+1);
            yb = a*xb+b;

            if(Math.sign(x2-xa)==Math.sign(xa-x1)) {xc=xa; yc=ya; cnt++;}
            if(Math.sign(x2-xb)==Math.sign(xb-x1)) {xc=xb; yc=yb; cnt++;}
        }

        if(cnt==0 || x2**2+y2**2<=bounce_radius**2) return [x2,y2,0,0];

        var alpha = Math.atan2(y1-yc,x1-xc);
        var beta = Math.atan2(-yc,-xc);
        var gamma = 2*beta - alpha;
        var c2_d = Math.sqrt((x2-xc)**2+(y2-yc)**2);
        var get = this.RotatePoint([c2_d,0],gamma,false);
        return [get[0]+xc,get[1]+yc,gamma,1];
    }

    CollisionLinearBulletSet(xa,ya,xb,yb,r1)
    {
        if(xa==xb) xb+=0.0001;
        if(ya==yb) yb+=0.0001;

        var a1 = (yb-ya)/(xb-xa);
        var a2 = -(1/a1);
        var b1 = ya - xa*a1;

        this.lps.xa = xa;
        this.lps.xb = xb;
        this.lps.ya = ya;
        this.lps.yb = yb;
        this.lps.r1 = r1;
        this.lps.a1 = a1;
        this.lps.a2 = a2;
        this.lps.b1 = b1;
    }
    CollisionLinearCheck(xt,yt,r2,capsule=true)
    {
        var mem = this.lps;
        var xa = mem.xa;
        var xb = mem.xb;
        var ya = mem.ya;
        var yb = mem.yb;
        var r1 = mem.r1;
        var a1 = mem.a1;
        var a2 = mem.a2;
        var b1 = mem.b1;

        var r = r1 + r2;
        var b2 = yt - xt*a2;
        
        var xc = (b2-b1)/(a1-a2);
        var yc = a1*xc + b1;

        if(capsule) if(this.CollisionPointCheck(xa,ya,xt,yt,r1,r2) || this.CollisionPointCheck(xb,yb,xt,yt,r1,r2)) return true; //border point collision detected
        if((xc>xa&&xc>xb) || (xc<xa&&xc<xb)) return false; //not in line area
        if(this.CollisionPointCheck(xc,yc,xt,yt,r1,r2)) return true; //linear collision detected
        return false; //linear collision not detected
    }
    CollisionPointCheck(xa,ya,xt,yt,r1,r2)
    {
        var r = r1 + r2;
        return ((xt-xa)**2 + (yt-ya)**2 <= r**2);
    }

    //Gameplay variables functions
    VarNumber(str,gnome)
    {
        var i;
        for(i=0;i<gnome;i++)
            if(this.gpl_info[i].split(":")[0]==str) return i;
        return -1;
    }
    FilterValue(n,value)
    {
        var spl = this.gpl_info[n].split(":");
        if(spl[1]!="s") {
            var parsed = this.parseFloatE(value);
            if(parsed<0 && spl[1]=="+") parsed*=-1;
            return parsed+"";
        }
        else {
            var ret="", str=value+"";
            var i,lngt=str.length;
            for(i=0;i<lngt;i++)
                if(["0","1","2","3","4","5","6","7","8","9","-","+"].includes(str[i])) ret+=str[i];
            return ret;
        }
    }
}
class CLinearPreset
{
    constructor() {
        this.xa = 0;
        this.xb = 0;
        this.ya = 0;
        this.yb = 0;
        this.r1 = 0;
        this.a1 = 0;
        this.a2 = 0;
        this.b1 = 0;
    }
}
let func = new cfun();

module.exports = {
    func, Parsing
};