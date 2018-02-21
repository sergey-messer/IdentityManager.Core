using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TzIdentityManager.Configuration;

namespace TzIdentityManager.Assets
{
    class EmbeddedHtmlResult : ActionResult
    {
        string path;
        string file;
        private readonly SecurityConfiguration _securityConfiguration;

        public EmbeddedHtmlResult(HttpRequest request, string file, SecurityConfiguration securityConfiguration)
        {

            var pathbase = request.PathBase;
            this.path = pathbase;
            this.file = file;
            _securityConfiguration = securityConfiguration;
            
        }

       
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
                        ShowLoginButton = this._securityConfiguration.ShowLoginButton,
                        oauthSettings = new
                        {
                            authority = _securityConfiguration.Authority,
                            client_id =_securityConfiguration.ClientId// Constants.IdMgrClientId
                        }
                    })
                });

               
                streamWriter.Write(html);
                await streamWriter.FlushAsync();
            }

        }
        
    }
}
