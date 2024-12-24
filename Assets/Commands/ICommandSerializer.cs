using System;

namespace MarsTS.Commands
{
    public interface ICommandSerializer
    {
        public string Key { get; }
        ISerializedCommand Reader();
        ISerializedCommand Writer(Commandlet data);
    }
}