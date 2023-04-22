//String functions
String.prototype.replaceAll = function replaceAll(search, replace) {
    return this.split(search).join(replace);
  };

//Classes
class cfun
{
    constructor() {
        this.lps = new CLinearPreset();
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
    func
};