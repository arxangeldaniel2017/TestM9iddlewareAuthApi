using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using static TestM9iddlewareAuthApi.Middleware.GisApiHelper;

namespace TestM9iddlewareAuthApi.Middleware
{
    public interface IGisApiHelper
    {
        public GisResult<GisModel, TFeatures> GetFeatures<TFeatures>(string LayerName, string SubData, System.Collections.Specialized.NameValueCollection ParmQuery);
        public GisResult<GisModel, TFeatures> GetRelatedFeatures<TFeatures>(string LayerName, string SubData, System.Collections.Specialized.NameValueCollection ParmQuery);
        public T GetFeatureAttachments<T>(string LayerName, string SubData, System.Collections.Specialized.NameValueCollection ParmQuery);
        public InsertRetModel InsertFeature(string LayerName, string SubData, string UpdateValues);
        public InsertRetModel UpdateFeature(string LayerName, string SubData, string UpdateValues);
        public DeleteRetModel DeleteFeature(string LayerName, string SubData, string objectId, string whrStr);
        public string GetToken();
    }

    public class GisApiHelper : IGisApiHelper
    {
        IAppSettings appSettings;
        IHostEnvironment env;
        string GisEnvPrefix = "";
        public GisApiHelper(IAppSettings appSettings, IHostEnvironment env)
        {
            this.appSettings = appSettings;
            this.env = env;
            if (env.IsDevelopment() || env.IsStaging()) this.GisEnvPrefix = "Test_";
            //this.GisEnvPrefix = "Test_";
        }

        public GisResult<GisModel, TFeatures> GetFeatures<TFeatures>(string LayerName, string SubData, System.Collections.Specialized.NameValueCollection ParmQuery)
        {
            //var T1Type = Type.GetType(typeof(List<TFeatures>).AssemblyQualifiedName);
            GisResult<GisModel, TFeatures> result = new GisResult<GisModel, TFeatures>();

            if (ParmQuery.Get("outFields") == null) ParmQuery.Add("outFields", "*");
            if (ParmQuery.Get("returnGeometry") == null) ParmQuery.Add("returnGeometry", "false");
            if (ParmQuery.Get("token") == null) ParmQuery.Add("token", this.GetToken());
            if (ParmQuery.Get("f") == null) ParmQuery.Add("f", "json");
            ParmQuery.Add("timeReferenceUnknownClient", "false");
            SubData = SubData == "" ? "0" : this.GisEnvPrefix + SubData;
            using (WebClient clientGis = new WebClient())
            {
                byte[] responsebytesGis = clientGis.UploadValues(this.appSettings.GisApiEsriUrl + "/" + this.GisEnvPrefix + LayerName.ToString() + "/FeatureServer/" + SubData.ToString() + "/query", "POST", ParmQuery);
                string responsebodyGis = Encoding.UTF8.GetString(responsebytesGis);

                result.GisAttributes = JsonConvert.DeserializeObject<GisModel>(responsebodyGis);
                result.Features = (List<TFeatures>)Activator.CreateInstance(typeof(List<TFeatures>));
                //var azzz = result.GisModel.features.First().attributes;
                //JObject FeatureObjects = JObject.Parse(responsebodyGis);
                //var az= FeatureObjects["features"].ToObject(typeof(List<TFeatures>));
                //result.FeatureModel = (List<TFeatures>)FeatureObjects["features"].ToObject(typeof(List<TFeatures>));

                if (result.GisAttributes.features != null)
                {
                    foreach (var item in result.GisAttributes.features)
                    {
                        ((List<TFeatures>)result.Features).Add(((JObject)item.attributes).ToObject<TFeatures>());
                    }
                    //var ppp = result.Features.First().GetType().GetProperties();
                    //foreach (var p in ppp)
                    //{
                    //    if (p.CustomAttributes.Count()>0 && p.CustomAttributes.First().ToString().Contains("DomainConverter"))
                    //    {
                    //        foreach (var Feature in result.Features)
                    //        {
                    //            var Propery = Feature.GetType().GetProperty(p.Name).GetValue(Feature);
                    //            if (Propery.GetType().GetProperty(p.Name).GetValue(Propery) != null)
                    //            {
                    //                item.Status.StatusName = StatusDomain.Where(f => f.code == item.Status.Status.ToString()).First().name;
                    //            }
                    //        }


                    //    }
                    //}

                }
            }
            return result;
        }
        public GisResult<GisModel, TFeatures> GetRelatedFeatures<TFeatures>(string LayerName, string SubData, System.Collections.Specialized.NameValueCollection ParmQuery)
        {
            GisResult<GisModel, TFeatures> result = new GisResult<GisModel, TFeatures>();

            if (ParmQuery.Get("outFields") == null) ParmQuery.Add("outFields", "*");
            if (ParmQuery.Get("returnGeometry") == null) ParmQuery.Add("returnGeometry", "false");
            if (ParmQuery.Get("token") == null) ParmQuery.Add("token", this.GetToken());
            if (ParmQuery.Get("f") == null) ParmQuery.Add("f", "json");

            SubData = SubData == "" ? "0" : this.GisEnvPrefix + SubData;
            using (WebClient clientGis = new WebClient())
            {
                byte[] responsebytesGis = clientGis.UploadValues(this.appSettings.GisApiEsriUrl + "/" + this.GisEnvPrefix + LayerName.ToString() + "/FeatureServer/" + SubData.ToString() + "/queryRelatedRecords", "POST", ParmQuery);
                string responsebodyGis = Encoding.UTF8.GetString(responsebytesGis);

                result.GisAttributes = JsonConvert.DeserializeObject<GisModel>(responsebodyGis);
                result.Features = (List<TFeatures>)Activator.CreateInstance(typeof(List<TFeatures>));

                if (result.GisAttributes.relatedRecordGroups != null)
                {
                    foreach (var relatedRecord in result.GisAttributes.relatedRecordGroups)
                    {
                        foreach (var Record in relatedRecord.relatedRecords)
                        {
                            ((List<TFeatures>)result.Features).Add((((JObject)Record.attributes)).ToObject<TFeatures>());
                        }
                    }
                }
            }
            return result;
        }

        public T GetFeatureAttachments<T>(string LayerName, string SubData, System.Collections.Specialized.NameValueCollection ParmQuery)
        {
            T result = default(T);

            SubData = SubData == "" ? "0" : this.GisEnvPrefix + SubData;

            if (ParmQuery.Get("returnGeometry") == null) ParmQuery.Add("returnGeometry", "false");
            if (ParmQuery.Get("token") == null) ParmQuery.Add("token", this.GetToken());
            if (ParmQuery.Get("f") == null) ParmQuery.Add("f", "json");

            using (WebClient clientGis = new WebClient())
            {
                byte[] responsebytesGis = clientGis.UploadValues(this.appSettings.GisApiEsriUrl + "/" + this.GisEnvPrefix + LayerName.ToString() + "/FeatureServer/" + SubData.ToString() + "/queryAttachments", "POST", ParmQuery);
                string responsebodyGis = Encoding.UTF8.GetString(responsebytesGis);
                result = JsonConvert.DeserializeObject<T>(responsebodyGis);
            }

            return result;
        }

        public InsertRetModel InsertFeature(string LayerName, string SubData, string InsertValues)
        {
            InsertRetModel InsertRet = null;
            string val = "";

            SubData = SubData == "" ? "0" : this.GisEnvPrefix + SubData;
            using (WebClient client = new WebClient())
            {
                var reqparm = new System.Collections.Specialized.NameValueCollection
                {
                    {"features", InsertValues},
                    {"token", this.GetToken()},
                    {"f", "json"}
                };

                byte[] responsebytes = client.UploadValues(this.appSettings.GisApiEsriUrl + "/" + this.GisEnvPrefix + LayerName.ToString() + "/FeatureServer/" + SubData.ToString() + "/addFeatures", "POST", reqparm);
                string responsebody = Encoding.UTF8.GetString(responsebytes);
                InsertRet = JsonConvert.DeserializeObject<InsertRetModel>(responsebody);
            }
            return InsertRet;
        }

        public InsertRetModel UpdateFeature(string LayerName, string SubData, string UpdateValues)
        {
            InsertRetModel InsertRet = null;
            //var UpdateValuesDictionary = UpdateValues.AllKeys.ToDictionary(x => x, x => UpdateValues[x]);
            //var UpdateValuesJson = JsonConvert.SerializeObject(UpdateValuesDictionary);

            SubData = SubData == "" ? "0" : this.GisEnvPrefix + SubData;
            using (WebClient client = new WebClient())
            {
                var reqparm = new System.Collections.Specialized.NameValueCollection
                {
                    {"features", UpdateValues},
                    {"token", this.GetToken()},
                    {"f", "json"}
                };

                byte[] responsebytes = client.UploadValues(this.appSettings.GisApiEsriUrl + "/" + this.GisEnvPrefix + LayerName.ToString() + "/FeatureServer/" + SubData.ToString() + "/updateFeatures", "POST", reqparm);
                string responsebody = Encoding.UTF8.GetString(responsebytes);
                InsertRet = JsonConvert.DeserializeObject<InsertRetModel>(responsebody);
            }
            return InsertRet;
        }
        public DeleteRetModel DeleteFeature(string LayerName, string SubData, string objectId, string whrStr)
        {
            DeleteRetModel DeleteRet = null;
            //var UpdateValuesDictionary = UpdateValues.AllKeys.ToDictionary(x => x, x => UpdateValues[x]);
            //var UpdateValuesJson = JsonConvert.SerializeObject(UpdateValuesDictionary);

            SubData = SubData == "" ? "0" : this.GisEnvPrefix + SubData;
            using (WebClient client = new WebClient())
            {
                var reqparm = new System.Collections.Specialized.NameValueCollection
                {
                    {"objectIds", objectId},
                    {"where", whrStr},
                    {"token", this.GetToken()},
                    {"f", "json"},
                    {"rollbackOnFailure","true" }
                };

                byte[] responsebytes = client.UploadValues(this.appSettings.GisApiEsriUrl + "/" + this.GisEnvPrefix + LayerName.ToString() + "/FeatureServer/" + SubData.ToString() + "/deleteFeatures", "POST", reqparm);
                string responsebody = Encoding.UTF8.GetString(responsebytes);
                DeleteRet = JsonConvert.DeserializeObject<DeleteRetModel>(responsebody);
            }
            return DeleteRet;
        }

        public string GetToken()
        {
            string token = string.Empty;

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            using (WebClient client = new WebClient())
            {
                var reqparm = new System.Collections.Specialized.NameValueCollection
                {
                    {"username",this.appSettings.GisApiUserName },
                    {"password", this.appSettings.GisApiPassword},
                    {"referer", "arcgis.com"},
                    {"expiration", "1440"},
                    {"f", "json"}
                };

                byte[] responsebytes = client.UploadValues(this.appSettings.GisApiTokenUrl, "POST", reqparm);
                string responsebody = Encoding.UTF8.GetString(responsebytes);
                dynamic data = JsonConvert.DeserializeObject(responsebody);

                token = data.token;
            }
            return token;
        }

        public static string GetModelFields(Type T)
        {
            return string.Join(",", T.GetProperties().Select(a => a.Name));
        }

        public class GisResult<T, T1>
        {
            public T GisAttributes;
            public IList<T1> Features;

        }

        public class DateTimeConverter : JsonConverter
        {
            private static readonly DateTime _epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                // writer.WriteRawValue(((DateTime)value - _epoch).TotalMilliseconds + "000");
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                if (reader.Value == null) { return null; }
                return _epoch.AddMilliseconds((long)reader.Value);
            }
            public override bool CanConvert(Type objectType)
            {
                return true;// objectType == typeof(User);
            }
        }
        public class DomainConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {

            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                dynamic r = Activator.CreateInstance(objectType);
                r.GetType().GetProperty(reader.Path).SetValue(r, reader.Value);
                return r;
            }

            public override bool CanConvert(Type objectType)
            {
                return true;// objectType == typeof(User);
            }
        }
    }

    public class GisErrorModel
    {
        public int code { get; set; }
        public string message { get; set; }
        public string[] details { get; set; }
        public string description { get; set; }
    }
    public class InsertRetModel
    {
        public addResults[] addResults { get; set; }
    }
    public class addResults
    {
        public int objectId { get; set; }
        public int uniqueId { get; set; }
        public string globalId { get; set; }
        public bool success { get; set; }
    }
    public class DeleteRetModel
    {
        public GisErrorModel error { get; set; }
        public deleteResults[] DeleteResults { get; set; }

    }
    public class deleteResults
    {
        public string objectId { get; set; }
        public string globalId { get; set; }
        public bool success { get; set; }
        public GisErrorModel error { get; set; }

    }
    //    {"addResults":[{"objectId":8700,"uniqueId":8700,"globalId":"B8062064-4A13-4E66-8DA3-E05E92D62DA0","success":true}]}
    public class GisModel
    {
        public GisErrorModel error { get; set; }
        public string objectIdFieldName { get; set; }
        public UniqueIdField uniqueIdField { get; set; }
        public Fields[] fields { get; set; }
        public attachmentGroups[] attachmentGroups { get; set; }
        public Features[] features { get; set; }
        public relatedRecordGroups[] relatedRecordGroups { get; set; }


    }
    public class Features
    {
        public dynamic attributes { get; set; }
    }
    public class UniqueIdField
    {
        public string name { get; set; }
        public Boolean isSystemMaintained { get; set; }
    }
    public class Fields
    {
        public string name { get; set; }
        public string type { get; set; }
        public string alias { get; set; }
        public string sqlType { get; set; }
        public int length { get; set; }
        public domain domain { get; set; }
    }
    public class relatedRecordGroups
    {
        public string objectId { get; set; }
        public relatedRecords[] relatedRecords { get; set; }
    }
    public class relatedRecords
    {
        public dynamic attributes { get; set; }
    }
    public class domain
    {
        public string type { get; set; }
        public string name { get; set; }
        public codedValues[] codedValues { get; set; }

    }
    public class codedValues
    {
        public string name { get; set; }
        public string code { get; set; }
    }
    public class attachmentGroups
    {
        public long parentObjectId { get; set; }
        public string parentGlobalId { get; set; }
        public attachmentInfos[] attachmentInfos { get; set; }

    }

    public class attachmentInfos
    {
        public long id { get; set; }
        public string globalId { get; set; }
        public string contentType { get; set; }
        public long size { get; set; }
        public string keywords { get; set; }
        public string exifInfo { get; set; }
        public string url { get; set; }
        public string Name { get; set; }

    }
    public class FilesAttachments
    {
        public string Url { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
    }
}
