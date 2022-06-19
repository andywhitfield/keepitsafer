﻿// <auto-generated />
using KeepItSafer.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace KeepItSafer.Web.Migrations
{
    [DbContext(typeof(SqliteDataContext))]
    [Migration("20201129093607_DropboxRefreshToken")]
    partial class DropboxRefreshToken
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.0");

            modelBuilder.Entity("KeepItSafer.Web.Models.UserAccount", b =>
                {
                    b.Property<int>("UserAccountId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("AuthenticationUri")
                        .HasColumnType("TEXT");

                    b.Property<string>("DropboxAccessToken")
                        .HasColumnType("TEXT");

                    b.Property<string>("DropboxRefreshToken")
                        .HasColumnType("TEXT");

                    b.Property<string>("PasswordDatabase")
                        .HasColumnType("TEXT");

                    b.HasKey("UserAccountId");

                    b.ToTable("UserAccounts");
                });
#pragma warning restore 612, 618
        }
    }
}