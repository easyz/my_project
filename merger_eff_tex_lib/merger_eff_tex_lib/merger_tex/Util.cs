﻿using System;
using System.IO;
using System.Text;

public static class Util {
    public static string GetMD5HashFromFile(string fileName) {
        try {
            FileStream file = new FileStream(fileName, FileMode.Open);
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(file);
            file.Close();

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++) {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        } catch (Exception ex) {
            throw new Exception("GetMD5HashFromFile() fail, error:" +ex.Message);
        }
    }
}