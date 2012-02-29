using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace WindowsProductKeyFinder
{
    public enum WindowsEditions
    {
        Windows7,

        [Description("Windows 7 Home Basic")]
        Windows7HomeBasic,

        [Description("Windows 7 Home Premium")]
        Windows7HomePremium,

        [Description("Windows 7 Professional")]
        Windows7Professional,

        [Description("Windows 7 Enterprise")]
        Windows7Enterprise,

        [Description("Windows 7 Ultimate")]
        Windows7Ultimate,

        WindowsXP,

        [Description("Windows XP Starter")]
        WindowsXPStarter,

        [Description("Windows XP Home")]
        WindowsXPHome,

        [Description("Windows XP Professional")]
        WindowsXPProfessional,

        NotSupported
    }
}
