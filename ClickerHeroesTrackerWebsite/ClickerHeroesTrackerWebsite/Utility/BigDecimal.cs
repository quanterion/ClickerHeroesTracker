﻿// <copyright file="BigDecimal.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Utility
{
    using System;
    using System.Numerics;

    /// <summary>
    /// Arbitrary precision decimal.
    /// </summary>
    /// <remarks>
    /// All operations are exact, except for division. Division never determines more digits than the given precision.
    /// Taken from http://stackoverflow.com/a/13813535
    /// </remarks>
    public struct BigDecimal : IComparable, IComparable<BigDecimal>
    {
        /// <summary>
        /// Specifies whether the significant digits should be truncated to the given precision after each operation.
        /// </summary>
        private const bool AlwaysTruncate = false;

        /// <summary>
        /// Sets the maximum precision of division operations.
        /// If AlwaysTruncate is set to true all operations are affected.
        /// </summary>
        private const int Precision = 50;

        /// <summary>
        /// The part of a number in floating-point number, consisting of its significant digits.
        /// </summary>
        private BigInteger significand;

        /// <summary>
        /// The part of a number in floating-point number, consisting of the exponent which determines the decimal point, eg 3 means 10^3.
        /// </summary>
        private int exponent;

        /// <summary>
        /// Initializes a new instance of the <see cref="BigDecimal"/> struct.
        /// </summary>
        public BigDecimal(BigInteger mantissa, int exponent)
            : this()
        {
            this.significand = mantissa;
            this.exponent = exponent;

            this.Normalize();
        }

        /// <summary>
        /// An implicit conversion from an <see cref="int"/> to a <see cref="BigDecimal"/>.
        /// </summary>
        public static implicit operator BigDecimal(int value)
        {
            return new BigDecimal(value, 0);
        }

        /// <summary>
        /// An implicit conversion from a <see cref="double"/> to a <see cref="BigDecimal"/>.
        /// </summary>
        public static implicit operator BigDecimal(double value)
        {
            var mantissa = (BigInteger)value;
            var exponent = 0;
            double scaleFactor = 1;
            while (Math.Abs((value * scaleFactor) - (double)mantissa) > 0)
            {
                exponent -= 1;
                scaleFactor *= 10;
                mantissa = (BigInteger)(value * scaleFactor);
            }

            return new BigDecimal(mantissa, exponent);
        }

        /// <summary>
        /// An implicit conversion from a <see cref="decimal"/> to a <see cref="BigDecimal"/>.
        /// </summary>
        public static implicit operator BigDecimal(decimal value)
        {
            var mantissa = (BigInteger)value;
            var exponent = 0;
            decimal scaleFactor = 1;
            while ((decimal)mantissa != value * scaleFactor)
            {
                exponent -= 1;
                scaleFactor *= 10;
                mantissa = (BigInteger)(value * scaleFactor);
            }

            return new BigDecimal(mantissa, exponent);
        }

        /// <summary>
        /// An explicit conversion from a <see cref="BigDecimal"/> to a <see cref="double"/>.
        /// </summary>
        public static explicit operator double(BigDecimal value)
        {
            return (double)value.significand * Math.Pow(10, value.exponent);
        }

        /// <summary>
        /// An explicit conversion from a <see cref="BigDecimal"/> to a <see cref="float"/>.
        /// </summary>
        public static explicit operator float(BigDecimal value)
        {
            return Convert.ToSingle((double)value);
        }

        /// <summary>
        /// An explicit conversion from a <see cref="BigDecimal"/> to a <see cref="decimal"/>.
        /// </summary>
        public static explicit operator decimal(BigDecimal value)
        {
            return (decimal)value.significand * (decimal)Math.Pow(10, value.exponent);
        }

        /// <summary>
        /// An explicit conversion from a <see cref="BigDecimal"/> to a <see cref="int"/>.
        /// </summary>
        public static explicit operator int(BigDecimal value)
        {
            return (int)(value.significand * BigInteger.Pow(10, value.exponent));
        }

        /// <summary>
        /// An explicit conversion from a <see cref="BigDecimal"/> to a <see cref="uint"/>.
        /// </summary>
        public static explicit operator uint(BigDecimal value)
        {
            return (uint)(value.significand * BigInteger.Pow(10, value.exponent));
        }

        /// <summary>
        /// The unary plus operator
        /// </summary>
        /// <returns>The same value</returns>
        public static BigDecimal operator +(BigDecimal value)
        {
            return value;
        }

        /// <summary>
        /// The unary minus operator
        /// </summary>
        /// <returns>The negation of the value</returns>
        public static BigDecimal operator -(BigDecimal value)
        {
            value.significand *= -1;
            return value;
        }

        /// <summary>
        /// The increment operator
        /// </summary>
        /// <returns>The value, incremented by 1</returns>
        public static BigDecimal operator ++(BigDecimal value)
        {
            return value + 1;
        }

        /// <summary>
        /// The decrement operator
        /// </summary>
        /// <returns>The value, decremented by 1</returns>
        public static BigDecimal operator --(BigDecimal value)
        {
            return value - 1;
        }

        /// <summary>
        /// The addition operation
        /// </summary>
        /// <returns>The sum of the two numbers</returns>
        public static BigDecimal operator +(BigDecimal left, BigDecimal right)
        {
            return Add(left, right);
        }

        /// <summary>
        /// The subtraction operation
        /// </summary>
        /// <returns>The difference of the two numbers</returns>
        public static BigDecimal operator -(BigDecimal left, BigDecimal right)
        {
            return Add(left, -right);
        }

        /// <summary>
        /// The multiplication operation
        /// </summary>
        /// <returns>The product of the two numbers</returns>
        public static BigDecimal operator *(BigDecimal left, BigDecimal right)
        {
            return new BigDecimal(left.significand * right.significand, left.exponent + right.exponent);
        }

        /// <summary>
        /// The division operation
        /// </summary>
        /// <returns>The division of the two numbers</returns>
        public static BigDecimal operator /(BigDecimal dividend, BigDecimal divisor)
        {
            var exponentChange = Precision - (NumberOfDigits(dividend.significand) - NumberOfDigits(divisor.significand));
            if (exponentChange < 0)
            {
                exponentChange = 0;
            }

            dividend.significand *= BigInteger.Pow(10, exponentChange);
            return new BigDecimal(dividend.significand / divisor.significand, dividend.exponent - divisor.exponent - exponentChange);
        }

        /// <summary>
        /// The equality operation
        /// </summary>
        /// <returns>True if the numbers are equal, false otherwise</returns>
        public static bool operator ==(BigDecimal left, BigDecimal right)
        {
            return left.exponent == right.exponent && left.significand == right.significand;
        }

        /// <summary>
        /// The inequality operation
        /// </summary>
        /// <returns>True if the numbers are not equal, false otherwise</returns>
        public static bool operator !=(BigDecimal left, BigDecimal right)
        {
            return left.exponent != right.exponent || left.significand != right.significand;
        }

        /// <summary>
        /// The less than operation
        /// </summary>
        /// <returns>True left is less than the right, false otherwise</returns>
        public static bool operator <(BigDecimal left, BigDecimal right)
        {
            return left.exponent > right.exponent ? AlignExponent(left, right) < right.significand : left.significand < AlignExponent(right, left);
        }

        /// <summary>
        /// The greater than operation
        /// </summary>
        /// <returns>True left is greater than the right, false otherwise</returns>
        public static bool operator >(BigDecimal left, BigDecimal right)
        {
            return left.exponent > right.exponent ? AlignExponent(left, right) > right.significand : left.significand > AlignExponent(right, left);
        }

        /// <summary>
        /// The less than or equal operation
        /// </summary>
        /// <returns>True left is less than or equal to the right, false otherwise</returns>
        public static bool operator <=(BigDecimal left, BigDecimal right)
        {
            return left.exponent > right.exponent ? AlignExponent(left, right) <= right.significand : left.significand <= AlignExponent(right, left);
        }

        /// <summary>
        /// The greater than or equal operation
        /// </summary>
        /// <returns>True left is greater than or equal to the right, false otherwise</returns>
        public static bool operator >=(BigDecimal left, BigDecimal right)
        {
            return left.exponent > right.exponent ? AlignExponent(left, right) >= right.significand : left.significand >= AlignExponent(right, left);
        }

        /// <summary>
        /// Returns e raised to the specified power.
        /// </summary>
        /// <returns>The number e raised to the power exponent</returns>
        public static BigDecimal Exp(double exponent)
        {
            var tmp = (BigDecimal)1;
            while (Math.Abs(exponent) > 100)
            {
                var diff = exponent > 0 ? 100 : -100;
                tmp *= Math.Exp(diff);
                exponent -= diff;
            }

            return tmp * Math.Exp(exponent);
        }

        /// <summary>
        /// Returns a specified number raised to the specified power.
        /// </summary>
        /// <returns>The basis raised to the power exponent.</returns>
        public static BigDecimal Pow(double basis, double exponent)
        {
            var tmp = (BigDecimal)1;
            while (Math.Abs(exponent) > 100)
            {
                var diff = exponent > 0 ? 100 : -100;
                tmp *= Math.Pow(basis, diff);
                exponent -= diff;
            }

            return tmp * Math.Pow(basis, exponent);
        }

        /// <summary>
        /// Compute the natural logarithm of x.
        /// </summary>
        /// <returns>The natural logarithm of x</returns>
        public static double Log(BigDecimal x)
        {
            if (x.significand.Sign < 0)
            {
                throw new InvalidOperationException("Value must be > 0");
            }

            // If it's less than int's max value, convert it to a double and take the log
            if (x < int.MaxValue)
            {
                return Math.Log((double)x);
            }
            else
            {
                // If larger than int.MaxValue, the part after the decimal point really
                // doesn't matter, so truncate (or pad) and use BigInteger's log.
                BigInteger number;
                if (x.exponent < 0)
                {
                    // Truncate the last N digits where N is the number of digits after the decimal point.
                    var significand = x.significand.ToString();
                    number = BigInteger.Parse(significand.Substring(0, significand.Length + x.exponent));
                }
                else
                {
                    // expand the significand with the exponent.
                    number = x.significand * BigInteger.Pow(10, x.exponent);
                }

                return BigInteger.Log(number);
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Concat(this.significand.ToString(), "E", this.exponent);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <returns>True if the specified object is equal to the current object, false otherwise</returns>
        public bool Equals(BigDecimal other)
        {
            return other.significand.Equals(this.significand) && other.exponent == this.exponent;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is BigDecimal && this.Equals((BigDecimal)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (this.significand.GetHashCode() * 397) ^ this.exponent;
            }
        }

        /// <inheritdoc />
        public int CompareTo(object obj)
        {
            if (ReferenceEquals(obj, null) || !(obj is BigDecimal))
            {
                throw new ArgumentException();
            }

            return this.CompareTo((BigDecimal)obj);
        }

        /// <inheritdoc />
        public int CompareTo(BigDecimal other)
        {
            return this < other ? -1 : (this > other ? 1 : 0);
        }

        private static int NumberOfDigits(BigInteger value)
        {
            // do not count the sign
            return (value * value.Sign).ToString().Length;
        }

        private static BigDecimal Add(BigDecimal left, BigDecimal right)
        {
            return left.exponent > right.exponent
                ? new BigDecimal(AlignExponent(left, right) + right.significand, right.exponent)
                : new BigDecimal(AlignExponent(right, left) + left.significand, left.exponent);
        }

        /// <summary>
        /// Returns the significand of value, aligned to the exponent of reference.
        /// Assumes the exponent of value is larger than of reference.
        /// </summary>
        /// <returns>The significand of value</returns>
        private static BigInteger AlignExponent(BigDecimal value, BigDecimal reference)
        {
            return value.significand * BigInteger.Pow(10, value.exponent - reference.exponent);
        }

        /// <summary>
        /// Removes trailing zeros on the significand
        /// </summary>
        private void Normalize()
        {
            if (this.significand.IsZero)
            {
                this.exponent = 0;
            }
            else
            {
                BigInteger remainder = 0;
                while (remainder == 0)
                {
                    var shortened = BigInteger.DivRem(this.significand, 10, out remainder);
                    if (remainder == 0)
                    {
                        this.significand = shortened;
                        this.exponent++;
                    }
                }
            }
        }

        /// <summary>
        /// Truncate the number to the given precision by removing the least significant digits.
        /// </summary>
        /// <returns>The truncated number</returns>
        private BigDecimal Truncate(int precision)
        {
            // copy this instance (remember its a struct)
            var shortened = this;

            // save some time because the number of digits is not needed to remove trailing zeros
            shortened.Normalize();

            // remove the least significant digits, as long as the number of digits is higher than the given Precision
            while (NumberOfDigits(shortened.significand) > precision)
            {
                shortened.significand /= 10;
                shortened.exponent++;
            }

            // normalize again to make sure there are no trailing zeros left
            shortened.Normalize();
            return shortened;
        }

        private BigDecimal Truncate()
        {
            return this.Truncate(Precision);
        }
    }
}