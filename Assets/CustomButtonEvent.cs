using UnityEngine;
using UnityEngine.UIElements;

public class CustomButtonEvent : MonoBehaviour
{
    private Renderer[] rends;

    void Start()
    {
        // Automatically find all renderers on RMCar26 and its children
        rends = GameObject.Find("RMCar26").GetComponentsInChildren<Renderer>(true);

        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        Button button1 = root.Q<Button>("Button1");
        Button button2 = root.Q<Button>("Button2");
        Button button3 = root.Q<Button>("Button3");

        button1.clicked += () => SetColor(Color.green);
        button2.clicked += () => SetColor(Color.blue);
        button3.clicked += () => SetColor(Color.red);
    }

    public void SetColor(Color _color)
    {
        foreach (Renderer rend in rends)
            rend.material.color = _color;
    }
}