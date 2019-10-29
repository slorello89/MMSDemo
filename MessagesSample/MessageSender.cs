using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Claims;

namespace MessagesSample
{
    public class MessageSender
    {

        const string APP_ID = "APP_ID";
        const string PRIVATE_KEY_PATH = @"PRIVATE_KEY_PATH";
        private static string BuildJwt()
        {
            var appId = APP_ID;
            var priavteKeyPath = PRIVATE_KEY_PATH;
            string privateKey = "";
            using (var reader = File.OpenText(priavteKeyPath)) // file containing RSA PKCS1 private key
                privateKey = reader.ReadToEnd();

            var jwt = TokenGenerator.GenerateToken(GetClaimsList(appId), privateKey);
            return jwt;
        }

        public static void SendMessage(string imageUrl, string fromNumber, string toNumber)
        {
            const string MESSAGING_URL = @"https://api.nexmo.com/v0.1/messages";
            try
            {
                var jwt = BuildJwt();

                var requestObject = new MessageRequest()
                {
                    to = new MessageRequest.To()
                    {
                        number = toNumber,
                        type = "mms"
                    },
                    from = new MessageRequest.From()
                    {
                        number = fromNumber,
                        type = "mms"
                    },
                    message = new MessageRequest.Message()
                    {
                        content = new MessageRequest.Message.Content()
                        {
                            type = "image",
                            image = new MessageRequest.Message.Content.Image
                            {
                                url = imageUrl
                            }
                        }                        
                    }
                };
                var requestPayload = JsonConvert.SerializeObject(requestObject, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore });
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(MESSAGING_URL);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Accept = "application/json";
                httpWebRequest.Method = "POST";
                httpWebRequest.PreAuthenticate = true;
                httpWebRequest.Headers.Add("Authorization", "Bearer " + jwt);
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(requestPayload);
                }
                using (var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse())
                {
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();
                        Console.WriteLine(result);
                        Debug.WriteLine(result);
                        Console.WriteLine("Message Sent");
                    }
                }
                
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }

        private static List<Claim> GetClaimsList(string appId)
        {
            const int SECONDS_EXPIRY = 3600;
            var t = DateTime.UtcNow - new DateTime(1970, 1, 1);
            var iat = new Claim("iat", ((Int32)t.TotalSeconds).ToString(), ClaimValueTypes.Integer32); // Unix Timestamp for right now
            var application_id = new Claim("application_id", appId); // Current app ID
            var exp = new Claim("exp", ((Int32)(t.TotalSeconds + SECONDS_EXPIRY)).ToString(), ClaimValueTypes.Integer32); // Unix timestamp for when the token expires
            var jti = new Claim("jti", Guid.NewGuid().ToString()); // Unique Token ID
            var claims = new List<Claim>() { iat, application_id, exp, jti };

            return claims;
        }
    }
}
