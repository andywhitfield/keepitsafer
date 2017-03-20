using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using KeepItSafer.Data;

namespace KeepItSafer.Migrations
{
    [DbContext(typeof(SqliteDataContext))]
    [Migration("20170320101642_InitialMigration")]
    partial class InitialMigration
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.1");

            modelBuilder.Entity("KeepItSafer.Models.UserAccount", b =>
                {
                    b.Property<int>("UserAccountId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AuthenticationUri");

                    b.HasKey("UserAccountId");

                    b.ToTable("UserAccounts");
                });
        }
    }
}
