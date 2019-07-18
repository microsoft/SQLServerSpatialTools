//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using System;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using MST = Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;

namespace SQLSpatialTools.UnitTests.Extension
{
    public static class SqlAssert
    {
        public static void IsTrue(SqlBoolean sqlBoolean)
        {
            MST.Assert.IsTrue((bool)sqlBoolean);
        }

        public static void IsFalse(SqlBoolean sqlBoolean)
        {
            MST.Assert.IsFalse((bool)sqlBoolean);
        }

        public static void AreEqual(SqlDouble sqlDouble, double targetValue)
        {
            MST.Assert.AreEqual(Math.Round((double)sqlDouble, 4), Math.Round(targetValue, 4));
        }

        public static string GetResult(this bool result)
        {
            return result ? "Passed" : "Failed";
        }
    }

    public static class TestExtension
    {
        private const string DecimalPointMatch = @"\.0([\s\,\)])";

        /// <summary>
        /// Trim null values in the input geometry WKT.
        /// </summary>
        /// <param name="inputGeom">input geometry in WKT</param>
        /// <returns>Null trimmed geom text</returns>
        public static string TrimNullValue(this string inputGeom)
        {
            return Regex.Replace(inputGeom, @"\s*null\s*", " ", RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Trims the decimal precision.
        /// </summary>
        /// <param name="inputGeom">The input geom.</param>
        /// <returns></returns>
        private static string RoundOffDecimalPrecision(this string inputGeom)
        {
            var output = inputGeom;
            //var matches = Regex.Matches(inputGeom, @"(\d+\.\d{5,16})", RegexOptions.Compiled);
            var matches = Regex.Matches(inputGeom, @"(\d+\.\d+)", RegexOptions.Compiled);

            foreach (Match match in matches)
            {
                var inputStr = match.Groups[1].Value;
                if (double.TryParse(inputStr, out var trimValue))
                {
                    output = output.Replace(inputStr, Math.Round(trimValue, 4).ToString(CultureInfo.InvariantCulture));
                }
            }

            return output;
        }

        /// <summary>
        /// Compare the two results after converting to lower and trimming space.
        /// </summary>
        /// <param name="firstResult"></param>
        /// <param name="secondResult"></param>
        /// <returns></returns>
        public static bool Compare(this string firstResult, string secondResult)
        {
            // check for null words and assign null
            if (!string.IsNullOrEmpty(firstResult))
                firstResult = firstResult.ToLower(CultureInfo.CurrentCulture).Trim().Equals("null", StringComparison.CurrentCulture) ? null : firstResult.Trim();

            if (!string.IsNullOrEmpty(secondResult))
                secondResult = secondResult.ToLower(CultureInfo.CurrentCulture).Trim().Equals("null", StringComparison.CurrentCulture) ? null : secondResult.Trim();

            if (string.IsNullOrEmpty(firstResult) && string.IsNullOrEmpty(secondResult))
                return true;
            if (string.IsNullOrEmpty(firstResult) || string.IsNullOrEmpty(secondResult))
                return false;

            firstResult = firstResult.RoundOffDecimalPrecision();
            secondResult = secondResult.RoundOffDecimalPrecision();
            firstResult = Regex.Replace(firstResult, @"\s+", string.Empty).ToLower(CultureInfo.CurrentCulture);
            secondResult = Regex.Replace(secondResult, @"\s+", string.Empty).ToLower(CultureInfo.CurrentCulture);
            return firstResult.Equals(secondResult, StringComparison.CurrentCulture);
        }

        /// <summary>
        /// Trims the decimal points in input WKT geometry.
        /// </summary>
        /// <param name="inputGeomWKT"></param>
        /// <returns></returns>
        public static string TrimDecimalPoints(this string inputGeomWKT)
        {
            if (!string.IsNullOrEmpty(inputGeomWKT))
                return Regex.Replace(inputGeomWKT, DecimalPointMatch, "$1");
            return inputGeomWKT;
        }

        /// <summary>
        /// Escape single quotation in input query.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static string EscapeQueryString(this string query)
        {
            return query?.Replace("'", "''");
        }
    }

    public class TestLogger
    {
        private readonly MST.TestContext _testContext;

        public TestLogger(MST.TestContext testContext)
        {
            _testContext = testContext;
        }

        public void Log(string msgFormat, params object[] args)
        {
            _testContext.WriteLine(string.Format(CultureInfo.CurrentCulture, msgFormat, args));
        }

        public void LogLine(string msgFormat, params object[] args)
        {
            var message = new StringBuilder();
            message.AppendLine();
            if (args != null && args.Length > 0)
                message.AppendFormat(CultureInfo.CurrentCulture, msgFormat, args);
            else
                message.Append(msgFormat);

            _testContext.WriteLine(message.ToString());
        }

        public void LogError(Exception ex, string errorMessage = "", params object[] args)
        {
            var message = new StringBuilder();
            var trace = new StackTrace(ex, true);
            var frame = trace.GetFrame(0);
            message.AppendLine();
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                if (args != null && args.Length > 0)
                    message.AppendFormat(CultureInfo.CurrentCulture, errorMessage, args);
                else
                    message.Append(errorMessage);
            }

            message.AppendLine();
            if (frame != null)
            {
                message.AppendFormat(CultureInfo.CurrentCulture, "Error module: {0}", frame.GetMethod().Name);
                message.AppendLine();
                message.AppendFormat(CultureInfo.CurrentCulture, "File Name: {0}", frame.GetFileName());
                message.AppendLine();
                message.AppendFormat(CultureInfo.CurrentCulture, "Line Number: {0}", frame.GetFileLineNumber());
                message.AppendLine();
            }
            message.AppendFormat(CultureInfo.CurrentCulture, "Exception: {0}", ex.Message);
            message.AppendLine();
            if (ex.StackTrace != null)
            {
                message.AppendFormat(CultureInfo.CurrentCulture, "Stack trace: {0}", ex.StackTrace);
                message.AppendLine();
            }

            if (ex.InnerException != null)
            {
                message.AppendFormat(CultureInfo.CurrentCulture, "Inner Exception: {0}", ex.InnerException.Message);
                message.AppendLine();
                if (ex.InnerException.StackTrace != null)
                    message.AppendFormat(CultureInfo.CurrentCulture, "Inner Stack trace: {0}", ex.InnerException.StackTrace);
            }

            _testContext.WriteLine(message.ToString());
        }
    }
}
