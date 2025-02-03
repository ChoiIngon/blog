using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    public class Actor
    {
        public int health; // 건강. 0되면 죽음

        public int strangth;    // 물리 대미지에 영향
        public int agility;     // 물리 공격 성공 여부, 회피 여부에 영향, 치명타, 공격 순서에 영향

        public int intelligence;    // 마법 공격력에 영향

        public int defense;     // 방어력

        public int resistance;
        public int perception; // 지각, 함정 탐지 등에 영향
        public int luck;

        public int stamina;
        public int sightRange; // 시야. 지각이 높아질 수록 시야도 커진다.

        public class Meta
        {
            public int baseHealth;
            public float helthScalingFactor;
            public int baseMana;
            public float manaScalingFactor;

            public int GetMaxHealth(int level)
            {
                return baseHealth + (int)(level * helthScalingFactor);
            }

            public int GetMaxMana(int intelligence)
            {
                return baseMana + (int)(intelligence * manaScalingFactor);
            }
        }
    }
}