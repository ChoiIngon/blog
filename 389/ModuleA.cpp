// 모듈 구현 파일
module ModuleA;         // 시작 부분에 모듈 선언을 배치하여 파일 내용이 명명된 모듈(ModuleA)에 속하도록 지정

namespace Bar
{
    int f()
    {
        return 0;
    }

    double d()
    {
        return 0.5f;
    }

    double internal_f()
    {
        return 0.5f;
    }
}