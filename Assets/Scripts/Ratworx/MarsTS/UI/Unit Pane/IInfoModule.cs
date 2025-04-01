using UnityEngine;

namespace Ratworx.MarsTS.UI.Unit_Pane {
    public interface IInfoModule {
        GameObject GameObject { get; }
        string Name { get; }
        T Get<T> ();
        void Deactivate();
    }
}