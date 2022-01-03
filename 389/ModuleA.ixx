// ModuleA.ixx 모듈 인터페이스 파일

export module ModuleA;      // 내보낼 모듈의 이름 지정

namespace Bar
{
    export int f();         // 모듈에서 내보낼 기능(함수)의 인터페이스를 지정
    export double d();
    double internal_f();    // not exported
}