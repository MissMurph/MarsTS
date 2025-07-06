using Ratworx.MarsTS.UI;
using UnityEngine;

namespace Ratworx.MarsTS.Commands.Interfaces
{
    public interface ICommandInterface
    {
        public string CommandKey { get; }
        public string Description { get; }
        public CursorSprite Cursor { get; }
        public Sprite GetIcon();
        public void StartSelection();
        public void CancelSelection();
    }
}