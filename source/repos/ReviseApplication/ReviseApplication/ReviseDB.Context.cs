﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class ReviseDBEntities : DbContext
    {
        public ReviseDBEntities()
            : base("name=ReviseDBEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<category> categories { get; set; }
        public virtual DbSet<chat> chats { get; set; }
        public virtual DbSet<department> departments { get; set; }
        public virtual DbSet<gamification> gamifications { get; set; }
        public virtual DbSet<message> messages { get; set; }
        public virtual DbSet<projCat> projCats { get; set; }
        public virtual DbSet<project> projects { get; set; }
        public virtual DbSet<projUser> projUsers { get; set; }
        public virtual DbSet<role> roles { get; set; }
        public virtual DbSet<user> users { get; set; }
        public virtual DbSet<userCatReq> userCatReqs { get; set; }
        public virtual DbSet<requirement> requirements { get; set; }
    }
}
