import ctypes
import platform

print(platform.architecture())

# Window 운영체제에서 c 모듈 로드
path = './x64/Debug/c_module.dll'
dll_module = ctypes.windll.LoadLibrary(path)

# 리눅스 운영체제에서 c 모듈 로드
#path = "./libhelloworld_dll.so"
#mod = ctypes.cdll.LoadLibrary(path);
print(dll_module)

# 1. add 호출
add = dll_module.add
add.argtypes = (ctypes.c_int, ctypes.c_int)
add.restype = ctypes.c_int

print(add(1, 2))

# 2. sub
sub = dll_module.sub
sub.argtypes = (ctypes.c_double, ctypes.c_double, ctypes.POINTER(ctypes.c_double))
sub.restype = None

outparam = ctypes.c_double()

res = sub(3.2, 2.2, outparam)

print(outparam.value)

# 3. accumulate
accumulate = dll_module.accumulate
accumulate.argtypes = (ctypes.POINTER(ctypes.c_int), ctypes.c_int)
accumulate.restype = ctypes.c_int

s = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10]

arr = (ctypes.c_int * len(s))(*s)

print(accumulate(arr, len(s)))

# 4. int getarea(Rect* r)
class Rect(ctypes.Structure) :
    _fields_ = [('x', ctypes.c_int),
                ('y', ctypes.c_int),
                ('width', ctypes.c_int),
                ('height', ctypes.c_int)
               ]

getarea = dll_module.getarea
getarea.argtypes = ctypes.POINTER(Rect),
getarea.restype = ctypes.c_int

r = Rect(0, 0, 5, 10)
print(getarea(r))