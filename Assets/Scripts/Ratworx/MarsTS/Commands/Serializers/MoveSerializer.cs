using System;
using Unity.Netcode;
using UnityEngine;

namespace Ratworx.MarsTS.Commands.Serializers
{
    public class MoveSerializer : MonoBehaviour, ICommandSerializer
    {
        public string Key => _commandKey;

        [SerializeField] private string _commandKey;

        public ISerializedCommand Reader() =>
            new SerializedMoveCommandlet
            {
                SerializerKey = Key
            };

        public ISerializedCommand Writer(Commandlet data) {
            if (data is not Commandlet<Vector3> superType)
                throw new ArgumentException(
                    $"Serializer expected command of type {typeof(Commandlet<Vector3>)}, got {data.GetType()} instead");

            return new SerializedMoveCommandlet
            {
                Name = data.Name,
                SerializerKey = Key,
                Faction = data.Commander.Id,
                Id = data.Id,
                TargetPosition = superType.Target
            };
        }
    }

    public struct SerializedMoveCommandlet : ISerializedCommand
    {
        public string Name { get; set; }
        public string SerializerKey { get; set; }
        public int Faction { get; set; }
        public int Id { get; set; }

        public Vector3 TargetPosition;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref TargetPosition);
        }
    }
}