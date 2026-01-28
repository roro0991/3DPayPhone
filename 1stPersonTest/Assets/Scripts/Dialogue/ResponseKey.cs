using UnityEngine;

namespace Dialogue.Core
{
    public readonly struct ResponseKey
    {
        public readonly Intent Intent;
        public readonly WordID Topic;

        public ResponseKey(Intent intent, WordID topic)
        {
            Intent = intent;
            Topic = topic;
        }
    }
}
