using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class TitleScreen : MonoBehaviour
{
    public bool isTitleClosed = false;
    private PanelRenderer titlePanel;
    private VisualElement titleElement;
    private float titleX;
    private float scrollSpeed = 1;
    void Awake()
    {
        titlePanel = GetComponent<PanelRenderer>();
        titlePanel.RegisterUIReloadCallback(OnUIReload);
    }
    void Start()
    {
        if (isTitleClosed)
        {
            titleElement.visible = false;
        }
        else
        {
            UnityEngine.Cursor.visible = true;
            UnityEngine.Cursor.lockState = CursorLockMode.None;
        }
    }
    void Update()
    {
        if (isTitleClosed && titleElement != null)
        {
            if (titleElement.visible)
            {
                titleX -= 55f * scrollSpeed * Time.deltaTime;
                scrollSpeed += 1;
                titleElement.style.translate = new Translate(titleX,0);
                if (titleX <= -2000) titleElement.visible = false;
            }
        }
        else
        {
            if (Keyboard.current.anyKey.wasPressedThisFrame) isTitleClosed = true;
            else if (Mouse.current.leftButton.wasPressedThisFrame) isTitleClosed = true;
            if (isTitleClosed)
            {
                UnityEngine.Cursor.visible = false;
                UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }
    private void OnUIReload(PanelRenderer renderer, VisualElement root, int version)
    {
        titleElement = root.Q<VisualElement>("Title");
    }
}
