using UnityEngine;

namespace Ratworx.MarsTS.Commands.Interfaces
{
    public interface ICommandInterfaceArgumentAccepter<in T>
    {
        public string GetArgDescription(T arg);
        public Sprite GetArgIcon(T arg);
        public void StartArgSelection(T arg);
    }
}