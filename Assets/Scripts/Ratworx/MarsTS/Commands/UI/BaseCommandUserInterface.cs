using Ratworx.MarsTS.UI;
using UnityEngine;

namespace Ratworx.MarsTS.Commands.UI
{
    public abstract class BaseCommandUserInterface : MonoBehaviour, 
                                                     ICommandUserInterface
    {
        public string CommandKey => _commandKey;
        public string Description => _description;
        public CursorSprite Cursor => _cursorSprite;

        [SerializeField] private string _commandKey;
        [SerializeField] private string _description;
        [SerializeField] private Sprite _icon;
        [SerializeField] private CursorSprite _cursorSprite;

        public virtual Sprite GetIcon() => _icon;

        public abstract void StartSelection();
        public abstract void CancelSelection();
    }
}