using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;


namespace GENProfiles.Pages
{
    public partial class ProfileForm
    {
        [Parameter]
        public Profile profile { get; set; } = new Profile();

        private EditContext EditContext;

        protected override void OnInitialized()
        {
            EditContext = new EditContext(profile);
        }
    }
}
