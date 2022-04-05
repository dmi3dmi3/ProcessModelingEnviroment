using System;
using System.Collections.Generic;

namespace PluginSDK
{
    public interface ICommand
    {
        String Name { get; }
        void Execute(ICell cell, INeighbors neighbors);
    }
}