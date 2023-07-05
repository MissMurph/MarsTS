using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Players {

    [Serializable]
    public class CursorSprite {
        public string name;
        public Texture2D texture;
        public Vector2 target;
    }
}