using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Drives a multi-page tutorial panel. Page navigation (Next/Back/Close) is
/// triggered by the UI Buttons, which the player operates via the XR Poke Interactor.
/// </summary>
public class TutorialPanelController : MonoBehaviour
{
    [Header("Content")]
    [Tooltip("One entry per tutorial page. Supports multi-line text.")]
    [TextArea(2, 6)]
    public string[] pages;

    [Header("Text References")]
    public TMP_Text bodyText;
    [Tooltip("Optional. Shows e.g. '1/3'.")]
    public TMP_Text pageIndicator;

    [Header("Buttons")]
    public Button nextButton;
    public Button backButton;
    public Button closeButton;

    [Header("Behaviour")]
    [Tooltip("The GameObject hidden when Close is pressed (usually the TipsCanvas root).")]
    public GameObject panelRoot;

    int index;

    void OnEnable()
    {
        // Always restart the tutorial from the first page when shown.
        index = 0;
        Refresh();
    }

    void Start()
    {
        if (nextButton != null) nextButton.onClick.AddListener(Next);
        if (backButton != null) backButton.onClick.AddListener(Back);
        if (closeButton != null) closeButton.onClick.AddListener(Close);
        Refresh();
    }

    public void Next()
    {
        if (pages != null && index < pages.Length - 1)
        {
            index++;
            Refresh();
        }
    }

    public void Back()
    {
        if (index > 0)
        {
            index--;
            Refresh();
        }
    }

    public void Close()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
        else
            gameObject.SetActive(false);
    }

    void Refresh()
    {
        int count = pages != null ? pages.Length : 0;

        if (bodyText != null)
            bodyText.text = count > 0 ? pages[Mathf.Clamp(index, 0, count - 1)] : "";

        if (pageIndicator != null)
            pageIndicator.text = count > 0 ? $"{index + 1}/{count}" : "";

        if (backButton != null)
            backButton.interactable = index > 0;

        if (nextButton != null)
            nextButton.interactable = index < count - 1;
    }
}
