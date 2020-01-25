using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using KeepItSafer.Web.Data;

namespace KeepItSafer.Web.Migrations
{
    [DbContext(typeof(SqliteDataContext))]
    [Migration("20200119123600_AddDropboxTokenToUserAccount")]
    partial class AddDropboxTokenToUserAccount
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.1");

            modelBuilder.Entity("KeepItSafer.Web.Models.UserAccount", b =>
                {
                    b.Property<int>("UserAccountId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AuthenticationUri");

                    b.Property<string>("PasswordDatabase");

                    b.Property<string>("DropboxToken");

                    b.HasKey("UserAccountId");

                    b.ToTable("UserAccounts");
                });
        }
    }
}
