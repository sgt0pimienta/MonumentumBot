using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure; // Namespace for CloudConfigurationManager
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Table; // Namespace for Table storage types

namespace MonumentumBot
{
    class ScheduledMemo : TableEntity
    {
        // El formato para cada mensaje de usuario es "/minuto/hora/día/mes/año/mensaje"
        // Datetime = YMDHMS
        public DateTime ScheduledTime { get; set; }
        public string ScheduledMessage { get; set; }
        public bool MemoValidity { get; set; }
        public string MemoID { get; set; }
        public string ChatId { get; set; }
        public bool MemoCompleted { get; set; }

        public ScheduledMemo(DateTime time, string message, bool validity, string id, string senderChatId)
        {
            ScheduledTime = time;
            ScheduledMessage = message;
            MemoValidity = validity;
            MemoID = id;
            ChatId = senderChatId;
            MemoCompleted = false;
            this.PartitionKey = "memo";
            this.RowKey = id.ToString();
        }

        public ScheduledMemo()
        {

        }
    }
}
