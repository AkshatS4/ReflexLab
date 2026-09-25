using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ColorOption : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text label;

    private bool correct;
    private Game2Controller controller;

    public void Setup(
        string text,
        Color textColor,
        bool isCorrect,
        Game2Controller owner
    )
    {
        correct = isCorrect;
        controller = owner;

        label.text = text;
        label.color = textColor;

        button.onClick.RemoveAllListeners();

        button.onClick.AddListener(
            HandleClick
        );
    }

    private void HandleClick()
    {
        if (controller != null)
        {
            controller.SelectOption(correct);
        }
    }
}