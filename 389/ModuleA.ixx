// ModuleA.ixx ��� �������̽� ����

export module ModuleA;      // ������ ����� �̸� ����

namespace Bar
{
    export int f();         // ��⿡�� ������ ���(�Լ�)�� �������̽��� ����
    export double d();
    double internal_f();    // not exported
}