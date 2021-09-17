# -*- coding: utf-8 -*-
import ctypes
import platform                         # 파이썬 아키텍처를 확인하기 위한 모듈

print(platform.architecture())          # 파이썬 아키텍처 출력

if 'Windows' == platform.system() :     # 윈도우 운영체제에서 c 모듈 로드
    path = './x64/Debug/c_module.dll'
    c_module = ctypes.windll.LoadLibrary(path)
elif 'Linux' == platform.system() :     # 리눅스 운영체제에서 c 모듈 로드
    path = "./libc_module.so"
    c_module = ctypes.cdll.LoadLibrary(path)
else :
    raise OSError()

print(c_module)

# 1. int 타입 인자를 받고, int 타입을 리턴하는 예
add = c_module.add
add.argtypes = (ctypes.c_int, ctypes.c_int)
add.restype = ctypes.c_int

res = add(1, 2)
print(res)

# 2. out 파라메터를 사용하는 예
sub = c_module.sub
sub.argtypes = (ctypes.c_double, ctypes.c_double, ctypes.POINTER(ctypes.c_double))
sub.restype = None
outparam = ctypes.c_double()

sub(3.2, 2.2, outparam)
print(outparam.value)

# 3. 배열 파라메터를 사용하는 예
accumulate = c_module.accumulate
accumulate.argtypes = (ctypes.POINTER(ctypes.c_int), ctypes.c_int)
accumulate.restype = ctypes.c_int

s = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10]
arr = (ctypes.c_int * len(s))(*s)

res = accumulate(arr, len(s))
print(res)

# 4. 구조체 파라메터를 사용하는 예
class Rect(ctypes.Structure) :
    _fields_ = [
        ('x', ctypes.c_int),
        ('y', ctypes.c_int),
        ('width', ctypes.c_int),
        ('height', ctypes.c_int)
    ]

getarea = c_module.getarea
getarea.argtypes = ctypes.POINTER(Rect),
getarea.restype = ctypes.c_int

r = Rect(0, 0, 5, 10)
res = getarea(r)
print(res)