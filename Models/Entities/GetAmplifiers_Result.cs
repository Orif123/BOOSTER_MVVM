//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Models.Entities
{
    using System;
    
    public partial class GetAmplifiers_Result
    {
        public System.Guid ID { get; set; }
        public string Name { get; set; }
        public string IP { get; set; }
        public Nullable<int> Port { get; set; }
        public Nullable<bool> Enabled { get; set; }
        public Nullable<System.Guid> SettingId { get; set; }
    }
}
