/*
    JavaScript Quick NTLM Hash Computation
    Just Cause. 
    I know. Its not efficient.
     
*/
//Example Server Implmentation Here:
//https://www.tobtu.com/lmntlm.php
 
var map = new Array
    ( 0,   1,   2,   3,   4,   5,   6,   7,   8,   9,  10,  11,  12,  13,  14,  15,
     16,  17,  18,  19,  20,  21,  22,  23,  24,  25,  26,  27,  28,  29,  30,  31,
     32,  33,  34,  35,  36,  37,  38,  39,  40,  41,  42,  43,  44,  45,  46,  47,
     48,  49,  50,  51,  52,  53,  54,  55,  56,  57,  58,  59,  60,  61,  62,  63,
     64,  65,  66,  67,  68,  69,  70,  71,  72,  73,  74,  75,  76,  77,  78,  79,
     80,  81,  82,  83,  84,  85,  86,  87,  88,  89,  90,  91,  92,  93,  94,  95,
     96,  65,  66,  67,  68,  69,  70,  71,  72,  73,  74,  75,  76,  77,  78,  79,
     80,  81,  82,  83,  84,  85,  86,  87,  88,  89,  90, 123, 124, 125, 126, 127,
     -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,
     -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,
    255, 173, 155, 156,  15, 157, 221,  21,  34,  67, 166, 174, 170,  45,  82,  95,
    248, 241, 253,  51,  39, 230,  20, 250,  44,  49, 167, 175, 172, 171,  95, 168,
     65,  65,  65,  65, 142, 143, 146, 128,  69, 144,  69,  69,  73,  73,  73,  73,
     68, 165,  79,  79,  79,  79, 153,  88,  79,  85,  85,  85, 154,  89,  95, 225,
     65,  65,  65,  65, 142, 143, 146, 128,  69, 144,  69,  69,  73,  73,  73,  73,
     68, 165,  79,  79,  79,  79, 153, 246,  79,  85,  85,  85, 154,  89,  95,  89);
var map2 = new Array
    (44, 159,  44,  46,  43, 216,  94,  37,  83,  60,  79,  90,  96,
     39,  34,  34,   7,  45,  45, 126,  84,  83,  62,  79,  90,  89);
var mapMap2 = new Array
    (0x201A, 0x0192, 0x201E, 0x2026, 0x2020, 0x2021, 0x02C6, 0x2030, 0x0160, 0x2039, 0x0152, 0x017D, 0x2018,
     0x2019, 0x201C, 0x201D, 0x2022, 0x2013, 0x2014, 0x02DC, 0x2122, 0x0161, 0x203A, 0x0153, 0x017E, 0x0178);
 
 
function calculateNTLMHashes(str)
{
    var splitvar = "\n";
    if (str.indexOf("\r\n") != -1)
    {
        splitvar = "\r\n";
    }
    else if (str.indexOf("\r") != -1)
    {
        splitvar = "\r";
    }
    var arr = str.split(splitvar);
    var aNTLMHashes = new Array(arr.length);
     
    for (var a = 0; a < arr.length; a++)
    {
        aNTLMHashes[a] = hex_md4(arr[a]);
         
    }
     
    return aNTLMHashes;
 
}
 
/*
 * A JavaScript implementation of the RSA Data Security, Inc. MD4 Message
 * Digest Algorithm, as defined in RFC 1320.
 * Version 2.1 Copyright (C) Jerrad Pierce, Paul Johnston 1999 - 2002.
 * Other contributors: Greg Holt, Andrew Kepert, Ydnar, Lostinet
 * Distributed under the BSD License
 * See http://pajhome.org.uk/crypt/md5 for more info.
 */
  
// This is not the original I got rid of a few functions that were not being used.
 
/*
 * Configurable variables. You may need to tweak these to be compatible with
 * the server-side, but the defaults work in most cases.
 */
var hexcase = 1;  /* hex output format. 0 - lowercase; 1 - uppercase        */
var chrsz   = 16;  /* bits per input character. 8 - ASCII; 16 - Unicode      */
 
/*
 * These are the functions you'll usually want to call
 */
function hex_md4(s){ return binl2hex(core_md4(str2binl(s), s.length * chrsz));}
 
/*
 * Calculate the MD4 of an array of little-endian words, and a bit length
 */
function core_md4(x, len)
{
  /* append padding */
  x[len >> 5] |= 0x80 << (len % 32);
  x[(((len + 64) >>> 9) << 4) + 14] = len;
   
  var a =  1732584193;
  var b = -271733879;
  var c = -1732584194;
  var d =  271733878;
 
  for(var i = 0; i < x.length; i += 16)
  {
    var olda = a;
    var oldb = b;
    var oldc = c;
    var oldd = d;
 
    a = md4_ff(a, b, c, d, x[i+ 0], 3 );
    d = md4_ff(d, a, b, c, x[i+ 1], 7 );
    c = md4_ff(c, d, a, b, x[i+ 2], 11);
    b = md4_ff(b, c, d, a, x[i+ 3], 19);
    a = md4_ff(a, b, c, d, x[i+ 4], 3 );
    d = md4_ff(d, a, b, c, x[i+ 5], 7 );
    c = md4_ff(c, d, a, b, x[i+ 6], 11);
    b = md4_ff(b, c, d, a, x[i+ 7], 19);
    a = md4_ff(a, b, c, d, x[i+ 8], 3 );
    d = md4_ff(d, a, b, c, x[i+ 9], 7 );
    c = md4_ff(c, d, a, b, x[i+10], 11);
    b = md4_ff(b, c, d, a, x[i+11], 19);
    a = md4_ff(a, b, c, d, x[i+12], 3 );
    d = md4_ff(d, a, b, c, x[i+13], 7 );
    c = md4_ff(c, d, a, b, x[i+14], 11);
    b = md4_ff(b, c, d, a, x[i+15], 19);
    a = md4_gg(a, b, c, d, x[i+ 0], 3 );
    d = md4_gg(d, a, b, c, x[i+ 4], 5 );
    c = md4_gg(c, d, a, b, x[i+ 8], 9 );
    b = md4_gg(b, c, d, a, x[i+12], 13);
    a = md4_gg(a, b, c, d, x[i+ 1], 3 );
    d = md4_gg(d, a, b, c, x[i+ 5], 5 );
    c = md4_gg(c, d, a, b, x[i+ 9], 9 );
    b = md4_gg(b, c, d, a, x[i+13], 13);
    a = md4_gg(a, b, c, d, x[i+ 2], 3 );
    d = md4_gg(d, a, b, c, x[i+ 6], 5 );
    c = md4_gg(c, d, a, b, x[i+10], 9 );
    b = md4_gg(b, c, d, a, x[i+14], 13);
    a = md4_gg(a, b, c, d, x[i+ 3], 3 );
    d = md4_gg(d, a, b, c, x[i+ 7], 5 );
    c = md4_gg(c, d, a, b, x[i+11], 9 );
    b = md4_gg(b, c, d, a, x[i+15], 13);
    a = md4_hh(a, b, c, d, x[i+ 0], 3 );
    d = md4_hh(d, a, b, c, x[i+ 8], 9 );
    c = md4_hh(c, d, a, b, x[i+ 4], 11);
    b = md4_hh(b, c, d, a, x[i+12], 15);
    a = md4_hh(a, b, c, d, x[i+ 2], 3 );
    d = md4_hh(d, a, b, c, x[i+10], 9 );
    c = md4_hh(c, d, a, b, x[i+ 6], 11);
    b = md4_hh(b, c, d, a, x[i+14], 15);
    a = md4_hh(a, b, c, d, x[i+ 1], 3 );
    d = md4_hh(d, a, b, c, x[i+ 9], 9 );
    c = md4_hh(c, d, a, b, x[i+ 5], 11);
    b = md4_hh(b, c, d, a, x[i+13], 15);
    a = md4_hh(a, b, c, d, x[i+ 3], 3 );
    d = md4_hh(d, a, b, c, x[i+11], 9 );
    c = md4_hh(c, d, a, b, x[i+ 7], 11);
    b = md4_hh(b, c, d, a, x[i+15], 15);
    a = safe_add(a, olda);
    b = safe_add(b, oldb);
    c = safe_add(c, oldc);
    d = safe_add(d, oldd);
  }
  return Array(a, b, c, d);
 
}
 
/*
 * These functions implement the basic operation for each round of the
 * algorithm.
 */
function md4_cmn(q, a, b, x, s, t)
{
  return safe_add(rol(safe_add(safe_add(a, q), safe_add(x, t)), s), b);
}
function md4_ff(a, b, c, d, x, s)
{
  return md4_cmn((b & c) | ((~b) & d), a, 0, x, s, 0);
}
function md4_gg(a, b, c, d, x, s)
{
  return md4_cmn((b & c) | (b & d) | (c & d), a, 0, x, s, 1518500249);
}
function md4_hh(a, b, c, d, x, s)
{
  return md4_cmn(b ^ c ^ d, a, 0, x, s, 1859775393);
}
 
/*
 * Add integers, wrapping at 2^32. This uses 16-bit operations internally
 * to work around bugs in some JS interpreters.
 */
function safe_add(x, y)
{
  var lsw = (x & 0xFFFF) + (y & 0xFFFF);
  var msw = (x >> 16) + (y >> 16) + (lsw >> 16);
  return (msw << 16) | (lsw & 0xFFFF);
}
 
/*
 * Bitwise rotate a 32-bit number to the left.
 */
function rol(num, cnt)
{
  return (num << cnt) | (num >>> (32 - cnt));
}
 
/*
 * Convert a string to an array of little-endian words
 * If chrsz is ASCII, characters >255 have their hi-byte silently ignored.
 */
function str2binl(str)
{
  var bin = Array();
  var mask = (1 << chrsz) - 1;
  for(var i = 0; i < str.length * chrsz; i += chrsz)
    bin[i>>5] |= (str.charCodeAt(i / chrsz) & mask) << (i%32);
  return bin;
}
 
/*
 * Convert an array of little-endian words to a hex string.
 */
function binl2hex(binarray)
{
  var hex_tab = hexcase ? "0123456789ABCDEF" : "0123456789abcdef";
  var str = "";
  for(var i = 0; i < binarray.length * 4; i++)
  {
    str += hex_tab.charAt((binarray[i>>2] >> ((i%4)*8+4)) & 0xF) +
           hex_tab.charAt((binarray[i>>2] >> ((i%4)*8  )) & 0xF);
  }
  return str;
}
 
function int2hex(num)
{
  var hex_tab = "0123456789abcdef";
  var str = "";
    str += hex_tab.charAt(((num & 0x70000000) >> (28)) + (num < 0 ? 0x8: 0x0)) +
           hex_tab.charAt((num & 0x0f000000) >> (24)) +
           hex_tab.charAt((num & 0x00f00000) >> (20)) +
           hex_tab.charAt((num & 0x000f0000) >> (16)) +
           hex_tab.charAt((num & 0x0000f000) >> (12)) +
           hex_tab.charAt((num & 0x00000f00) >> (8)) +
           hex_tab.charAt((num & 0x000000f0) >> (4)) +
           hex_tab.charAt((num & 0x0000000f) >> (0));
  return str;
}
 
var strMyPass;
WScript.StdOut.Write("Enter Password>");
WScript.StdIn.Read(0);
strMyPass = WScript.StdIn.ReadLine();
var NTLMHash = calculateNTLMHashes(strMyPass);
WScript.StdOut.Write(NTLMHash);