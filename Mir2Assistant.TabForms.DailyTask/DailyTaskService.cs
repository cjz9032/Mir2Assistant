using Mir2Assistant.Common.Functions;
using Mir2Assistant.Common.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mir2Assistant.TabForms.DailyTask
{
    public delegate void CompleteEvent();
    public class DailyTaskService
    {
        public readonly MirGameInstanceModel gameInstance;
        /// <summary>
        /// 任务中要打的怪
        /// </summary>
        private ConcurrentDictionary<string, int> TaskMonster { get; set; } = new ConcurrentDictionary<string, int>();
        /// <summary>
        /// 找怪的路径
        /// </summary>
        public ConcurrentQueue<Point> FindPath { get; set; } = new ConcurrentQueue<Point>();

        public bool Pause = false;
        public event CompleteEvent? CompleteEvent;
        private bool _complete = false;
        public bool Complete
        {
            get
            {
                return _complete;
            }
            set
            {
                _complete = value;
                if (value)
                {
                    CompleteEvent?.Invoke();
                }
            }

        }



        /// <summary>
        /// 完成任务后，去哪个NPC交任务
        /// </summary>
        public Point NPCPoint { get; set; }
        public int pathIndex = 0;
        private MonsterModel? lockedMonster = null;

        public Point NextPoint { get; set; }
        public bool stopRun = false;

        string[] availableSkills = "裂神符,冰霜雪雨,灭天火".Split(',');
        Random rand = new Random();
        public DailyTaskService(MirGameInstanceModel _gameInstance)
        {
            gameInstance = _gameInstance;

        }
        public void AddTaskMonster(string name, int qty)
        {
            if (TaskMonster.ContainsKey(name))
            {
                TaskMonster[name] = Math.Max(qty, TaskMonster[name]);
            }
            else
            {
                TaskMonster.TryAdd(name, qty);
            }
        }

        public void RunTask()
        {
            Complete = false;
            pathIndex = 0;
            bool oldPause = false;
            Task.Run(() =>
            {
                while (!Complete)
                {
                    try
                    {
                        oldPause = Pause;
                        if (Pause)
                        {
                            if (!stopRun)
                            {
                                stopRun = true;
                                GoRunFunction.FindPath(gameInstance, gameInstance.CharacterStatus!.X!.Value, gameInstance.CharacterStatus.Y!.Value);
                            }
                            continue;
                        }
                        else
                        {
                            if (oldPause)
                            {
                                lockedMonster = null;
                                stopRun = false;
                            }
                        }

                        if (lockedMonster == null)
                        {
                            lockedMonster = gameInstance.Monsters.LastOrDefault(o => o.Flag != 0 && TaskMonster.Keys.Contains(o.Name!));
                        }
                        else
                        {
                            var m = gameInstance.Monsters.FirstOrDefault(o => o.Addr == lockedMonster.Addr);
                            if (m == null)
                            {
                                continue;
                            }
                            lockedMonster = m;
                        }
                        if (lockedMonster != null)
                        {
                            if (!stopRun)
                            {
                                stopRun = true;
                                GoRunFunction.FindPath(gameInstance, lockedMonster.X!.Value + new int[] { 1, -1 }.OrderBy(o => rand.Next()).First(), lockedMonster.Y!.Value + new int[] { 1, -1 }.OrderBy(o => rand.Next()).First());
                            }
                            if (lockedMonster.Flag == 0)
                            {
                                TaskMonster[lockedMonster.Name!] -= 1;
                                if (TaskMonster[lockedMonster.Name!] <= 0)
                                {
                                    TaskMonster.Remove(lockedMonster.Name!, out int v);
                                    Thread.Sleep(200);
                                }
                                lockedMonster = null;
                            }
                            else
                            {
                                MonsterFunction.SlayingMonster(gameInstance, lockedMonster.Addr);
                                Thread.Sleep(200);
                                var skill = gameInstance.Skills.Where(o => availableSkills.Contains(o.Name)).OrderBy(o => rand.Next()).FirstOrDefault();
                                if (skill != null)
                                {
                                    MonsterFunction.LockMonster(gameInstance, lockedMonster.Addr);
                                    SkillFunction.SkillCall(gameInstance, skill.Addr!.Value);
                                }
                            }
                        }
                        else
                        {
                            stopRun = false;
                            if (FindPath.Count > 0)
                            {
                                if (pathIndex >= FindPath.Count)
                                {
                                    pathIndex = 0;
                                }
                                var p = FindPath.Skip(pathIndex).First();
                                GoRunFunction.FindPath(gameInstance, p.X, p.Y);
                                if (Math.Abs(gameInstance.CharacterStatus!.X!.Value - p.X) < 3 && Math.Abs(gameInstance.CharacterStatus.Y!.Value - p.Y) < 3)
                                {
                                    pathIndex++;
                                }
                            }
                        }
                        if (TaskMonster.Count == 0)
                        {
                            Complete = true;
                            GoRunFunction.FindPath(gameInstance, NPCPoint.X, NPCPoint.Y);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.ToString());
                    }

                    Thread.Sleep(500);
                }
            });
        }
    }
}
