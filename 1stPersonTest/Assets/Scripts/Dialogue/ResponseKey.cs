using System;
using UnityEngine;

namespace Dialogue.Core
{
    public readonly struct ResponseKey : IEquatable<ResponseKey>
    {
        public readonly Intent Intent;
        public readonly WordID Topic;

        public ResponseKey(Intent intent, WordID topic)
        {
            Intent = intent;
            Topic = topic;
        }

        // For performance: prevent C# from converting to obj by boxing
        // instead compare values directly.
        public bool Equals(ResponseKey other) =>
            Intent == other.Intent &&
            Topic == other.Topic;

        // Override default function to treat keys with same values as
        // equal instead of different due to being different instances.
        public override bool Equals(object obj) =>
            obj is ResponseKey other &&
            Intent == other.Intent &&
            Topic == other.Topic;

        // Ensure keys with same values have the same hashes or
        // in other words, put keys with same values into the same box.
        public override int GetHashCode() =>
            HashCode.Combine(Intent, Topic);
    }
}
