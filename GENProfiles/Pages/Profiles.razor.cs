using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.JSInterop;

namespace GENProfiles.Pages
{
    public partial class Profiles
    {
        //by default the form is displayed for editing a profile, not for adding
        bool displayForm = false, addProfile = false, cloneProfile = false;
        string titleAddProfile = "Add Profile";
        string titleEditProfile = "Edit Profile";
        string titleCloneProfile = "Clone Profile";

        private List<Profile> profiles = Profile.GetProfiles();
        private long valNumber = Profile.MaxNumber() + 1;
        private long selectedNumber = 0;
        private long? valPulseWidth;
        private long? valFrequency;
        private long? valScanSpeed;
        private long? valFocalDistance;
        private long? valShapeSize;
        private long? valPower;
        private bool bActive = false;
        private long? valActive;
        private string valName = "";

        private long minPulseWidth = 2, maxPulseWidth = 500;    // ns
        private long minFrequency = 1, maxFrequency = 4000;     // kHz
        private long minScanSpeed = 1, maxScanSpeed = 10;        // m/s
        private long minFocalDistance = 163, maxFocalDistance = 254;    // mm
        private long minShapeSize = 5, maxShapeSize = 100;     // mm
        private long minPower = 5, maxPower = 100;             // W

        private bool flgName, flgPulseWidth, flgFrequency, flgScanSpeed, flgFocalDistance, flgShapeSize, flgPower;

        // Pulse widths - Cutoff frequencies table

        static long[] pw = { 2, 4, 6, 9, 13, 20, 30, 45, 60, 80, 100, 150, 200, 250, 350, 500 };
        static long[] frmin = { 3000, 2000, 1500, 1000, 700, 400, 300, 250, 210, 190, 165, 80, 70, 65, 65, 65 };
        static long[] frmax = { 4000, 4000, 4000, 4000, 3000, 3000, 3000, 2000, 2000, 2000, 1000, 1000, 1000, 900, 600, 500 };

        int i = 0;              // Pointer
        int len = pw.Length;    // Arrays length

        // Table lookup
        private long lookup(long[] xx, long[] yy, long x)
        {
            i = 0;
            while (x >= xx[i + 1] && i < len - 2) i++;
            if (i == 0 && x < xx[i]) return -1; // Won't happen
            if (i == len - 2)
            {
                if (x == xx[i + 1]) return yy[i + 1];
                else if (x > xx[i + 1]) return -1;  // Won't happen
            }
            return yy[i] + (x - xx[i]) * (yy[i + 1] - yy[i]) / (xx[i + 1] - xx[i]);
        }

        string FormTitle()
        {
            if (addProfile)
                return titleAddProfile;
            else if (cloneProfile)
                return titleCloneProfile;
            else
                return titleEditProfile;
        }

        void AddProfileForm()
        {
            displayForm = addProfile = true;
        }

        void CloseForm()
        {
            displayForm = addProfile = cloneProfile = false;
            if (selectedNumber != 0)
            {
                Profile profile = Profile.GetProfileByNumber(selectedNumber);
                profile.Selected = false;
                uriHelper.NavigateTo(uriHelper.Uri, forceLoad: true);   //Refresh component
            }
        }

        private void ProcessProfile(bool clone)
        {
            valActive = bActive ? 1 : 0;
            if (addProfile)
            {
                AddProfile();
            }
            else if (clone)
            {
                CloneProfile();
                //uriHelper.NavigateTo(uriHelper.Uri, forceLoad: true);   //Refresh component
            }
            else
            {
                EditProfile();
                //uriHelper.NavigateTo(uriHelper.Uri, forceLoad: true);   //Refresh component
            }
        }

        private void AddProfile()
        {
            if (ValidProfile(valPulseWidth, valFrequency, valScanSpeed, valFocalDistance, valShapeSize, valPower, valActive, valName))
            {
                Profile profile = new Profile
                {
                    Number = valNumber,
                    PulseWidth = (long)valPulseWidth,
                    Frequency = (long)valFrequency,
                    ScanSpeed = (long)valScanSpeed,
                    FocalDistance = (long)valFocalDistance,
                    ShapeSize = (long)valShapeSize,
                    Power = (long)valPower,
                    Active = (long)valActive,
                    Name = valName
                };
                profiles.Add(profile);
                Profile.AddProfile(profile);
                valNumber = Profile.MaxNumber() + 1;
                valPulseWidth = null;
                valFrequency = null;
                valScanSpeed = null;
                valFocalDistance = null;
                valShapeSize = null;
                valPower = null;
                valActive = null;
                valName = "";
            }
        }

        private bool ValidProfile(long? pwdth, long? freq, long? sSpeed, long? fDist, long? sSize, long? pwr, long? act, string name)
        {
            flgName = name.Trim() == "" ? true : false;
            flgPulseWidth = (pwdth < minPulseWidth || pwdth > maxPulseWidth || pwdth == null) ? true : false;

            // Find min cutoff freq
            minFrequency = lookup(pw, frmin, (long)pwdth);
            // Find max cutoff freq
            maxFrequency = lookup(pw, frmax, (long)pwdth);

            flgFrequency = (freq < minFrequency || freq > maxFrequency || freq == null) ? true : false;
            flgScanSpeed = (sSpeed < minScanSpeed || sSpeed > maxScanSpeed || sSpeed == null) ? true : false;
            flgFocalDistance = (fDist != minFocalDistance && fDist != maxFocalDistance) ? true : false;
            flgShapeSize = (sSize < minShapeSize || sSize > maxShapeSize || sSize == null) ? true : false;
            flgPower = (pwr < minPower || pwr > maxPower || pwr == null) ? true : false;

            return !flgName && !flgPulseWidth && !flgFrequency && !flgScanSpeed && !flgFocalDistance && !flgShapeSize && !flgPower;
        }

        private void SelectRow(Profile profile, bool selected)
        {
            profile.Selected = selected;
            selectedNumber = selected ? profile.Number : 0;
        }

        private void EditProfileForm(Profile profile)
        {
            SelectRow(profile, true);
            StateHasChanged();

            valNumber = profile.Number;
            valPulseWidth = profile.PulseWidth;
            valFrequency = profile.Frequency;
            valScanSpeed = profile.ScanSpeed;
            valFocalDistance = profile.FocalDistance;
            valShapeSize = profile.ShapeSize;
            valPower = profile.Power;
            valActive = profile.Active;
            bActive = valActive == 1 ? true : false;
            valName = profile.Name;

            displayForm = true;
        }

        private void EditProfile()
        {
            Profile profile = Profile.GetProfileByNumber(valNumber);

            if (ValidProfile(valPulseWidth, valFrequency, valScanSpeed, valFocalDistance, valShapeSize, valPower, valActive, valName))
            {
                profile.PulseWidth = (long)valPulseWidth;
                profile.Frequency = (long)valFrequency;
                profile.ScanSpeed = (long)valScanSpeed;
                profile.FocalDistance = (long)valFocalDistance;
                profile.ShapeSize = (long)valShapeSize;
                profile.Power = (long)valPower;
                profile.Active = (long)valActive;
                profile.Name = valName;
                profile.Selected = false;

                Profile.EditProfile(profile);
                CloseForm();
            }

        }

        private void CloneProfileForm(Profile profile)
        {
            SelectRow(profile, true);
            StateHasChanged();

            valPulseWidth = profile.PulseWidth;
            valFrequency = profile.Frequency;
            valScanSpeed = profile.ScanSpeed;
            valFocalDistance = profile.FocalDistance;
            valShapeSize = profile.ShapeSize;
            valPower = profile.Power;
            valActive = profile.Active;
            valName = profile.Name;

            displayForm = cloneProfile = true;
        }

        private void CloneProfile()
        {
            if (ValidProfile(valPulseWidth, valFrequency, valScanSpeed, valFocalDistance, valShapeSize, valPower, valActive, valName))
            {
                Profile profile = new Profile
                {
                    Number = Profile.MaxNumber() + 1,
                    PulseWidth = (long)valPulseWidth,
                    Frequency = (long)valFrequency,
                    ScanSpeed = (long)valScanSpeed,
                    FocalDistance = (long)valFocalDistance,
                    ShapeSize = (long)valShapeSize,
                    Power = (long)valPower,
                    Active = (long)valActive,
                    Name = valName
                };
                profiles.Add(profile);
                Profile.AddProfile(profile);
                valNumber = Profile.MaxNumber() + 1;
                valPulseWidth = null;
                valFrequency = null;
                valScanSpeed = null;
                valFocalDistance = null;
                valShapeSize = null;
                valPower = null;
                valActive = null;
                valName = "";

                CloseForm();
            }
        }

        private async void DelProfile(Profile profile)
        {
            SelectRow(profile, true);
            await InvokeAsync(StateHasChanged);
            await Task.Delay(100);
            jsPrompt(profile);
        }

        private async void jsPrompt(Profile profile)
        {
            if (await jsRuntime.InvokeAsync<bool>("confirm", "Delete profile?"))
            {
                profiles.Remove(profile);
                RenumProfiles(profile.Number);
                Profile.DeleteProfile(profile);
                Profile.RenumProfiles(profile.Number);
                selectedNumber = 0;
                valNumber = Profile.MaxNumber() + 1;
                StateHasChanged();
            }
            else
            {
                SelectRow(profile, false);
                StateHasChanged();

            }
        }

        private void RenumProfiles(long deletedNum)
        {
            foreach (var profile in profiles)
            {
                if (profile.Number >= deletedNum)
                {
                    profile.Number--;
                }
            }
        }

        private string Active(Profile profile)
        {
            if (profile.Active == 1)
                return "Active";
            else
                return "";
        }
    }
}
