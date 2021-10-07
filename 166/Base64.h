/*
 * Base64.h
 *
 *  Created on: Aug 7, 2014
 *      Author: kukuta
 */

#ifndef BASE64_H_
#define BASE64_H_
#include <string>

std::string Base64Encode(const char* bytes_to_encode, size_t len);
std::string Base64Decode(std::string const& encoded_string);


#endif /* BASE64_H_ */
