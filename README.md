# ReflectInsight-Extensions-PostSharp

[![Build status](https://ci.appveyor.com/api/projects/status/github/reflectsoftware/reflectinsight-extensions-postsharp?svg=true)](https://ci.appveyor.com/project/reflectsoftware/reflectinsight-extensions-postsharp)
[![Release](https://img.shields.io/github/release/reflectsoftware/reflectinsight-extensions-PostSharp.svg)](https://github.com/reflectsoftware/reflectinsight-extensions-PostSharp/releases/latest)
[![NuGet Version](http://img.shields.io/nuget/v/reflectsoftware.insight.extensions.PostSharp.svg?style=flat)](http://www.nuget.org/packages/ReflectSoftware.Insight.Extensions.PostSharp/)
[![NuGet](https://img.shields.io/nuget/dt/reflectsoftware.insight.extensions.PostSharp.svg)](http://www.nuget.org/packages/ReflectSoftware.Insight.Extensions.PostSharp/)
[![Stars](https://img.shields.io/github/stars/reflectsoftware/reflectinsight-extensions-PostSharp.svg)](https://github.com/reflectsoftware/reflectinsight-extensions-PostSharp/stargazers)

## Overview ##

We've added support for the PostSharp tracer. This allows you to leverage your current investment in PostSharp, but leverage the power and flexibility that comes with the ReflectInsight viewer. You can view your PostSharp messages in real-time, in a rich viewer that allows you to filter out and search for what really matters to you.

The benefits of using the PostSharp extension gives you automatic traceability across all methods at a class level or only at a specific method, or a specific field. If using the extension at a class level there are cases where may want to ignore certain methods ie. constructor. You can also define if method traceability will log parameters and their values. At a field level traceability, changing field values will can be shown in the Viewer's Scratchpad and/or Log Window. 

## Benefits of ReflectInsight Extensions ##

The benefits to using the Insight Extensions is that you can easily and quickly add them to your applicable with little effort and then use the ReflectInsight Viewer to view your logging in real-time, allowing you to filter, search, navigate and see the details of your logged messages.

### Specific to PostSharp ###

The benefits of using the PostSharp extension gives you automatic traceability across all methods at a class level or only at a specific method, or a specific field. 

If using the extension at a class level there are cases where you may want to ignore certain methods ie. constructor. 

You can also define if method traceability that will log parameters and their values. 

At a field level traceability, changing field values will only be shown in the Viewer's Scratchpad. Note that Scratchpad values are not persisted and they are only available via the Viewer.

## Getting Started

```powershell
Install-Package ReflectSoftware.Insight.Extensions.PostSharp
```

Then in your app.config or web.config file, add the following configuration sections:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <configSections>    
    <section name="insightSettings" type="ReflectSoftware.Insight.ConfigurationHandler,ReflectSoftware.Insight" />
  </configSections>

  <insightSettings>
    <baseSettings>
      <configChange enabled="true" />
      <enable state="all" />
      <propagateException enabled="false" />
      <global category="ReflectInsight" />
      <exceptionEventTracker time="20" />
      <requestObject requestLifeSpan="10" />
      <debugMessageProcess enabled="true" />
    </baseSettings>

    <listenerGroups active="Debug">
      <group name="Debug" enabled="true" maskIdentities="false">
        <destinations>
          <destination name="Viewer" enabled="true" filter="" details="Viewer" />
        </destinations>
      </group>
    </listenerGroups>
    <logManager>
      <!-- used by the PostSharp Tracer Test -->
      <instance name="fields" />
      <instance name="serviceClass" category="ServiceLayer" />
      <instance name="businessClass" category="BusinessLayer" />
      <instance name="dataAccessClass" category="DataAccessLayer" />
    </logManager>

    <!--
		tracer properties = "None|MethodName|Parameters|HashedParameters|IgnoreExceptions" - default: MethodName
    
    None - will get nothing
    MethodName - will only show method name without parameters ( default )
    Parameters - will show method name and parameters but only for primitve types
    HashedParameters - will show method name but hash all parameter values
    IgnoreExceptions - will ignore logging exceptions between enter/exit block    
		-->

    <extensions>
      <extension name="tracer.fields" instance="fields" />
      <extension name="tracer.service" instance="serviceClass" properties="MethodName" />
      <extension name="tracer.business" instance="businessClass" properties="Parameters" />
      <extension name="tracer.dataAccess" instance="dataAccessClass" properties="HashedParameters" />
    </extensions>
  </insightSettings>   
</configuration>
```

Additional configuration details for the ReflectSoftware.Insight.Extensions.PostSharp logging extension can be found [here](https://reflectsoftware.atlassian.net/wiki/display/RI5/PostSharp+Extension).

## Additional Resources

[Documentation](https://reflectsoftware.atlassian.net/wiki/display/RI5/ReflectInsight+5+documentation)

[Submit User Feedback](http://reflectsoftware.uservoice.com/forums/158277-reflectinsight-feedback)

[Contact Support](support@reflectsoftware.com)

[ReflectSoftware Website](http://reflectsoftware.com)
