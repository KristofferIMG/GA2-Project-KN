using UnityEngine;
using System.Collections.Generic;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
// Coordinates a match: input, simulation, saved tower prefabs, camera and effects.
// Battle math lives in Core/FrontierRules; Canvas values live in UI/FrontierGameHUD.
namespace WildWestTD
{
    public partial class FrontierGame : MonoBehaviour
    {
        [Header("Saved mesh prefabs, in shop order")]
        public TowerView[] towerPrefabs;
        public FrontierRules.Battle battle;
        public bool Active
        {
            get
            {
                return battle != null;
            }
        }

        public int Unlocked
        {
            get
            {
                return Mathf.Clamp(PlayerPrefs.GetInt("LastChance.Unlocked", 0), 0, 2);
            }
        }

        SaloonMaps maps;
        SaloonWalkthrough walk;
        Camera cam;
        Transform stage;
        Transform table;
        Transform actors;
        Transform ghost;
        Transform defendersRoot;
        Transform enemiesRoot;
        Transform feedbackRoot;
        readonly Dictionary<int, Transform> models = new Dictionary<int, Transform>();
        readonly List<Effect> effects = new List<Effect>();
        LineRenderer ring;
        LineRenderer mark;
        int build = -1;
        int selected = -1;
        int hoverX = -1;
        int hoverZ = -1;
        bool auto;
        bool paused;
        bool confirmExit;
        bool awarded;
        float speed = 1;
        float autoDelay;
        float noticeTime;
        string notice = "";
        Vector3 savedPosition;
        Quaternion savedRotation;
        float savedFov;
        Rect savedRect;
        bool savedOrtho;
        float savedSize;
        const float Tile = .079f;
        Color amber = new Color(.95f, .67f, .27f);
        class Effect
        {
            public GameObject obj;
            public float life;
            public float total;
            public Vector3 from;
            public Vector3 to;
            public bool arc;
        }

        void Start()
        {
            maps = GetComponent<SaloonMaps>();
            walk = GetComponentInChildren<SaloonWalkthrough>();
            cam = walk.GetComponent<Camera>();
        }

        // Called both by Update and the Continue button, ensuring the win reward cannot be skipped or duplicated.
        void AwardVictoryOnce()
        {
            if (battle.won && !awarded)
            {
                awarded = true;
                if (IsTutorial)
                {
                    SaloonProgress.CompleteTutorial();
                }
                else
                {
                    SaloonEconomy.AwardVictory(battle.difficulty);
                    PlayerPrefs.SetInt("LastChance.Unlocked", Mathf.Max(Unlocked, Mathf.Min(2, battle.difficulty + 1)));
                    if (battle.difficulty == 2)
                    {
                        SaloonProgress.AwardEntranceKey();
                    }

                    PlayerPrefs.Save();
                }
            }

        }

        public void ContinueEndless()
        {
            if (!Active || !battle.won || battle.difficulty < 0) return;
            AwardVictoryOnce();
            if (!battle.ContinueEndless()) return;
            paused = false;
            confirmExit = false;
            auto = false;
            autoDelay = 1.8f;
            Notice("INFINITE MODE - Start the next wave when ready.");
        }

        public void AddTestCash()
        {
            if (!Active || battle.won || battle.lost) return;
            battle.gold += 1000;
            Notice("TEST CASH +$1000");
        }

        void Beep(bool gun = false)
        {
            SaloonAudio.Play(gun ? "revolver" : "click");
        }

        public bool BeginMatch(int difficulty)
        {
            if (Active || difficulty > Unlocked || difficulty < -1 || difficulty > 2 || (difficulty >= 0 && !SaloonProgress.TutorialComplete))
            {
                return false;
            }

            int tableIndex = difficulty < 0 ? 3 : difficulty;
            if (tableIndex >= maps.tables.Length)
            {
                return false;
            }

            if (walk.SeatedTable != tableIndex)
            {
                if (walk.SeatedTable >= 0)
                {
                    walk.LeaveTable();
                }

                walk.SitAt(tableIndex);
            }

            if (difficulty < 0)
            {
                maps.ApplyMap(0);
            }

            table = maps.tables[tableIndex];
            savedPosition = cam.transform.position;
            savedRotation = cam.transform.rotation;
            savedFov = cam.fieldOfView;
            savedRect = cam.rect;
            savedOrtho = cam.orthographic;
            savedSize = cam.orthographicSize;
            walk.enabled = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            stage = FrontierStage.Create(maps);
            table = stage;
            battle = new FrontierRules.Battle(maps.activeMap, difficulty);
            battle.onShot = OnShot;
            battle.onRemoved = OnRemoved;
            actors = new GameObject("Live tabletop battle").transform;
            actors.SetParent(table, false);
            defendersRoot = RuntimeGroup("Defenders");
            enemiesRoot = RuntimeGroup("Enemies");
            feedbackRoot = RuntimeGroup("Effects and previews");
            cam.orthographic = false;
            cam.fieldOfView = 43;
            cam.transform.position = table.position + new Vector3(0, 3.12f, -2.12f);
            cam.transform.LookAt(table.position + new Vector3(0, 1.34f, -.05f));
            cam.rect = new Rect(0, 0, 1, 1);
            acknowledgedTutorialWave = -1;
            build = -1;
            selected = -1;
            auto = false;
            paused = false;
            confirmExit = false;
            awarded = false;
            speed = 1;
            autoDelay = 1.8f;
            Notice("Build your defense, then start wave 1.");
            return true;
        }

        public void EndMatch()
        {
            if (!Active)
            {
                return;
            }

            foreach (var e in effects)
            {
                if (e.obj)
                {
                    Destroy(e.obj);
                }
            }

            effects.Clear();
            ClearIncomePopups();
            if (stage)
            {
                Destroy(stage.gameObject);
            }

            stage = null;
            models.Clear();
            ghost = null;
            ring = null;
            mark = null;
            battle = null;
            cam.rect = savedRect;
            cam.orthographic = savedOrtho;
            cam.orthographicSize = savedSize;
            cam.fieldOfView = savedFov;
            cam.transform.SetPositionAndRotation(savedPosition, savedRotation);
            walk.enabled = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        Vector3 World(Vector2 p, float y = 0)
        {
            int x = Mathf.Clamp(Mathf.RoundToInt(p.x), 0, 19), z = Mathf.Clamp(Mathf.RoundToInt(p.y), 0, 19);
            return table.position + new Vector3((p.x - 9.5f) * Tile, 1.3375f + battle.heights[x, z] * .07f + y, (p.y - 9.5f) * Tile);
        }

        public Vector3 TileWorld(int x, int z)
        {
            return World(new Vector2(x, z));
        }

        void Notice(string s)
        {
            notice = s;
            noticeTime = 4;
        }

        public FrontierRules.Tower Place(int type, int x, int z)
        {
            if (!Active || !SaloonEconomy.IsUnlocked(type))
            {
                return null;
            }

            var t = battle.Build(type, x, z);
            if (t != null)
            {
                CreateTower(t);
                SaloonAudio.Play("build");
            }

            return t;
        }

        void CreateTower(FrontierRules.Tower t)
        {
            var view = Instantiate(towerPrefabs[t.type], defendersRoot);
            view.Bind(t);
            var m = view.transform;
            m.position = World(t.position);
            m.localScale = Vector3.one * Tile;
            models[t.id] = m;
            for (int i = 0; i < t.level; i++)
            {
                var star = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Destroy(star.GetComponent<Collider>());
                star.name = "Upgrade badge";
                star.transform.SetParent(m, false);
                star.transform.localPosition = new Vector3((i - 1) * .13f, .15f, -.29f);
                star.transform.localScale = Vector3.one * .085f;
                star.GetComponent<Renderer>().sharedMaterial = FrontierModels.Mat(amber);
            }
        }

        void OnShot(FrontierRules.Shot s)
        {
            Transform shooter;
            if (models.TryGetValue(s.tower.id, out shooter))
            {
                shooter.GetComponent<TowerView>().AimAt(World(s.point));
            }

            var a = World(s.tower.position, .055f);
            var b = World(s.point, .04f);
            if (s.tower.type == 2)
            {
                var g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Destroy(g.GetComponent<Collider>());
                g.name = "Flying dynamite";
                g.transform.SetParent(feedbackRoot);
                g.transform.localScale = Vector3.one * .012f;
                g.GetComponent<Renderer>().sharedMaterial = FrontierModels.Mat(amber);
                effects.Add(new Effect { obj = g, from = a, to = b, life = .23f, total = .23f, arc = true });
            }
            else if (s.tower.type == 6)
            {
                Vector3 direction = (b - a).normalized;
                for (int i = -2; i <= 2; i++)
                {
                    var g = new GameObject("Shotgun pellet");
                    g.transform.SetParent(feedbackRoot);
                    var l = g.AddComponent<LineRenderer>();
                    l.sharedMaterial = FrontierModels.Mat(amber);
                    l.startWidth = .0014f;
                    l.endWidth = .0005f;
                    l.positionCount = 2;
                    l.SetPosition(0, a);
                    l.SetPosition(1, a + Quaternion.Euler(0, i * 15, 0) * direction * battle.TowerRange(s.tower) * Tile);
                    effects.Add(new Effect { obj = g, life = .09f, total = .09f });
                }
            }
            else
            {
                var g = new GameObject(s.tower.type == 3 ? "Lasso cast" : "Shot tracer");
                g.transform.SetParent(feedbackRoot);
                var l = g.AddComponent<LineRenderer>();
                l.sharedMaterial = FrontierModels.Mat(s.tower.type == 3 ? amber : new Color(1, .86f, .46f));
                l.startWidth = .0025f;
                l.endWidth = .001f;
                l.positionCount = 2;
                l.SetPosition(0, a);
                l.SetPosition(1, b);
                effects.Add(new Effect { obj = g, life = .075f, total = .075f });
            }

            if (s.tower.type != 2)
            {
                SaloonAudio.Play(new[] { "revolver", "sniper", "explosion", "lasso", "click", "gatling", "shotgun", "sniper" }[s.tower.type]);
            }
        }

        void Pulse(Vector3 point, Color color, float size, float duration)
        {
            var g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(g.GetComponent<Collider>());
            g.name = "Impact dust";
            g.transform.SetParent(feedbackRoot);
            g.transform.position = point;
            g.transform.localScale = Vector3.one * size;
            g.GetComponent<Renderer>().sharedMaterial = FrontierModels.Mat(color);
            effects.Add(new Effect { obj = g, life = duration, total = duration });
        }

        void OnRemoved(FrontierRules.Enemy e, bool killed)
        {
            if (!killed)
            {
                Pulse(World(e.position, .025f), Color.red, .025f, .20f);
            }

            Transform m;
            if (models.TryGetValue(e.id, out m))
            {
                Destroy(m.gameObject);
                models.Remove(e.id);
            }

            SaloonAudio.Play(killed ? "coin" : "hurt");
            if (killed && e.bountyGold > 0)
            {
                Notice("BOUNTY CLAIMED  +$" + e.bountyGold + " bonus");
            }

            if (!killed)
            {
                Notice("A bandit escaped!  -1 town health");
            }
        }

        void Update()
        {
            if (!Active)
            {
                return;
            }

            float dt = Time.unscaledDeltaTime;
            noticeTime -= dt;
            Vector2 mouse;
            bool left = false, right = false, escape = false, space = false, testCash = false;
#if ENABLE_INPUT_SYSTEM
 mouse=Mouse.current!=null?Mouse.current.position.ReadValue():Vector2.zero;if(Mouse.current!=null){left=Mouse.current.leftButton.wasPressedThisFrame;right=Mouse.current.rightButton.wasPressedThisFrame;}if(Keyboard.current!=null){escape=Keyboard.current.escapeKey.wasPressedThisFrame;space=Keyboard.current.spaceKey.wasPressedThisFrame;testCash=Keyboard.current.mKey.wasPressedThisFrame;}
#else
            mouse = Input.mousePosition;
            left = Input.GetMouseButtonDown(0);
            right = Input.GetMouseButtonDown(1);
            escape = Input.GetKeyDown(KeyCode.Escape);
            space = Input.GetKeyDown(KeyCode.Space);
            testCash = Input.GetKeyDown(KeyCode.M);
#endif
            if (testCash) AddTestCash();
            if (escape)
            {
                if (build >= 0)
                {
                    build = -1;
                    ClearGhost();
                }
                else
                {
                    paused = !paused;
                }
            }

            if (right)
            {
                build = -1;
                selected = -1;
                ClearGhost();
            }

            if (space && !paused && !confirmExit)
            {
                StartNext();
            }

            if (!paused && !confirmExit && !battle.won && !battle.lost)
            {
                bool wasRunning = battle.running;
                float remain = Mathf.Min(dt, .1f) * speed;
                while (remain > 0)
                {
                    float step = Mathf.Min(remain, 1 / 60f);
                    battle.Step(step);
                    remain -= step;
                }

                if (wasRunning && !battle.running)
                {
                    if (!battle.lost)
                    {
                        ShowBankIncome();
                    }

                    autoDelay = 1.8f;
                    Notice(battle.lost ? "The town has fallen." : "Wave cleared! +$25" + (battle.lastBankIncome > 0 ? "  /  Banks +$" + battle.lastBankIncome : ""));
                    SaloonAudio.Play(battle.lost ? "defeat" : battle.won ? "victory" : "clear");
                }

                if (!battle.running && auto && !battle.won && !battle.lost)
                {
                    autoDelay -= dt;
                    if (autoDelay <= 0)
                    {
                        StartNext();
                    }
                }
            }

            AwardVictoryOnce();

            foreach (var e in battle.enemies)
            {
                Transform model;
                if (!models.TryGetValue(e.id, out model))
                {
                    model = FrontierModels.Make(enemiesRoot, e.kind, false, e.wanted);
                    model.localScale = Vector3.one * Tile * (e.wanted ? 1.08f : 1);
                    models[e.id] = model;
                }

                Vector3 next = World(e.position);
                var delta = next - model.position;
                delta.y = 0;
                if (delta.sqrMagnitude > .00000001f)
                {
                    model.rotation = Quaternion.LookRotation(-delta);
                }

                model.position = next;
                FrontierModels.Animate(model, battle.time, e.speed * (e.slowUntil > battle.time ? .55f : 1));
            }

            UpdateIncomePopups(dt);
            UpdateWantedMark();
            foreach (var t in battle.towers)
            {
                Transform m;
                if (models.TryGetValue(t.id, out m) && !paused && !confirmExit)
                {
                    m.GetComponent<TowerView>().Animate(dt * speed);
                }
            }

            for (int i = effects.Count - 1; i >= 0; i--)
            {
                var e = effects[i];
                e.life -= dt;
                if (e.life <= 0)
                {
                    if (e.arc)
                    {
                        DynamiteBurst(e.to);
                    }

                    Destroy(e.obj);
                    effects.RemoveAt(i);
                    continue;
                }

                if (e.arc)
                {
                    float t = 1 - e.life / e.total;
                    e.obj.transform.position = Vector3.Lerp(e.from, e.to, t) + Vector3.up * Mathf.Sin(t * Mathf.PI) * .065f;
                }
            }

            hoverX = hoverZ = -1;
            bool overBoard = cam.pixelRect.Contains(mouse) && !PointerOverHUD(mouse);
            if (overBoard && !paused && !confirmExit && !battle.won && !battle.lost)
            {
                var ray = cam.ScreenPointToRay(mouse);
                float best = float.MaxValue;
                for (int x = 0; x < 20; x++)
                {
                    for (int z = 0; z < 20; z++)
                    {
                        float hit;
                        var bounds = new Bounds(World(new Vector2(x, z), -.015f), new Vector3(Tile, .03f, Tile));
                        if (bounds.IntersectRay(ray, out hit) && hit < best)
                        {
                            best = hit;
                            hoverX = x;
                            hoverZ = z;
                        }
                    }
                }

                if (hoverX >= 0 && left)
                {
                    if (build >= 0)
                    {
                        if (Place(build, hoverX, hoverZ) == null)
                        {
                            Notice(battle.PlacementError(build, hoverX, hoverZ));
                        }
                    }
                    else
                    {
                        var t = battle.At(hoverX, hoverZ);
                        float nearest = float.MaxValue;
                        foreach (var placed in battle.towers)
                        {
                            float distance;
                            var bounds = new Bounds(World(placed.position, .043f), new Vector3(.055f, .095f, .06f));
                            if (bounds.IntersectRay(ray, out distance) && distance < nearest)
                            {
                                nearest = distance;
                                t = placed;
                            }
                        }

                        selected = t != null ? t.id : -1;
                    }
                }
            }

            UpdatePreview();
        }

        void UpdateWantedMark()
        {
            var e = battle.enemies.Find(a => a.id == battle.markedEnemyId);
            if (e == null)
            {
                if (mark)
                {
                    mark.gameObject.SetActive(false);
                }

                return;
            }

            if (!mark)
            {
                var g = new GameObject("Wanted target - all damage x1.5");
                g.transform.SetParent(feedbackRoot);
                mark = g.AddComponent<LineRenderer>();
                mark.sharedMaterial = FrontierModels.Mat(new Color(1, .72f, .12f));
                mark.widthMultiplier = .0035f;
                mark.positionCount = 33;
                mark.useWorldSpace = true;
            }

            mark.gameObject.SetActive(true);
            Vector3 p = World(e.position, .095f);
            for (int i = 0; i <= 32; i++)
            {
                float a = i * Mathf.PI / 16;
                mark.SetPosition(i, p + new Vector3(Mathf.Cos(a) * .023f, 0, Mathf.Sin(a) * .023f));
            }
        }

        Transform RuntimeGroup(string name)
        {
            var g = new GameObject(name).transform;
            g.SetParent(actors, false);
            return g;
        }

        void ClearGhost()
        {
            if (ghost)
            {
                Destroy(ghost.gameObject);
            }

            ghost = null;
            if (ring)
            {
                Destroy(ring.gameObject);
            }

            ring = null;
        }

        void UpdatePreview()
        {
            if (build >= 0 && hoverX >= 0 && !paused && !confirmExit && !battle.won && !battle.lost)
            {
                if (!ghost)
                {
                    ghost = Instantiate(towerPrefabs[build], feedbackRoot).transform;
                    ghost.localScale = Vector3.one * Tile;
                }

                ghost.gameObject.SetActive(true);
                ghost.position = World(new Vector2(hoverX, hoverZ));
                bool valid = battle.PlacementError(build, hoverX, hoverZ) == "Click to build";
                FrontierModels.Tint(ghost, valid ? new Color(.34f, .82f, .61f, .55f) : new Color(.93f, .24f, .2f, .55f));
                if (build != 4)
                {
                    DrawRing(new Vector2(hoverX, hoverZ), build == 1 ? 3 : battle.TowerRange(new FrontierRules.Tower { type = build, x = hoverX, z = hoverZ }), valid ? (build == 1 ? Color.gray : new Color(.35f, .9f, .66f)) : Color.red);
                }
            }
            else
            {
                if (ghost)
                {
                    ghost.gameObject.SetActive(false);
                }

                var t = Selected();
                if (t != null && t.type != 4)
                {
                    DrawRing(t.position, t.type == 1 ? 3 : battle.TowerRange(t), t.type == 1 ? Color.gray : new Color(.35f, .9f, .66f));
                }
                else if (ring)
                {
                    ring.gameObject.SetActive(false);
                }
            }
        }

        public void SelectTower(int id)
        {
            build = -1;
            ClearGhost();
            selected = battle.towers.Exists(t => t.id == id) ? id : -1;
        }

        public void ChooseTower(int type)
        {
            if (type < 0 || type >= FrontierRules.Prices.Length || !SaloonEconomy.IsUnlocked(type))
            {
                return;
            }

            ClearGhost();
            build = type;
            selected = -1;
            Beep();
        }

        void DrawRing(Vector2 p, float radius, Color c)
        {
            if (!ring)
            {
                var g = new GameObject("Range / sniper blind zone");
                g.transform.SetParent(feedbackRoot);
                ring = g.AddComponent<LineRenderer>();
                ring.positionCount = 97;
                ring.widthMultiplier = .003f;
                ring.useWorldSpace = true;
            }

            ring.gameObject.SetActive(true);
            ring.sharedMaterial = FrontierModels.Mat(c);
            for (int i = 0; i <= 96; i++)
            {
                float a = i * Mathf.PI / 48;
                Vector2 point = p + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius;
                point.x = Mathf.Clamp(point.x, -.49f, 19.49f);
                point.y = Mathf.Clamp(point.y, -.49f, 19.49f);
                ring.SetPosition(i, World(point, .006f));
            }
        }

        FrontierRules.Tower Selected()
        {
            return battle.towers.Find(t => t.id == selected);
        }

        void StartNext()
        {
            if (TutorialNeedsExplanation)
            {
                return;
            }

            if (battle.StartWave())
            {
                Notice(battle.wave % 5 == 0 ? "WANTED GANG  /  Reinforced enemies incoming" : "Wave " + battle.wave + " incoming");
                SaloonAudio.Play("wave");
            }
        }
    }
}


