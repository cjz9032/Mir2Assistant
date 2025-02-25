using Mir2Assistant.Common.Functions;
using Mir2Assistant.Common.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
                    lockedMonster = null;
                    CompleteEvent?.Invoke();
                }
            }

        }

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

        public async Task RunTask()
        {
            Complete = false;
            stopRun = false;
            lockedMonster = null;
            pathIndex = 0;
            bool oldPause = false;
            gameInstance.NewSysMsg += (str) =>
            {
                if (string.IsNullOrEmpty(str))
                {
                    return;
                }
                if (str.EndsWith("天罡正气经验！"))
                {
                    Complete = true;
                }
                var regex = new Regex(@"你现在已经击败(.*?)：(\d+)/(\d+)");
                Match match = regex.Match(str);
                if (match.Success)
                {
                    if (match.Groups[2].Value == match.Groups[3].Value)
                    {
                        TaskMonster.TryRemove(match.Groups[1].Value, out int v);
                    }
                }
            };
            await Task.Run(() =>
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
                            lockedMonster = gameInstance.Monsters.Values?.FirstOrDefault(o => o.Flag != 0 && TaskMonster.Keys.Contains(o.Name!));
                        }
                        else
                        {
                            lockedMonster = gameInstance.Monsters.Values?.FirstOrDefault(o => o.Addr == lockedMonster.Addr);
                        }
                        if (lockedMonster != null)
                        {
                            if (!stopRun)
                            {
                                if (Math.Abs(lockedMonster.X!.Value - gameInstance!.CharacterStatus!.X!.Value) <= 5 && Math.Abs(lockedMonster.Y!.Value - gameInstance.CharacterStatus.Y!.Value) <= 5)
                                {
                                    GoRunFunction.FindPath(gameInstance, gameInstance!.CharacterStatus!.X!.Value, gameInstance!.CharacterStatus!.Y!.Value);
                                    stopRun = true;
                                }else
                                {
                                    GoRunFunction.FindPath(gameInstance, lockedMonster.X!.Value + new int[] { 1, -1 }.OrderBy(o => rand.Next()).First(), lockedMonster.Y!.Value + new int[] { 1, -1 }.OrderBy(o => rand.Next()).First());
                                }
                            }
                            if (lockedMonster.Flag == 0)
                            {
                                //TaskMonster[lockedMonster.Name!] -= 1;
                                //if (TaskMonster[lockedMonster.Name!] <= 0)
                                //{
                                //    TaskMonster.Remove(lockedMonster.Name!, out int v);
                                //    Thread.Sleep(200);
                                //}
                                lockedMonster = null;
                            }
                            else
                            {
                                var lmAddr = lockedMonster.Addr;
                                MonsterFunction.SlayingMonster(gameInstance, lmAddr);
                                Thread.Sleep(200);
                                var skill = gameInstance.Skills.Where(o => availableSkills.Contains(o.Name)).OrderBy(o => rand.Next()).FirstOrDefault();
                                if (skill != null)
                                {
                                    MonsterFunction.LockMonster(gameInstance, lmAddr);
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
