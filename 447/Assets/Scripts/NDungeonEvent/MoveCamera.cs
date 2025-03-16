using System.Collections;
using UnityEngine;

namespace NDungeonEvent
{
    public class MoveCamera : DungeonEvent
    {
        private Vector3 position;
        private float seconds;

        public MoveCamera(Vector3 position, float seconds)
        {
            this.position = position;
            this.seconds = seconds;
        }

        public IEnumerator OnEvent()
        {
            if (0 < seconds)
            {
                float interpolation = 0.0f;
                Vector3 start = Camera.main.transform.position;
                position.z = Camera.main.transform.position.z;
                while (1.0f > interpolation)
                {
                    interpolation += Time.deltaTime / seconds;

                    Camera.main.transform.position = Vector3.Lerp(start, this.position, interpolation);
                    yield return null;
                }
            }
            Camera.main.transform.position = this.position;
        }
    }
}