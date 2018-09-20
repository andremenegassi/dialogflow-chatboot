using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Google.Apis.Dialogflow.v2.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Chatbot.Controllers
{
    public class DialogFlowController : Controller
    {
 

        private bool Autorizado(NameValueCollection httpHeader) {

            string basicAuth = httpHeader["Authorization"];

            if (!string.IsNullOrEmpty(basicAuth))
            {
                basicAuth = basicAuth.Replace("Basic ", "");

                byte[] aux = System.Convert.FromBase64String(basicAuth);
                basicAuth = System.Text.Encoding.UTF8.GetString(aux);

                if (basicAuth == "usuario:senha")
                    return true;
            }

            return false;
        }

        [HttpPost]
        public ContentResult FulfillmentV2()
        {
            if (!Autorizado(Request.Headers))
            {
                return Content("Não autorizado");
            }

            string requestJson = new System.IO.StreamReader(Request.InputStream).ReadToEnd();
            GoogleCloudDialogflowV2WebhookRequest request =
                 JsonConvert.DeserializeObject<GoogleCloudDialogflowV2WebhookRequest>(requestJson);

            string action = request.QueryResult.Action;
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            List<GoogleCloudDialogflowV2Context> contexts = new List<GoogleCloudDialogflowV2Context>();

            if (request.QueryResult.Parameters != null && request.QueryResult.Parameters.Count > 0)
                parameters = (Dictionary<string, object>)request.QueryResult.Parameters;

            if (request.QueryResult.OutputContexts != null && request.QueryResult.OutputContexts.Count > 0)
                contexts = (List<GoogleCloudDialogflowV2Context>)request.QueryResult.OutputContexts;

            var response = new GoogleCloudDialogflowV2WebhookResponse();

            try
            {
                
                switch (action)
                {

                    case "input.AAAA":
                        //buscar a resposta....
                        response.FulfillmentText = "resposta";
                        break;

                    case "input.BBBB":
                        response.FulfillmentText = "resposta";
                        break;
                   
                    default:
                        response.FulfillmentText = "Não compreendi";
                        break;
                }
            }
            catch (Exception ex)
            {
                response.FulfillmentText = ex.Message;
            }

     
            string json =  Newtonsoft.Json.JsonConvert.SerializeObject(response, Formatting.Indented);

            //Revome a propriedade ETag não aceita pelo DialogFlow (bug do google?)
            JObject jo = JObject.Parse(json);
            RemoveFields(jo, new string[] { "ETag"});
            json = jo.ToString();


            return Content( json, "application/json", Encoding.UTF8);
        }


        private void RemoveFields(JToken token, string[] fields)
        {
            JContainer container = token as JContainer;
            if (container == null) return;

            List<JToken> removeList = new List<JToken>();
            foreach (JToken el in container.Children())
            {
                JProperty p = el as JProperty;
                if (p != null && fields.Contains(p.Name))
                {
                    removeList.Add(el);
                }
                RemoveFields(el, fields);
            }

            foreach (JToken el in removeList)
            {
                el.Remove();
            }
        }


    }
}