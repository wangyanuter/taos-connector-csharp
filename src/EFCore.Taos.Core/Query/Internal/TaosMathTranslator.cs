// Copyright (c)  Maikebing. All rights reserved.
// Licensed under the MIT License, See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace IoTSharp.EntityFrameworkCore.Taos.Query.Internal
{
    public class TaosMathTranslator : IMethodCallTranslator
    {
        private static readonly Dictionary<MethodInfo, string> _supportedMethods = new Dictionary<MethodInfo, string>
        {
            { typeof(Math).GetMethod(nameof(Math.Abs), new[] { typeof(double) }), "abs" },
            { typeof(Math).GetMethod(nameof(Math.Abs), new[] { typeof(float) }), "abs" },
            { typeof(Math).GetMethod(nameof(Math.Abs), new[] { typeof(int) }), "abs" },
            { typeof(Math).GetMethod(nameof(Math.Abs), new[] { typeof(long) }), "abs" },
            { typeof(Math).GetMethod(nameof(Math.Abs), new[] { typeof(sbyte) }), "abs" },
            { typeof(Math).GetMethod(nameof(Math.Abs), new[] { typeof(short) }), "abs" },
            { typeof(Math).GetMethod(nameof(Math.Max), new[] { typeof(byte), typeof(byte) }), "max" },
            { typeof(Math).GetMethod(nameof(Math.Max), new[] { typeof(double), typeof(double) }), "max" },
            { typeof(Math).GetMethod(nameof(Math.Max), new[] { typeof(float), typeof(float) }), "max" },
            { typeof(Math).GetMethod(nameof(Math.Max), new[] { typeof(int), typeof(int) }), "max" },
            { typeof(Math).GetMethod(nameof(Math.Max), new[] { typeof(long), typeof(long) }), "max" },
            { typeof(Math).GetMethod(nameof(Math.Max), new[] { typeof(sbyte), typeof(sbyte) }), "max" },
            { typeof(Math).GetMethod(nameof(Math.Max), new[] { typeof(short), typeof(short) }), "max" },
            { typeof(Math).GetMethod(nameof(Math.Max), new[] { typeof(uint), typeof(uint) }), "max" },
            { typeof(Math).GetMethod(nameof(Math.Max), new[] { typeof(ushort), typeof(ushort) }), "max" },
            { typeof(Math).GetMethod(nameof(Math.Min), new[] { typeof(byte), typeof(byte) }), "min" },
            { typeof(Math).GetMethod(nameof(Math.Min), new[] { typeof(double), typeof(double) }), "min" },
            { typeof(Math).GetMethod(nameof(Math.Min), new[] { typeof(float), typeof(float) }), "min" },
            { typeof(Math).GetMethod(nameof(Math.Min), new[] { typeof(int), typeof(int) }), "min" },
            { typeof(Math).GetMethod(nameof(Math.Min), new[] { typeof(long), typeof(long) }), "min" },
            { typeof(Math).GetMethod(nameof(Math.Min), new[] { typeof(sbyte), typeof(sbyte) }), "min" },
            { typeof(Math).GetMethod(nameof(Math.Min), new[] { typeof(short), typeof(short) }), "min" },
            { typeof(Math).GetMethod(nameof(Math.Min), new[] { typeof(uint), typeof(uint) }), "min" },
            { typeof(Math).GetMethod(nameof(Math.Min), new[] { typeof(ushort), typeof(ushort) }), "min" },
            { typeof(Math).GetMethod(nameof(Math.Round), new[] { typeof(double) }), "round" },
            { typeof(Math).GetMethod(nameof(Math.Round), new[] { typeof(double), typeof(int) }), "round" }
        };

        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public TaosMathTranslator(ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public virtual SqlExpression Translate(SqlExpression instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments, IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            if (_supportedMethods.TryGetValue(method, out var sqlFunctionName))
            {
                RelationalTypeMapping typeMapping;
                List<SqlExpression> newArguments = null;
                if (string.Equals(sqlFunctionName, "max")
                    || string.Equals(sqlFunctionName, "max"))
                {
                    typeMapping = ExpressionExtensions.InferTypeMapping(arguments[0], arguments[1]);
                    newArguments = new List<SqlExpression>
                    {
                        _sqlExpressionFactory.ApplyTypeMapping(arguments[0], typeMapping),
                        _sqlExpressionFactory.ApplyTypeMapping(arguments[1], typeMapping)
                    };
                }
                else
                {
                    typeMapping = arguments[0].TypeMapping;
                }

                return _sqlExpressionFactory.Function(
                    sqlFunctionName,
                    newArguments ?? arguments, true, null,
                    method.ReturnType,
                    typeMapping);
            }

            return null;
        }

     
    }
}
