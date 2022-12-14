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
    using Models.Interfaces;
    using System;
    using System.Collections.Generic;
    
    public partial class GeneralSetting : IEntityWithId
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public GeneralSetting()
        {
            this.Amplifiers = new HashSet<Amplifier>();
            this.Logs = new HashSet<Log>();
        }
    
        public System.Guid ID { get; set; }
        public Nullable<double> CapturingMinute { get; set; }
        public Nullable<double> RxPowerMinFix { get; set; }
        public Nullable<double> RxPowerMinWar { get; set; }
        public Nullable<double> RxPowerMaxWar { get; set; }
        public Nullable<double> RxSensMinFix { get; set; }
        public Nullable<double> RxSensMinWar { get; set; }
        public Nullable<double> RxSensMaxWar { get; set; }
        public Nullable<double> TxPowerMinFix { get; set; }
        public Nullable<double> TxPowerMinWar { get; set; }
        public Nullable<double> TxPowerMaxWar { get; set; }
        public Nullable<double> TxSensMinFix { get; set; }
        public Nullable<double> TxSensMinWar { get; set; }
        public Nullable<double> TxSensMaxWar { get; set; }
        public Nullable<double> TempMinFix { get; set; }
        public Nullable<double> TempMinWar { get; set; }
        public Nullable<double> TempMaxWar { get; set; }
        public Nullable<double> TxModeMinFix { get; set; }
        public Nullable<double> TxModeMinWar { get; set; }
        public Nullable<double> TxModeMaxWar { get; set; }
        public Nullable<bool> Crush { get; set; }
        public Nullable<int> RemovingInterval { get; set; }
        public Nullable<System.DateTime> DeadlineDate { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Amplifier> Amplifiers { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Log> Logs { get; set; }
        public override string ToString()
        {
            return "Settings";
        }
    }
}
