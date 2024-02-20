using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace WkyFast.Service.Model
{

    public record Aria2Event
    {
        protected Aria2Event()
        {
        }

        /// <summary>
        /// 事件类型
        /// </summary>
        [JsonProperty("type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public virtual Events Type { get; set; }


    }

    public record LoginResultEvent : Aria2Event
    {
        public bool IsSuccess { get; set; }

        public LoginResultEvent(bool isSuccess) => (IsSuccess) = (isSuccess);

        public override Events Type { get; set; } = Events.LoginResultEvent;
    }


    public enum Events
    {
        /// <summary>
        /// 登录成功结果
        /// </summary>
        [Description("LoginResultEvent")]
        [EnumMember(Value = "LoginResultEvent")]
        LoginResultEvent,


        /// <summary>
        /// 设备信息更新结果
        /// </summary>
        [Description("UpdateDeviceResultEvent")]
        [EnumMember(Value = "UpdateDeviceResultEvent")]
        UpdateDeviceResultEvent,


        /// <summary>
        /// 下载成功
        /// </summary>
        [Description("DownloadSuccessEvent")]
        [EnumMember(Value = "DownloadSuccessEvent")]
        DownloadSuccessEvent,


        /// <summary>
        /// 任务列表更新
        /// </summary>
        [Description("UpdateTaskListEvent")]
        [EnumMember(Value = "UpdateTaskListEvent")]
        UpdateTaskListEvent,


        /// <summary>
        /// UpdateUsbInfo
        /// </summary>
        [Description("UpdateUsbInfoEvent")]
        [EnumMember(Value = "UpdateUsbInfoEvent")]
        UpdateUsbInfoEvent,



    }
}
