﻿#ifdef _WIN32
#include <Windows.h>
#endif
#include <iostream>
#include <string>
#include <concepts>

template <class T>
struct remove_modifier
{
	typedef std::remove_reference_t<T> non_ref_type;
	typedef std::remove_all_extents_t<non_ref_type> non_extent_type;
	typedef std::remove_pointer_t<non_extent_type> non_ptr_type;
	typedef std::remove_const_t<non_ptr_type> type;
};

template <class T>
struct is_basic_string : std::false_type {};

template <class T>
struct is_basic_string<std::basic_string<T>> : std::true_type {};

template <class T>
concept StringType = requires(const T & t)
{
	requires is_basic_string<T>::value;
	requires std::is_convertible_v<T, std::basic_string_view<typename T::value_type>>;
};

template <class T>
concept StringLike = requires(const T & t)
{
	requires !is_basic_string<T>::value;
	requires std::is_convertible_v<T, std::basic_string_view<typename remove_modifier<T>::type>>;
};

template <class T> requires StringType<T>
size_t UTF8Length(const T& str)
{
	size_t utf8_char_count = 0;

	for (int i = 0; i < str.length(); )
	{
		// 4 바이트 문자인지 확인
		// 0xF0 = 1111 0000
		if (0xF0 == (0xF0 & str[i]))
		{
			if (str.length() - i < 4)
			{
				return -1;
			}
			// 나머지 3 바이트 확인
			// 0x80 = 1000 0000
			if (0x80 != (0x80 & str[i + 1]) || 0x80 != (0x80 & str[i + 2]) || 0x80 != (0x80 & str[i + 3]))
			{
				throw std::exception("not utf-8 encoded string");
			}

			i += 4;
			utf8_char_count++;
			continue;
		}
		// 3 바이트 문자인지 확인
		// 0xE0 = 1110 0000
		else if (0xE0 == (0xE0 & str[i]))
		{
			if (str.length() - i < 3)
			{
				return -1;
			}

			// 나머지 2 바이트 확인
			// 0x80 = 1000 0000
			if (0x80 != (0x80 & str[i + 1]) || 0x80 != (0x80 & str[i + 2]))
			{
				throw std::exception("not utf-8 encoded string");
			}

			i += 3;
			utf8_char_count++;
			continue;
		}
		// 2 바이트 문자인지 확인
		// 0xC0 = 1100 0000
		else if (0xC0 == (0xC0 & str[i]))
		{
			if (str.length() - i < 2)
			{
				return -1;
			}

			// 나머지 1 바이트 확인
			// 0x80 = 1000 0000
			if (0x80 != (0x80 & str[i + 1]))
			{
				throw std::exception("not utf-8 encoded string");
			}

			i += 2;
			utf8_char_count++;
			continue;
		}
		// 최상위 비트가 0인지 확인
		else if (0 == (str[i] >> 7))
		{
			i += 1;
			utf8_char_count++;
		}
		else
		{
			throw std::exception("not utf-8 encoded string");
		}
	}
	return utf8_char_count;
}

template <class T> requires StringLike<T>
size_t UTF8Length(const T& str)
{
	typedef std::basic_string<typename remove_modifier<T>::type> string_type;
	return UTF8Length(string_type(str));
}

int main()
{
#ifdef _WIN32
    SetConsoleOutputCP(CP_UTF8);				// 윈도우 콘솔에 utf-8 문자열 출력하기 위해 필요
#endif
	auto s = u8"\"가나다 ABC abc 123 !@#\"";			// C++20 이전까지 utf-8로 인코딩된 const char* 타입
												// C++20 부터는 const char8_t* 타입
	std::cout << "char8_t* / count:" << UTF8Length(s) << std::endl;

	// 특수문자 '\'를 포함하고 있는 Raw string literals
	auto r = u8R"("가나다 ABC abc 123 !@#")";		// C++20 이전까지 utf-8로 인코딩된 const char* 타입
												// C++20 부터는 const char8_t* 타입
	std::cout << "char8_t* / count:" << UTF8Length(r) << std::endl;

	using namespace std::string_literals;		// std::string 리터럴 오퍼레이터 활성화
												// 접미사 s가 붙으면 string 객체를 의미한다.
	auto S = u8"\"가나다 ABC abc 123 !@#\""s;     	// C++20 이전까지 std::string, C++20 부터 std::u8string
	std::cout << "std::u8string / count:" << UTF8Length(S) << std::endl;

	auto R = u8R"("가나다 ABC abc 123 !@#")"s;	// C++20 이전까지 std::string, C++20 부터 std::u8string
	std::cout << "std::u8string /count:" << UTF8Length(R) << std::endl;

	char8_t a[] = u8"\"가나다 ABC abc 123 !@#\"";     	// C++20 이전까지 char[26], C++20 부터 char8_t[26]
	std::cout << "char8_t[] / count:" << UTF8Length(a) << std::endl;
	
	char8_t A[] = u8R"("가나다 ABC abc 123 !@#")";     	// C++20 이전까지 char[28], C++20 부터 char8_t[28]
	std::cout << "char8_t[] / count:" << UTF8Length(A) << std::endl;

	return 0;
}