using Ratworx.MarsTS.UI;
using UnityEngine;

namespace Ratworx.MarsTS.Commands.UI
{
    public interface ICommandUserInterface
    {
        public string CommandKey { get; }
        public string Description { get; }
        public CursorSprite Cursor { get; }
        public Sprite GetIcon();
        public void StartSelection();
        public void CancelSelection();
    }
}