## Authorization
Every request to API must contain authentication information to establish the identity of contragent making the request. This is done by putting the headers in canonical format, then signing the headers using Secret Key.


Signature is send via standart Authorization HTTP header using following format:

    "Authorization: UNIHMAC " + APPLICATION_ID + ":" + base64(hmac-sha256(APPLICATION_SECRET,
																	  to-upper(VERB) + "\n"
																	  + CONTENT-MD5 + "\n"
																	  + DATE + "\n"
																	  + to-lower(PATHANDQUERY) ))

| Parameter      | Description |
|--------------------------------------------------------------------|
| VERB           | upper case http method name                         |
| CONTENT-MD5    | md5 hash of request content, empty for GET requests |
| DATE           | header in RFC1123                                   |
| PATH AND QUERY | resource path and query in lowercase                |



    {{C#}} 
    static class HttpRequestMessageExtensions
    {
        public static string ToStringRepresentation(this HttpRequestMessage request)
        {
            return new StringBuilder()
                .Append(request.Method.ToString().ToUpperInvariant())
                .Append("\n")
                .Append(request.Content == null || request.Content.Headers == null || request.Content.Headers.ContentMD5 == null 
							? "" 
							: Convert.ToBase64String(request.Content.Headers.ContentMD5))
                .Append("\n")
                .Append(request.Headers.Date==null 
							? "" 
							: request.Headers.Date.Value.ToString("R", CultureInfo.InvariantCulture))
                .Append("\n")
                .Append(request.RequestUri.PathAndQuery.ToLowerInvariant())
                .ToString();
        }
    }
##
	{{Java}} 
    String generateAuthHttpHeader(String appId, String appSecret, String verb, String contentMd5, String date, String pathAndQuery) {
        byte [] secret = Base64.decode(appSecret);
        String data = verb.toUpperCase() + "\n"
                             + contentMd5 + "\n"
                             + date + "\n"
                             + pathAndQuery.toLowerCase();
        
        return  data;
    }

	public String encodeHmacSHA256(byte [] key, byte [] data) {
        try {
            String algo = "HmacSHA256";
            javax.crypto.spec.SecretKeySpec secret_key = new javax.crypto.spec.SecretKeySpec(key, algo);
            
            javax.crypto.Mac sha256_HMAC = javax.crypto.Mac.getInstance(algo);
            sha256_HMAC.init(secret_key);
            
            byte [] result = sha256_HMAC.doFinal(data);
            return Base64.encode(result);
        } 
        catch (java.security.NoSuchAlgorithmException e) {} 
        catch (java.security.InvalidKeyException e) {}
        
        return null;
    }

    public String getContentMD5(String contentStr) throws DeclineException {
        try {
            java.security.MessageDigest md = java.security.MessageDigest.getInstance("MD5");
            byte [] contentMD5Byte = md.digest(contentStr.getBytes(java.nio.charset.StandardCharsets.UTF_8));
            return Base64.encode(contentMD5Byte);
        } catch (java.security.NoSuchAlgorithmException e) {
             .....
        }
    }
