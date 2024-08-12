﻿using System.Text;
using System.Text.Json;


namespace ChatCommon.Extentions
{
    public static class MessageExtentions
    {
        public static Message? ToMessage(this byte[] data)  =>
            JsonSerializer.Deserialize<Message?>(Encoding.UTF8.GetString(data));
        
        public static byte[]? ToBytes(this Message message) =>
            Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
    }
}
