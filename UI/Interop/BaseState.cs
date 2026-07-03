namespace Mesen.Interop;

//This is defined as an interface for C# boxing purposes, but is defined as an empty struct in the C++ core
#pragma warning disable IDE1006 // Naming Styles
public interface BaseState
#pragma warning restore IDE1006 // Naming Styles
{
}

public struct EmptyPpuToolsState : BaseState
{
}
