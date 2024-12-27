using System.Collections.Generic;
using MarsTS.Players;
using UnityEngine;
using UnityEngine.Serialization;

namespace MarsTS.Commands
{
    public class Research : Produce
    {
        protected override string CommandKey => "research";
        
        public override string Description => _description;

        public override Sprite Icon => icon;
    }
}