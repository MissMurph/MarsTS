using System;
using Unity.Netcode;
using UnityEngine;

namespace MarsTS.Commands
{
    public class MoveSerializer : MonoBehaviour, ICommandSerializer
    {
        public string Key => _commandKey;

        [SerializeField] private string _commandKey;

        public ISerializedCommand Reader() =>
            new SerializedMoveCommandlet
            {
                Key = Key
            };

        public ISerializedCommand Writer(Commandlet data)
        {
            MoveCommandlet superType = data as MoveCommandlet;

            return new SerializedMoveCommandlet
            {
                Key = Key,
                Faction = superType.Commander.Id,
                Id = superType.Id,
                TargetPosition = superType.Target
            };
        }
    }

    public struct SerializedMoveCommandlet : ISerializedCommand
    {
        public string Key { get; set; }
        public int Faction { get; set; }
        public int Id { get; set; }

        public Vector3 TargetPosition;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref TargetPosition);
        }
    }
}