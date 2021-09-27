# -*- coding: utf-8 -*-

from distutils.core import setup, Extension

setup(
    name="c_module",
    ext_modules=[
        Extension(
            "c_module",
            ["c_module_main.cpp"],
            include_dirs = ["."],
        )
    ]
)