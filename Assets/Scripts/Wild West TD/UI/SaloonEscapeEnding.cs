using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace WildWestTD
{
    /// <summary>Fades the saved ending Canvas in, celebrates escape, then returns home.</summary>
    public sealed class SaloonEscapeEnding : MonoBehaviour
    {
        public CanvasGroup overlay;
        public Text message;
        public SaloonMenu menu;
        public SaloonWalkthrough player;
        [Min(0.1f)] public float fadeSeconds = 1.2f;
        [Min(0.1f)] public float messageSeconds = 3f;
        public bool IsPlaying { get; private set; }

        void Awake()
        {
            overlay.alpha = 0;
            overlay.blocksRaycasts = false;
            message.gameObject.SetActive(false);
        }

        public void Begin()
        {
            if (IsPlaying || !SaloonProgress.EntranceOpen) return;
            StartCoroutine(ShowEnding());
        }

        IEnumerator ShowEnding()
        {
            IsPlaying = true;
            player.enabled = false;
            overlay.blocksRaycasts = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = false;
            yield return Fade(0, 1);
            message.text = "YOU ESCAPED";
            message.gameObject.SetActive(true);
            yield return new WaitForSecondsRealtime(messageSeconds);
            message.gameObject.SetActive(false);
            menu.Show();
            yield return Fade(1, 0);
            overlay.blocksRaycasts = false;
            IsPlaying = false;
        }

        IEnumerator Fade(float from, float to)
        {
            float elapsed = 0;
            overlay.alpha = from;
            // Start on the next rendered frame, avoiding a stale long frame on entry.
            yield return null;
            while (elapsed < fadeSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                overlay.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / fadeSeconds));
                yield return null;
            }
            overlay.alpha = to;
        }
    }
}
