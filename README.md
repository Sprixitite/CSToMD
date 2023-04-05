# CSToMD
Simple C# reflection to automatically output Markdown tables describing the composition of all user-defined classes/structs/interfaces/enums in the calling assembly.

# How To Use
## 1) Download CSToMD
In github's web UI, press "Code -> Local -> Download ZIP", extract this zip into any location of your choice, **remember where you put it**.

## 2) Add CSToMD as a dependency of your project
Add CSToMD as a dependency of your project, either via the Visual Studio GUI (I am unfamiliar with how this works), or by adding:
```xml
<ItemGroup>
    <ProjectReference Include="Path\To\CSToMD.csproj"/> <!--This is wherever you put the downloaded folder-->
</ItemGroup>
```
To your project's `.csproj` file.

## 3) Call `CSToMD.CSToMD.gen()` in your `Main()` method
For every assembly you have, call `CSToMD.CSToMD.gen()` in that assembly, preferably via the `Main()` method if possible.

## 4) Check that `output.md` was outputted with the correct information into your project's running directory (wherever the .exe is)
If it is not present ensure that `CSToMD.CSToMD.gen()` _is_ being called, if it is erroring please submit an issue report _**with**_ appropriate information (e.g: a screenshot of the output)

## 5) If `output.md` was successfully outputted, you're done!
You may now remove CSToMD as a dependency of your project and go about your day!