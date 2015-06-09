// ***********************************************************************
// Assembly         : ReflectSoftware.Insight.Extensions.PostSharp
// Author           : ReflectSoftware Inc.
// Created          : 03-19-2014
// Last Modified On : 03-19-2014
// ***********************************************************************
// <copyright file="TraceMethodHelper.cs" company="ReflectSoftware, Inc.">
//     Copyright (c) ReflectSoftware, Inc.. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Security.Cryptography;

using PostSharp.Aspects;
using ReflectSoftware.Insight.Common;

namespace ReflectSoftware.Insight.Extensions.PostSharp
{
    [Serializable]
    internal class MethodReturnInfo
    {
        public Type ReturnType;
        public String ReturnTypeName;
        public String ClassName;
        public String MethodName;
        public Boolean Parameters;
        public Boolean HashedParameters;
    }


    static internal class TraceMethodHelper
    {
        private readonly static String HashValue;
        private readonly static Type StringType;
        private readonly static Type StringBuilderType;

        ///--------------------------------------------------------------------
        static TraceMethodHelper()
        {
            HashValue = GetRandomHashValue();
            StringType = typeof(String);
            StringBuilderType = typeof(StringBuilder);
        }
        ///--------------------------------------------------------------------
        static private String GetRandomHashValue()
        {
            Random rnd = new Random((Int32)DateTime.Now.Ticks);

            Byte[] rValue = new Byte[rnd.Next(5, 10)];

            #if !NET20
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(rValue);
            }
            #else
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            rng.GetBytes(rValue);
            #endif

            return Convert.ToBase64String(rValue);
        }
        ///--------------------------------------------------------------------
        static public String GetMethodName(String classMethodName, MethodExecutionArgs methodArgs, MethodDisplayFlags flags, out MethodReturnInfo rInfo)
        {
            rInfo = new MethodReturnInfo();
            rInfo.Parameters = (flags & MethodDisplayFlags.Parameters) == MethodDisplayFlags.Parameters;
            rInfo.HashedParameters = (flags & MethodDisplayFlags.HashedParameters) == MethodDisplayFlags.HashedParameters;
                                    
            ParameterInfo[] parameters = methodArgs.Method.GetParameters();
            Type[] types = new Type[parameters.Length];

            StringBuilder sParameters = new StringBuilder("(");

            Object[] args = methodArgs.Arguments.ToArray(); 
            if (args != null)
            {
                for (Int32 i = 0; i < parameters.Length; i++)
                {
                    types[i] = parameters[i].ParameterType;
                    if (rInfo.Parameters)
                    {
                        String sValue = args[i] != null ? args[i].ToString() : "null";
                        Type aType = args[i] != null ? args[i].GetType() : types[i];

                        String sParamType = aType.Name;
                        if (!RIUtils.IsPrimitiveType(aType))
                        {
                            if (sParamType.Contains(","))
                                sParamType = sValue;

                            sParameters.AppendFormat("{0} {1}", sParamType, parameters[i].Name);
                        }
                        else
                        {
                            if( rInfo.HashedParameters )
                                sValue = HashValue;

                            if (aType == StringType || aType == StringBuilderType)
                                sValue = String.Format("\"{0}\"", sValue);

                            sParameters.AppendFormat("{0} {1} = ", sParamType, parameters[i].Name);
                            sParameters.AppendFormat("{0}", sValue);
                        }

                        if (i != parameters.Length - 1)
                            sParameters.Append(", ");
                    }
                }
            }

            sParameters.Append(")");

            rInfo.ReturnType = null;
            String sReturnType = sReturnType = "void ";
            MethodInfo mInfo = methodArgs.Method.ReflectedType.GetMethod(methodArgs.Method.Name, types);
            if (mInfo != null)
            {
                rInfo.ReturnType = mInfo.ReturnType;
                sReturnType = mInfo.ReturnType.Name.Replace("Void", "void ");
                if (!sReturnType.EndsWith(" "))
                    sReturnType = String.Format("{0} ", sReturnType);
            }

            rInfo.ReturnTypeName = sReturnType;
            rInfo.ClassName = classMethodName;
            rInfo.MethodName = methodArgs.Method.Name.Replace(".", String.Empty);

            return String.Format("{0}{1}.{2}{3}", rInfo.ReturnTypeName, rInfo.ClassName, rInfo.MethodName, sParameters);
        }
        ///--------------------------------------------------------------------
        static public String GetReturnValue(MethodExecutionArgs methodArgs, MethodReturnInfo rInfo)
        {
            String sReturnValue = String.Empty;
            if (rInfo.Parameters && String.Compare(rInfo.ReturnTypeName, "void ", false) != 0)
            {                
                String sValue = "null";
                if( methodArgs.ReturnValue != null)
                {
                    sValue = methodArgs.ReturnValue.ToString();
                    if (RIUtils.IsPrimitiveType(rInfo.ReturnType))
                    {
                        if (rInfo.HashedParameters)
                            sValue = HashValue;

                        if (methodArgs.ReturnValue.GetType() == StringType || methodArgs.ReturnValue.GetType() == StringBuilderType)
                            sValue = String.Format("\"{0}\"", sValue);
                    }
                }

                sReturnValue = String.Format(": {0}", sValue);
            }

            return String.Format("{0}{1}.{2}(){3}", rInfo.ReturnTypeName, rInfo.ClassName, rInfo.MethodName, sReturnValue);
        }
    }
}
