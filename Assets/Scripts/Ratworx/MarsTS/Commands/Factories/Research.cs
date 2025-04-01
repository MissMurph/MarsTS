using UnityEngine;

namespace Ratworx.MarsTS.Commands.Factories
{
    public class Research : Produce
    {
        protected override string CommandKey => "research";
        
        public override string Description => _description;

        public override Sprite Icon => icon;
    }
}