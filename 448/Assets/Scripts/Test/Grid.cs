using UnityEngine;

namespace NTest
{
    public class Grid : MonoBehaviour
    {
        public int width = 10;
        public int height = 10;

        public GameObject[] FloorPrefab;
        public GameObject[] WallStraightPrefab;
        public GameObject[] WallCrossPrefab;
        public GameObject[] WallCornerPrefab;
        public GameObject[] WallTSplitPrefab;

        private void Start()
        {
            NDungeon.Gizmo.Grid grid = new NDungeon.Gizmo.Grid("Grid", width, height);

            Vector3 cameraPosition = Camera.main.transform.position;
            cameraPosition.x = width / 2;
            cameraPosition.y = height / 2;
            Camera.main.transform.position = cameraPosition;
        }

        private void Update()
        {
            if (true == Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (false == Physics.Raycast(ray, out hit))
                {
                    return;
                }

                Debug.Log("Hit: " + hit.point);
            }
        }
    }
}