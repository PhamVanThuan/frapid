using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using Frapid.Areas.Authorization;
using Frapid.Areas.CSRF;
using Frapid.Authorization.Models;
using Frapid.Authorization.ViewModels;
using Frapid.Dashboard;
using Frapid.Dashboard.Controllers;

namespace Frapid.Authorization.Controllers
{
    [AntiForgery]
    public class EntityAccessGroupPolicyController: DashboardController
    {
        [RestrictAnonymous]
        [Route("dashboard/authorization/entity-access/group-policy")]
        [MenuPolicy]
        public async Task<ActionResult> GroupPolicyAsync()
        {
            var model = await GroupEntityAccessPolicyModel.GetAsync().ConfigureAwait(true);
            return this.FrapidView(this.GetRazorView<AreaRegistration>("AccessPolicy/GroupPolicy.cshtml", this.Tenant), model);
        }

        [RestrictAnonymous]
        [Route("dashboard/authorization/entity-access/group-policy/{officeId}/{roleId}")]
        public async Task<ActionResult> GetGroupPolicyAsync(int officeId, int roleId)
        {
            var model = await GroupEntityAccessPolicyModel.GetAsync(officeId, roleId).ConfigureAwait(true);
            return this.Ok(model);
        }

        [RestrictAnonymous]
        [Route("dashboard/authorization/entity-access/group-policy/{officeId}/{roleId}")]
        [HttpPost]
        public async Task<ActionResult> SaveGroupPolicyAsync(int officeId, int roleId, List<AccessPolicyInfo> model)
        {
            if(!this.ModelState.IsValid)
            {
                return this.InvalidModelState();
            }

            await GroupEntityAccessPolicyModel.SaveAsync(officeId, roleId, model).ConfigureAwait(true);
            return this.Ok();
        }
    }
}