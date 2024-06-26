using System;

namespace EltraUiCommon.Containers
{
    public interface IContainerRegistry
    {
        bool IsRegistered(Type type);
        void Register(Type type1, Type type2, string dialogViewName);
    }
}
