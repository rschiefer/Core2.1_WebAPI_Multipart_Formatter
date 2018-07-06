using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using WebAPI_Multipart_Formatter.Models;

namespace WebAPI_Multipart_Formatter.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {

        public ValuesController()
        {

        }
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<ModelWithFile> Get(int id)
        {
            return new ModelWithFile { Name = "Joe", Number = 2 };
        }

        // ----------- SAMPLE POST Request ----------------------
        //POST /api/values HTTP/1.1
        //Host: localhost:44366
        //Content-Type: multipart/form-data;boundary=boundary
        //Cache-Control: no-cache
        //Postman-Token: 2ad0be11-0daf-3907-b5cb-3e16372269d5

        //--boundary
        //Content-Disposition: form-data; name="metadata"
        //Content-Type: application/json

        //{"name":"Robb","number":43}
        //--boundary
        //Content-Disposition: form-data; name="document"; filename="12348024_1150631324960893_344096225642532672_n.jpg"
        ////Content-Type: image/jpeg

        //rawimagecontentwhichlooksfunnyandgoesonforever.d.sf.d.f.sd.fsdkfjkslhfdkshfkjsdfdkfh
        //--boundary--

        //// POST api/values
        [DisableFormValueModelBinding]
        [HttpPost]
        public void Post([FromBody]ModelWithFile model)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
    public class MultipartFormDataFormatter : InputFormatter
    {
        public MultipartFormDataFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("multipart/form-data"));
        }

        public override bool CanRead(InputFormatterContext context)
        {
            return context.ModelType == typeof(ModelWithFile);
        }

        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
        {
            var boundary = MediaTypeHeaderValue.Parse(context.HttpContext.Request.ContentType).Boundary.Value;
            var reader = new MultipartReader(boundary, context.HttpContext.Request.Body);

            var section = await reader.ReadNextSectionAsync();

            ModelWithFile model = null;
            var file = new Models.File();

            while (section != null)
            {
                var disposition = ContentDispositionHeaderValue.Parse(section.ContentDisposition);
                if (disposition.Name == "metadata")
                {
                    model = JsonConvert.DeserializeObject<ModelWithFile>(await section.ReadAsStringAsync());
                }
                if (disposition.Name == "document")
                {
                    var stream = new MemoryStream();
                    var fileSection = section.AsFileSection();
                    fileSection.FileStream.CopyTo(stream);
                    file.Contents = stream.ToArray();
                    file.MediaType = fileSection.FileName;
                }


                try
                {
                    section = await reader.ReadNextSectionAsync();
                }
                catch
                {
                    section = null;
                }
            }

            model.MyFile = file;

            return InputFormatterResult.Success(model);
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class DisableFormValueModelBindingAttribute : Attribute, IResourceFilter
    {
        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            var factories = context.ValueProviderFactories;
            factories.RemoveType<FormValueProviderFactory>();
            factories.RemoveType<JQueryFormValueProviderFactory>();
        }

        public void OnResourceExecuted(ResourceExecutedContext context)
        {
        }
    }
}
