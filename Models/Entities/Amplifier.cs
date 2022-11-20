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
    using Models.Extensions;
    using Models.Interfaces;
    using System;
    using System.Collections.Generic;
    
    public partial class Amplifier : ExtensionAmplifier, IEntityWithId
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Amplifier()
        {
            this.Logs = new HashSet<Log>();
        }
    
        public System.Guid ID { get; set; }
        public string Name { get; set; }
        public string IP { get; set; }
        public Nullable<int> Port { get; set; }
        public Nullable<bool> Enabled { get; set; }
        public Nullable<System.Guid> SettingId { get; set; }
    
        public virtual GeneralSetting GeneralSetting { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Log> Logs { get; set; }
        
        
        public override string ToString()
        {
            return Name;
        }
    }
}
