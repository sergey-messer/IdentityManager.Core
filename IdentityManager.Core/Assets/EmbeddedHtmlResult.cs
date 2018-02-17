using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TzIdentityManager.Assets
{
    class EmbeddedHtmlResult : ActionResult
    {
        string path;
        string file;
        string authorization_endpoint;
        

        public EmbeddedHtmlResult(HttpRequest request, string file)
        {

            var pathbase = request.PathBase;// request.Scheme + "://" + request.Host;// GetOwinContext().Request.PathBase;
            this.path = pathbase;// pathbase.Value;
            this.file = file;
            this.authorization_endpoint = pathbase + Constants.AuthorizePath;
            
        }

        //public Task<System.Net.Http.HttpResponseMessage> ExecuteAsync(System.Threading.CancellationToken cancellationToken)
        //{
        //    return Task.FromResult(GetResponseMessage());
        //}
        public override async Task ExecuteResultAsync(ActionContext context) {

            var response = context.HttpContext.Response;
            response.ContentType = "text/html";
            using (var streamWriter = new StreamWriter(response.Body))
            {

                var html = AssetManager.LoadResourceString(this.file,
                new
                {
                    pathBase = this.path,
                    model = Newtonsoft.Json.JsonConvert.SerializeObject(new
                    {
                        PathBase = this.path,
                        ShowLoginButton = true,// this.securityConfiguration.ShowLoginButton,
                        oauthSettings = new
                        {
                            authorization_endpoint = this.authorization_endpoint,
                            client_id = Constants.IdMgrClientId
                        }
                    })
                });

                //streamWriter.Write("Lorem ipsum dolor sit amet, vim iudico utroque complectitur id." +
                //    " Graecis quaestio euripidis vis an. Dictas voluptua salutatus sed an," +
                //    " mnesarchum posidonium eos at. Pro ad latine accusam," +
                //    " epicurei invenire ocurreret ex nec, unum similique adolescens vel an." +
                //    " Nam adolescens incorrupte argumentum in.");
                streamWriter.Write(html);


                await streamWriter.FlushAsync();
            }


            //return Task.FromResult(GetResponseMessage());
        }
        //public Task ExecuteResultAsync(ActionContext context)
        //{
        //    return Task.FromResult(GetResponseMessage());
        //}

        public HttpResponseMessage GetResponseMessage()
        {
            var html = AssetManager.LoadResourceString(this.file,
                new {
                    pathBase = this.path,
                    model = Newtonsoft.Json.JsonConvert.SerializeObject(new
                    {
                        PathBase = this.path,
                        ShowLoginButton = true,// this.securityConfiguration.ShowLoginButton,
                        oauthSettings = new
                        {
                            authorization_endpoint = this.authorization_endpoint,
                            client_id = Constants.IdMgrClientId
                        }
                    })
                });

            return new HttpResponseMessage()
            {
                Content = new StringContent(html, Encoding.UTF8, "text/html")
            };
        }
    }
}
