﻿using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Acquiredapisdkdotnet.Lib.AQPay
{
    public class AQPayCommon
    {
        public string TrimString(string value){
            string ret = null;
            if(value != null){
                ret = value;

                if(ret.Length == 0){
                    ret = null;
                }
            }
            return ret;
        }

        public string Now(){
            return DateTime.Now.ToString("yyyyMMddHmmss");
        }

        public string Sha256hash(string key){
            
            byte[] tmpByte = Encoding.UTF8.GetBytes(key);
            SHA256 Sha256 = new SHA256Managed();
            byte[] by = Sha256.ComputeHash(tmpByte);
            Sha256.Clear();
            string result = BitConverter.ToString(by).Replace("-", "").ToLower(); //64

            return result;

        }

        public string RequestHash(Hashtable param, string secret){

            string str = "";

            string transaction_type = param["transaction_type"].ToString();
            string[] transaction_type_1 = { "AUTH_ONLY", "AUTH_CAPTURE", "CREDIT" };
            string[] transaction_type_2 = { "CAPTURE", "VOID", "REFUND", "SUBSCRIPTION_MANAGE" };

            if(Array.IndexOf(transaction_type_1, transaction_type) != -1){
                str = param["timestamp"].ToString() + param["transaction_type"] + param["company_id"].ToString() + param["merchant_order_id"];
            }else if(Array.IndexOf(transaction_type_2, transaction_type) != -1){
                str = param["timestamp"].ToString() + param["transaction_type"] + param["company_id"].ToString() + param["original_transaction_id"];
            }

            string secstr = str + secret;

            return Sha256hash(secstr);

        }


        public string ReponseHash(JObject param, string secret){

            string str = "";

            str = param["timestamp"].ToString() + param["transaction_type"].ToString() + param["company_id"].ToString() + param["transaction_id"] + param["response_code"];

            String secstr = str + secret;

            return Sha256hash(secstr);

        }

        public string GenerateWebhookHash(JObject data){
            string hash_tmp = data["id"].ToString() + data["timestamp"].ToString() + data["company_id"].ToString() + data["event"].ToString();
            string hash_tmp2 = Sha256hash(hash_tmp);
            hash_tmp2 = hash_tmp2 + data["company_hash_code"];
            string hash = Sha256hash(hash_tmp2);

            return hash;
        }

        public string Http_request(string url, string data, int connectTimeout){

            return Http_request(url, data, connectTimeout, "");

        }

        public string Http_request(string url, string data, int connectTimeout, string type){


            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            byte[] bdata = Encoding.UTF8.GetBytes(data);

            request.Method = "POST";
            switch(type){
                case "JSON":request.ContentType = "application/json;charset=UTF-8";break;
                default:request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";break;
            }
            request.ContentLength = bdata.Length;
            request.Timeout = connectTimeout*1000;

            Stream reqStream = request.GetRequestStream();
            reqStream.Write(bdata, 0, bdata.Length);
            reqStream.Close();

            WebResponse response = request.GetResponse();
            string result = "";
            using (Stream responseStm = response.GetResponseStream())
            {
                StreamReader redStm = new StreamReader(responseStm, Encoding.UTF8);
                result = redStm.ReadToEnd();
            }

            return result;

        }

    }
}
