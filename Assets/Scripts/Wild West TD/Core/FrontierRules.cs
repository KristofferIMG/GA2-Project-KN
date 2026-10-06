using System;
using System.Collections.Generic;
using UnityEngine;

namespace WildWestTD
{
    // Shared by the live game and deterministic balance/contract checks.
    public static class FrontierRules
    {
        public static readonly string[] TowerNames =
        {
            "Revolver Man",
            "Sharpshooter",
            "Dynamite Prospector",
            "Lasso Deputy",
            "Frontier Bank",
            "Gatling Gunner",
            "Shotgun Sheriff",
            "Bounty Hunter",
            "Sheriff Office"
        };
        public static readonly int[] Prices =
        {
            RevolverTower.Cost,
            SharpshooterTower.Cost,
            DynamiteTower.Cost,
            LassoTower.Cost,
            BankTower.Cost,
            GatlingTower.Cost,
            ShotgunTower.Cost,
            BountyHunterTower.Cost,
            SheriffOfficeTower.Cost
        };
        public static readonly float[] Damage =
        {
            RevolverTower.StartingDamage,
            SharpshooterTower.StartingDamage,
            DynamiteTower.StartingDamage,
            LassoTower.StartingDamage,
            BankTower.StartingDamage,
            GatlingTower.StartingDamage,
            ShotgunTower.StartingDamage,
            BountyHunterTower.StartingDamage,
            SheriffOfficeTower.StartingDamage
        };
        public static readonly float[] Rate =
        {
            RevolverTower.StartingFireRate,
            SharpshooterTower.StartingFireRate,
            DynamiteTower.StartingFireRate,
            LassoTower.StartingFireRate,
            BankTower.StartingFireRate,
            GatlingTower.StartingFireRate,
            ShotgunTower.StartingFireRate,
            BountyHunterTower.StartingFireRate,
            SheriffOfficeTower.StartingFireRate
        };
        public static readonly float[] Range =
        {
            RevolverTower.StartingRange,
            SharpshooterTower.StartingRange,
            DynamiteTower.StartingRange,
            LassoTower.StartingRange,
            BankTower.StartingRange,
            GatlingTower.StartingRange,
            ShotgunTower.StartingRange,
            BountyHunterTower.StartingRange,
            SheriffOfficeTower.StartingRange
        };
        // Level means the number of upgrades already purchased (0 to 3).
        public static int UpgradePrice(int type, int level)
        {
            if (type == 8) return SheriffOfficeTower.UpgradeCost(level);
            return type == 4 ? BankTower.FirstUpgradePrice + BankTower.UpgradePriceStep * level : Mathf.RoundToInt(Prices[type] * (.5f + .25f * level));
        }

        public static int TotalWaves(int difficulty)
        {
            return difficulty < 0 ? 10 : difficulty == 0 ? 20 : difficulty == 1 ? 30 : 40;
        }

        public static int Reward(int wave)
        {
            return 2 + (wave - 1) / 4;
        }

        public static int Count(int wave)
        {
            return 6 + Mathf.FloorToInt((wave - 1) * .754f);
        }

        public static int Kind(int wave, int index)
        {
            if (wave >= 10 && index % 5 == 2)
            {
                return 2;
            }

            if (wave >= 5 && index % 3 == 1)
            {
                return 1;
            }

            return 0;
        }

        // Keep campaign opening health familiar; 5% growth after wave 30 adds sustained infinite-mode pressure.
        public static float HP(int wave, int difficulty, int kind)
        {
            return Mathf.Round(14 * Mathf.Pow(1.10f, Mathf.Min(wave - 1, 19)) * Mathf.Pow(1.064f, Mathf.Clamp(wave - 20, 0, 10)) * Mathf.Pow(1.05f, Mathf.Max(0, wave - 30)) * (1 + difficulty * .28f) * (kind == 1 ? .58f : kind == 2 ? 3.5f : 1) * (wave % 5 == 0 ? 1.3f : 1));
        }

        public static float Speed(int wave, int difficulty, int kind)
        {
            return (1.25f + Mathf.Min(wave - 1, 27) * .012f) * (1 + .08f * difficulty) * (kind == 1 ? 1.8f : kind == 2 ? .65f : 1);
        }

        public static float Interval(int wave)
        {
            float interval = Mathf.Max(.68f, 1.4f - wave * .025f);
            return wave > 20 ? interval / 1.5f : interval;
        }

        public static List<Vector2> Route(int map, int branch)
        {
            var routes = SaloonMaps.Routes(map);
            var p = new List<Vector2>();
            foreach (var a in routes[branch])
            {
                p.Add(a);
            }

            if (branch > 0)
            {
                var join = routes[branch][routes[branch].Length - 1];
                bool found = false;
                for (int i = 1; i < routes[0].Length; i++)
                {
                    var a = routes[0][i - 1];
                    var b = routes[0][i];
                    if (!found && ((a.x == b.x && join.x == a.x && join.y >= Mathf.Min(a.y, b.y) && join.y <= Mathf.Max(a.y, b.y)) || (a.y == b.y && join.y == a.y && join.x >= Mathf.Min(a.x, b.x) && join.x <= Mathf.Max(a.x, b.x))))
                    {
                        found = true;
                        if (join != b)
                        {
                            p.Add(b);
                        }
                    }
                    else if (found)
                    {
                        p.Add(b);
                    }
                }

                if (!found)
                {
                    throw new Exception("Unconnected route");
                }
            }

            return p;
        }

        // A tower record stores game data. Its visible mesh uses a TowerView component.
        public class Tower
        {
            public int id;
            public int type;
            public int level;
            public int x;
            public int z;
            public int spent;
            public int income
            {
                get
                {
                    return type == 4 ? BankTower.StartingIncome + BankTower.IncomePerUpgrade * level : 0;
                }
            }

            public float cooldown;
            public float spin;
            public float currentRate
            {
                get
                {
                    return type == 5 ? Mathf.Lerp(GatlingTower.StartingFireRate, GatlingTower.MaximumFireRate, spin) * Mathf.Pow(1.2f, level) : rate;
                }
            }

            public Vector2 position
            {
                get
                {
                    return new Vector2(x, z);
                }
            }

            public float damage
            {
                get
                {
                    return Damage[type] * Mathf.Pow(1.2f, level);
                }
            }

            public float rate
            {
                get
                {
                    return Rate[type] * Mathf.Pow(1.2f, level);
                }
            }

            public int sell
            {
                get
                {
                    return Mathf.FloorToInt(spent * .7f);
                }
            }
        }

        // Enemy movement is measured in board tiles, independent of the model scale.
        public class Enemy
        {
            public int id;
            public int kind;
            public int branch;
            public int bountyGold;
            public float hp;
            public float maxHP;
            public float speed;
            public float distance;
            public float total;
            public float slowUntil;
            public Vector2 position;
            public bool wanted;
        }

        public struct Shot
        {
            public Tower tower;
            public Enemy target;
            public Vector2 point;
        }

        public class Battle
        {
            public int markedEnemyId = -1;
            public int markOwnerId = -1;
            public int totalBountyGold;
            public int map;
            public int difficulty;
            public int gold = 100;
            public int lives = 5;
            public int wave;
            public int spawned;
            public int kills;
            public int leaks;
            public int lastBankIncome;
            public int BankIncome
            {
                get
                {
                    int total = 0;
                    foreach (var tower in towers)
                    {
                        total += tower.income;
                    }

                    return total;
                }
            }

            public bool running;
            public bool won;
            public bool endless;
            public bool lost;
            public float time;
            public List<Tower> towers = new List<Tower>();
            public List<Enemy> enemies = new List<Enemy>();
            public Action<Shot> onShot;
            public Action<Enemy, bool> onRemoved;
            public int[, ] heights;
            public bool[, ] blocked;
            public List<Vector2>[] routes;
            int serial;
            float spawnClock;
            public Battle(int m, int d)
            {
                map = m;
                difficulty = d;
                if (d < 0)
                {
                    gold = 200;
                }

                heights = SaloonMaps.Heights(m);
                blocked = SaloonMaps.Path(m);
                routes = new List<Vector2>[SaloonMaps.SpawnCount(m)];
                for (int i = 0; i < routes.Length; i++)
                {
                    routes[i] = Route(m, i);
                }
            }

            public float TowerRange(Tower t)
            {
                if (t.type == 8) return SheriffOfficeTower.StartingRange;
                float normal = Range[t.type] * Mathf.Pow(1.08f, t.level) * (1 + .15f * heights[t.x, t.z]);
                int support = SupportLevel(t);
                return normal * (support < 0 ? 1 : 1 + SheriffOfficeTower.RangeBonus(support));
            }

            // Evaluate current positions and levels, so selling/upgrading an office takes effect immediately.
            // Auras never expand each other, and the office cannot buff itself.
            public int SupportLevel(Tower target)
            {
                if (target.type == 8) return -1;
                int strongest = -1;
                foreach (var office in towers)
                    if (office.type == 8 && office != target && Vector2.Distance(office.position, target.position) <= SheriffOfficeTower.StartingRange)
                        strongest = Mathf.Max(strongest, office.level);
                return strongest;
            }

            public float TowerDamage(Tower tower)
            {
                int support = SupportLevel(tower);
                return tower.damage * (support < 0 ? 1 : 1 + SheriffOfficeTower.DamageBonus(support));
            }

            public bool ContinueEndless()
            {
                if (!won || lost || difficulty < 0 || endless) return false;
                endless = true;
                won = false;
                return true;
            }

            public Tower At(int x, int z)
            {
                return towers.Find(t => t.x == x && t.z == z);
            }

            // Returning a readable reason lets the preview explain invalid placement.
            public string PlacementError(int type, int x, int z)
            {
                if (type < 0 || type >= Prices.Length)
                {
                    return "Unknown tower";
                }

                if (x < 0 || x > 19 || z < 0 || z > 19)
                {
                    return "Choose a board tile";
                }

                if (blocked[x, z])
                {
                    return "Keep the enemy trail clear";
                }

                if (SaloonMaps.IsRiver(map, x, z))
                {
                    return "Cannot build on water";
                }

                if (At(x, z) != null)
                {
                    return "This tile is occupied";
                }

                if (type == 1 && heights[x, z] != 2)
                {
                    return "Sharpshooter needs height 3";
                }

                if (gold < Prices[type])
                {
                    return "Not enough gold";
                }

                return "Click to build";
            }

            public Tower Build(int type, int x, int z)
            {
                if (type < 0 || type >= Prices.Length || won || lost || PlacementError(type, x, z) != "Click to build")
                {
                    return null;
                }

                var t = new Tower
                {
                    id = ++serial,
                    type = type,
                    x = x,
                    z = z,
                    spent = Prices[type]
                };
                gold -= t.spent;
                towers.Add(t);
                return t;
            }

            public bool Upgrade(Tower t)
            {
                if (t == null || !towers.Contains(t) || t.level >= 3 || won || lost)
                {
                    return false;
                }

                int cost = UpgradePrice(t.type, t.level);
                if (gold < cost)
                {
                    return false;
                }

                gold -= cost;
                t.spent += cost;
                t.level++;
                return true;
            }

            public bool Sell(Tower t)
            {
                if (t == null || won || lost || !towers.Remove(t))
                {
                    return false;
                }

                gold += t.sell;
                if (t.id == markOwnerId)
                {
                    markedEnemyId = markOwnerId = -1;
                }

                return true;
            }

            public bool StartWave()
            {
                if (running || won || lost)
                {
                    return false;
                }

                wave++;
                spawned = 0;
                spawnClock = 0;
                running = true;
                foreach (var t in towers)
                {
                    t.cooldown = 0;
                    t.spin = 0;
                }

                markedEnemyId = markOwnerId = -1;
                return true;
            }

            public float Length(List<Vector2> p)
            {
                float length = 0;
                for (int i = 1; i < p.Count; i++)
                {
                    length += Vector2.Distance(p[i - 1], p[i]);
                }

                return length;
            }

            Vector2 Along(List<Vector2> p, float distance)
            {
                for (int i = 1; i < p.Count; i++)
                {
                    float l = Vector2.Distance(p[i - 1], p[i]);
                    if (distance <= l)
                    {
                        return Vector2.Lerp(p[i - 1], p[i], distance / l);
                    }

                    distance -= l;
                }

                return p[p.Count - 1];
            }

            // One team-wide mark. Multiple hunters cannot stack the 1.5x multiplier.
            void UpdateMark()
            {
                if (markedEnemyId >= 0 && enemies.Exists(e => e.id == markedEnemyId && e.hp > 0) && towers.Exists(t => t.id == markOwnerId))
                {
                    return;
                }

                markedEnemyId = markOwnerId = -1;
                Enemy best = null;
                Tower owner = null;
                foreach (var t in towers)
                {
                    if (t.type != 7)
                    {
                        continue;
                    }

                    foreach (var e in enemies)
                    {
                        if (e.hp > 0 && Vector2.Distance(t.position, e.position) <= TowerRange(t) && (best == null || e.hp > best.hp))
                        {
                            best = e;
                            owner = t;
                        }
                    }
                }

                if (best != null)
                {
                    markedEnemyId = best.id;
                    markOwnerId = owner.id;
                }
            }

            // Wanted multiplies every attack, but only a Hunter finishing the target earns the bonus.
            void Hit(Tower t, Enemy e, float damage)
            {
                if (e.hp <= 0)
                {
                    return;
                }

                bool marked = e.id == markedEnemyId;
                int support = SupportLevel(t);
                float boost = support < 0 ? 1 : 1 + SheriffOfficeTower.DamageBonus(support);
                e.hp -= damage * boost * (marked ? BountyHunterTower.WantedDamageMultiplier : 1);
                if (e.hp <= 0 && marked && t.type == 7)
                {
                    e.bountyGold = Reward(wave) * (BountyHunterTower.StartingBonusRewards + t.level);
                }
            }

            public int ShotgunPellets(Tower t, Vector2 aim, Vector2 point)
            {
                Vector2 offset = point - t.position;
                if (offset.magnitude > TowerRange(t))
                {
                    return 0;
                }

                float angle = Vector2.Angle(aim - t.position, offset);
                if (angle > ShotgunTower.HalfConeAngle)
                {
                    return 0;
                }

                return Mathf.Clamp(Mathf.CeilToInt(ShotgunTower.PelletCount * (1 - angle / 36)), 1, ShotgunTower.PelletCount);
            }

            /// <summary>Advance one small simulation step. No rendering or UI belongs in these rules.</summary>
            public void Step(float seconds)
            {
                if (!running || won || lost)
                {
                    return;
                }

                time += seconds;
                spawnClock -= seconds;
                SpawnEnemies();
                FireTowers(seconds);
                MoveEnemiesAndCollectRewards(seconds);
                if (!lost)
                {
                    FinishClearedWave();
                }
            }

            // Alternate entrance branches while following the wave's enemy schedule.
            void SpawnEnemies()
            {
                while (spawned < Count(wave) && spawnClock <= 0)
                {
                    int kind = Kind(wave, spawned), branch = spawned % routes.Length;
                    var e = new Enemy
                    {
                        id = ++serial,
                        kind = kind,
                        branch = branch,
                        hp = HP(wave, difficulty, kind),
                        maxHP = HP(wave, difficulty, kind),
                        speed = Speed(wave, difficulty, kind),
                        wanted = wave % 5 == 0,
                        total = Length(routes[branch]),
                        position = routes[branch][0]
                    };
                    enemies.Add(e);
                    spawned++;
                    spawnClock += Interval(wave);
                }
            }

            // Choose targets, advance cooldowns, and apply each tower's attack.
            void FireTowers(float dt)
            {
                UpdateMark();
                foreach (var t in towers)
                {
                    if (t.type == 4 || t.type == 8)
                    {
                        continue;
                    }

                    Enemy target = null;
                    float best = float.MaxValue;
                    foreach (var e in enemies)
                    {
                        if (e.hp <= 0)
                        {
                            continue;
                        }

                        float dist = Vector2.Distance(t.position, e.position);
                        if (t.type == 1 ? dist < 3 : dist > TowerRange(t))
                        {
                            continue;
                        }

                        float remain = e.total - e.distance;
                        if (t.type == 1)
                        {
                            remain -= e.maxHP * 10;
                        }

                        if (t.type == 7 && e.id == markedEnemyId)
                        {
                            remain -= 100000;
                        }

                        if (t.type == 3 && e.slowUntil > time + .25f)
                        {
                            remain += 1000;
                        }

                        if (remain < best)
                        {
                            best = remain;
                            target = e;
                        }
                    }

                    if (t.type == 5)
                    {
                        t.spin = Mathf.Clamp01(t.spin + (target != null ? dt / GatlingTower.SpinUpSeconds : -dt / GatlingTower.CoolDownSeconds));
                    }

                    t.cooldown -= dt;
                    if (target == null)
                    {
                        t.cooldown = 0;
                        continue;
                    }

                    if (t.cooldown > 0)
                    {
                        continue;
                    }

                    t.cooldown += 1 / t.currentRate;
                    var point = target.position;
                    if (t.type == 2)
                    {
                        foreach (var e in enemies)
                        {
                            if (Vector2.Distance(e.position, point) <= DynamiteTower.SplashRadius)
                            {
                                Hit(t, e, t.damage);
                            }
                        }
                    }
                    else if (t.type == 6)
                    {
                        foreach (var e in enemies)
                        {
                            int pellets = ShotgunPellets(t, point, e.position);
                            if (pellets > 0)
                            {
                                Hit(t, e, t.damage * pellets);
                            }
                        }
                    }
                    else
                    {
                        Hit(t, target, t.damage);
                        if (t.type == 3)
                        {
                            target.slowUntil = time + LassoTower.SlowDuration;
                        }
                    }

                    if (onShot != null)
                    {
                        onShot(new Shot { tower = t, target = target, point = point });
                    }
                }
            }

            // Remove defeated enemies first. Survivors walk along their route and can cost health.
            void MoveEnemiesAndCollectRewards(float dt)
            {
                for (int i = enemies.Count - 1; i >= 0; i--)
                {
                    var e = enemies[i];
                    if (e.hp <= 0)
                    {
                        gold += Reward(wave) + e.bountyGold;
                        totalBountyGold += e.bountyGold;
                        kills++;
                        if (onRemoved != null)
                        {
                            onRemoved(e, true);
                        }

                        enemies.RemoveAt(i);
                        if (e.id == markedEnemyId)
                        {
                            markedEnemyId = markOwnerId = -1;
                        }

                        continue;
                    }

                    e.distance += e.speed * (e.slowUntil > time ? LassoTower.SlowedSpeed : 1) * dt;
                    e.position = Along(routes[e.branch], e.distance);
                    if (e.distance >= e.total)
                    {
                        lives--;
                        leaks++;
                        if (onRemoved != null)
                        {
                            onRemoved(e, false);
                        }

                        enemies.RemoveAt(i);
                        if (e.id == markedEnemyId)
                        {
                            markedEnemyId = markOwnerId = -1;
                        }

                        if (lives <= 0)
                        {
                            lives = 0;
                            lost = true;
                            running = false;
                            return;
                        }
                    }
                }
            }

            // This is the single place that pays wave and bank income.
            void FinishClearedWave()
            {
                if (spawned == Count(wave) && enemies.Count == 0)
                {
                    lastBankIncome = BankIncome;
                    gold += 25 + lastBankIncome;
                    running = false;
                    if (!endless && wave >= TotalWaves(difficulty))
                    {
                        won = true;
                    }
                }
            }
        }
    }
}


