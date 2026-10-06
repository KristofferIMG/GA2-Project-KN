using UnityEngine;

// Opens the saloon exit only after the Hard victory key has been earned.
namespace WildWestTD
{
    public class SaloonEntranceLock : MonoBehaviour
    {
        public SaloonEscapeEnding ending;
        public Transform leftDoor;
        public Transform rightDoor;
        public GameObject lockVisual;
        public Collider barrier;
        public bool IsOpen
        {
            get
            {
                return SaloonProgress.EntranceOpen;
            }
        }

        public string Prompt
        {
            get
            {
                return IsOpen ? "[ E ] Leave the saloon" : SaloonProgress.HasEntranceKey ? "[ E ]  Unlock entrance with your key" : "Entrance locked - defeat the Undertaker on Hard to earn the key";
            }
        }

        public bool IsLookedAt(Transform camera)
        {
            RaycastHit hit;
            return Physics.Raycast(camera.position, camera.forward, out hit, 3.2f, ~0, QueryTriggerInteraction.Ignore) && hit.transform.IsChildOf(transform) && Mathf.Abs(transform.InverseTransformPoint(hit.point).x) < 1.65f;
        }

        public bool Interact()
        {
            if (ending && ending.IsPlaying) return false;
            if (!SaloonProgress.UnlockEntrance())
            {
                SaloonAudio.Play("locked");
                return false;
            }

            SaloonAudio.Play("door");
            Apply();
            if (ending) ending.Begin();
            return true;
        }

        void Start()
        {
            Apply();
        }

        void Apply()
        {
            if (lockVisual)
            {
                lockVisual.SetActive(!IsOpen);
            }

            if (barrier)
            {
                barrier.enabled = !IsOpen;
            }
        }

        void Update()
        {
            Apply();
            float angle = IsOpen ? 95 : 0;
            if (leftDoor)
            {
                leftDoor.localRotation = Quaternion.Slerp(leftDoor.localRotation, Quaternion.Euler(0, -angle, 0), Time.deltaTime * 4);
            }

            if (rightDoor)
            {
                rightDoor.localRotation = Quaternion.Slerp(rightDoor.localRotation, Quaternion.Euler(0, angle, 0), Time.deltaTime * 4);
            }
        }
    }
}

