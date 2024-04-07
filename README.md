# FileSystemLinks
A cross-platform library for creating and reading file system links.

## Features
- .NET Standard 2.0 and up, .NET Framework 4.5.1 and up, Mono and .NET Core compatible
- No dependencies
- Symbolic link code ported from .NET 6+
- Functions:
  - Creating hard links
  - Creating symbolic links
  - Creating junctions on Windows
  - Getting link target

## Usage
All functions are available in two variants - as a static method accepting paths and as an excension method accepting a `FileSystemInfo`.
For example, to create a symbolic link, use:
```
FileSystemLink.CreateFileSymbolicLink("path/to/link", "path/to/target");
```
or
```
new FileInfo("path/to/link").CreateAsSymbolicLink("path/to/target")
```