#include <openssl/evp.h>
#include <openssl/pem.h>
#include <memory>
#include <iostream>
#include "Base64.h" // 별도 Base64 코드 필요

bool InappBillingVerify(const char* data, const char* signature, const char* pub_key_id)
{
    std::shared_ptr<EVP_MD_CTX> mdctx = std::shared_ptr<EVP_MD_CTX>(EVP_MD_CTX_create(), EVP_MD_CTX_destroy);
    const EVP_MD* md = EVP_get_digestbyname("SHA1");

    EVP_VerifyInit_ex(mdctx.get(), md, NULL);

    EVP_VerifyUpdate(mdctx.get(), (void*)data, strlen(data));

    std::shared_ptr<BIO> b64 = std::shared_ptr<BIO>(BIO_new(BIO_f_base64()), BIO_free);
    BIO_set_flags(b64.get(), BIO_FLAGS_BASE64_NO_NL);

    std::shared_ptr<BIO> bPubKey = std::shared_ptr<BIO>(BIO_new(BIO_s_mem()), BIO_free);
    BIO_puts(bPubKey.get(), pub_key_id);
    BIO_push(b64.get(), bPubKey.get());
    std::shared_ptr<EVP_PKEY> pubkey = std::shared_ptr<EVP_PKEY>(d2i_PUBKEY_bio(b64.get(), NULL), EVP_PKEY_free);
    std::string decoded_signature = Base64Decode(std::string(signature)); // 별도 Base64 코드 필요. 그냥 컴파일하면 에러남

    return 1 == EVP_VerifyFinal(
        mdctx.get(),
        (unsigned char*)decoded_signature.c_str(),
        decoded_signature.length(),
        pubkey.get()
    );
}

int main()
{
    // 결제 완료 후 구글로 부터 받은 영수증
    const char* receipt = "{"
        "\"orderId\":\"GPA.3331-7513-9788-96070\","
        "\"packageName\":\"com.kukuta.pentatiles\","
        "\"productId\":\"pentatiles.google.hint.10\","
        "\"purchaseTime\":1633449519729,"
        "\"purchaseState\":0,"
        "\"purchaseToken\":\"apookopndinajikkicgkkifo.AO-J1OzvmCTyKoD4-I93-1xHhddHSpseIRCbBup53Vl83o7A2LwUX9Wl3-2Hnml69AI3p6ZNtHrNoQYE7mMt3VYopfkCrPfAJ9m_HBIrjd_ZTHCTW6TMQlQ\","
        "\"acknowledged\":false"
        "}";

    // 공개키
    const char* signature = "nGNND0XpGqUNMA8GZ69BFsGEXYtqWukTaETrzf8dhxqWGo2zB1ZV7xzujruLnRVqwJD3cb9PtV2bEgTF7VrNpuxoXIiOxJNleJ05L0g+O0ex6BClBUscPeE5TnjMnEBfk6IOs0r8VFaq9/EmDSG4f4KkurprNVenpCmtBqSQPPj9wYR1BNu8fW9qVrTzx3RqpN41ytwyqm2OmW4Of0gLDlvrAYBsv43pzJD+J6ejX9fcVfZc1ZpO7pgi/fsirYah9R+BFZQCML6spFZwrzG5w+WfmpNTfwIzBFJ9m4d7DckKxIwCoQNsORaKSMCIGvynRGYaalGFFG4Bx5FNWcpsDg==";

    const char* public_key = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAzc9hFdWcGG6eONxzt/bfnk+MYIwbroAY+V/5b8I8R+z8VIKUvYyLcNEYOzYhOdQ+F0lPVS49t/TTAJaynrvdbixdbb0zMftCWcBFBOOnsU30D3jo7yzzhsOpbClI+hi4fApb0/I21VLVlol8mW0r++537cKKibaYZy1MbvCDJyUDRfmTVaAg3X1ZDROhGS8epZuDrEXXfGkKrmlXV9gA+0pRiZn3cjb3E13KE1ljCbjTUQNdBEK/pcTj9RhBnbn2qd3R7HiLdmBuctXALgRoupLNg37nhKi2rZKcY+afk6h82oPKROYhtdiaFUviZ1w4c3u8p9GJ+RzRN2Yc8eoM2QIDAQAB";

    OpenSSL_add_all_digests();
    std::cout << std::boolalpha << InappBillingVerify(receipt, signature, public_key) << std::endl;
    EVP_cleanup();

    return 0;
}

// OUTPUT :
// true