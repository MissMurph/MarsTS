using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.UI {
    public interface IInfoModule {
        GameObject GameObject { get; }
        string Name { get; }
        public T Get<T> ();
    }
}