using System.Collections.Generic;

namespace Data
{
    public sealed class MonsterManager
    {
        #region Singleton
        private MonsterManager() { }
        private static readonly MonsterManager _instance = new MonsterManager();
        public static MonsterManager Instance
        {
            get { return _instance; }
        }
        #endregion

        public Dictionary<int, Monster> monsters = new Dictionary<int, Monster>();

        public void Remove(Monster monster)
        {
            monsters.Remove(monster.monsterNo);
        }

        public void Update()
        {
            foreach (var pair in monsters)
            {
                Monster monster = pair.Value;
                monster.behaviour.blackboard.Set("Self", monster);
                monster.behaviour.Update();
            }
        }
    }
}
