﻿// tslint:disable-next-line:interface-name - We don't own this interface name, just extending it */
interface Math
{
    log10(x: number): number;
}

namespace MathExtensions
{
    "use strict";

    Math.log10 = Math.log10 || function (x: number): number
    {
        return Math.log(x) / Math.LN10;
    };
}
