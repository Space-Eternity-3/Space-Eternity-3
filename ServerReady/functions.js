class cfun
{
    constructor() {}

    // PARSING FUNCTIONS
    parseFloatE(str) { //Parsing generating error on failied
        str = this.parseFloatP(str+"");
        if (!isNaN(str)) return str;
        else return nev;
    }
    parseIntE(str) {
        str = this.parseIntP(str);
        if (!isNaN(str)) return str;
        else return nev;
    }
    parseIntP(str) { //Parsing returning isNaN on failied
        return parseInt(str);
    }
    parseFloatP(str) {
        return parseFloat(str.replaceAll(",", "."));
    }
    parseIntU(str) { //Parsing returning 0 on failied
        str = this.parseIntP(str);
        if (!isNaN(str)) return str;
        else return 0;
    }
    parseFloatU(str) {
        str = this.parseFloatP(str);
        if (!isNaN(str)) return str;
        else return 0;
    }
}

module.exports = {
    cfun
};