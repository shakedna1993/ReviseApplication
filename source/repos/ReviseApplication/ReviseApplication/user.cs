//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ReviseApplication
{
    using System;
    using System.Collections.Generic;
    
    public partial class user
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public user()
        {
            this.messages = new HashSet<message>();
            this.projUsers = new HashSet<projUser>();
            this.chats = new HashSet<chat>();
            this.userCatReqs = new HashSet<userCatReq>();
        }
    
        public string userid { get; set; }
        public string fname { get; set; }
        public string lname { get; set; }
        public Nullable<System.DateTime> birthday { get; set; }
        public string PhoneNum { get; set; }
        public Nullable<int> score { get; set; }
        public string UserName { get; set; }
        public string password { get; set; }
        public Nullable<int> isConnected { get; set; }
        public string Email { get; set; }
        public Nullable<System.Guid> ActivationCode { get; set; }
        public string ResetPasswordCode { get; set; }
        public Nullable<bool> IsAdmin { get; set; }
        public string pic { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<message> messages { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<projUser> projUsers { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<chat> chats { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<userCatReq> userCatReqs { get; set; }
    }
}
