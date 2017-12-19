# Example: ls 'C:\Windows\System32\*' -Include '*.dll' | Get-AuthenticodeSignature | Select -ExpandProperty SignerCertificate | Get-TBSHash
 
filter Get-TBSHash {
    [OutputType([String])]
    param (
        [Parameter(Mandatory, ValueFromPipeline)]
        [Security.Cryptography.X509Certificates.X509Certificate2]
        $Certificate
    )
 
    Add-Type -TypeDefinition @'
    using System;
    using System.Runtime.InteropServices;
 
    namespace Crypto {
        public struct CRYPT_DATA_BLOB
        {
            public uint cbData;
            public IntPtr pbData;
        }
 
        public struct CRYPT_OBJID_BLOB
        {
            public uint cbData;
            public IntPtr pbData;
        }
 
        public struct CRYPT_ALGORITHM_IDENTIFIER
        {
            public string pszObjId;
            public CRYPT_OBJID_BLOB Parameters;
        }
 
        public struct CRYPT_BIT_BLOB
        {
            public uint cbData;
            public IntPtr pbData;
            public uint cUnusedBits;
        }
 
        public struct CERT_SIGNED_CONTENT_INFO
        {
            public CRYPT_DATA_BLOB ToBeSigned;
            public CRYPT_ALGORITHM_IDENTIFIER SignatureAlgorithm;
            public CRYPT_BIT_BLOB Signature;
        }
 
        public class NativeMethods {
            [DllImport("crypt32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern bool CryptDecodeObject(uint dwCertEncodingType, IntPtr lpszStructType, [In] byte[] pbEncoded, uint cbEncoded, uint dwFlags, [Out] IntPtr pvStructInto, ref uint pcbStructInfo);
        }
    }
'@
 
 
    $HashOIDs = @{
        '1.2.840.113549.1.1.4' = 'MD5'
        '1.2.840.113549.1.1.5' = 'SHA1'
        '1.3.14.3.2.29' = 'SHA1'
        '1.2.840.113549.1.1.11' = 'SHA256'
        '1.2.840.113549.1.1.12' = 'SHA384'
        '1.2.840.113549.1.1.13' = 'SHA512'
    }
 
    $CertBytes = $Certificate.RawData
 
    $X509_PKCS7_ENCODING = 65537
    $X509_CERT = 1
    $CRYPT_DECODE_TO_BE_SIGNED_FLAG = 2
    $ErrorMoreData = 234
 
    $TBSData = [IntPtr]::Zero
    [UInt32] $TBSDataSize = 0
 
    $Success = [Crypto.NativeMethods]::CryptDecodeObject(
        $X509_PKCS7_ENCODING,
        [IntPtr] $X509_CERT,
        $CertBytes,
        $CertBytes.Length,
        $CRYPT_DECODE_TO_BE_SIGNED_FLAG,
        $TBSData,
        [ref] $TBSDataSize
    ); $LastError = [Runtime.InteropServices.Marshal]::GetLastWin32Error()
 
    if((-not $Success) -and ($LastError -ne $ErrorMoreData)) 
    {
        throw "[CryptDecodeObject] Error: $(([ComponentModel.Win32Exception] $LastError).Message)"
    }
 
    $TBSData = [Runtime.InteropServices.Marshal]::AllocHGlobal($TBSDataSize)
 
    $Success = [Crypto.NativeMethods]::CryptDecodeObject(
        $X509_PKCS7_ENCODING,
        [IntPtr] $X509_CERT,
        $CertBytes,
        $CertBytes.Length,
        $CRYPT_DECODE_TO_BE_SIGNED_FLAG,
        $TBSData,
        [ref] $TBSDataSize
    ); $LastError = [Runtime.InteropServices.Marshal]::GetLastWin32Error()
 
    if((-not $Success)) 
    {
        throw "[CryptDecodeObject] Error: $(([ComponentModel.Win32Exception] $LastError).Message)"
    }
 
    $SignedContentInfo = [System.Runtime.InteropServices.Marshal]::PtrToStructure($TBSData, [Type][Crypto.CERT_SIGNED_CONTENT_INFO])
 
    $TBSBytes = New-Object Byte[]($SignedContentInfo.ToBeSigned.cbData)
    [Runtime.InteropServices.Marshal]::Copy($SignedContentInfo.ToBeSigned.pbData, $TBSBytes, 0, $TBSBytes.Length)
 
    [Runtime.InteropServices.Marshal]::FreeHGlobal($TBSData)
 
    $HashAlgorithmStr = $HashOIDs[$SignedContentInfo.SignatureAlgorithm.pszObjId]
 
    if (-not $HashAlgorithmStr) { throw 'Hash algorithm is not supported or it could not be retrieved.' }
 
    $HashAlgorithm = [Security.Cryptography.HashAlgorithm]::Create($HashAlgorithmStr)
 
    $TBSHashBytes = $HashAlgorithm.ComputeHash($TBSBytes)
 
    ($TBSHashBytes | % { $_.ToString('X2') }) -join ''
}