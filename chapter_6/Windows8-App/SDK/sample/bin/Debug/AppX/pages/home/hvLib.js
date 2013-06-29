// (c) Microsoft. All rights reserved
//---------------
//
// Random Number Extensions
//
//---------------
Math.randomIntInRange = function (min, max) {    
    var randNum = Math.random();
    var offset = Math.floor(randNum * (max - min + 1)); // inclusive
    return min + offset;
}

Math.randomInt = function() {
    return Math.randomIntInRange(int.min, int.max);
}

Math.randomInRange = function (minInt, maxInt) {
    return Math.randomIntInRange(minInt, maxInt) + Math.random();
}

Math.roundToPrecision = function (number, numDecimalPlaces) {
    var multiple = Math.pow(10, numDecimalPlaces);
    return Math.round(number * multiple) / multiple;
}

Array.randomItemFrom = function() {
    if (arguments.length <= 1) {
        return;
    }

    var index = Math.randomIntInRange(0, arguments.length - 1);
    return arguments[index];
}

Date.random = function(maxDaysOffset) {
    
    if (arguments.length == 0) {
        maxDaysOffset = 365;
    }
    var dt = new Date();
    dt.setDate(dt.getDate() + Math.randomIntInRange(0, -maxDaysOffset));

    return dt;
}