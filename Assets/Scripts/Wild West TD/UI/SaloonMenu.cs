using UnityEngine;

// Controls the home-screen flow. The visible interface is saved under 07 Interface.
namespace WildWestTD
{
    public class SaloonMenu : MonoBehaviour
    {
        public bool Visible { get; private set; }
        public bool WipeConfirmationVisible { get; private set; }

        FrontierGame game;
        SaloonWalkthrough walk;
        Camera cam;
        void Start()
        {
            game = GetComponent<FrontierGame>();
            walk = GetComponentInChildren<SaloonWalkthrough>();
            cam = walk.GetComponent<Camera>();
            Show();
        }

        public void Show()
        {
            var shop = GetComponentInChildren<SaloonShop>();
            if (shop)
            {
                shop.Close();
            }

            if (game.Active)
            {
                game.EndMatch();
            }

            if (walk.SeatedTable >= 0)
            {
                walk.LeaveTable();
            }

            walk.enabled = false;
            Visible = true;
            WipeConfirmationVisible = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            cam.rect = new Rect(0, 0, 1, 1);
            cam.fieldOfView = 65;
            cam.transform.localPosition = new Vector3(-4, 3.4f, -8.8f);
            cam.transform.LookAt(transform.position + new Vector3(1, 3, 4));
        }

        public void Play()
        {
            if (WipeConfirmationVisible)
            {
                return;
            }

            Visible = false;
            SaloonAudio.Play("click");
            if (!SaloonProgress.TutorialComplete)
            {
                game.BeginMatch(-1);
                return;
            }

            walk.enabled = true;
            cam.transform.localPosition = new Vector3(0, 1.8f, -8.5f);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void SkipTutorial()
        {
            if (WipeConfirmationVisible)
            {
                return;
            }

            SaloonProgress.CompleteTutorial();
            Show();
            Play();
        }

        public void RequestWipeProgress()
        {
            if (!Visible)
            {
                return;
            }

            WipeConfirmationVisible = true;
            SaloonAudio.Play("click");
        }

        public void CancelWipeProgress()
        {
            WipeConfirmationVisible = false;
            SaloonAudio.Play("click");
        }

        public void ConfirmWipeProgress()
        {
            if (!Visible || !WipeConfirmationVisible)
            {
                return;
            }

            SaloonProgress.WipeProgress();
            Show();
            SaloonAudio.Play("click");
        }
    }
}

