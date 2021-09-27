#include <python.h>
#include <iostream>

int add(int a, int b)
{
    std::cout << __FUNCTION__ << "(" << a << ", " << b << ")" << std::endl;
    return a + b;
}

int sub(int a, int b)
{
    std::cout << __FUNCTION__ << "(" << a << ", " << b << ")" << std::endl;
    return a - b;
}

PyObject* py_add(PyObject* self, PyObject* args)
{
    PyTupleObject* tuple = (PyTupleObject*)args;
    PyLongObject* a = (PyLongObject*)(tuple->ob_item[0]);
    PyLongObject* b = (PyLongObject*)(tuple->ob_item[1]);

    int result = add(a->ob_digit[0], b->ob_digit[0]);

    PyObject* pyResult = Py_BuildValue("i", result);
    return pyResult;
}

PyObject* py_sub(PyObject* self, PyObject* args)
{
    PyTupleObject* tuple = (PyTupleObject*)args;
    int a = 0;
    int b = 0;

    if (false == PyArg_ParseTuple(args, "ii", &a, &b))
    {
        return nullptr;
    }

    int result = sub(a, b);
    PyObject* pyResult = Py_BuildValue("i", result);
    return pyResult;
}

// 모듈에 있는 메소드의 정보(필수)
PyMethodDef method_infos[] = {
    {"add", py_add, METH_VARARGS /* 가변 인자를 의미*/, "Integer add" /*설명*/},
    {"sub", py_sub, METH_VARARGS, "Integer sub"},
    {nullptr, nullptr, 0, nullptr}
};

// 모듈 자체의 정보(필수)
PyModuleDef module_info = {
    PyModuleDef_HEAD_INIT,
    "c_module",                 // 모듈 이름
    "document string",          // 모듈 설명
    -1,                         // Size of per-interpreter state 또는 그냥 -1
    method_infos                // 메소드 정보를 담은 배열
};

// 모듈 초기화 함수
// PyInit_XX
PyMODINIT_FUNC PyInit_c_module()
{
    return PyModule_Create(&module_info);
}