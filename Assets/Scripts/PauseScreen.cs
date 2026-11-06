using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PauseScreen : MonoBehaviour
{
    public bool isPaused = false;
    private PanelRenderer pausePanel;
    private VisualElement pauseElement;
    private Button buttonQuit;
    void Awake()
    {
        pausePanel = GetComponent<PanelRenderer>();
        pausePanel.RegisterUIReloadCallback(OnUIReload);
        buttonQuit.clicked += Quit;
    }
    void Update()
    {
        if (pausePanel.enabled) pausePanel.enabled = false;
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (!isPaused)
            {
                UnityEngine.Cursor.visible = true;
                UnityEngine.Cursor.lockState = CursorLockMode.None;
                isPaused = true;
            }
            else if (isPaused && UnityEngine.Cursor.visible)
            {
                Quit();
            }
            else
            {
                UnityEngine.Cursor.visible = false;
                UnityEngine.Cursor.lockState = CursorLockMode.Locked;
                isPaused = false;
            }
        }
    }
    private void Quit()
    {
        Application.Quit();
    }
    private void OnUIReload(PanelRenderer renderer, VisualElement root, int version)
    {
        pauseElement = root.Q<VisualElement>("Pause");
        buttonQuit = pauseElement.Q<Button>("Quit");
    }
}
