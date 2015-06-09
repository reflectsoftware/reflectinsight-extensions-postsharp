// ***********************************************************************
// Assembly         : ReflectSoftware.Insight.Extensions.PostSharp
// Author           : ReflectSoftware Inc.
// Created          : 03-19-2014
// Last Modified On : 03-28-2014
// ***********************************************************************
// <copyright file="RITraceFieldAttribute.cs" company="ReflectSoftware, Inc.">
//     Copyright (c) ReflectSoftware, Inc.. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;

using PostSharp.Aspects;
using PostSharp.Reflection;

using ReflectSoftware.Insight;

/// <summary>
/// Namespace - PostSharp
/// </summary>
namespace ReflectSoftware.Insight.Extensions.PostSharp
{
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   RITraceFieldDispatchType Enumeration. </summary>
    ///
    /// <remarks>   ReflectInsight Version 5.3. </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////

    public enum RITraceFieldDispatchType
    {
        /// <summary>
        /// Log
        /// </summary>
        Log = 0,
        /// <summary>
        /// Watch
        /// </summary>
        Watch = 1,
        /// <summary>
        /// Both
        /// </summary>
        Both = 2
    }

    ///------------------------------------------------------------------------
    /// <summary>   RITraceFieldAttribute Class. </summary>
    /// <seealso cref="T:PostSharp.Aspects.LocationInterceptionAspect"/>
    ///------------------------------------------------------------------------

    [Serializable]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class RITraceFieldAttribute : LocationInterceptionAspect
    {    
        private readonly String ExtensionName;
        private readonly RITraceFieldDispatchType DispatchType;
        private ReflectInsight ReflectInsightInstance;
        private String Label;
                
        ///--------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="RITraceFieldAttribute"/> class.
        /// </summary>
        ///
        /// <param name="extension">    The extension. </param>
        /// <param name="label">        The label. </param>
        /// <param name="dispatchType"> <see cref="RITraceFieldDispatchType"/> the dispatch. </param>
        ///--------------------------------------------------------------------
        public RITraceFieldAttribute(String extension, String label, RITraceFieldDispatchType dispatchType)
        {
            ExtensionName = extension ?? String.Empty;
            DispatchType = dispatchType;
            Label = label ?? String.Empty;
        }
        ///--------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="RITraceFieldAttribute"/> class.
        /// </summary>
        ///
        /// <remarks>   The default <see cref="RITraceFieldDispatchType"/> of Watch will be used. </remarks>
        ///
        /// <param name="extension">    The extension. </param>
        /// <param name="label">        The label. </param>
        ///--------------------------------------------------------------------

        public RITraceFieldAttribute(String extension, String label): this(extension, label, RITraceFieldDispatchType.Watch)
        {
        }
        
        ///--------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="RITraceFieldAttribute"/> class.
        /// </summary>
        ///
        /// <remarks>   No label will be used. </remarks>
        ///
        /// <param name="extension">    The extension. </param>
        ///--------------------------------------------------------------------
        public RITraceFieldAttribute(String extension): this(extension, String.Empty)
        {
        }

        ///--------------------------------------------------------------------
        /// <summary>
        /// Method invoked at build time to initialize the instance fields of the current aspect. This
        /// method is invoked before any other build-time method.
        /// </summary>
        ///        
        ///
        /// <seealso cref="M:PostSharp.Aspects.LocationLevelAspect.CompileTimeInitialize(LocationInfo,AspectInfo)"/>
        /// ### <param name="targetLocation">   Location to which the current aspect is applied. </param>
        /// ### <param name="aspectInfo">       Reserved for future usage. </param>
        ///--------------------------------------------------------------------
        public override void CompileTimeInitialize(LocationInfo targetLocation, AspectInfo aspectInfo)
        {
            base.CompileTimeInitialize(targetLocation, aspectInfo);

            if (String.Compare(Label.Trim(), String.Empty, false) == 0)
                Label = targetLocation.FieldInfo.Name;
        }
        ///--------------------------------------------------------------------
        /// <summary>   Initializes the current aspect. </summary>
        ///
        /// <seealso cref="M:PostSharp.Aspects.LocationLevelAspect.RuntimeInitialize(LocationInfo)"/>
        /// ### <param name="locationInfo"> Location to which the current aspect is applied. </param>
        ///--------------------------------------------------------------------
        public override void RuntimeInitialize(LocationInfo locationInfo)
        {
            base.RuntimeInitialize(locationInfo);

            OnConfigFileChange();
            RIEventManager.OnServiceConfigChange += OnConfigFileChange;
        }
        
        ///--------------------------------------------------------------------
        /// <summary>   Called upon a configuration file change. </summary>
        ///--------------------------------------------------------------------
        protected void OnConfigFileChange()
        {
            try
            {
                lock (this)
                {
                    ReflectInsightInstance = RILogManager.Get(ReflectInsightConfig.Settings.GetExtensionAttribute(ExtensionName, "instance", String.Empty)) ?? RILogManager.Default;
                }
            }
            catch (Exception ex)
            {
                RIExceptionManager.Publish(ex, "Failed during: RITraceFieldAttribute.OnConfigFileChange()");
            }
        }

        ///--------------------------------------------------------------------
        /// <summary>
        /// Method invoked
        /// <i>instead</i> of the <c>Set</c> semantic of the field or property to which the current aspect is applied, i.e. when the value of this field or property is changed.
        /// </summary>
        ///
        /// <seealso cref="M:PostSharp.Aspects.LocationInterceptionAspect.OnSetValue(LocationInterceptionArgs)"/>
        /// ### <param name="args"> Advice arguments. </param>
        ///--------------------------------------------------------------------
        public override void OnSetValue(LocationInterceptionArgs args)
        {
            base.OnSetValue(args);

            switch(DispatchType)
            {
                case RITraceFieldDispatchType.Watch:
                    ReflectInsightInstance.ViewerSendWatch(Label, args.Value);
                    break;

                case RITraceFieldDispatchType.Log:
                    ReflectInsightInstance.SendTrace("{0} = {1}", Label, args.Value);
                    break;

                default:
                    ReflectInsightInstance.ViewerSendWatch(Label, args.Value);
                    ReflectInsightInstance.SendTrace("{0} = {1}", Label, args.Value);
                    break;
            }
        }
    }
}
