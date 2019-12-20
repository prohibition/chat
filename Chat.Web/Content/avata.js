String.prototype.hashCode = function () {
    var hash = 0,
                i, chr, len;
    if (this.length == 0) return hash;
    for (i = 0, len = this.length; i < len; i++) {
        chr = this.charCodeAt(i);
        hash = ((hash << 5) - hash) + chr;
        hash |= 0; // Convert to 32bit integer
    }
    return hash;
};

String.prototype.getInitials = function (glue) {
    if (typeof glue == "undefined") {
        var glue = true;
    }

    var initials = this.replace(/[^a-zA-Z- ]/g, "").match(/\b\w/g);

    if (glue) {
        return initials.join('').substring(0, 2);
    }

    return initials.substring(0, 2);
};


function getColorHash(name){
    var numColors = 3;
    var initials = name.getInitials();
    var hash = Math.abs(name.hashCode()) % numColors;
    return 'color-' + hash;
}