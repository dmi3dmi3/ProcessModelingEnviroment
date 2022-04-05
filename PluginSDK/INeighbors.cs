namespace PluginSDK
{
    public interface INeighbors
    {
        ICell[] NeighborsCell { get; }

        ICell NW { get; }
        ICell W { get; }
        ICell SW { get; }
        ICell N { get; }
        ICell S { get; }
        ICell NE { get; }
        ICell E { get; }
        ICell SE { get; }
    }
}