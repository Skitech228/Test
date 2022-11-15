using ANG24.Sys.Application.Types.CommunicationControllerTypes.AbstractModels;
using System.Globalization;

namespace ANG24.Sys.Application.Types.CommunicationControllerTypes
{
    public sealed class SA7100Data : TCPData
    {
        public SA7100Data() { CultureInfo.CurrentCulture = CultureInfo.InvariantCulture; }

        public override void ParseData(string input)
        {
            Message = input.Replace("&dq;", "\"")
                            .Replace("&e;", "=")
                            .Replace("&bq;", "`")
                            .Replace("&q;", "'")
                            .Replace("&c;", ",")
                            .Replace("&r;", "CR")
                            .Replace("&a;", "&")
                            .Replace("\r\n", "");
            var data = input.Split(new char[] { ' ' }, 2);
            Dictionary<string, string> Arguments = new Dictionary<string, string>();
            if (data[0] != "Hello") //Разделение аргументов на пары "ключ" "значение"
                foreach (var arg in data[1].Split(','))
                {
                    var a = arg.Split('=');
                    Arguments.Add(a[0], a[1].Replace("&dq;", "\"")
                            .Replace("&e;", "=")
                            .Replace("&bq;", "`")
                            .Replace("&q;", "'")
                            .Replace("&c;", ",")
                            .Replace("&a;", "&")
                            .Replace("\r\n", ""));
                }
            if (Arguments.TryGetValue("tag", out var tag)) Tag = int.Parse(tag);
            if (Arguments.TryGetValue("error.code", out var eCode)) ErrorCode = int.Parse(eCode);
            if (Arguments.TryGetValue("error.text", out var eText)) ErrorMessage = eText;
            switch (data[0])
            {
                case "notification":
                    RespType = ResponseType.Notification;
                    if (Arguments.TryGetValue("type", out var type)) NotyType = type;
                    if (Arguments.TryGetValue("source", out var source)) Source = source;
                    if (Arguments.TryGetValue("payload", out var payload)) Payload = payload;
                    if (Arguments.TryGetValue("extra", out var extra)) Extra = extra;
                    break;
                case "response":
                    RespType = ResponseType.Response;
                    Command = Arguments["command"] ?? null;
                    if (Command == "measure")
                    {
                        if (Arguments.TryGetValue("cx", out var cx)) Cx = float.Parse(cx);
                        if (Arguments.TryGetValue("tgx", out var tgx)) Tgx = float.Parse(tgx);
                        if (Arguments.TryGetValue("sko_cx", out var sko_cx)) Sko_cx = float.Parse(sko_cx);
                        if (Arguments.TryGetValue("sko_tgx", out var sko_tgx)) Sko_tgx = float.Parse(sko_tgx);

                        if (Arguments.TryGetValue("ux", out var ux)) Ux = (float)Math.Round(float.Parse(ux), 1);
                        if (Arguments.TryGetValue("fq", out var fq)) Fq = (float)Math.Round(float.Parse(fq), 1);

                        if (Arguments.TryGetValue("rmeas", out var rmeas)) Rmeas = float.Parse(rmeas);
                        if (Arguments.TryGetValue("sko_rmeas", out var sko_rmeas)) Sko_rmeas = float.Parse(sko_rmeas);
                    }
                    if (Command == "state") if (Arguments.TryGetValue("state", out var state)) State = state;
                    break;
                case "error": RespType = ResponseType.Error; break;
                default: RespType = ResponseType.Hello; break;
            }

            var date = DateTime.Now;
            MeasDate = date.ToString("dd.MM.yy");
            MeasTime = date.ToString("HH:mm");
        }//Rx и Ka не реализованы
        public ResponseType RespType { get; set; }
        public string NotyType { get; set; }
        public string State { get; set; }
        public string Source { get; set; }
        public string Payload { get; set; }
        public string Extra { get; set; }
        public int Tag { get; set; }

        /// <summary>
        /// Измеренная емкость в фарадах
        /// </summary>
        public float Cx { get; set; }
        /// <summary>
        /// Измеренный тангенс
        /// </summary>
        public float Tgx { get; set; }
        /// <summary>
        /// Измеренное напряжение в вольтах
        /// </summary>
        public float Ux { get; set; }
        /// <summary>
        /// Измеренная частота в герцах
        /// </summary>
        public float Fq { get; set; }
        /// <summary>
        /// Среднеквадратическое отклонение по емкости
        /// </summary>
        public float Sko_cx { get; set; }
        /// <summary>
        /// Среднеквадратическое отклонение по тангенсу
        /// </summary>
        public float Sko_tgx { get; set; }
        /// <summary>
        /// Измеренное сопротивление, Ом. Либо токен >t1, обозначающий сопротивление более 1ТОм
        /// </summary>
        public float Rmeas { get; set; }
        /// <summary>
        /// Среднеквадратическое отклонение по сопротивлению
        /// </summary>
        public float Sko_rmeas { get; set; }
        #region Дополнительная информация об объекте измерений и самом измерении
        public string ObjectName { get; set; }
        public string ObjectNote { get; set; }
        public string FactoryNumber { get; set; }
        public string ReleaseDate { get; set; }
        public string MeasDate { get; set; }
        public string MeasTime { get; set; }
        public string Scheme { get; set; } //Инверт\прям\В/В коммутатор не подключен
        public string PhaseChanging { get; set; } //Вкл/Выкл
        public string Avg { get; set; } //Накопление
        public string SubMeas { get; set; } //Поддиапазон измерения?
        #endregion
        private string Screening(string s)
        {
            return s.Replace("&e;", "=")
                    .Replace("&dq;", "\"")
                    .Replace("&bq;", "`")
                    .Replace("&q;", "'")
                    .Replace("&c;", ",")
                    .Replace("&n;", "\n")
                    .Replace("&a;", "&");
            //.Replace("&r;", "CR")
        }
    }
    public enum ResponseType
    {
        Response,
        Notification,
        Error,
        Hello
    }
}
