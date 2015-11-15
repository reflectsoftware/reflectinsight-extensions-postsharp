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
    [Serializable]
    [RITraceAttribute(AttributeExclude = true)]
    public class RITraceAttribute : OnMethodBoundaryAspect, IRITrace
    {
        public ReflectInsight Logger { get; internal set; }

        public String Name { get; internal set; }

        private readonly Boolean IsNameNullOrEmpty;
        private MethodDisplayFlags DisplayFlags;
        private MethodReturnInfo ReturnInfo;
        private Boolean IgnoreExceptions;
        private Boolean IsDisplayFlagNone;
        private String ClassName;

        ///--------------------------------------------------------------------
        public RITraceAttribute(String extension)
        {
            Name = extension ?? String.Empty;
            IsNameNullOrEmpty = StringHelper.IsNullOrEmpty(Name);            
            Logger = null;
        }
        ///--------------------------------------------------------------------
        public RITraceAttribute(): this(null)
        {
        }
        ///--------------------------------------------------------------------
        public override void CompileTimeInitialize(MethodBase method, AspectInfo aspectInfo)
        {
            base.CompileTimeInitialize(method, aspectInfo);

            // only extract the class method name
            ClassName = method.ReflectedType.FullName;
            Int32 idx = ClassName.LastIndexOf('.');
            if (idx != -1)
            {
                ClassName = ClassName.Remove(0, idx + 1);
            }
        }
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
        public override void OnException(MethodExecutionArgs methodArgs)
        {
            if (IsNameNullOrEmpty || IgnoreExceptions)
            {
                return;
            }

            TraceThreadInfo threadInfo = RITraceManager.GetTraceInfo();

            if (threadInfo.MethodException == null)
            {
                threadInfo.MethodException = methodArgs.Exception;
                Logger.SendException(Name, methodArgs.Exception);
            }
        }
    }
}
