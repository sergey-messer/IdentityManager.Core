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
    [Route(template: Constants.UserRoutePrefix)]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public class UserController : Controller
    {
        readonly IIdentityManagerService idmService;
        public UserController(IIdentityManagerService idmService)
        {
            this.idmService = idmService ?? throw new ArgumentNullException(nameof(idmService));
        }

        public IActionResult BadRequest<T>(T data)
        {
            return StatusCode((int)HttpStatusCode.BadRequest, data);
        }

        //public IActionResult NoContent()
        //{
        //    return StatusCode((int)HttpStatusCode.NoContent);
        //}

        public IActionResult MethodNotAllowed()
        {
            return StatusCode((int)HttpStatusCode.MethodNotAllowed);
        }

        IdentityManagerMetadata _metadata;
        async Task<IdentityManagerMetadata> GetMetadataAsync()
        {
            if (_metadata == null)
            {
                _metadata = await idmService.GetMetadataAsync();
                if (_metadata == null) throw new InvalidOperationException("GetMetadataAsync returned null");
                _metadata.Validate();
            }

            return _metadata;
        }

        [HttpGet, Route("", Name = Constants.RouteNames.GetUsers)]
        public async Task<IActionResult> GetUsersAsync(string filter = null, int start = 0, int count = 100)
        {
            var user = User.IsInRole("admin");

            var result = await idmService.QueryUsersAsync(filter, start, count);
            if (result.IsSuccess)
            {
                var meta = await GetMetadataAsync();
                var resource = new UserQueryResultResource(result.Result, Url, meta.UserMetadata);
                return Ok(resource);
            }

            return BadRequest(result.ToError());
        }

        [HttpPost, Route("", Name = Constants.RouteNames.CreateUser)]
        public async Task<IActionResult> CreateUserAsync([FromBody]PropertyValue[] properties)
        {
            var meta = await GetMetadataAsync();
            if (!meta.UserMetadata.SupportsCreate)
            {
                return MethodNotAllowed();
            }

            var errors = ValidateCreateProperties(meta.UserMetadata, properties);
            foreach (var error in errors)
            {
                ModelState.AddModelError("", error);
            }

            if (ModelState.IsValid)
            {
                var result = await this.idmService.CreateUserAsync(properties);
                if (result.IsSuccess)
                {
                    var url = Url.Link(Constants.RouteNames.GetUser, new { subject = result.Result.Subject });
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

        [HttpGet, Route("{subject}", Name = Constants.RouteNames.GetUser)]
        public async Task<IActionResult> GetUserAsync(string subject)
        {
            if (String.IsNullOrWhiteSpace(subject))
            {
                ModelState["subject.String"].Errors.Clear();
                ModelState.AddModelError("", Messages.SubjectRequired);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.ToError());
            }

            var result = await this.idmService.GetUserAsync(subject);
            if (result.IsSuccess)
            {
                if (result.Result == null)
                {
                    return NotFound();
                }

                var meta = await GetMetadataAsync();
                RoleSummary[] roles = null;
                if (!String.IsNullOrWhiteSpace(meta.RoleMetadata.RoleClaimType))
                {
                    var roleResult = await idmService.QueryRolesAsync(null, -1, -1);
                    if (!roleResult.IsSuccess)
                    {
                        return BadRequest(roleResult.Errors);
                    }

                    roles = roleResult.Result.Items.ToArray();
                }

                return Ok(new UserDetailResource(result.Result, Url, meta, roles));
            }

            return BadRequest(result.ToError());
        }

        [HttpDelete, Route("{subject}", Name = Constants.RouteNames.DeleteUser)]
        public async Task<IActionResult> DeleteUserAsync(string subject)
        {
            var meta = await GetMetadataAsync();
            if (!meta.UserMetadata.SupportsDelete)
            {
                return MethodNotAllowed();
            }

            if (String.IsNullOrWhiteSpace(subject))
            {
                ModelState["subject.String"].Errors.Clear();
                ModelState.AddModelError("", Messages.SubjectRequired);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.ToError());
            }

            var result = await this.idmService.DeleteUserAsync(subject);
            if (result.IsSuccess)
            {
                return NoContent();
            }

            return BadRequest(result.ToError());
        }

        [HttpPut, Route("{subject}/properties/{type}", Name = Constants.RouteNames.UpdateUserProperty)]
        public async Task<IActionResult> SetPropertyAsync(string subject, string type)
        {
            if (String.IsNullOrWhiteSpace(subject))
            {
                ModelState["subject.String"].Errors.Clear();
                ModelState.AddModelError("", Messages.SubjectRequired);
            }

            type = type.FromBase64UrlEncoded();

            string value = await Request.GetRawBodyStringAsync(); // await Request.Content.ReadAsStringAsync();
           
            var meta = await this.GetMetadataAsync();
            ValidateUpdateProperty(meta.UserMetadata, type, value);

            if (ModelState.IsValid)
            {
                var result = await this.idmService.SetUserPropertyAsync(subject, type, value);
                if (result.IsSuccess)
                {
                    return NoContent();
                }

                ModelState.AddErrors(result);
            }

            return BadRequest(ModelState.ToError());
        }

        [HttpPost, Route("{subject}/claims", Name = Constants.RouteNames.AddClaim)]
        public async Task<IActionResult> AddClaimAsync(string subject, [FromBody]ClaimValue model)
        {
            var meta = await GetMetadataAsync();
            if (!meta.UserMetadata.SupportsClaims)
            {
                return MethodNotAllowed();
            }

            if (String.IsNullOrWhiteSpace(subject))
            {
                ModelState["subject.String"].Errors.Clear();
                ModelState.AddModelError("", Messages.SubjectRequired);
            }

            if (model == null)
            {
                ModelState.AddModelError("", Messages.ClaimDataRequired);
            }

            if (ModelState.IsValid)
            {
                var result = await this.idmService.AddUserClaimAsync(subject, model.Type, model.Value);
                if (result.IsSuccess)
                {
                    return NoContent();
                }

                ModelState.AddErrors(result);
            }

            return BadRequest(ModelState.ToError());
        }

        [HttpDelete, Route("{subject}/claims/{type}/{value}", Name = Constants.RouteNames.RemoveClaim)]
        public async Task<IActionResult> RemoveClaimAsync(string subject, string type, string value)
        {
            type = type.FromBase64UrlEncoded();
            value = value.FromBase64UrlEncoded();

            var meta = await GetMetadataAsync();
            if (!meta.UserMetadata.SupportsClaims)
            {
                return MethodNotAllowed();
            }

            if (String.IsNullOrWhiteSpace(subject) ||
                String.IsNullOrWhiteSpace(type) ||
                String.IsNullOrWhiteSpace(value))
            {
                return NotFound();
            }

            var result = await this.idmService.RemoveUserClaimAsync(subject, type, value);
            if (result.IsSuccess)
            {
                return NoContent();
            }

            return BadRequest(result.ToError());
        }

        [HttpPost, Route("{subject}/roles/{role}", Name = Constants.RouteNames.AddRole)]
        public async Task<IActionResult> AddRoleAsync(string subject, string role)
        {
            var meta = await GetMetadataAsync();
            if (String.IsNullOrWhiteSpace(meta.RoleMetadata.RoleClaimType))
            {
                return MethodNotAllowed();
            }

            if (String.IsNullOrWhiteSpace(subject))
            {
                return NotFound();
            }

            role = role.FromBase64UrlEncoded();

            var result = await this.idmService.AddUserRoleAsync(subject,  role);
            if (result.IsSuccess)
            {
                return NoContent();
            }

            return BadRequest(result.ToError());
        }

        [HttpDelete, Route("{subject}/roles/{role}", Name = Constants.RouteNames.RemoveRole)]
        public async Task<IActionResult> RemoveRoleAsync(string subject, string role)
        {
            var meta = await GetMetadataAsync();
            if (String.IsNullOrWhiteSpace(meta.RoleMetadata.RoleClaimType))
            {
                return MethodNotAllowed();
            }

            if (String.IsNullOrWhiteSpace(subject))
            {
                return NotFound();
            }

            role = role.FromBase64UrlEncoded();

            var result = await this.idmService.RemoveUserRoleAsync(subject, role);
            if (result.IsSuccess)
            {
                return NoContent();
            }

            return BadRequest(result.ToError());
        }

        private IEnumerable<string> ValidateCreateProperties(UserMetadata userMetadata, IEnumerable<PropertyValue> properties)
        {
            if (userMetadata == null) throw new ArgumentNullException("userMetadata");
            properties = properties ?? Enumerable.Empty<PropertyValue>();

            var meta = userMetadata.GetCreateProperties();
            return meta.Validate(properties);
        }

        private void ValidateUpdateProperty(UserMetadata userMetadata, string type, string value)
        {
            if (userMetadata == null) throw new ArgumentNullException("userMetadata");

            if (String.IsNullOrWhiteSpace(type))
            {
                ModelState.AddModelError("", Messages.PropertyTypeRequired);
                return;
            }

            var prop = userMetadata.UpdateProperties.SingleOrDefault(x => x.Type == type);
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