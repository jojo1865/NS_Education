using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using NS_Education.Tools.Extensions;

namespace NS_Education.Models.Entities
{
    public static class BSiteDataExtension
    {
        public static SiteDevicesDto GetDevicesFromSiteNotes(this B_SiteData siteData)
        {
            // 原本場地的設備是用設備對照檔的設備
            // 但經過開發需求調整，場地的設備和設備對照檔無關
            // 所以改成直接寫在場地本身的 Note

            if (siteData.Note.IsNullOrWhiteSpace())
                return new SiteDevicesDto();

            return JsonConvert.DeserializeObject<SiteDevicesDto>(siteData.Note);
        }

        public static void SetDevicesToSiteNotes(this B_SiteData siteData, SiteDevicesDto devices)
        {
            // 原本場地的設備是用設備對照檔的設備
            // 但經過開發需求調整，場地的設備和設備對照檔無關
            // 所以改成直接寫在場地本身的 Note

            siteData.Note = JsonConvert.SerializeObject(devices);
        }
    }

    public class SiteDeviceDto
    {
        public string DeviceName { get; set; }
        public int? Count { get; set; }
    }

    public class SiteDevicesDto
    {
        public IEnumerable<SiteDeviceDto> Devices { get; set; } = Array.Empty<SiteDeviceDto>();
    }
}