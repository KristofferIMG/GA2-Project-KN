using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
// Moves the player around the saloon and smoothly frames the selected challenge table.
namespace WildWestTD
{
    [RequireComponent(typeof(Camera), typeof(AudioListener))]
    public class SaloonWalkthrough : MonoBehaviour
    {
        public Transform[] tables;
        public Transform saloon;
        SaloonShop shop;
        bool lookingAtShop;
        SaloonEntranceLock entrance;
        bool lookingAtDoor;
        FrontierGame game;
        Camera view;
        CharacterController walker;
        SaloonMaps maps;
        float yaw;
        float pitch;
        int seated = -1;
        int near = -1;
        Vector3 returnPosition;
        Quaternion returnRotation;
        readonly string[] names =
        {
            "Dusty Joe",
            "Rose Blackthorn",
            "The Undertaker",
            "Marshal Finch"
        };
        public bool Paused { get; private set; }

        public void Pause()
        {
            Paused = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void Resume()
        {
            Paused = false;
            Cursor.lockState = seated >= 0 ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = seated >= 0;
        }

        public void HomeScreen()
        {
            Paused = false;
            var menu = saloon.GetComponent<SaloonMenu>();
            if (menu)
            {
                menu.Show();
            }
        }

        public void ChangeDifficulty(int delta)
        {
            if (seated < 0 || Paused || game.Active)
            {
                return;
            }

            seated = seated == 3 ? (delta < 0 ? 2 : 0) : (seated + delta % 3 + 3) % 3;
            SaloonAudio.Play("click");
        }

        public string InteractionPrompt => lookingAtShop ? "[ E ] Talk to Otis - Tower Shop" : lookingAtDoor ? entrance.Prompt : near >= 0 ? "[ E ] Inspect " + names[near] + "'s table" : "";
        public string OpponentName => seated >= 0 ? names[seated] : "";

        public int FocusedTable
        {
            get
            {
                return near;
            }
        }

        public int SeatedTable
        {
            get
            {
                return seated;
            }
        }

        void Awake()
        {
            Application.runInBackground = true;
            view = GetComponent<Camera>();
            maps = saloon.GetComponent<SaloonMaps>();
            game = saloon.GetComponent<FrontierGame>();
            entrance = saloon.GetComponentInChildren<SaloonEntranceLock>();
            shop = saloon.GetComponentInChildren<SaloonShop>();
            walker = GetComponent<CharacterController>();
            if (!walker)
            {
                walker = gameObject.AddComponent<CharacterController>();
            }

            walker.height = 1.7f;
            walker.radius = .22f;
            walker.center = new Vector3(0, -.72f, 0);
            walker.stepOffset = .23f;
            walker.skinWidth = .025f;
            transform.localPosition = new Vector3(0, 1.8f, -8.5f);
            pitch = 5;
            yaw = 0;
            transform.rotation = Quaternion.Euler(pitch, yaw, 0);
        }

        void OnDisable()
        {
            Paused = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public int FindLookedAtTable()
        {
            RaycastHit hit;
            if (!Physics.Raycast(transform.position, transform.forward, out hit, 4.2f, ~0, QueryTriggerInteraction.Ignore))
            {
                return -1;
            }

            for (int i = 0; i < tables.Length; i++)
            {
                if (hit.transform.IsChildOf(tables[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        void Update()
        {
            Vector2 move = Vector2.zero, look = Vector2.zero;
            bool interact = false, escape = false, click = false, prev = false, next = false;
#if ENABLE_INPUT_SYSTEM
 var k=Keyboard.current;var m=Mouse.current;if(k!=null){move.x=(k.dKey.isPressed||k.rightArrowKey.isPressed?1:0)-(k.aKey.isPressed||k.leftArrowKey.isPressed?1:0);move.y=(k.wKey.isPressed||k.upArrowKey.isPressed?1:0)-(k.sKey.isPressed||k.downArrowKey.isPressed?1:0);interact=k.eKey.wasPressedThisFrame;escape=k.escapeKey.wasPressedThisFrame;prev=k.leftArrowKey.wasPressedThisFrame;next=k.rightArrowKey.wasPressedThisFrame;}if(m!=null){look=m.delta.ReadValue()*.085f;click=m.leftButton.wasPressedThisFrame;}
#else
            move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            look = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * 2;
            interact = Input.GetKeyDown(KeyCode.E);
            escape = Input.GetKeyDown(KeyCode.Escape);
            click = Input.GetMouseButtonDown(0);
            prev = Input.GetKeyDown(KeyCode.LeftArrow);
            next = Input.GetKeyDown(KeyCode.RightArrow);
#endif
            if (escape)
            {
                if (Paused)
                {
                    Resume();
                }
                else
                {
                    Pause();
                }

                return;
            }

            if (Paused)
            {
                return;
            }

            if (seated >= 0)
            {
                var target = tables[seated].TransformPoint(new Vector3(0, 2.9f, -1.95f));
                var rotation = Quaternion.LookRotation(tables[seated].TransformPoint(new Vector3(.48f, 1.32f, 0)) - target);
                transform.position = Vector3.Lerp(transform.position, target, 1 - Mathf.Exp(-6 * Time.deltaTime));
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 1 - Mathf.Exp(-6 * Time.deltaTime));
                view.fieldOfView = Mathf.Lerp(view.fieldOfView, 53, Time.deltaTime * 6);
                if (maps)
                {
                    if (prev)
                    {
                        maps.ChangeMap(-1);
                    }

                    if (next)
                    {
                        maps.ChangeMap(1);
                    }
                }

                if (interact)
                {
                    LeaveTable();
                }

                return;
            }

            if (click)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            if (Cursor.lockState == CursorLockMode.Locked)
            {
                yaw += look.x;
                pitch = Mathf.Clamp(pitch - look.y, -75, 75);
                transform.rotation = Quaternion.Euler(pitch, yaw, 0);
                var planar = Quaternion.Euler(0, yaw, 0) * new Vector3(move.x, 0, move.y).normalized;
                walker.Move((planar * 3f + Vector3.down * 3) * Time.deltaTime);
                var p = transform.localPosition;
                p.x = Mathf.Clamp(p.x, -9.98f, 9.98f);
                p.z = Mathf.Clamp(p.z, entrance && entrance.IsOpen && Mathf.Abs(p.x) < 1.55f ? -11.5f : -9.6f, 9.7f);
                transform.localPosition = p;
            }

            view.fieldOfView = Mathf.Lerp(view.fieldOfView, 67, Time.deltaTime * 6);
            lookingAtShop = shop && shop.IsLookedAt(transform);
            lookingAtDoor = entrance && entrance.IsLookedAt(transform);
            near = lookingAtDoor ? -1 : FindLookedAtTable();
            if (interact && lookingAtShop)
            {
                shop.Open();
            }
            else if (interact && lookingAtDoor)
            {
                entrance.Interact();
            }
            else if (interact && near >= 0)
            {
                SitAt(near);
            }
        }

        public void SitAt(int index)
        {
            if (index < 0 || index >= tables.Length || seated >= 0)
            {
                return;
            }

            returnPosition = transform.position;
            returnRotation = transform.rotation;
            seated = index;
            walker.enabled = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void LeaveTable()
        {
            if (seated < 0)
            {
                return;
            }

            seated = -1;
            transform.position = returnPosition;
            transform.rotation = returnRotation;
            walker.enabled = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}

