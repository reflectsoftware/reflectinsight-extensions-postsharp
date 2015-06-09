// ***********************************************************************
// Assembly         : ReflectSoftware.Insight.Extensions.PostSharp
// Author           : ReflectSoftware Inc.
// Created          : 03-19-2014
// Last Modified On : 03-28-2014
// ***********************************************************************
// <copyright file="RITraceAttribute.cs" company="ReflectSoftware, Inc.">
//     Copyright (c) ReflectSoftware, Inc.. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Reflection;

using PostSharp.Aspects;

using ReflectSoftware.Insight;
using ReflectSoftware.Insight.Common;

using RI.Utils.Strings;

/// <summary>
/// Namespace - PostSharp
/// </summary>
namespace ReflectSoftware.Insight.Extensions.PostSharp
{
    ///------------------------------------------------------------------------
    /// <summary>   RITraceAttribute Class. </summary>
    ///
    /// <seealso cref="T:PostSharp.Aspects.OnMethodBoundaryAspect"/>
    /// <seealso cref="T:ReflectSoftware.Insight.IRITrace"/>
    ///------------------------------------------------------------------------

    [Serializable]
    [RITraceAttribute(AttributeExclude = true)]
    public class RITraceAttribute : OnMethodBoundaryAspect, IRITrace
    {
        /// <summary>   Gets the logger assigned to this interface. </summary>
        /// <seealso cref="P:ReflectSoftware.Insight.IRITrace.Logger"/>     
        public ReflectInsight Logger { get; internal set; }

        /// <summary>   Gets the name of the interface. </summary>
        /// <seealso cref="P:ReflectSoftware.Insight.IRITrace.Name"/>
        public String Name { get; internal set; }

        private readonly Boolean IsNameNullOrEmpty;
        private MethodDisplayFlags DisplayFlags;
        private MethodReturnInfo ReturnInfo;
        private Boolean IgnoreExceptions;
        private Boolean IsDisplayFlagNone;
        private String ClassName;

        ///--------------------------------------------------------------------                
        /// <summary>   Initializes a new instance of the <see cref="RITraceAttribute"/> class. </summary>        
        /// <param name="extension">    The extension. </param>
        ///--------------------------------------------------------------------
        public RITraceAttribute(String extension)
        {
            Name = extension ?? String.Empty;
            IsNameNullOrEmpty = StringHelper.IsNullOrEmpty(Name);            
            Logger = null;
        }

        ///--------------------------------------------------------------------
        /// <summary>   Initializes a new instance of the <see cref="RITraceAttribute"/> class. </summary>        
        ///--------------------------------------------------------------------
        public RITraceAttribute(): this(null)
        {
        }
        ///--------------------------------------------------------------------
        /// <summary>
        /// Method invoked at build time to initialize the instance fields of the current aspect. This method is invoked 
        /// before any other build-time method.
        /// </summary>
        ///        
        ///
        /// <seealso cref="M:PostSharp.Aspects.MethodLevelAspect.CompileTimeInitialize(MethodBase,AspectInfo)"/>
        /// ### <param name="method">       Method to which the current aspect is applied. </param>
        /// ### <param name="aspectInfo">   Reserved for future usage. </param>
        ///--------------------------------------------------------------------
        public override void CompileTimeInitialize(MethodBase method, AspectInfo aspectInfo)
        {
            base.CompileTimeInitialize(method, aspectInfo);

            // only extract the class method name
            ClassName = method.ReflectedType.FullName;
            Int32 idx = ClassName.LastIndexOf('.');
            if (idx != -1)
                ClassName = ClassName.Remove(0, idx + 1);
        }

        ///--------------------------------------------------------------------
        /// <summary>   Initializes the current aspect. </summary>        
        /// <seealso cref="M:PostSharp.Aspects.MethodLevelAspect.RuntimeInitialize(MethodBase)"/>
        /// ### <param name="method">   Method to which the current aspect is applied. </param>
        ///--------------------------------------------------------------------
        public override void RuntimeInitialize(MethodBase method)
        {
            base.RuntimeInitialize(method);

            Logger = null;

            if (IsNameNullOrEmpty)
                return;

            OnConfigFileChange();
            RIEventManager.OnServiceConfigChange += OnConfigFileChange;
        }

        ///--------------------------------------------------------------------
        /// <summary>   Called upon a configuration change. </summary>        
        ///--------------------------------------------------------------------
        protected void OnConfigFileChange()
        {
            try
            {
                lock (this)
                {
                    Logger = RILogManager.Get(ReflectInsightConfig.Settings.GetExtensionAttribute(Name, "instance", String.Empty)) ?? RILogManager.Default;
                    DisplayFlags = MethodDisplayFlags.MethodName;  // default
                    
                    IgnoreExceptions = false;
                    String displayFlags = ReflectInsightConfig.Settings.GetExtensionAttribute(Name, "properties", String.Empty).Trim().ToLower();
                    if (displayFlags.Contains("ignoreexceptions"))
                    {
                        IgnoreExceptions = true;                        
                        displayFlags = displayFlags.Replace("|ignoreexceptions", String.Empty);
                        displayFlags = displayFlags.Replace("ignoreexceptions|", String.Empty);
                        displayFlags = displayFlags.Replace("ignoreexceptions", String.Empty);
                    }

                    Type methodDisplayFlagsType = typeof(MethodDisplayFlags);

                    IsDisplayFlagNone = displayFlags.Contains("none");
                    if (!IsDisplayFlagNone)
                    {
                        if (displayFlags != String.Empty)
                        {
                            String[] flags = displayFlags.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (String flag in flags)
                            {
                                try
                                {
                                    DisplayFlags |= (MethodDisplayFlags)Enum.Parse(methodDisplayFlagsType, flag.Trim(), true);
                                }
                                catch (ArgumentException)
                                {
                                    // bad display flag - just ignore
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                RIExceptionManager.Publish(ex, "Failed during: RITracerAttribute.OnConfigFileChange()");
            }
        }
        ///--------------------------------------------------------------------
        /// <summary>   Called upon entry. </summary>        
        /// <seealso cref="M:PostSharp.Aspects.OnMethodBoundaryAspect.OnEntry(MethodExecutionArgs)"/>
        /// ### <param name="args"> 
        /// Event arguments specifying which method is being executed, which are
        /// its arguments, and how should the execution continue after the
        /// execution of
        /// <see cref="M:PostSharp.Aspects.IOnMethodBoundaryAspect.OnEntry(PostSharp.Aspects.MethodExecutionArgs)" />.
        /// </param>
        ///--------------------------------------------------------------------
        public override void OnEntry(MethodExecutionArgs methodArgs)
        {
            if (IsNameNullOrEmpty)
                return;

            RITraceManager.EnterMethod(this);

            if (!IsDisplayFlagNone)
            {
                Logger.EnterMethod(TraceMethodHelper.GetMethodName(ClassName, methodArgs, DisplayFlags, out ReturnInfo));
            }
        }
        ///--------------------------------------------------------------------          
        /// <summary>   Called uopon exit. </summary>
        
        /// <seealso cref="M:PostSharp.Aspects.OnMethodBoundaryAspect.OnExit(MethodExecutionArgs)"/>
        /// ### <param name="args"> Event arguments specifying which method is being executed and which are its arguments. </param>        
        ///--------------------------------------------------------------------
        public override void OnExit(MethodExecutionArgs methodArgs)
        {
            if (IsNameNullOrEmpty)
                return;

            try
            {
                if (!IsDisplayFlagNone)
                {
                    Logger.ExitMethod(TraceMethodHelper.GetReturnValue(methodArgs, ReturnInfo));
                }
            }
            finally
            {
                RITraceManager.ExitMethod();
            }
        }

        ///--------------------------------------------------------------------
        /// <summary>   Called on an exception. </summary>        
        /// <seealso cref="M:PostSharp.Aspects.OnMethodBoundaryAspect.OnException(MethodExecutionArgs)"/>
        /// ### <param name="args"> Event arguments specifying which method is being executed and which are its arguments. </param>
        ///--------------------------------------------------------------------
        public override void OnException(MethodExecutionArgs methodArgs)
        {
            if (IsNameNullOrEmpty || IgnoreExceptions)
                return;

            TraceThreadInfo threadInfo = RITraceManager.GetTraceInfo();

            if (threadInfo.MethodException == null)
            {
                threadInfo.MethodException = methodArgs.Exception;
                Logger.SendException(Name, methodArgs.Exception);
            }
        }
    }
}
