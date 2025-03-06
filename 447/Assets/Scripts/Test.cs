using TMPro;
using UnityEngine;

public class Test : MonoBehaviour
{
    public RectTransform content;
    public GameObject textPrefab;
    void Start()
    {
        GameObject go = GameObject.Instantiate(textPrefab);
        TextMeshPro text = go.GetComponent<TextMeshPro>();
        text.text = "Hello world";
        text.transform.parent = content;
    }

    void Update()
    {
    }
}
