using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    public class Actor
    {
        public int health; // �ǰ�. 0�Ǹ� ����

        public int strangth;    // ���� ������� ����
        public int agility;     // ���� ���� ���� ����, ȸ�� ���ο� ����, ġ��Ÿ, ���� ������ ����

        public int intelligence;    // ���� ���ݷ¿� ����

        public int defense;     // ����

        public int resistance;
        public int perception; // ����, ���� Ž�� � ����
        public int luck;

        public int stamina;
        public int sightRange; // �þ�. ������ ������ ���� �þߵ� Ŀ����.

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