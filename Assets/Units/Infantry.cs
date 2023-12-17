using MarsTS.Commands;
using MarsTS.Entities;
using MarsTS.Teams;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Units {

	public class Infantry : MonoBehaviour, ISelectable, ITaggable<Infantry>, ICommandable {
		public GameObject GameObject => throw new System.NotImplementedException();

		public int ID => throw new System.NotImplementedException();

		public string UnitType => throw new System.NotImplementedException();

		public string RegistryKey => throw new System.NotImplementedException();

		public Faction Owner => throw new System.NotImplementedException();

		public Sprite Icon => throw new System.NotImplementedException();

		public string Key => throw new NotImplementedException();

		public Type Type => throw new NotImplementedException();

		public Commandlet CurrentCommand => throw new NotImplementedException();

		public Commandlet[] CommandQueue => throw new NotImplementedException();

		public Commandlet Auto (ISelectable target) {
			throw new NotImplementedException();
		}

		public string[] Commands () {
			throw new NotImplementedException();
		}

		public void Enqueue (Commandlet order) {
			throw new NotImplementedException();
		}

		public Command Evaluate (ISelectable target) {
			throw new NotImplementedException();
		}

		public void Execute (Commandlet order) {
			throw new NotImplementedException();
		}

		public Infantry Get () {
			throw new NotImplementedException();
		}

		public Relationship GetRelationship (Faction player) {
			throw new System.NotImplementedException();
		}

		public void Hover (bool status) {
			throw new System.NotImplementedException();
		}

		public void Select (bool status) {
			throw new System.NotImplementedException();
		}

		public bool SetOwner (Faction player) {
			throw new System.NotImplementedException();
		}
	}
}