# file : setup.py

# -*- coding: utf-8 -*-

from distutils.core import setup, Extension

setup(
    name="PackageName",                 # 패키지 이름
    ext_modules=[
        Extension(
            "c_module",                 # 모듈 이름. 
                                        # 초기화 함수 PyInit_<modulename>에서 <modulename>과 같아야 한다
            ["c_module_main.cpp"],      # 빌드에 사용 될 C/C++ 파일
            include_dirs = ["."],       # include 디렉토리 패스
        )
    ]
)