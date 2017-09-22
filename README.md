# Roslyn CodeTaskFactory

[![Build status](https://ci.appveyor.com/api/projects/status/4uy3aoqsa5lwi0en?svg=true)](https://ci.appveyor.com/project/CBT/roslyncodetaskfactory)  [![NuGet package](https://img.shields.io/nuget/v/RoslynCodeTaskFactory.svg)](https://www.nuget.org/packages/RoslynCodeTaskFactory)


An MSBuild TaskFactory that uses the Roslyn compiler to generate .NET Standard task libraries which can be used by inline tasks.  It is a replacement of the built in CodeTaskFactory which uses CodeDom and does not work in .NET Core.

# Getting Started

To get started, add a PackageReference to the RoslynCodeTaskFactory package.

#### NuGet Package Manager UI
Search for `RoslynCodeTaskFactory`

#### NuGet Package Manager Console
```
Install-Package RoslynCodeTaskFactory
```

#### DotNet CLI
```
dotnet add package RoslynCodeTaskFactory
```
The package sets an MSBuild property named `$(RoslynCodeTaskFactory)` which should be used in a `<UsingTask />`.  The RoslynCodeTaskFactory is implemented exactly like the stock MSBuild [CodeTaskFactory](https://msdn.microsoft.com/en-us/library/dd722601.aspx?f=255&MSPPError=-2147217396) with the only difference being which `AssemblyFile` to use.

This example is the same as the one from MSDN with only the `AssemblyFile` attribute changed to use the RoslynCodeTaskFactory:
```xml
<Project ToolsVersion="12.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">  
  <!-- This simple inline task does nothing. -->  
  <UsingTask  
    TaskName="DoNothing"  
    TaskFactory="CodeTaskFactory"  
    AssemblyFile="$(RoslynCodeTaskFactory)">
    <ParameterGroup />  
    <Task>  
      <Reference Include="" />  
      <Using Namespace="" />  
      <Code Type="Fragment" Language="cs">  
      </Code>  
    </Task>  
  </UsingTask>  
</Project>  
```

## Hello World
Here is a more robust inline task. The HelloWorld task displays "Hello, world!" on the default error logging device, which is typically the system console or the Visual Studio Output window. The Reference element in the example is included just for illustration.
```xml
<Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">  
  <!-- This simple inline task displays "Hello, world!" -->  
  <UsingTask  
    TaskName="HelloWorld"
    TaskFactory="CodeTaskFactory"
    AssemblyFile="$(RoslynCodeTaskFactory)">
    <ParameterGroup />  
    <Task>  
      <Reference Include="System.Xml.dll"/>  
      <Using Namespace="System"/>  
      <Using Namespace="System.IO"/>  
      <Code Type="Fragment" Language="cs">  
<![CDATA[  
// Display "Hello, world!"  
Log.LogError("Hello, world!");  
]]>  
      </Code>  
    </Task>  
  </UsingTask>  
</Project>  
```
You could save the HelloWorld task in a file that is named HelloWorld.targets, and then invoke it from a project as follows.
```xml
<Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">  
  <Import Project="HelloWorld.targets" />  
  <Target Name="Hello">  
    <HelloWorld />  
  </Target>  
</Project>  
```

## Samples
Open [Samples.sln](https://github.com/jeffkl/RoslynCodeTaskFactory/blob/master/src/Samples.sln) to see more samples of the [inline task](https://github.com/jeffkl/RoslynCodeTaskFactory/blob/master/src/Samples/Directory.Build.targets#L5) in a .NET Framework and .NET Standard project.  
