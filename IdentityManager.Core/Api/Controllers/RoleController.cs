using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TzIdentityManager.Api.Models;
using TzIdentityManager.Core;
using TzIdentityManager.Core.Extensions;
using TzIdentityManager.Core.Metadata;
using TzIdentityManager.Extensions;
using TzIdentityManager.Resources;

namespace TzIdentityManager.Api.Controllers
{
    [Route(template: Constants.RoleRoutePrefix)]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public class RoleController : Controller
    {
        readonly IIdentityManagerService _idmService;
        public RoleController(IIdentityManagerService idmService)
        {
            this._idmService = idmService ?? throw new ArgumentNullException("idmService");
        }

        public IActionResult BadRequest<T>(T data)
        {
            return StatusCode((int)HttpStatusCode.BadRequest, data);
        }

        public IActionResult MethodNotAllowed()
        {
            return StatusCode((int)HttpStatusCode.MethodNotAllowed);
        }

        IdentityManagerMetadata _metadata;
        async Task<IdentityManagerMetadata> GetMetadataAsync()
        {
            if (_metadata == null)
            {
                _metadata = await _idmService.GetMetadataAsync();
                if (_metadata == null) throw new InvalidOperationException("GetMetadataAsync returned null");
                _metadata.Validate();
            }

            return _metadata;
        }

        [HttpGet, Route("", Name = Constants.RouteNames.GetRoles)]
        public async Task<IActionResult> GetRolesAsync(string filter = null, int start = 0, int count = 100)
        {
            var meta = await GetMetadataAsync();
            if (!meta.RoleMetadata.SupportsListing)
            {
                return MethodNotAllowed();
            }

            var result = await _idmService.QueryRolesAsync(filter, start, count);
            if (result.IsSuccess)
            {
                var resource = new RoleQueryResultResource(result.Result, Url, meta.RoleMetadata);
                return Ok(resource);
            }

            return BadRequest(result.ToError());
        }
        
        [HttpPost, Route("", Name = Constants.RouteNames.CreateRole)]
        public async Task<IActionResult> CreateRoleAsync([FromBody]PropertyValue[] properties)
        {
            var meta = await GetMetadataAsync();
            if (!meta.RoleMetadata.SupportsCreate)
            {
                return MethodNotAllowed();
            }

            var errors = ValidateCreateProperties(meta.RoleMetadata, properties);
            foreach (var error in errors)
            {
                ModelState.AddModelError("", error);
            }

            if (ModelState.IsValid)
            {
                var result = await this._idmService.CreateRoleAsync(properties);
                if (result.IsSuccess)
                {
                    var url = Url.Link(Constants.RouteNames.GetRole, new { subject = result.Result.Subject });
                    var resource = new
                    {
                        Data = new { subject = result.Result.Subject },
                        Links = new { detail = url }
                    };
                    return Created(url, resource);
                }

                ModelState.AddErrors(result);
            }

            return BadRequest(ModelState.ToError());
        }
        
        [HttpGet, Route("{subject}", Name = Constants.RouteNames.GetRole)]
        public async Task<IActionResult> GetRoleAsync(string subject)
        {
            var meta = await GetMetadataAsync();
            if (!meta.RoleMetadata.SupportsListing)
            {
                return MethodNotAllowed();
            }

            var result = await _idmService.GetRoleAsync(subject);
            if (result.IsSuccess)
            {
                if (result.Result == null)
                {
                    return NotFound();
                }

                return Ok(new RoleDetailResource(result.Result, Url, meta.RoleMetadata));
            }

            return BadRequest(result.ToError());
        }

        [HttpDelete, Route("{subject}", Name = Constants.RouteNames.DeleteRole)]
        public async Task<IActionResult> DeleteRoleAsync(string subject)
        {
            var meta = await GetMetadataAsync();
            if (!meta.RoleMetadata.SupportsDelete)
            {
                return MethodNotAllowed();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.ToError());
            }

            var result = await this._idmService.DeleteRoleAsync(subject);
            if (result.IsSuccess)
            {
                return NoContent();
            }

            return BadRequest(result.ToError());
        }

        [HttpPut, Route("{subject}/properties/{type}", Name = Constants.RouteNames.UpdateRoleProperty)]
        public async Task<IActionResult> SetPropertyAsync(string subject, string type)
        {
            if (String.IsNullOrWhiteSpace(subject))
            {
                ModelState["subject.String"].Errors.Clear();
                ModelState.AddModelError("", Messages.SubjectRequired);
            }

            type = type.FromBase64UrlEncoded();

            string value = await Request.GetRawBodyStringAsync();
            var meta = await this.GetMetadataAsync();
            ValidateUpdateProperty(meta.RoleMetadata, type, value);

            if (ModelState.IsValid)
            {
                var result = await this._idmService.SetRolePropertyAsync(subject, type, value);
                if (result.IsSuccess)
                {
                    return NoContent();
                }

                ModelState.AddErrors(result);
            }

            return BadRequest(ModelState.ToError());
        }

        private IEnumerable<string> ValidateCreateProperties(RoleMetadata roleMetadata, IEnumerable<PropertyValue> properties)
        {
            if (roleMetadata == null) throw new ArgumentNullException("roleMetadata");
            properties = properties ?? Enumerable.Empty<PropertyValue>();

            var meta = roleMetadata.GetCreateProperties();
            return meta.Validate(properties);
        }

        private void ValidateUpdateProperty(RoleMetadata roleMetadata, string type, string value)
        {
            if (roleMetadata == null) throw new ArgumentNullException("roleMetadata");

            if (String.IsNullOrWhiteSpace(type))
            {
                ModelState.AddModelError("", Messages.PropertyTypeRequired);
                return;
            }

            var prop = roleMetadata.UpdateProperties.SingleOrDefault(x => x.Type == type);
            if (prop == null)
            {
                ModelState.AddModelError("", String.Format(Messages.PropertyInvalid, type));
            }
            else
            {
                var error = prop.Validate(value);
                if (error != null)
                {
                    ModelState.AddModelError("", error);
                }
            }
        }
    }
}
