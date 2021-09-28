# file : setup.py

# -*- coding: utf-8 -*-

from distutils.core import setup, Extension

setup(
    name="PackageName",                 # 패키지 이름
    ext_modules=[
        Extension(
            "c_module",
            ["c_module_main.cpp"],
            include_dirs = ["."],
        )
    ]
)